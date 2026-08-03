using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
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

        // Round-robin pool of avatar prototypes for nameplate proxies. The client appears to
        // cache avatar pawn data by prototype ref - when a second proxy uses the same avatar
        // prototype, the client may reuse cached pawn data (including a default costume/mesh),
        // making the invisible proxy visible. Using different avatar prototypes per proxy
        // avoids this client-side cache collision. The full pool of 62 shipping avatars
        // minimizes the chance of collision, even when many proxies are alive simultaneously
        // or when the player's own avatar matches a pool entry.
        private static readonly string[] NameplateAvatarPoolNames = new string[]
        {
            "Entity/Characters/Avatars/Shipping/Angela.prototype",
            "Entity/Characters/Avatars/Shipping/AntMan.prototype",
            "Entity/Characters/Avatars/Shipping/Beast.prototype",
            "Entity/Characters/Avatars/Shipping/BlackBolt.prototype",
            "Entity/Characters/Avatars/Shipping/BlackCat.prototype",
            "Entity/Characters/Avatars/Shipping/BlackPanther.prototype",
            "Entity/Characters/Avatars/Shipping/BlackWidow.prototype",
            "Entity/Characters/Avatars/Shipping/Blade.prototype",
            "Entity/Characters/Avatars/Shipping/Cable.prototype",
            "Entity/Characters/Avatars/Shipping/CaptainAmerica.prototype",
            "Entity/Characters/Avatars/Shipping/Carnage.prototype",
            "Entity/Characters/Avatars/Shipping/Colossus.prototype",
            "Entity/Characters/Avatars/Shipping/Cyclops.prototype",
            "Entity/Characters/Avatars/Shipping/Daredevil.prototype",
            "Entity/Characters/Avatars/Shipping/Deadpool.prototype",
            "Entity/Characters/Avatars/Shipping/DoctorStrange.prototype",
            "Entity/Characters/Avatars/Shipping/DrDoom.prototype",
            "Entity/Characters/Avatars/Shipping/Elektra.prototype",
            "Entity/Characters/Avatars/Shipping/EmmaFrost.prototype",
            "Entity/Characters/Avatars/Shipping/Gambit.prototype",
            "Entity/Characters/Avatars/Shipping/GhostRider.prototype",
            "Entity/Characters/Avatars/Shipping/GreenGoblin.prototype",
            "Entity/Characters/Avatars/Shipping/Hawkeye.prototype",
            "Entity/Characters/Avatars/Shipping/Hulk.prototype",
            "Entity/Characters/Avatars/Shipping/HumanTorch.prototype",
            "Entity/Characters/Avatars/Shipping/Iceman.prototype",
            "Entity/Characters/Avatars/Shipping/InvisibleWoman.prototype",
            "Entity/Characters/Avatars/Shipping/IronFist.prototype",
            "Entity/Characters/Avatars/Shipping/IronMan.prototype",
            "Entity/Characters/Avatars/Shipping/JeanGrey.prototype",
            "Entity/Characters/Avatars/Shipping/Juggernaut.prototype",
            "Entity/Characters/Avatars/Shipping/KittyPryde.prototype",
            "Entity/Characters/Avatars/Shipping/Loki.prototype",
            "Entity/Characters/Avatars/Shipping/LukeCage.prototype",
            "Entity/Characters/Avatars/Shipping/Magik.prototype",
            "Entity/Characters/Avatars/Shipping/Magneto.prototype",
            "Entity/Characters/Avatars/Shipping/MoonKnight.prototype",
            "Entity/Characters/Avatars/Shipping/MrFantastic.prototype",
            "Entity/Characters/Avatars/Shipping/MsMarvel.prototype",
            "Entity/Characters/Avatars/Shipping/NickFury.prototype",
            "Entity/Characters/Avatars/Shipping/Nightcrawler.prototype",
            "Entity/Characters/Avatars/Shipping/Nova.prototype",
            "Entity/Characters/Avatars/Shipping/Psylocke.prototype",
            "Entity/Characters/Avatars/Shipping/Punisher.prototype",
            "Entity/Characters/Avatars/Shipping/RocketRaccoon.prototype",
            "Entity/Characters/Avatars/Shipping/Rogue.prototype",
            "Entity/Characters/Avatars/Shipping/ScarletWitch.prototype",
            "Entity/Characters/Avatars/Shipping/SheHulk.prototype",
            "Entity/Characters/Avatars/Shipping/SilverSurfer.prototype",
            "Entity/Characters/Avatars/Shipping/Spiderman.prototype",
            "Entity/Characters/Avatars/Shipping/SquirrelGirl.prototype",
            "Entity/Characters/Avatars/Shipping/Starlord.prototype",
            "Entity/Characters/Avatars/Shipping/Storm.prototype",
            "Entity/Characters/Avatars/Shipping/Taskmaster.prototype",
            "Entity/Characters/Avatars/Shipping/Thing.prototype",
            "Entity/Characters/Avatars/Shipping/Thor.prototype",
            "Entity/Characters/Avatars/Shipping/Ultron.prototype",
            "Entity/Characters/Avatars/Shipping/Venom.prototype",
            "Entity/Characters/Avatars/Shipping/Vision.prototype",
            "Entity/Characters/Avatars/Shipping/WarMachine.prototype",
            "Entity/Characters/Avatars/Shipping/WinterSoldier.prototype",
            "Entity/Characters/Avatars/Shipping/Wolverine.prototype",
            "Entity/Characters/Avatars/Shipping/X23.prototype",
        };
        private static PrototypeId[] s_nameplateAvatarPool;
        private static int s_nameplateAvatarIndex;

        /// <summary>
        /// Selects the next avatar prototype from the round-robin pool, excluding any
        /// avatar prototypes currently in use by player avatars or render-as-avatar
        /// entities in the given region. This prevents client-side pawn cache collisions
        /// where the client reuses cached avatar pawn data (including costumes/meshes)
        /// when a nameplate proxy shares the same avatar prototype as an existing entity.
        /// </summary>
        private static PrototypeId GetNextNameplateAvatarRef(Region region)
        {
            if (s_nameplateAvatarPool == null)
            {
                var refs = new List<PrototypeId>(NameplateAvatarPoolNames.Length);
                foreach (var name in NameplateAvatarPoolNames)
                {
                    var id = GameDatabase.GetPrototypeRefByName(name);
                    if (id != PrototypeId.Invalid && id.As<AvatarPrototype>() != null)
                        refs.Add(id);
                }
                if (refs.Count == 0)
                    refs.Add(SheHulkAvatarProtoRef);
                s_nameplateAvatarPool = refs.ToArray();
            }

            // Build exclusion set from existing avatar-rendered entities in the region.
            // This includes player avatars and any entity with IsClientRenderedAsAvatar
            // (e.g. IncursionEnemyAvatar enemies, other nameplate proxies).
            var excluded = new HashSet<PrototypeId>();
            if (region != null)
            {
                foreach (var entity in region.Entities)
                {
                    if (entity is Avatar avatar)
                    {
                        excluded.Add(avatar.PrototypeDataRef);
                    }
                    else if (entity is WorldEntity we && we.IsClientRenderedAsAvatar)
                    {
                        if (we.ClientPrototypeRefOverride != PrototypeId.Invalid)
                            excluded.Add(we.ClientPrototypeRefOverride);
                    }
                }
            }

            // Try the next round-robin index, skipping excluded avatars.
            // We try up to pool.Length entries to find a non-excluded one.
            for (int i = 0; i < s_nameplateAvatarPool.Length; i++)
            {
                int idx = Interlocked.Increment(ref s_nameplateAvatarIndex);
                int index = (int)((uint)idx % (uint)s_nameplateAvatarPool.Length);
                PrototypeId candidate = s_nameplateAvatarPool[index];
                if (excluded.Contains(candidate) == false)
                    return candidate;
            }

            // All pool entries are excluded - fall back to the last candidate.
            // This is unlikely with 62 avatars but handles the edge case gracefully.
            int fallbackIdx = Interlocked.Increment(ref s_nameplateAvatarIndex);
            return s_nameplateAvatarPool[(int)((uint)fallbackIdx % (uint)s_nameplateAvatarPool.Length)];
        }

        // Rank that hides all overhead info (name, level, health bar). Used on nameplate proxies.
        private static PrototypeId s_bossNoOverheadRankRef = PrototypeId.Invalid;
        private static PrototypeId BossNoOverheadRankRef
        {
            get
            {
                if (s_bossNoOverheadRankRef == PrototypeId.Invalid)
                    s_bossNoOverheadRankRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
                return s_bossNoOverheadRankRef;
            }
        }

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
            bool HardcodeExclude,
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

                // Exclude Calamity Vampire enemies - they are specific to the Vampire Blood
                // Ritual event and should not spawn in generic Incursion events.
                if (type.Namespace?.StartsWith("MHServerEmu.Games.Entities.CalamityEntity") == true)
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
        /// Returns true if the given entity id is a live incursion enemy.
        /// </summary>
        public bool IsIncursionEntity(ulong entityId)
        {
            return _controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller)
                && controller.IsFinished == false;
        }

        /// <summary>
        /// Registers an externally-created controller (e.g. from VampireBloodRitualEvent)
        /// so the damage pipeline can look up its outgoing damage scale.
        /// </summary>
        public void RegisterController(IncursionEnemyController controller)
        {
            if (controller == null || controller.EntityId == 0) return;
            _controllers.Add(controller);
            _controllersByEntity[controller.EntityId] = controller;
        }

        /// <summary>
        /// Registers a proxy entity (e.g. Augmented power proxy caster) with an existing
        /// controller so the damage pipeline scales the proxy's damage using the parent
        /// controller's per-power damage scales.
        /// </summary>
        public void RegisterProxyEntity(ulong proxyEntityId, IncursionEnemyController parentController)
        {
            if (parentController == null || proxyEntityId == 0) return;
            _controllersByEntity[proxyEntityId] = parentController;
        }

        /// <summary>
        /// Removes a proxy entity from the controller registry.
        /// </summary>
        public void UnregisterProxyEntity(ulong proxyEntityId)
        {
            _controllersByEntity.Remove(proxyEntityId);
        }

        /// <summary>
        /// Resolves the controller class name and enemy type for the given invader entity.
        /// Used by <see cref="Powers.PowerPayload"/> damage logging so the log parser can
        /// group damage by the exact controller class (e.g. IncursionEnemyBossMODOK)
        /// rather than guessing from display names or prototype names.
        /// </summary>
        public bool TryGetControllerInfo(ulong entityId, out string className, out string enemyType)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
            {
                className = controller.GetType().Name;
                enemyType = controller.EnemyType;
                return true;
            }

            className = null;
            enemyType = null;
            return false;
        }

        /// <summary>
        /// Resolves the invader display name for the given entity, or null if not a live invader.
        /// Used by DeathRecap to show the nice name (e.g. "CaptainAmerica Invader") instead of
        /// the raw host-body prototype name.
        /// </summary>
        public bool TryGetInvaderDisplayName(ulong entityId, out string displayName)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
            {
                displayName = controller.InvaderDisplayName;
                return displayName != null;
            }

            displayName = null;
            return false;
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
        /// Incoming damage scale for the given invader entity, or 1.0 if not a live invader.
        /// Queried by <see cref="Powers.PowerPayload"/> to apply per-enemy damage taken multipliers
        /// (e.g. DamageTakenMultiplier) directly in the damage pipeline, bypassing the
        /// DamagePctVulnerability property which can be overridden by conditions.
        /// </summary>
        public float GetIncomingDamageScale(ulong entityId)
        {
            if (_controllersByEntity.TryGetValue(entityId, out IncursionEnemyController controller) && controller.IsFinished == false)
                return controller.GetIncomingDamageScale();

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
        public (WorldEntity, string) ForceIncursionForAvatar(Avatar avatar, Player player = null)
        {
            if (avatar == null || avatar.IsAliveInWorld == false)
                return (null, "avatar is not alive in world");

            var region = avatar.Region;
            if (region == null)
                return (null, "avatar has no region");

            bool isHub = IsHubRegion(region);
            LogInfo($"[Incursion] FORCE spawn requested by avatar {avatar.Id} in region " +
                        $"'{region.PrototypeName}' (hub={isHub}).");

            var entity = SpawnInvaderNearAvatar(avatar, player: player);
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

                // Record hunt kill if this invader was killed (not culled/expired) and
                // hasn't been recorded yet.
                RecordHuntKillIfPending(controller);

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
                if (charLevel < 55)
                { LogVerbose($"[Incursion]  skip player '{player.GetName()}': avatar level {charLevel} < 55."); continue; }

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
                if (SpawnInvaderNearAvatar(avatar, player: player) != null)
                    spawned++;
            }

            return spawned;
        }

        private WorldEntity SpawnInvaderNearAvatar(Avatar avatar, IncursionEnemyController specificController = null, Player player = null)
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

            // Create the controller early so we can read per-enemy overrides (e.g. visual scale).
            IncursionEnemyController controller = specificController ?? CreateRandomController(player);

            // Try several positions in a ring around the avatar to find an open nav spot.
            // Avoids spawning inside walls when the player is facing one.
            // Scale the clearance bounds by the same BoundsScaleOverride that will be applied
            // post-spawn, plus a safety margin, so the navmesh check accounts for the actual
            // entity size and the rendered model's collision being larger than the combat body.
            float configScale = _game.CustomGameOptions?.IncursionEnemyVisualScale ?? 1.5f;
            float boundsScale = controller.VisualScaleOverride > 0f
                ? controller.VisualScaleOverride
                : configScale;
            const float SpawnClearanceMargin = 1.2f;  // extra padding to avoid edge-clipping
            float clearanceScale = boundsScale * SpawnClearanceMargin;
            PathFlags pathFlags = Region.GetPathFlagsForEntity(entityProto);
            Vector3 spawnPosition = ChooseOpenSpawnPosition(region, avatar, entityProto, pathFlags, clearanceScale);
            if (spawnPosition == Vector3.Zero)
            {
                controller.Dispose();
                return Logger.WarnReturn<WorldEntity>(null, $"[Incursion] SpawnInvaderNearAvatar: could not find open spawn position near {avatar.RegionLocation.Position.ToStringNames()}.");
            }

            var cell = region.GetCellAtPosition(spawnPosition);
            if (cell == null)
            {
                controller.Dispose();
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
            spec.BoundsScaleOverride = boundsScale;

            // Apply the controller's render skin (avatar/team-up/boss).
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
                // Set hunt tracking context on the controller.
                if (player != null)
                {
                    controller.HuntPlayerDbId = player.DatabaseUniqueId;
                    controller.HuntAvatarName = ModLootFilterHelper.GetAvatarShortName(avatar.PrototypeDataRef);
                }

                controller.Start(invaderAgent);

                // Spawn the nameplate proxy BEFORE scheduling the intro - the intro VFX
                // sends AOI proximity updates that can cause the client to re-process
                // nearby entities. By deferring the intro to the first think tick (via
                // ScheduleIntro), the client gets a full network tick to process the
                // proxy spawn + attachment before any AOI-disrupting VFX is sent.
                if (controller.RenderAvatarRef == PrototypeId.Invalid && controller.NeedsNameplateProxy)
                    SpawnNameplateProxy(region, invaderAgent, controller);

                controller.ScheduleIntro(invaderAgent);
                _controllers.Add(controller);
                _controllersByEntity[controller.EntityId] = controller;
                string logName = controller.LogTrueName ?? controller.GetLabel() ?? controller.GetType().Name;
                IncursionLogCollator.BeginSession(invaderAgent.Id, controller.LogFilePrefix, logName);
            }
            else
            {
                controller.Dispose();
            }

            return entity;
        }

        /// <summary>
        /// Spawns an invisible avatar-rendered proxy entity that provides a red prestige
        /// nameplate for TeamUp and Boss incursion enemies. The client only applies
        /// prestige-based name coloring for AvatarPrototype entities, not AgentTeamUpPrototype
        /// or standard AgentPrototype bosses. The proxy's 3D model is hidden via
        /// IsClientEntityHidden, and the spoof avatar player name provides the display name.
        /// </summary>
        internal void SpawnNameplateProxy(Region region, Agent combatBody, IncursionEnemyController controller)
        {
            // Use a round-robin avatar prototype for the render override to avoid client-side
            // pawn cache collisions when multiple nameplate proxies are alive simultaneously.
            // Dynamic exclusion skips avatar prototypes already in use by players or other
            // render-as-avatar entities in this region.
            PrototypeId avatarRef = GetNextNameplateAvatarRef(region);
            var avatarProto = avatarRef.As<AvatarPrototype>();
            if (avatarProto == null)
            {
                Logger.Warn($"[Incursion] SpawnNameplateProxy: avatarRef {GameDatabase.GetPrototypeName(avatarRef)} is not a valid AvatarPrototype.");
                return;
            }

            Vector3 position = combatBody.RegionLocation.Position;
            var manager = region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(position, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = EffectiveEnemyRef;  // same combat body prototype
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Render as an avatar so the client applies prestige name colors.
            spec.ClientRenderPrototypeRef = avatarRef;

            // Custom overhead name with prefix/suffix.
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

            // Intentionally NOT setting CostumeCurrent: the client creates the avatar pawn
            // from the hierarchy End message + avatar serialization tail, and uses the
            // costume's CostumeUnrealClass for the visible mesh. Without a costume, the
            // pawn exists for nameplate/prestige rendering but has no mesh to draw.
            // (Setting IsClientEntityHidden + Visible=false didn't hide the mesh because
            // the client doesn't respect those flags for modded render-as-avatar entities.)

            // Prestige level 5 = red nameplate.
            spec.Properties[PropertyEnum.AvatarPrestigeLevel] = controller.NameplatePrestigeLevel;

            // Set level to -1 so the client doesn't display a level number next to the
            // spoofed name. The client typically clamps level 0 to 1 for avatars, so we
            // use -1 to make the client skip the level display entirely.
            spec.Properties[PropertyEnum.CharacterLevel] = -1;
            spec.Properties[PropertyEnum.CombatLevel] = -1;

            // Set rank to BossNoOverheadInfo so the client hides all overhead info (including
            // the level number that avatar-rendered entities show by default).
            spec.Properties[PropertyEnum.Rank] = BossNoOverheadRankRef;

            // Make the proxy non-hostile, untargetable, and invulnerable so it doesn't
            // interfere with combat or get killed by AoE damage (which would drop loot).
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.Unaffectable] = true;
            spec.Properties[PropertyEnum.Invulnerable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;

            // Hide the proxy's 3D model using IsClientEntityHidden. This flag tells the
            // client to create the avatar pawn but NOT add it to the visible render scene,
            // while keeping the pawn alive for nameplate/UI rendering. This is the same
            // mechanism used by PlayableExpandedController to hide the main avatar body.
            // We also set Visible=false and BoundsScaleOverride=0.001f as secondary measures.
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;

            // CRITICAL: Set Dormant=true BEFORE spawn so the entity never activates powers.
            // During spec.Spawn(), WorldEntity.SetSimulated(true) calls
            // Agent.SetSimulated which calls TryAutoActivatePowersInCollection() if the
            // entity is NOT dormant. This auto-activates passive powers (including the
            // SpidermanClone's buff/passive powers), which triggers the client to load
            // visual effects and potentially assign a default SheHulk costume/mesh to the
            // avatar pawn. By setting Dormant=true on the spec, the entity spawns dormant
            // and TryAutoActivatePowersInCollection is skipped (it checks IsDormant first).
            // This matches the approach used by CalamityRitualCenterpiece which works reliably.
            spec.Properties[PropertyEnum.Dormant] = true;

            spec.Spawn();

            var proxy = spec.ActiveEntity;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                Logger.Warn($"[Incursion] SpawnNameplateProxy: failed to spawn proxy entity.");
                return;
            }

            // NOTE: Do NOT write properties on the proxy after spawn - any property write
            // triggers replication to the client, which can cause it to re-process the
            // avatar pawn and assign a default SheHulk costume/mesh. The level is already
            // set to -1 on the spec, and BossNoOverheadInfo rank hides the level display.
            //
            // All post-spawn operations (loot strip, power strip, AI disable, dormant,
            // simulate, attach) are deferred to the first think tick via ScheduleProxyConfig.
            // See IncursionEnemyController.ScheduleProxyConfig / ConfigureSpawnedProxy for
            // detailed documentation of why this is necessary.

            controller.ProxyEntityId = proxy.Id;
            controller._proxySpawnGameTime = _game.CurrentTime;
            controller.ScheduleProxyConfig(combatBody.Id);

            var proxyWe = proxy as WorldEntity;
            uint spoofId = proxyWe?.SpoofAvatarWorldInstanceId ?? 0;
            LogInfo($"[Incursion] Spawned nameplate proxy (id {proxy.Id}) for TeamUp invader " +
                    $"{controller.GetLabel()} at {position.ToStringNames()} in region '{region.PrototypeName}'. " +
                    $"AvatarProto='{GameDatabase.GetPrototypeName(avatarRef)}', SpoofAvatarWorldInstanceId={spoofId}, " +
                    $"IsClientEntityHidden, prestige={controller.NameplatePrestigeLevel}. " +
                    $"Post-spawn config deferred to first think tick.");
        }

        /// <summary>
        /// Overload for controllerless spawns (e.g. BloodLord summoned adds).
        /// Spawns an invisible avatar-rendered proxy with a custom display name and red prestige.
        /// </summary>
        internal void SpawnNameplateProxy(Region region, Agent combatBody, string displayName, int prestigeLevel = 5)
        {
            PrototypeId avatarRef = GetNextNameplateAvatarRef(region);
            var avatarProto = avatarRef.As<AvatarPrototype>();
            if (avatarProto == null) return;

            Vector3 position = combatBody.RegionLocation.Position;
            var manager = region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(position, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = EffectiveEnemyRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            spec.ClientRenderPrototypeRef = avatarRef;
            spec.ClientRenderPlayerName = displayName;
            spec.Properties[PropertyEnum.AvatarPrestigeLevel] = prestigeLevel;
            spec.Properties[PropertyEnum.CharacterLevel] = -1;
            spec.Properties[PropertyEnum.CombatLevel] = -1;
            spec.Properties[PropertyEnum.Rank] = BossNoOverheadRankRef;
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.Unaffectable] = true;
            spec.Properties[PropertyEnum.Invulnerable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;
            spec.Properties[PropertyEnum.Dormant] = true;

            spec.Spawn();

            var proxy = spec.ActiveEntity;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                return;
            }

            // NOTE: Do NOT write properties on the proxy after spawn - see comment above.
            // Level is already set to -1 on the spec, and BossNoOverheadInfo hides it.

            // Strip all loot tables from the proxy so it can never drop loot if killed.
            IncursionEnemyController.RemoveDeathLootTables(proxy as Agent);

            if (proxy is Agent proxyAgent && proxyAgent.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxyAgent.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                {
                    if (proxyAgent.PowerCollection.ContainsPower(powerRef))
                        proxyAgent.UnassignPower(powerRef);
                }
            }

            if (proxy is Agent aiAgent)
            {
                aiAgent.AIController?.SetIsEnabled(false);
                // Dormant already set on spec before spawn - no need to reassert.
            }

            proxy.SetSimulated(false);
            proxy.AttachToEntity(combatBody);

            var proxyWe2 = proxy as WorldEntity;
            uint spoofId2 = proxyWe2?.SpoofAvatarWorldInstanceId ?? 0;
            LogInfo($"[Incursion] Spawned nameplate proxy (id {proxy.Id}) for '{displayName}' at {position.ToStringNames()}. " +
                    $"AvatarProto='{GameDatabase.GetPrototypeName(avatarRef)}', SpoofAvatarWorldInstanceId={spoofId2}.");
        }

        /// <summary>
        /// Picks the next enemy type from the roster, preferring types the player has
        /// hunted the least. Falls back to round-robin when no player context is available
        /// or when all enemies have been hunted equally.
        /// Respects the IncursionExcludeEnemies config filter.
        /// </summary>
        private IncursionEnemyController CreateRandomController(Player player = null)
        {
            var factories = GetRandomFactories();

            // If we have a player with hunt data, select the least-hunted enemy type.
            if (player?.IncursionHuntData != null)
            {
                var controller = CreateHuntWeightedController(player, factories);
                if (controller != null)
                    return controller;
            }

            // Fallback: round-robin selection.
            int idx = Interlocked.Increment(ref s_roundRobinIndex);
            int index = (int)((uint)idx % (uint)factories.Length);
            var fallback = factories[index](_game);
            LogInfo($"[Incursion] Round-robin selected {fallback.GetLabel()} ({index + 1}/{factories.Length})");
            return fallback;
        }

        /// <summary>
        /// Selects an enemy type the player has hunted the least. Creates a temporary
        /// instance to read the shorthand, then picks the one with the lowest effective
        /// kill count. Ties are broken randomly.
        /// </summary>
        private IncursionEnemyController CreateHuntWeightedController(Player player, Func<Game, IncursionEnemyController>[] factories)
        {
            EnsureEnemyMeta();

            string avatarName = ModLootFilterHelper.GetAvatarShortName(player.CurrentAvatar?.PrototypeDataRef ?? PrototypeId.Invalid);
            var huntData = player.IncursionHuntData;

            // Build (factory, killCount) pairs for all eligible factories.
            var candidates = new List<(Func<Game, IncursionEnemyController> Factory, int KillCount, string Shorthand)>();
            foreach (var meta in s_enemyMeta)
            {
                if (meta.HardcodeExclude) continue;

                // Find the matching factory by type name.
                // s_enemyMeta and s_randomFactories may differ in order after shuffle,
                // so we match by creating a temp to read the shorthand.
                // Optimization: use the meta's shorthand directly.
                int killCount = huntData.GetEffectiveKillCount(meta.Shorthand, avatarName);
                candidates.Add((meta.Factory, killCount, meta.Shorthand));
            }

            if (candidates.Count == 0)
                return null;

            // Find the minimum kill count.
            int minKills = candidates.Min(c => c.KillCount);

            // Gather all candidates at the minimum (ties).
            var leastHunted = candidates.Where(c => c.KillCount == minKills).ToList();

            // Pick randomly among the tied least-hunted entries.
            var chosen = leastHunted[_game.Random.Next(leastHunted.Count)];
            var result = chosen.Factory(_game);
            LogInfo($"[Incursion] Hunt-weighted selected {result.GetLabel()} (kills={chosen.KillCount}, " +
                    $"tied among {leastHunted.Count} least-hunted type(s), avatar={avatarName ?? "?"})");
            return result;
        }

        /// <summary>
        /// Builds (or returns the cached) filtered factory array for random spawns,
        /// excluding any enemy whose shorthand, display name, or avatar name matches
        /// a pattern in the IncursionExcludeEnemies config.
        /// HardcodeExclude entries are always excluded regardless of config.
        /// </summary>
        private static Func<Game, IncursionEnemyController>[] GetRandomFactories()
        {
            if (s_randomFactories != null) return s_randomFactories;

            lock (s_randomFactoriesLock)
            {
                if (s_randomFactories != null) return s_randomFactories;

                var options = ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>();
                var excluded = ParseExcludedPatterns(options.IncursionExcludeEnemies);

                EnsureEnemyMeta();

                var filtered = new List<Func<Game, IncursionEnemyController>>();
                var excludedNames = new List<string>();

                foreach (var meta in s_enemyMeta)
                {
                    // Hardcoded exclusions always apply, even if config is wiped.
                    if (meta.HardcodeExclude)
                    {
                        excludedNames.Add(meta.Shorthand);
                        continue;
                    }

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
                    // Fallback: all non-hardcoded-excluded factories (still respects HardcodeExclude).
                    s_randomFactories = s_enemyMeta.Where(m => m.HardcodeExclude == false).Select(m => m.Factory).ToArray();
                    Logger.Warn($"[Incursion] All {s_enemyFactories.Length} enemy type(s) matched exclusion patterns ({string.Join(", ", excluded)}). " +
                                $"Falling back to roster minus hardcoded exclusions ({s_randomFactories.Length} type(s)).");
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
        /// Three render paths, checked in priority order:
        ///   1. <strong>Boss</strong> (<see cref="IncursionEnemyController.RenderBossRef"/>):
        ///      overrides the spawn spec's EntityRef to the boss prototype - the boss IS the combat body.
        ///   2. <strong>Avatar</strong> (<see cref="IncursionEnemyController.RenderAvatarRef"/>):
        ///      sets ClientRenderPrototypeRef + CostumeCurrent so a generic combat body renders as the avatar.
        ///   3. <strong>Team-Up</strong> (<see cref="IncursionEnemyController.RenderTeamupRef"/>):
        ///      sets ClientRenderPrototypeRef to the Team-Up prototype (no costume; client resolves model from UnrealClass).
        /// </summary>
        private void ApplyRenderSkin(SpawnSpec spec, IncursionEnemyController controller)
        {
            // --- Boss spawn path ---
            // The boss prototype IS the combat body. Override the spawn spec's EntityRef
            // so the entity spawns as the boss itself, with its native model, animations, and powers.
            // No ClientRenderPrototypeRef or CostumeCurrent is set.
            PrototypeId bossRef = controller.RenderBossRef;
            if (bossRef != PrototypeId.Invalid)
            {
                spec.EntityRef = bossRef;

                // Custom overhead name for the boss invader.
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

                LogInfo($"[Incursion]   render skin: {controller.GetType().Name} as boss '{GameDatabase.GetPrototypeName(bossRef)}' " +
                            $"(entityRef override, no client render proto).");
                return;
            }

            // --- Avatar render path ---
            PrototypeId renderRef = controller.RenderAvatarRef;
            if (renderRef != PrototypeId.Invalid)
            {
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
                return;
            }

            // --- Team-Up render path ---
            PrototypeId teamupRef = controller.RenderTeamupRef;
            if (teamupRef != PrototypeId.Invalid)
            {
                var teamUpProto = teamupRef.As<AgentTeamUpPrototype>();
                if (teamUpProto == null)
                {
                    Logger.Warn($"[Incursion] {controller.GetType().Name}.RenderTeamupRef '{GameDatabase.GetPrototypeName(teamupRef)}' is not a Team-Up; rendering the combat body itself.");
                    return;
                }

                spec.ClientRenderPrototypeRef = teamupRef;

                // Custom overhead name drawn above the rendered Team-Up.
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

                // Team-Ups use CostumeUnrealOverrides (AssetId pairs), not CostumePrototype refs.
                // No CostumeCurrent property is set; the client resolves the model from the
                // Team-Up prototype's UnrealClass directly.

                LogInfo($"[Incursion]   render skin: {controller.GetType().Name} as Team-Up '{GameDatabase.GetPrototypeName(teamupRef)}'.");
            }
        }

        #endregion

        #region Spawning

        /// <summary>
        /// Tries positions ahead of the player's viewing direction first, then falls back
        /// to a wider arc around the player, and finally right next to the player.
        /// </summary>
        private Vector3 ChooseOpenSpawnPosition(Region region, Avatar avatar, WorldEntityPrototype entityProto, PathFlags pathFlags, float boundsScale = 1f)
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
                bounds.Scale(boundsScale);
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
                bounds.Scale(boundsScale);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, SpawnRadius);
                if (candidate != origin)
                    return candidate;
            }

            // 3) Last resort: try right next to the player.
            {
                Vector3 origin = playerPos;
                Bounds bounds = new(entityProto.Bounds, origin);
                bounds.Scale(boundsScale);
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
        /// <param name="mode">"all" (default), "avatar", "teamup", or "boss" to filter the roster.</param>
        public string StartTrial(Player player, string mode = "all")
        {
            if (_trialRunning) return "An incursion trial is already in progress.";
            if (player == null) return "Player not found.";

            Avatar avatar = player.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false)
                return "Avatar not found or not alive in world.";

            Region region = avatar.Region;
            if (region == null || IsHubRegion(region))
                return "Cannot start an incursion trial in a hub region.";

            // Filter factories by trial mode.
            var filteredFactories = FilterFactoriesByMode(mode);
            if (filteredFactories.Count == 0)
                return $"No incursion enemies match trial mode '{mode}'. Valid modes: all, avatar, teamup, boss.";

            // Build a shuffled roster of the filtered enemy types (highlander - each once).
            _trialRoster.Clear();
            foreach (var factory in filteredFactories)
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
            return $"Incursion trial started ({mode})! Defeat {_trialRoster.Count} invaders one by one.";
        }

        /// <summary>
        /// Filters <see cref="s_enemyFactories"/> by the given trial mode.
        /// "avatar" excludes TeamUp subclasses; "teamup" returns only TeamUp subclasses;
        /// "boss" returns only Boss subclasses; "all" returns everything.
        /// </summary>
        private static List<Func<Game, IncursionEnemyController>> FilterFactoriesByMode(string mode)
        {
            var excluded = ParseExcludedPatterns(ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>().IncursionExcludeEnemies);

            var result = new List<Func<Game, IncursionEnemyController>>();
            foreach (var factory in s_enemyFactories)
            {
                var temp = factory(null);
                if (temp == null) continue;

                bool match = mode.ToLowerInvariant() switch
                {
                    "boss" => temp is IncursionEnemyBoss,
                    "teamup" => temp is IncursionEnemyTeamup,
                    "avatar" => temp is IncursionEnemyAvatar && temp is not IncursionEnemyTeamup,
                    _ => true,
                };

                if (match == false) continue;

                // Hardcoded exclusions always apply, even if config is wiped.
                if (temp.HardcodeExclude)
                { temp.Dispose(); continue; }

                // Exclude enemies matching the exclusion list so trials don't spawn
                // excluded bosses like Surtur.
                if (excluded.Count > 0)
                {
                    string shorthand = IncursionEnemyController.StripControllerPrefix(temp.GetType().Name);
                    string displayName = temp.InvaderDisplayName ?? string.Empty;
                    string avatarName = temp.RenderAvatarRef != PrototypeId.Invalid
                        ? GameDatabase.GetPrototypeName(temp.RenderAvatarRef)
                        : temp.RenderTeamupRef != PrototypeId.Invalid
                            ? GameDatabase.GetPrototypeName(temp.RenderTeamupRef)
                            : temp.RenderBossRef != PrototypeId.Invalid
                                ? GameDatabase.GetPrototypeName(temp.RenderBossRef)
                                : string.Empty;

                    var meta = new EnemyMeta(temp.GetType().Name, shorthand, displayName, avatarName, false, factory);
                    if (IsExcludedEnemy(meta, excluded))
                    { temp.Dispose(); continue; }
                }

                result.Add(factory);
            }
            return result;
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

            // Current enemy defeated - advance index.
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

        #region Hunt Tracking

        /// <summary>
        /// Records a hunt kill for the given controller's player if the controller
        /// was spawned for a player and the kill hasn't been recorded yet.
        /// Called when a controller is pruned (finished) in the tick loop.
        /// </summary>
        private void RecordHuntKillIfPending(IncursionEnemyController controller)
        {
            if (controller == null || controller.HuntKillRecorded) return;
            if (controller.HuntPlayerDbId == 0) return;

            controller.MarkHuntKillRecorded();

            // Look up the player to access their hunt data.
            Player player = _game.EntityManager.GetEntityByDbGuid<Player>(controller.HuntPlayerDbId);
            if (player?.IncursionHuntData == null) return;

            string shorthand = controller.HuntShorthand;
            string avatarName = controller.HuntAvatarName;

            // Record in both global and per-character sections.
            player.IncursionHuntData.RecordKill(shorthand, avatarName, perCharacter: true);
            IncursionHuntStorage.Save(player.DatabaseUniqueId, player.IncursionHuntData);

            int globalCount = player.IncursionHuntData.Global.KillCounts.GetValueOrDefault(shorthand);
            int charCount = avatarName != null
                ? player.IncursionHuntData.GetCharacterSection(avatarName)?.KillCounts.GetValueOrDefault(shorthand) ?? 0
                : 0;

            LogInfo($"[Incursion:Hunt] Recorded kill for {shorthand} (player={player.GetName()}, " +
                    $"avatar={avatarName ?? "?"}, global={globalCount}, char={charCount}).");
        }

        /// <summary>
        /// Builds a human-readable hunt status string for the given player.
        /// Shows per-enemy kill counts and unique completion progress.
        /// </summary>
        public static string GetHuntStatusString(Player player)
        {
            if (player?.IncursionHuntData == null)
                return "Incursion hunt data not available.";

            EnsureEnemyMeta();

            string avatarName = ModLootFilterHelper.GetAvatarShortName(player.CurrentAvatar?.PrototypeDataRef ?? PrototypeId.Invalid);
            var huntData = player.IncursionHuntData;

            // Build the set of all non-hardcode-excluded enemy shorthands.
            var allShorthands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var meta in s_enemyMeta)
            {
                if (meta.HardcodeExclude) continue;
                allShorthands.Add(meta.Shorthand);
            }

            int totalTypes = allShorthands.Count;
            int uniqueHunted = huntData.GetUniqueCount(avatarName, allShorthands);

            var sb = new System.Text.StringBuilder();
            sb.Append($"Incursion Hunt Status for {player.GetName()} (avatar: {avatarName ?? "?"}):\n");
            sb.Append($"  Unique encounters completed: {uniqueHunted}/{totalTypes}\n\n");

            // Sort by kill count descending, then alphabetically.
            var entries = allShorthands
                .Select(s => (Shorthand: s, Kills: huntData.GetEffectiveKillCount(s, avatarName)))
                .OrderByDescending(e => e.Kills)
                .ThenBy(e => e.Shorthand)
                .ToList();

            sb.Append("  Enemy              Kills\n");
            sb.Append("  ------------------ -----\n");
            foreach (var entry in entries)
            {
                string marker = entry.Kills == 0 ? " [NEW]" : "";
                sb.Append($"  {entry.Shorthand,-18} {entry.Kills,5}{marker}\n");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Resets hunt data for the given player. If resetAll is true, clears everything.
        /// Otherwise clears only the current character's section.
        /// </summary>
        public static string ResetHuntData(Player player, bool resetAll)
        {
            if (player?.IncursionHuntData == null)
                return "Incursion hunt data not available.";

            if (resetAll)
            {
                int globalCount = player.IncursionHuntData.Global.KillCounts.Count;
                int charCount = player.IncursionHuntData.Characters.Count;
                player.IncursionHuntData.Global.KillCounts.Clear();
                player.IncursionHuntData.Characters.Clear();
                IncursionHuntStorage.Save(player.DatabaseUniqueId, player.IncursionHuntData);
                return $"Reset ALL incursion hunt data: cleared {globalCount} global kill count(s) and {charCount} character section(s).";
            }
            else
            {
                string avatarName = ModLootFilterHelper.GetAvatarShortName(player.CurrentAvatar?.PrototypeDataRef ?? PrototypeId.Invalid);
                if (string.IsNullOrEmpty(avatarName))
                    return "Could not determine current avatar for hunt reset.";

                var section = player.IncursionHuntData.GetCharacterSection(avatarName);
                if (section == null || section.KillCounts.Count == 0)
                    return $"No per-character hunt data to reset for {avatarName}.";

                int count = section.KillCounts.Count;
                player.IncursionHuntData.Characters.Remove(avatarName);
                IncursionHuntStorage.Save(player.DatabaseUniqueId, player.IncursionHuntData);
                return $"Reset incursion hunt data for {avatarName}: cleared {count} kill count(s). Global counts preserved.";
            }
        }

        #endregion

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
                    : temp.RenderTeamupRef != PrototypeId.Invalid
                        ? GameDatabase.GetPrototypeName(temp.RenderTeamupRef)
                        : temp.RenderBossRef != PrototypeId.Invalid
                            ? GameDatabase.GetPrototypeName(temp.RenderBossRef)
                            : string.Empty;

                s_enemyMeta.Add(new EnemyMeta(typeName, shorthand, displayName, avatarName, temp.HardcodeExclude, factory));
                temp.Dispose();
            }

            Logger.Info($"[Incursion] Enemy meta cached: {s_enemyMeta.Count} type(s).");
        }

        /// <summary>
        /// Finds enemy factories whose shorthand name, display name, or render avatar name contain the pattern.
        /// Returns a randomly chosen match, or an error message when no match is found.
        /// Exact shorthand matches bypass the exclusion list (so users can explicitly spawn a
        /// specific excluded boss like Surtur). Fuzzy matches respect exclusions so generic
        /// patterns like "boss" won't randomly pick excluded enemies.
        /// </summary>
        private (Func<Game, IncursionEnemyController>, string) ResolveFactoryByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return (null, "pattern is empty");

            EnsureEnemyMeta();

            string p = pattern.Trim();
            var excluded = ParseExcludedPatterns(ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>().IncursionExcludeEnemies);

            // Priority 1: exact shorthand match (case-insensitive) - bypasses exclusions
            // so an explicit "!incursion spawn surtur" works even if Surtur is excluded.
            var exactShorthand = s_enemyMeta.Where(m => string.Equals(m.Shorthand, p, StringComparison.OrdinalIgnoreCase)).ToList();
            if (exactShorthand.Count > 0)
                return (exactShorthand[_game.Random.Next(exactShorthand.Count)].Factory, null);

            // For fuzzy matches, filter out excluded enemies so generic patterns like
            // "boss" don't randomly spawn excluded bosses.
            // Hardcoded exclusions are also filtered from fuzzy matches (but exact shorthand above still works).
            var pool = s_enemyMeta.Where(m => m.HardcodeExclude == false).ToList();
            if (excluded.Count > 0)
                pool = pool.Where(m => IsExcludedEnemy(m, excluded) == false).ToList();

            // Priority 2: shorthand contains pattern
            var shorthandMatches = pool.Where(m => m.Shorthand.Contains(p, StringComparison.OrdinalIgnoreCase)).ToList();
            if (shorthandMatches.Count > 0)
                return (shorthandMatches[_game.Random.Next(shorthandMatches.Count)].Factory, null);

            // Priority 3: display name contains pattern
            var displayNameMatches = pool.Where(m => m.DisplayName.Contains(p, StringComparison.OrdinalIgnoreCase)).ToList();
            if (displayNameMatches.Count > 0)
                return (displayNameMatches[_game.Random.Next(displayNameMatches.Count)].Factory, null);

            // Priority 4: avatar/boss ref name contains pattern (least specific)
            var avatarMatches = pool.Where(m => m.AvatarName.Contains(p, StringComparison.OrdinalIgnoreCase)).ToList();
            if (avatarMatches.Count > 0)
                return (avatarMatches[_game.Random.Next(avatarMatches.Count)].Factory, null);

            var suggestions = s_enemyMeta
                .Select(m => $"- {m.Shorthand}{(string.IsNullOrEmpty(m.DisplayName) ? "" : $" ({m.DisplayName})")}")
                .ToList();
            return (null, $"No incursion enemy matches '{p}'. Known enemies:\r\n{string.Join("\r\n", suggestions)}");
        }

        /// <summary>
        /// Returns true if the given enemy meta matches any exclusion pattern.
        /// </summary>
        private static bool IsExcludedEnemy(EnemyMeta meta, List<string> excludedPatterns)
        {
            if (excludedPatterns == null || excludedPatterns.Count == 0) return false;
            foreach (var pattern in excludedPatterns)
            {
                if (meta.Shorthand.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                    || meta.DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                    || meta.AvatarName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Region prototype name substrings that block incursion spawning even when the
        /// region is not a hub. Add exact prototype name fragments here (case-insensitive).
        /// </summary>
        private static readonly HashSet<string> s_regionBlacklist = new(StringComparer.OrdinalIgnoreCase)
        {
            // SWORD headquarters building in Hightown - behaves like a safe zone
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
