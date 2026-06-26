using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Navi;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Populations
{
    /// <summary>
    /// Incursion 
    /// Spawns hostile invading Hero Variant enemies near players in non-hub regions on a fixed interval.
    /// The combat body is an enemy Agent , with a rendering Avatar override , and custom controller to use powers with damage scaling
    /// Currently approximating characters , and for single-player Rogue to steal powers from. 
    /// </summary>
    public class IncursionManager
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        // Reference-only: a playable AvatarPrototype that cannot be spawned as an NPC.
        public const PrototypeId SheHulkAvatarProtoRef = (PrototypeId)12394659164528645362;

        // Combat body driven by the server. Render skin is applied via ClientPrototypeRefOverride.
        private const string DefaultEnemyProtoName = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";

        private static PrototypeId s_autoResolvedEnemy = PrototypeId.Invalid;

        // Roster of incursion enemy types. Discovered once via reflection in BuildEnemyFactories.
        private static readonly Func<Game, IncursionEnemyController>[] s_enemyFactories = BuildEnemyFactories();

        // Metadata for pattern-matching incursion enemy types. Populated lazily in EnsureEnemyMeta.
        private readonly record struct EnemyMeta(
            string TypeName,
            string Shorthand,
            string DisplayName,
            string AvatarName,
            Func<Game, IncursionEnemyController> Factory);

        private static List<EnemyMeta> s_enemyMeta;

        // Filtered pool for random spawns (excludes types matching IncursionExcludeEnemies config).
        // Lazily built on first use so GameDatabase is ready.
        private static Func<Game, IncursionEnemyController>[] s_randomFactories;
        private static readonly object s_randomFactoriesLock = new();

        #region  discovery

        /// <summary>
        /// Discovers concrete <see cref="IncursionEnemyController"/> subclasses with a public
        /// <c>(Game)</c> constructor and compiles a factory delegate for each.
        /// </summary>
        private static Func<Game, IncursionEnemyController>[] BuildEnemyFactories()
        {
            Type baseType = typeof(IncursionEnemyController);
            ParameterExpression gameParam = Expression.Parameter(typeof(Game), "game");

            var discovered = new List<(string Name, Func<Game, IncursionEnemyController> Factory)>();

            foreach (Type type in baseType.Assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsGenericTypeDefinition || baseType.IsAssignableFrom(type) == false)
                    continue;

                var ctor = type.GetConstructor(new[] { typeof(Game) });
                if (ctor == null)
                {
                    Logger.Warn($"[Incursion] Skipping incursion enemy '{type.Name}': no public (Game) constructor.");
                    continue;
                }

                var lambda = Expression.Lambda<Func<Game, IncursionEnemyController>>(
                    Expression.New(ctor, gameParam), gameParam);
                discovered.Add((type.Name, lambda.Compile()));
            }

            Func<Game, IncursionEnemyController>[] factories = discovered
                .OrderBy(d => d.Name, StringComparer.Ordinal)
                .Select(d => d.Factory)
                .ToArray();

            Logger.Info($"[Incursion] Registered {factories.Length} incursion enemy type(s): " +
                        string.Join(", ", discovered.OrderBy(d => d.Name, StringComparer.Ordinal).Select(d => d.Name)));

            return factories;
        }

        #endregion

        #region VARIABLES

        // Process-global toggles safe for console and game threads.
        private static volatile bool s_spawningEnabled;
        private static bool s_initializedFromConfig;
        private static PrototypeId s_enemyOverride = PrototypeId.Invalid;

        private const float SpawnRadius = 128.0f;
        private const float MaxSpawnDistance = 600.0f;
        private const int MinIntervalMs = 1000;

        // Only cull invaders with a score below this to make room for new spawns.
        // In-combat invaders score +1000, so this preserves active fights.
        private const float PriorityCullThreshold = 500f;

        private readonly Game _game;
        private readonly EventGroup _pendingEvents = new();
        private readonly EventPointer<IncursionTickEvent> _tickEvent = new();

        // One per live invader. Accessed only on this game's thread.
        private readonly List<IncursionEnemyController> _controllers = new();

        // Maps combat-body entity id to controller for damage scaling and stealable-power lookup.
        private readonly Dictionary<ulong, IncursionEnemyController> _controllersByEntity = new();

        // Process-global round-robin counter so new games continue the sequence rather
        // than restarting from 0 every time a player transfers to a new region.
        private static int s_roundRobinIndex = -1;

        private PrototypeId _enemyProtoRef = PrototypeId.Invalid;

        public bool IsRunning => s_spawningEnabled;
        public PrototypeId EnemyProtoRef => EffectiveEnemyRef;

        // Runtime override takes precedence over the per-game resolved enemy.
        private PrototypeId EffectiveEnemyRef => s_enemyOverride != PrototypeId.Invalid ? s_enemyOverride : _enemyProtoRef;

        // Static view of control state for console commands (no Game context).
        public static bool IsSpawningEnabled => s_spawningEnabled;

        // ------------------------------------------------------------------
        // Trial gauntlet state
        // ------------------------------------------------------------------
        private ulong _trialPlayerId;
        private ulong _trialAvatarId;
        private readonly List<Func<Game, IncursionEnemyController>> _trialRoster = new();
        private int _trialIndex = -1;
        private IncursionEnemyController _trialCurrentController;
        private readonly EventPointer<TrialCheckEvent> _trialCheckEvent = new();
        private readonly EventPointer<TrialSpawnEvent> _trialSpawnEvent = new();
        private bool _trialRunning;

        public bool IsTrialRunning => _trialRunning;
        public int TrialProgress => _trialRunning ? _trialIndex + 1 : 0;
        public int TrialTotal => _trialRoster.Count;

        #endregion

        #region Lifecycle  API

        public IncursionManager(Game game)
        {
            _game = game;
        }

        /// <summary>
        /// Resolves the default invader and starts the recurring scheduler.
        /// </summary>
        public void Initialize()
        {
            ResolveEnemy();

            // Read config once so runtime commands are not reset by new games.
            if (s_initializedFromConfig == false)
            {
                s_spawningEnabled = _game.CustomGameOptions.IncursionEnable;
                s_initializedFromConfig = true;
            }

            // Sync collator immediately so trial spawns before the first tick aren't silently dropped.
            IncursionLogCollator.Enabled = _game.CustomGameOptions.IncursionLogCollatorEnable;

            int intervalMs = GetIntervalMs();
            int baseMs = Math.Max(MinIntervalMs, _game.CustomGameOptions.IncursionIntervalMs);
            int randomMaxMs = _game.CustomGameOptions.IncursionRandomIntervalMaxMs;
            string intervalDesc = randomMaxMs > 0
                ? $"{intervalMs} (base={baseMs}, randomMax={randomMaxMs})"
                : $"{intervalMs}";
            LogInfo($"[Incursion] Initialize: enabled={s_spawningEnabled}, " +
                        $"intervalMs={intervalDesc}, enemy={DescribeEnemy()}");

            ScheduleNextTick();

            if (s_spawningEnabled == false)
                LogInfo("[Incursion] Spawning currently disabled. Use '!incursion start' to enable at runtime.");
        }

        /// <summary>
        /// Starts the recurring incursion timer.
        /// </summary>
        public bool Start() => EnableSpawning();

        /// <summary>
        /// Stops incursion spawning. Already-spawned invaders are left alone.
        /// </summary>
        public bool Stop() => DisableSpawning();

        /// <summary>
        /// Enables incursion spawning 
        /// </summary>
        public static bool EnableSpawning()
        {
            bool changed = s_spawningEnabled == false;
            s_spawningEnabled = true;
            Logger.Info(changed ? "[Incursion] Spawning ENABLED." : "[Incursion] Spawning enable ignored: already enabled.");
            return changed;
        }

        /// <summary>
        /// Disables incursion spawning 
        /// </summary>
        public static bool DisableSpawning()
        {
            bool changed = s_spawningEnabled;
            s_spawningEnabled = false;
            Logger.Info(changed ? "[Incursion] Spawning DISABLED." : "[Incursion] Spawning disable ignored: already disabled.");
            return changed;
        }

        /// <summary>
        /// Builds a status string from process-global state. 
        /// </summary>
        public static string GetStatusString()
        {
            var options = ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>();
            PrototypeId enemy = s_enemyOverride != PrototypeId.Invalid ? s_enemyOverride : s_autoResolvedEnemy;
            string enemyName = enemy != PrototypeId.Invalid
                ? GameDatabase.GetPrototypeName(enemy)
                : "(unresolved - auto-resolved per game on first wave)";

            int baseInterval = Math.Max(MinIntervalMs, options.IncursionIntervalMs);
            int maxRandom = options.IncursionRandomIntervalMaxMs;
            string intervalDesc = maxRandom > 0
                ? $"{baseInterval}-{baseInterval + maxRandom} (base={baseInterval}, randomMax={maxRandom})"
                : $"{baseInterval}";

            return $"Incursion status: spawningEnabled={s_spawningEnabled}, " +
                   $"intervalMs={intervalDesc}, " +
                   $"verbose={options.IncursionLogVerboseEnable}, requireAdmin={options.IncursionCommandsRequireAdmin}, " +
                   $"enemy={enemyName}, enemyOverridden={s_enemyOverride != PrototypeId.Invalid}.";
        }

        #endregion

        #region Public query API

        /// <summary>
        /// Releases scheduler resources. Called on game shutdown.
        /// </summary>
        public void Shutdown()
        {
            // Cancel only this game's ticks; leave global state for other games.
            _game.GameEventScheduler?.CancelAllEvents(_pendingEvents);

            foreach (IncursionEnemyController controller in _controllers)
                controller.Dispose();
            _controllers.Clear();
            _controllersByEntity.Clear();
        }

        /// <summary>
        /// Damage scale for the given invader entity and root power, or 1.0 if not a live invader.
        /// Queried by <see cref="Powers.PowerPayload"/>.
        /// </summary>
        public float GetOutgoingDamageScale(ulong entityId, PrototypeId rootPowerRef)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
                return controller.GetOutgoingDamageScale(rootPowerRef);

            return 1f;
        }

        /// <summary>
        /// Resolves the stealable-power override for the given invader entity.
        /// Returns true for live invaders (even when the ref is Invalid, meaning nothing is exposed to steal).
        /// </summary>
        public bool TryGetStealablePowerInfo(ulong entityId, out PrototypeId stealablePowerInfoRef)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
            {
                stealablePowerInfoRef = controller.StealablePowerInfoRef;
                return true;
            }

            stealablePowerInfoRef = PrototypeId.Invalid;
            return false;
        }

        /// <summary>
        /// Resolves the parent (root) power for a combo child effect so logging and scaling
        /// treat the whole combo chain as a single ability.
        /// </summary>
        public PrototypeId GetParentPowerForEffect(ulong entityId, PrototypeId effectRef)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
                return controller.GetParentPowerForEffect(effectRef);
            return PrototypeId.Invalid;
        }

        /// <summary>
        /// Sets the invader prototype at runtime.
        /// </summary>
        public string SetEnemy(PrototypeId enemyProtoRef) => SetEnemyStatic(enemyProtoRef);

        /// <summary>
        /// Sets the invader prototype process-wide. Applies to all games.
        /// </summary>
        public static string SetEnemyStatic(PrototypeId enemyProtoRef)
        {
            if (enemyProtoRef == PrototypeId.Invalid)
                return "Invalid prototype.";

            var proto = GameDatabase.GetPrototype<WorldEntityPrototype>(enemyProtoRef);
            if (IsValidEnemy(proto, out string invalidReason) == false)
                return $"Cannot use {GameDatabase.GetPrototypeName(enemyProtoRef)} as an invader: {invalidReason}.";

            s_enemyOverride = enemyProtoRef;
            Logger.Info($"[Incursion] Enemy override set to {GameDatabase.GetPrototypeName(enemyProtoRef)} ({(ulong)enemyProtoRef}).");
            return $"Incursion enemy set to {GameDatabase.GetPrototypeName(enemyProtoRef)} (applies to all games).";
        }

        /// <summary>
        /// Forces an immediate spawn near the given avatar, bypassing enabled/hub checks.
        /// </summary>
        public (WorldEntity, string) ForceIncursionForAvatar(Avatar avatar)
        {
            if (avatar == null || avatar.IsAliveInWorld == false)
                return (null, "avatar is not alive in world");

            var region = avatar.Region;
            if (region == null)
                return (null, "avatar has no region");

            bool isHub = IsHubRegion(region);
            LogInfo($"[Incursion] FORCE spawn requested by avatar {avatar.Id} in region " +
                        $"'{region.PrototypeName}' (hub={isHub}).");

            var entity = SpawnInvaderNearAvatar(avatar);
            if (entity == null)
                return (null, "spawn failed (see server log)");

            return (entity, "ok");
        }

        #endregion

        #region Scheduling

        /// <summary>
        /// Forces a spawn of a specific incursion enemy type matching the pattern.
        /// </summary>
        public (WorldEntity, string) ForceSpawnByPattern(Avatar avatar, string pattern)
        {
            if (avatar == null || avatar.IsAliveInWorld == false)
                return (null, "avatar is not alive in world");

            var region = avatar.Region;
            if (region == null)
                return (null, "avatar has no region");

            var (factory, error) = ResolveFactoryByPattern(pattern);
            if (factory == null)
                return (null, error);

            var controller = factory(_game);
            var entity = SpawnInvaderNearAvatar(avatar, controller);
            if (entity == null)
                return (null, "spawn failed (see server log)");

            return (entity, "ok");
        }

        // ---------------------------------------------------------------------
        // Scheduling
        // ---------------------------------------------------------------------

        private void ScheduleNextTick()
        {
            var scheduler = _game.GameEventScheduler;
            if (scheduler == null)
            {
                Logger.Warn("[Incursion] ScheduleNextTick: scheduler is null.");
                return;
            }

            if (_tickEvent.IsValid)
                return;

            scheduler.ScheduleEvent(_tickEvent, TimeSpan.FromMilliseconds(GetIntervalMs()), _pendingEvents);
            _tickEvent.Get().Initialize(this);
        }

        private void OnIncursionTick()
        {
            // Sync collator master switch so it can be toggled live without restart.
            IncursionLogCollator.Enabled = _game.CustomGameOptions.IncursionLogCollatorEnable;

            // Prune finished controllers to keep lookups in sync.
            for (int i = _controllers.Count - 1; i >= 0; i--)
            {
                IncursionEnemyController controller = _controllers[i];
                if (controller.IsFinished == false) continue;

                _controllersByEntity.Remove(controller.EntityId);
                _controllers.RemoveAt(i);
            }

            // Cull invaders that have exceeded their max lifetime.
            TimeSpan maxLifetime = TimeSpan.FromMilliseconds(_game.CustomGameOptions.IncursionMaxLifetimeMs);
            List<IncursionEnemyController> toRemove = new();
            for (int i = _controllers.Count - 1; i >= 0; i--)
            {
                IncursionEnemyController controller = _controllers[i];
                if (controller.IsDying) continue; // let dying grace period finish
                if (controller.IsExpired(maxLifetime))
                    toRemove.Add(controller);
            }
            foreach (IncursionEnemyController controller in toRemove)
            {
                Agent a = _game.EntityManager.GetEntity<Agent>(controller.EntityId);
                long h = a?.Properties[PropertyEnum.Health] ?? 0;
                long hm = a?.Properties[PropertyEnum.HealthMax] ?? 0;
                LogInfo($"[Incursion] {controller.GetLabel()} removed: exceeded max lifetime ({maxLifetime.TotalMinutes:F1} min), health={h}/{hm}.");
                RemoveInvader(controller);
            }
            toRemove.Clear();

            // Cull invaders that have been idle for too long.
            TimeSpan idleTimeout = TimeSpan.FromMilliseconds(_game.CustomGameOptions.IncursionIdleTimeoutMs);
            for (int i = _controllers.Count - 1; i >= 0; i--)
            {
                IncursionEnemyController controller = _controllers[i];
                if (controller.IsDying) continue; // let dying grace period finish
                if (controller.IsIdle(idleTimeout))
                    toRemove.Add(controller);
            }
            foreach (IncursionEnemyController controller in toRemove)
            {
                Agent a = _game.EntityManager.GetEntity<Agent>(controller.EntityId);
                long h = a?.Properties[PropertyEnum.Health] ?? 0;
                long hm = a?.Properties[PropertyEnum.HealthMax] ?? 0;
                LogInfo($"[Incursion] {controller.GetLabel()} removed: idle for >{idleTimeout.TotalSeconds:F0}s, health={h}/{hm}.");
                RemoveInvader(controller);
            }

            if (s_spawningEnabled)
            {
                int spawned = RunIncursionWave();
                if (spawned > 0 || _controllers.Count > 0)
                    LogInfo($"[Incursion] Wave complete: spawned {spawned} invader(s). Active invaders: {_controllers.Count}.");
            }
            else
            {
                LogVerbose("[Incursion] Tick fired but spawning is disabled; idling.");
            }

            // Continue ticking so re-enable does not need rescheduling.
            ScheduleNextTick();
        }

        #endregion

        #region Wave logic

        private int RunIncursionWave()
        {
            int spawned = 0;
            int playerCount = _game.EntityManager.PlayerCount;
            int maxActive = _game.CustomGameOptions.IncursionMaxActiveInvaders;
            LogVerbose($"[Incursion] RunIncursionWave: evaluating {playerCount} player(s), maxActive={maxActive}.");

            foreach (Player player in _game.EntityManager.Players)
            {
                // Cap check: if we're at or above max, try to cull the lowest-priority invader.
                if (_controllers.Count >= maxActive && TryCullLowestPriorityForSpawn() == false)
                { LogVerbose($"[Incursion]  skip player '{player?.GetName()}': at max active ({maxActive}) and no low-priority invaders to cull."); continue; }

                Avatar avatar = player?.CurrentAvatar;
                if (avatar == null || avatar.IsAliveInWorld == false)
                { LogVerbose($"[Incursion]  skip player '{player?.GetName()}': no alive avatar in world."); continue; }

                int charLevel = avatar.Properties[PropertyEnum.CharacterLevel];
                if (charLevel < 30)
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': avatar level {charLevel} < 30."); continue; }

                Region region = avatar.Region;
                if (region == null)
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': avatar has no region."); continue; }

                if (IsHubRegion(region))
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': in hub region '{region.PrototypeName}'."); continue; }

                if (IsBlacklistedRegion(region))
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': region '{region.PrototypeName}' is blacklisted."); continue; }

                if (IsPlayerInTrial(player))
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': currently in incursion trial."); continue; }

                LogVerbose($"[Incursion]  spawning for player '{player.GetName()}' in '{region.PrototypeName}'.");
                if (SpawnInvaderNearAvatar(avatar) != null)
                    spawned++;
            }

            return spawned;
        }

        private WorldEntity SpawnInvaderNearAvatar(Avatar avatar, IncursionEnemyController specificController = null)
        {
            var region = avatar.Region;
            if (region == null)
            {
                specificController?.Dispose();
                return Logger.WarnReturn<WorldEntity>(null, "[Incursion] SpawnInvaderNearAvatar: region == null");
            }

            var entityProto = GameDatabase.GetPrototype<WorldEntityPrototype>(EffectiveEnemyRef);
            if (IsValidEnemy(entityProto, out string invalidReason) == false)
            {
                specificController?.Dispose();
                return Logger.WarnReturn<WorldEntity>(null, $"[Incursion] SpawnInvaderNearAvatar: cannot spawn enemy {DescribeEnemy()}: {invalidReason}. Set a valid enemy with '!incursion enemy <pattern>' or the IncursionEnemyPrototype config.");
            }

            // Try several positions in a ring around the avatar to find an open nav spot.
            // Avoids spawning inside walls when the player is facing one.
            PathFlags pathFlags = Region.GetPathFlagsForEntity(entityProto);
            Vector3 spawnPosition = ChooseOpenSpawnPosition(region, avatar, entityProto, pathFlags);
            if (spawnPosition == Vector3.Zero)
            {
                specificController?.Dispose();
                return Logger.WarnReturn<WorldEntity>(null, $"[Incursion] SpawnInvaderNearAvatar: could not find open spawn position near {avatar.RegionLocation.Position.ToStringNames()}.");
            }

            var cell = region.GetCellAtPosition(spawnPosition);
            if (cell == null)
            {
                specificController?.Dispose();
                return Logger.WarnReturn<WorldEntity>(null, $"[Incursion] SpawnInvaderNearAvatar: no cell at {spawnPosition.ToStringNames()}.");
            }

            spawnPosition = RegionLocation.ProjectToFloor(region, spawnPosition);

            var manager = region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPosition, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = EffectiveEnemyRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;
            spec.BoundsScaleOverride = _game.CustomGameOptions?.IncursionEnemyVisualScale ?? 1.5f;

            // Use the specified controller or pick a random one, then apply its render skin.
            IncursionEnemyController controller = specificController ?? CreateRandomController();
            ApplyRenderSkin(spec, controller);

            int level = region.GetAreaLevel(cell.Area);
            spec.Properties[PropertyEnum.CharacterLevel] = level;
            spec.Properties[PropertyEnum.CombatLevel] = level;
            spec.Properties[PropertyEnum.VariationSeed] = _game.Random.Next(1, 10000);
            LogVerbose($"[Incursion]   chosen spawnPos={spawnPosition.ToStringNames()}, cellId={cell.Id}, level={level}");

            spec.Spawn();

            var entity = spec.ActiveEntity;
            if (entity == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                return Logger.WarnReturn<WorldEntity>(null, $"[Incursion] Spawn failed for {GameDatabase.GetPrototypeName(_enemyProtoRef)}.");
            }

            string renderInfo = entity.ClientPrototypeRefOverride != PrototypeId.Invalid
                ? $"renderedAs='{GameDatabase.GetPrototypeName(entity.ClientPrototypeRefOverride)}' (worldAsset={(ulong)entity.GetEntityWorldAsset()})"
                : "renderedAs=self";

            LogInfo($"[Incursion] Spawned combat body '{entity.PrototypeName}' (id {entity.Id}) at " +
                        $"{spawnPosition.ToStringNames()} level {level} in '{region.PrototypeName}'. " +
                        $"boundsScale=x{spec.BoundsScaleOverride:0.#}, {renderInfo}, hostileToPlayers={entity.IsHostileToPlayers()}, " +
                        $"hasAI={(entity is Agent agent && agent.AIController != null)}.");

            if (entity.IsClientRenderedAsAvatar)
            {
                PrototypeId appliedCostume = entity.Properties[PropertyEnum.CostumeCurrent];
                var costumeProto = appliedCostume.As<CostumePrototype>();
                AssetId avatarUnreal = entity.GetEntityWorldAsset();
                AssetId costumeUnreal = costumeProto != null ? costumeProto.CostumeUnrealClass : AssetId.Invalid;
                string avatarUnrealName = avatarUnreal != AssetId.Invalid ? GameDatabase.GetAssetName(avatarUnreal) : "(none)";
                string costumeUnrealName = costumeUnreal != AssetId.Invalid ? GameDatabase.GetAssetName(costumeUnreal) : "(none)";

                LogInfo($"[Incursion]   render diag: costume={(appliedCostume != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(appliedCostume) : "(none)")}, " +
                            $"avatarUnreal='{avatarUnrealName}', costumeUnreal='{costumeUnrealName}'.");
            }

            if (entity is Agent invaderAgent)
            {
                controller.Start(invaderAgent);
                controller.BeginIntro(invaderAgent);
                _controllers.Add(controller);
                _controllersByEntity[controller.EntityId] = controller;
                IncursionLogCollator.BeginSession(invaderAgent.Id, controller.GetLabel() ?? controller.GetType().Name);
            }
            else
            {
                controller.Dispose();
            }

            return entity;
        }

        /// <summary>
        /// Picks the next enemy type from the roster in round-robin order. The returned controller is unbound; call Start after spawn.
        /// Respects the IncursionExcludeEnemies config filter.
        /// </summary>
        private IncursionEnemyController CreateRandomController()
        {
            var factories = GetRandomFactories();
            int idx = Interlocked.Increment(ref s_roundRobinIndex);
            int index = (int)((uint)idx % (uint)factories.Length);
            var controller = factories[index](_game);
            LogInfo($"[Incursion] Round-robin selected {controller.GetLabel()} ({index + 1}/{factories.Length})");
            return controller;
        }

        /// <summary>
        /// Builds (or returns the cached) filtered factory array for random spawns,
        /// excluding any enemy whose shorthand, display name, or avatar name matches
        /// a pattern in the IncursionExcludeEnemies config.
        /// </summary>
        private static Func<Game, IncursionEnemyController>[] GetRandomFactories()
        {
            if (s_randomFactories != null) return s_randomFactories;

            lock (s_randomFactoriesLock)
            {
                if (s_randomFactories != null) return s_randomFactories;

                var options = ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>();
                var excluded = ParseExcludedPatterns(options.IncursionExcludeEnemies);

                if (excluded.Count == 0)
                {
                    s_randomFactories = s_enemyFactories;
                    Logger.Info($"[Incursion] Random spawn pool: all {s_enemyFactories.Length} type(s) (no exclusions).");
                    return s_randomFactories;
                }

                EnsureEnemyMeta();

                var filtered = new List<Func<Game, IncursionEnemyController>>();
                var excludedNames = new List<string>();

                foreach (var meta in s_enemyMeta)
                {
                    bool isExcluded = false;
                    foreach (var pattern in excluded)
                    {
                        if (meta.Shorthand.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                            || meta.DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                            || meta.AvatarName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            isExcluded = true;
                            break;
                        }
                    }

                    if (isExcluded)
                        excludedNames.Add(meta.Shorthand);
                    else
                        filtered.Add(meta.Factory);
                }

                if (filtered.Count == 0)
                {
                    Logger.Warn($"[Incursion] All {s_enemyFactories.Length} enemy type(s) matched exclusion patterns ({string.Join(", ", excluded)}). Falling back to full roster.");
                    s_randomFactories = s_enemyFactories;
                }
                else
                {
                    Logger.Info($"[Incursion] Random spawn pool: {filtered.Count}/{s_enemyFactories.Length} type(s) after exclusions. Excluded: {string.Join(", ", excludedNames)}.");
                    s_randomFactories = filtered.ToArray();
                }

                // Shuffle so the round-robin order is random, not alphabetical by registration.
                int n = s_randomFactories.Length;
                for (int i = n - 1; i > 0; i--)
                {
                    int j = Random.Shared.Next(i + 1);
                    (s_randomFactories[i], s_randomFactories[j]) = (s_randomFactories[j], s_randomFactories[i]);
                }

                return s_randomFactories;
            }
        }

        private static List<string> ParseExcludedPatterns(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = part.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) == false)
                    result.Add(trimmed);
            }
            return result;
        }

        /// <summary>
        /// Applies the controller's render identity to the spawn spec before Spawn().
        /// Uses the controller's costume if valid, otherwise the avatar's starting costume.
        /// </summary>
        private void ApplyRenderSkin(SpawnSpec spec, IncursionEnemyController controller)
        {
            PrototypeId renderRef = controller.RenderAvatarRef;
            if (renderRef == PrototypeId.Invalid)
                return;

            var avatarProto = renderRef.As<AvatarPrototype>();
            if (avatarProto == null)
            {
                Logger.Warn($"[Incursion] {controller.GetType().Name}.RenderAvatarRef '{GameDatabase.GetPrototypeName(renderRef)}' is not an avatar; rendering the combat body itself.");
                return;
            }

            spec.ClientRenderPrototypeRef = renderRef;

            // Custom overhead name ( "Incursion Invader") drawn above the rendered avatar.
            string displayName = controller.InvaderDisplayName;
            if (string.IsNullOrEmpty(displayName) == false)
            {
                string prefix = controller.NameplatePrefix;
                string suffix = controller.NameplateSuffix;
                if (string.IsNullOrEmpty(prefix) == false)
                    displayName = prefix + displayName;
                if (string.IsNullOrEmpty(suffix) == false)
                    displayName = displayName + suffix;
            }
            spec.ClientRenderPlayerName = displayName;

            // The avatar's visible model is the costume's CostumeUnrealClass. 
            PrototypeId costumeRef = controller.RenderCostumeRef;
            if (costumeRef == PrototypeId.Invalid || costumeRef.As<CostumePrototype>() == null)
                costumeRef = avatarProto.GetStartingCostumeForPlatform(Platforms.PC);

            if (costumeRef != PrototypeId.Invalid)
                spec.Properties[PropertyEnum.CostumeCurrent] = costumeRef;

            LogInfo($"[Incursion]   render skin: {controller.GetType().Name} as avatar '{GameDatabase.GetPrototypeName(renderRef)}' " +
                        $"costume={(costumeRef != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeRef) : "(none)")}.");
        }

        #endregion

        #region Spawning

        /// <summary>
        /// Tries positions ahead of the player's viewing direction first, then falls back
        /// to a wider arc around the player, and finally right next to the player.
        /// </summary>
        private Vector3 ChooseOpenSpawnPosition(Region region, Avatar avatar, WorldEntityPrototype entityProto, PathFlags pathFlags)
        {
            Vector3 playerPos = avatar.RegionLocation.Position;
            float playerYaw = avatar.Orientation.Yaw;
            float baseDistance = 300f + (float)(_game.Random.NextDouble() * 200f); // 300-500 units away

            // 1) Forward arc: try angles centred on the player's yaw (-45 to +45 degrees).
            for (int i = 0; i < 5; i++)
            {
                float angleOffset = (i - 2) * (MathF.PI / 8f); // -π/4, -π/8, 0, +π/8, +π/4
                float angle = playerYaw + angleOffset;
                Vector3 origin = playerPos + new Vector3(MathF.Cos(angle) * baseDistance, MathF.Sin(angle) * baseDistance, 0f);
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, SpawnRadius);
                if (candidate != origin)
                    return candidate;
            }

            // 2) Wider fallback: random offset then sweep full circle in 8 steps.
            float fallbackAngleOffset = (float)(_game.Random.NextDouble() * MathF.PI * 2f);
            for (int i = 0; i < 8; i++)
            {
                float angle = fallbackAngleOffset + (i * MathF.PI / 4f);
                Vector3 origin = playerPos + new Vector3(MathF.Cos(angle) * baseDistance, MathF.Sin(angle) * baseDistance, 0f);
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, SpawnRadius);
                if (candidate != origin)
                    return candidate;
            }

            // 3) Last resort: try right next to the player.
            {
                Vector3 origin = playerPos;
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, SpawnRadius);
                if (candidate != origin)
                    return candidate;
            }

            return Vector3.Zero;
        }

        private static Vector3 ChooseSpawnPosition(Region region, Vector3 position, ref Bounds bounds, PathFlags pathFlags, float radius)
        {
            Vector3 spawnPosition = position;
            var posFlags = PositionCheckFlags.CanBeBlockedEntity | PositionCheckFlags.CanPathTo;
            var blockFlags = BlockingCheckFlags.CheckSpawns;

            if (region.IsLocationClear(ref bounds, pathFlags, posFlags, blockFlags))
                return bounds.Center;

            float minDistance;
            float maxDistance = 0.0f;
            bool spawnFound = false;

            while (spawnFound == false)
            {
                minDistance = maxDistance;
                maxDistance += radius;
                if (maxDistance > MaxSpawnDistance) return position;
                spawnFound = region.ChooseRandomPositionNearPoint(ref bounds, pathFlags, posFlags, blockFlags, minDistance, maxDistance, out spawnPosition);
            }

            return spawnPosition;
        }

        #endregion

        #region Trial gauntlet

        /// <summary>
        /// Starts a 1v1 gauntlet trial for the given player.
        /// Every incursion enemy type is shuffled into a highlander list and
        /// spawned one at a time. The next enemy appears 5 seconds after the
        /// previous one is defeated.
        /// </summary>
        public string StartTrial(Player player)
        {
            if (_trialRunning) return "An incursion trial is already in progress.";
            if (player == null) return "Player not found.";

            Avatar avatar = player.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false)
                return "Avatar not found or not alive in world.";

            Region region = avatar.Region;
            if (region == null || IsHubRegion(region))
                return "Cannot start an incursion trial in a hub region.";

            // Build a shuffled roster of every enemy type (highlander — each once).
            _trialRoster.Clear();
            foreach (var factory in s_enemyFactories)
                _trialRoster.Add(factory);

            // Fisher-Yates shuffle using the game's RNG.
            int n = _trialRoster.Count;
            while (n > 1)
            {
                int k = _game.Random.Next(n--);
                (_trialRoster[n], _trialRoster[k]) = (_trialRoster[k], _trialRoster[n]);
            }

            _trialPlayerId = player.Id;
            _trialAvatarId = avatar.Id;
            _trialIndex = 0;
            _trialRunning = true;

            SpawnTrialEnemy();
            return $"Incursion trial started! Defeat {_trialRoster.Count} invaders one by one.";
        }

        /// <summary>
        /// Ends an active trial, killing the current enemy and clearing state.
        /// </summary>
        public void EndTrial(string reason = null)
        {
            if (_trialRunning == false) return;

            var scheduler = _game.GameEventScheduler;
            if (scheduler != null)
            {
                scheduler.CancelEvent(_trialCheckEvent);
                scheduler.CancelEvent(_trialSpawnEvent);
            }

            if (_trialCurrentController != null && _trialCurrentController.IsFinished == false)
                RemoveInvader(_trialCurrentController);

            _trialRunning = false;
            _trialCurrentController = null;
            _trialRoster.Clear();
            _trialPlayerId = 0;
            _trialAvatarId = 0;
            _trialIndex = -1;

            string msg = reason != null
                ? $"[Incursion:Trial] Trial ended: {reason}"
                : "[Incursion:Trial] Trial ended.";
            LogInfo(msg);
        }

        private void SpawnTrialEnemy()
        {
            if (_trialRunning == false || _trialIndex >= _trialRoster.Count) return;

            Avatar avatar = _game.EntityManager.GetEntity<Avatar>(_trialAvatarId);
            if (avatar == null || avatar.IsAliveInWorld == false)
            {
                EndTrial("Avatar no longer available.");
                return;
            }

            Region region = avatar.Region;
            if (region == null || IsHubRegion(region))
            {
                EndTrial("Player entered a hub region.");
                return;
            }

            var factory = _trialRoster[_trialIndex];
            var controller = factory(_game);
            var entity = SpawnInvaderNearAvatar(avatar, controller);
            if (entity == null)
            {
                EndTrial("Failed to spawn trial invader.");
                return;
            }

            _trialCurrentController = controller;
            LogInfo($"[Incursion:Trial] Spawned enemy {_trialIndex + 1}/{_trialRoster.Count}: {controller.GetLabel()}.");
            ScheduleTrialCheck();
        }

        private void ScheduleTrialCheck()
        {
            var scheduler = _game.GameEventScheduler;
            if (scheduler == null) return;
            if (_trialCheckEvent.IsValid) return;
            scheduler.ScheduleEvent(_trialCheckEvent, TimeSpan.FromSeconds(1), _pendingEvents);
            _trialCheckEvent.Get().Initialize(this);
        }

        /// <summary>
        /// Called every ~1 second while a trial is active to check if the current
        /// enemy has been defeated. When it has, schedules the next spawn after 5s.
        /// </summary>
        private void OnTrialCheck()
        {
            if (_trialRunning == false) return;

            // Validate player still exists and is in a valid region.
            Player player = _game.EntityManager.GetEntity<Player>(_trialPlayerId);
            if (player == null)
            {
                EndTrial("Player disconnected.");
                return;
            }

            Avatar avatar = player.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false)
            {
                EndTrial("Avatar no longer available.");
                return;
            }

            Region region = avatar.Region;
            if (region == null || IsHubRegion(region))
            {
                EndTrial("Player entered a hub region.");
                return;
            }

            // Check whether the current enemy is dead.
            bool enemyDead = false;
            if (_trialCurrentController == null || _trialCurrentController.IsFinished)
            {
                enemyDead = true;
            }
            else
            {
                Agent agent = _game.EntityManager.GetEntity<Agent>(_trialCurrentController.EntityId);
                if (agent == null || agent.IsAliveInWorld == false || agent.Properties[PropertyEnum.Health] <= 0)
                    enemyDead = true;
            }

            if (enemyDead == false)
            {
                ScheduleTrialCheck();
                return;
            }

            // Current enemy defeated — advance index.
            _trialIndex++;
            if (_trialIndex >= _trialRoster.Count)
            {
                EndTrial("Trial complete! All invaders defeated.");
                return;
            }

            // Schedule next spawn after 5-second delay.
            var scheduler = _game.GameEventScheduler;
            if (scheduler == null) return;
            if (_trialSpawnEvent.IsValid) return;
            scheduler.ScheduleEvent(_trialSpawnEvent, TimeSpan.FromSeconds(5), _pendingEvents);
            _trialSpawnEvent.Get().Initialize(this);

            LogInfo($"[Incursion:Trial] Enemy defeated. Next invader in 5 seconds. Progress: {_trialIndex + 1}/{_trialRoster.Count}.");
        }

        /// <summary>
        /// Spawns the next enemy in the trial roster.
        /// </summary>
        private void OnTrialSpawnNext()
        {
            if (_trialRunning == false) return;
            SpawnTrialEnemy();
        }

        /// <summary>
        /// Returns true if the given player is currently participating in a trial.
        /// </summary>
        private bool IsPlayerInTrial(Player player) => _trialRunning && player != null && player.Id == _trialPlayerId;

        #endregion

        #region Helpers

        private void ResolveEnemy()
        {
            _enemyProtoRef = ResolveDefaultEnemy();
            if (_enemyProtoRef == PrototypeId.Invalid)
                Logger.Warn($"[Incursion] Default enemy '{DefaultEnemyProtoName}' could not be resolved. No invaders will spawn until you set one with '!incursion enemy <pattern>'.");
            else
                LogInfo($"[Incursion] Using default enemy (combat body): {DescribeEnemy()}.");
        }

        /// <summary>
        /// Resolves <see cref="DefaultEnemyProtoName"/> to a prototype ref.
        /// Result is cached statically. 
        /// </summary>
        private PrototypeId ResolveDefaultEnemy()
        {
            if (s_autoResolvedEnemy != PrototypeId.Invalid)
                return s_autoResolvedEnemy;

            PrototypeId resolved = GameDatabase.GetPrototypeRefByName(DefaultEnemyProtoName);
            if (resolved == PrototypeId.Invalid)
            {
                Logger.Warn($"[Incursion] Default enemy prototype '{DefaultEnemyProtoName}' not found in loaded data.");
                return PrototypeId.Invalid;
            }

            var proto = GameDatabase.GetPrototype<WorldEntityPrototype>(resolved);
            if (IsValidEnemy(proto, out string reason) == false)
            {
                Logger.Warn($"[Incursion] Default enemy prototype '{DefaultEnemyProtoName}' is not usable ({reason}).");
                return PrototypeId.Invalid;
            }

            s_autoResolvedEnemy = resolved;
            return resolved;
        }

        private static bool IsValidEnemy(WorldEntityPrototype proto, out string reason)
        {
            if (proto == null)
            {
                reason = "prototype is null or not a WorldEntityPrototype";
                return false;
            }

            if (proto is AvatarPrototype)
            {
                reason = "playable avatars require an owning player and cannot be spawned as NPCs";
                return false;
            }

            if (proto is AgentPrototype agentProto && agentProto.Locomotion?.Immobile == true)
            {
                reason = "immobile prototypes cannot be used as invasion combat bodies";
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>
        /// Kills the invader's agent and immediately disposes its controller.
        /// </summary>
        private void RemoveInvader(IncursionEnemyController controller)
        {
            if (controller.IsDying)
                return; // let dying grace period finish naturally

            IncursionLogCollator.EndSession(controller.EntityId);

            Agent agent = _game.EntityManager.GetEntity<Agent>(controller.EntityId);
            if (agent != null && agent.IsAliveInWorld)
                agent.Kill(null, KillFlags.NoLoot | KillFlags.NoExp | KillFlags.NoDeadEvent);

            controller.Dispose();
            _controllersByEntity.Remove(controller.EntityId);
            _controllers.Remove(controller);
        }

        /// <summary>
        /// Finds the lowest-priority invader and removes it if its score is below the threshold.
        /// Returns true if an invader was culled to make room.
        /// </summary>
        private bool TryCullLowestPriorityForSpawn()
        {
            if (_controllers.Count == 0) return false;

            IncursionEnemyController lowest = null;
            float lowestScore = float.MaxValue;

            foreach (IncursionEnemyController controller in _controllers)
            {
                if (controller.IsDying) continue;
                float score = controller.GetPriorityScore();
                if (score < lowestScore)
                {
                    lowestScore = score;
                    lowest = controller;
                }
            }

            if (lowest != null && lowestScore < PriorityCullThreshold)
            {
                Agent agent = _game.EntityManager.GetEntity<Agent>(lowest.EntityId);
                long health = agent?.Properties[PropertyEnum.Health] ?? 0;
                long healthMax = agent?.Properties[PropertyEnum.HealthMax] ?? 0;
                TimeSpan age = _game.CurrentTime - lowest.SpawnTime;
                LogInfo($"[Incursion] Culling low-priority invader {lowest.GetLabel()} (score={lowestScore:F1}, health={health}/{healthMax}, age={age.TotalSeconds:F0}s) to make room.");
                RemoveInvader(lowest);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Populates the enemy metadata cache by creating temporary instances to inspect virtual properties.
        /// </summary>
        private static void EnsureEnemyMeta()
        {
            if (s_enemyMeta != null) return;

            s_enemyMeta = new List<EnemyMeta>(s_enemyFactories.Length);
            foreach (var factory in s_enemyFactories)
            {
                var temp = factory(null);
                string typeName = temp.GetType().Name;
                string shorthand = IncursionEnemyController.StripControllerPrefix(typeName);
                string displayName = temp.InvaderDisplayName ?? string.Empty;
                string avatarName = temp.RenderAvatarRef != PrototypeId.Invalid
                    ? GameDatabase.GetPrototypeName(temp.RenderAvatarRef)
                    : string.Empty;

                s_enemyMeta.Add(new EnemyMeta(typeName, shorthand, displayName, avatarName, factory));
                temp.Dispose();
            }

            Logger.Info($"[Incursion] Enemy meta cached: {s_enemyMeta.Count} type(s).");
        }

        /// <summary>
        /// Finds enemy factories whose shorthand name, display name, or render avatar name contain the pattern.
        /// Returns a randomly chosen match, or an error message when no match is found.
        /// </summary>
        private (Func<Game, IncursionEnemyController>, string) ResolveFactoryByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return (null, "pattern is empty");

            EnsureEnemyMeta();

            string p = pattern.Trim();
            var matches = new List<EnemyMeta>();
            foreach (var meta in s_enemyMeta)
            {
                if (meta.Shorthand.Contains(p, StringComparison.OrdinalIgnoreCase)
                    || meta.DisplayName.Contains(p, StringComparison.OrdinalIgnoreCase)
                    || meta.AvatarName.Contains(p, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(meta);
                }
            }

            if (matches.Count == 0)
            {
                var suggestions = s_enemyMeta
                    .Select(m => $"- {m.Shorthand}{(string.IsNullOrEmpty(m.DisplayName) ? "" : $" ({m.DisplayName})")}")
                    .ToList();
                return (null, $"No incursion enemy matches '{p}'. Known enemies:\r\n{string.Join("\r\n", suggestions)}");
            }

            var chosen = matches[_game.Random.Next(matches.Count)];
            return (chosen.Factory, null);
        }

        /// <summary>
        /// Region prototype name substrings that block incursion spawning even when the
        /// region is not a hub. Add exact prototype name fragments here (case-insensitive).
        /// </summary>
        private static readonly HashSet<string> s_regionBlacklist = new(StringComparer.OrdinalIgnoreCase)
        {
            // SWORD headquarters building in Hightown — behaves like a safe zone
            "SwordHQ",
            "SwordHeadquarters",
        };

        private static bool IsHubRegion(Region region)
        {
            if (region.Prototype == null) return false;

            // Official hub behavior (town / safe zone)
            if (region.Prototype.Behavior == RegionBehavior.Town)
                return true;

            // Any prototype with "Hub" in the name (e.g. DangerRoomHubRegion)
            string name = region.PrototypeName;
            return string.IsNullOrEmpty(name) == false && name.Contains("Hub");
        }

        private static bool IsBlacklistedRegion(Region region)
        {
            if (region?.Prototype == null) return false;
            string name = region.PrototypeName;
            if (string.IsNullOrEmpty(name)) return false;
            return s_regionBlacklist.Any(b => name.Contains(b, StringComparison.OrdinalIgnoreCase));
        }

        private int GetIntervalMs()
        {
            int baseInterval = Math.Max(MinIntervalMs, _game.CustomGameOptions.IncursionIntervalMs);
            int maxRandom = _game.CustomGameOptions.IncursionRandomIntervalMaxMs;
            if (maxRandom > 0)
                baseInterval += _game.Random.Next(0, maxRandom + 1);
            return baseInterval;
        }

        private string DescribeEnemy()
        {
            return $"{GameDatabase.GetPrototypeName(EffectiveEnemyRef)} ({(ulong)EffectiveEnemyRef})";
        }

        private void LogInfo(string message)
        {
            if (_game?.CustomGameOptions?.IncursionLoggingEnable ?? false)
                Logger.Info(message);
        }

        private void LogVerbose(string message)
        {
            if (_game?.CustomGameOptions?.IncursionLogVerboseEnable ?? false)
                Logger.Info(message);
        }

        #endregion

        #region TIMERS

        private class IncursionTickEvent : CallMethodEvent<IncursionManager>
        {
            protected override CallbackDelegate GetCallback() => (manager) => manager.OnIncursionTick();
        }

        /// <summary>Called every ~1s to monitor the trial enemy's health.</summary>
        private class TrialCheckEvent : CallMethodEvent<IncursionManager>
        {
            protected override CallbackDelegate GetCallback() => (manager) => manager.OnTrialCheck();
        }

        /// <summary>Called once after the 5s post-death delay to spawn the next trial enemy.</summary>
        private class TrialSpawnEvent : CallMethodEvent<IncursionManager>
        {
            protected override CallbackDelegate GetCallback() => (manager) => manager.OnTrialSpawnNext();
        }

        #endregion
    }
}
