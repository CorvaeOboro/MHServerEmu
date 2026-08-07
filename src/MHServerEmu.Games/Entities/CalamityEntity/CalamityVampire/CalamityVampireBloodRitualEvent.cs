#region BLOOD RITUAL
// =============================================================================
// MOD CALAMITY 
// =============================================================================
//   CALAMITY is a collection of custom encounters that are short, small 
//   and play like the games existing "terminals". 
//
//   Vampire Blood Ritual is a one-shot Calamity encounter .
//   a Herald of Darkness warns of plans to corrupt the item "Amulet of Quiox" , 
//   Cloak will transport you to battle a Vampire coven and stop the ritual . 
//   some Heroes and Villains have already been turned  and command lesser thralls . 
//   set in HulkBusters the Hightown Arcade region 
//
//   Boss = Blood Lord ScarletWitch with Augmented Powers ( multi cast patterns )
//          and borrowed powers from onslaught , carnage , malekith , elektra , venom  ,  blade  
//   3 Minibosses chosen from = Malekith , Death Gambit , Wolverine , Moon Knight , Grim Reaper ) 
//  VERSION:: 20260713
// =============================================================================

using MHServerEmu.Core.Logging;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Coordinator for the "Vampire Blood Ritual" one-shot event.
    /// Spawned when a player enters the "HulkBusters the Hightown Arcade" region via the Cloak NPC portal.
    /// All enemies are spawned once at start.
    /// Win condition: Vampire Blood Lord dies.
    /// The event cleans up when the region shuts down (AlwaysShutdownWhenVacant).
    /// </summary>
    public class VampireBloodRitualEvent
    {
        #region Constants

        private static readonly Logger Logger = LogManager.CreateLogger();

        private readonly Game _game;
        private readonly Region _region;
        private bool _initialized = false;
        private bool _completed = false;

        // Tracked entity IDs for win/loss checks
        private ulong _bossEntityId;
        private readonly List<ulong> _enemyEntityIds = new();

        // Enemy controllers we created (for cleanup)
        private readonly List<IncursionEnemyController> _controllers = new();

        // Ritual centerpiece (amulet visual + VFX + nameplate)
        private RitualCenterpiece _centerpiece;

        // Combat body prototype for avatar-rendered enemies (same as IncursionManager default)
        private const string CombatBodyProtoName = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";

        // Region ref - TRGameCenterRegion (Hulk Busters arcade)
        public static readonly PrototypeId EventRegionRef =
            (PrototypeId)16693804270797857925;  // TRGameCenterRegion

        // Hub NPC - delegate to Region.CloakNPCRef (lazy resolution)
        public static PrototypeId CloakNPCRef => Region.CloakNPCRef;

        // Hub region where Cloak spawns
        public static readonly PrototypeId AvengersTowerHubRef =
            (PrototypeId)9142075282174842340;  // NPEAvengersTowerHUBRegion

        // Difficulty tier for the event (Green = normal)
        public static readonly PrototypeId EventDifficultyTierRef =
            (PrototypeId)DifficultyTier.Red; // Green

        // Toggle: include the dialog message text in the Herald NPC dialog.
        // When false, only Yes/No buttons are shown (the decision is implied by context).
        public static readonly bool ShowDialogMessage = false;

        // Dialog string IDs (optional - can be overridden via AchievementStringMap)
        public const ulong DialogMessageStringId = 423133379684795657;   // todo: "Travel to Vampire Blood Ritual?"
        public const ulong YesButtonStringId = 14959079863731815684;
        public const ulong NoButtonStringId = 16244338063872951558;

        // VFX for ritual prop
        public const string RitualVfxPropertyName = "InfinityPowerPointEarnedClass";

        #endregion

        #region Constructor

        public VampireBloodRitualEvent(Game game, Region region)
        {
            _game = game;
            _region = region;
        }

        #endregion

        #region START

        /// <summary>
        /// Main entry point: spawns all enemies, minibosses, boss, and ritual prop.
        /// Called from Region.Initialize() when the region is the HulkBusters
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Logger.Info($"[VampireBloodRitual] Initializing event in region '{_region.PrototypeName}'.");

            // Default population (Skrulls) is skipped in Region.GenerateAreas() for this region.
            // All spawn positions below are absolute world coordinates recorded with the AreaNote mod.

            // --- Trash mobs (30 positions, round-robin thrall types with random start) ---
            Vector3[] trashPositions = new Vector3[]
            {
                new(2682.375f, 195.75f, 136f),
                new(2634.25f, 1007.5f, 136f),
                new(3607f, 1086.125f, 136f),
                new(3513.375f, 229.25f, 136f),
                new(2302.875f, -641.125f, 135f),
                new(1837.125f, -1066.25f, 135f),
                new(2511.25f, -1428.875f, 136f),
                new(1951.5f, -2798.25f, 136f),
                new(2527.125f, -2960.5f, 136f),
                new(1756f, -2324.75f, 136f),
                new(2524.375f, -1953.125f, 136f),
                new(2460.375f, -2440.75f, 136f),
                new(1231.625f, -2844f, 288f),
                new(1216.125f, -2187f, 432f),
                new(1065f, -1427.25f, 432f),
                new(965.75f, -772.25f, 432f),
                new(496.25f, -1174.75f, 432f),
                new(-273.5f, -512.375f, 431f),
                new(-123f, -7.75f, 432f),
                new(437.625f, -277f, 431f),
                new(227.25f, 845.25f, 432f),
                new(-831.5f, 32.875f, 418f),
                new(-1468.75f, 46f, 338f),
                new(-2462.625f, 643.25f, 304f),
                new(-2617.625f, -7.25f, 304f),
                new(-2426.625f, -705.625f, 304f),
                new(-3658.375f, -580f, 304f),
                new(-3925.875f, 63f, 303f),
                new(-3684.875f, 671.5f, 304f),
                new(-3252.875f, 242.875f, 313f),
            };

            Type[] thrallPool = new Type[]
            {
                typeof(CalamityEnemyVampireThrallDarkElf),
                //typeof(CalamityEnemyVampireThrallHandNinja),
                typeof(CalamityEnemyVampireThrallPurifier),
                typeof(CalamityEnemyVampireThrallButcher),
                typeof(CalamityEnemyVampireThrallDarkElfAssassin),
            };

            // Random starting offset so each run has a different thrall distribution.
            int thrallStart = _game.Random.Next(0, thrallPool.Length);
            for (int i = 0; i < trashPositions.Length; i++)
            {
                Type thrallType = thrallPool[(thrallStart + i) % thrallPool.Length];
                SpawnEnemyAtPosition(trashPositions[i], thrallType);
            }

            // --- Minibosses (3 positions, randomly chosen from the vampire pool) ---
            Vector3[] minibossPositions = new Vector3[]
            {
                new(-276.125f, -170.5f, 432f),
                new(2014.375f, -2807.875f, 136f),
                new(-1785.375f, 8.5f, 304f),
            };

            Type[] minibossPool = new Type[]
            {
                typeof(CalamityEnemyVampireMidLadyDeathstrike),
                typeof(CalamityEnemyVampireMidMalekith),
                typeof(CalamityEnemyVampireMidGrimReaper),
                typeof(CalamityEnemyVampireMidGambit),
                typeof(CalamityEnemyVampireMidBlackWidow),
                typeof(CalamityEnemyVampireMidWolverine),
                typeof(CalamityEnemyVampireMidMoonKnight),
                typeof(CalamityEnemyVampireMidDaredevil),
            };

            List<Type> available = new(minibossPool);
            foreach (Vector3 pos in minibossPositions)
            {
                int idx = _game.Random.Next(0, available.Count);
                Type chosen = available[idx];
                available.RemoveAt(idx);  // no duplicates
                SpawnEnemyAtPosition(pos, chosen);
            }

            // --- Boss: at the ritual site ---
            Vector3 bossPos = new(3118.5f, 676.75f, 136f);
            SpawnBoss(bossPos);

            // --- Ritual centerpiece: floating amulet with looping VFX and blue nameplate ---
            SpawnCenterpiece(bossPos);

            Logger.Info($"[VampireBloodRitual] Event initialized. Spawned {_enemyEntityIds.Count} entities, bossId={_bossEntityId}.");
        }


        /// <summary>
        /// Called when an entity dies in the region. Checks for win condition.
        /// </summary>
        public void OnEntityDied(WorldEntity entity)
        {
            if (_completed) return;

            if (entity.Id == _bossEntityId)
            {
                _completed = true;
                Logger.Info($"[VampireBloodRitual] Boss defeated! Event completed.");
                // TODO: Send banner message to all players in region once AchievementStringMap overrides are set up
            }
        }

        /// <summary>
        /// Spawns a basic vampire thrall at the given position for the BloodLord's summon ability.
        /// Uses CalamityEnemyVampireThrallSummoned (no nameplate proxy) instead of the
        /// regular CalamityEnemyVampireThrallDarkElf (which has a red nameplate proxy).
        /// </summary>
        public Agent SpawnThrall(Vector3 position)
        {
            return SpawnEnemyAtPosition<CalamityEnemyVampireThrallSummoned>(position);
        }

        /// <summary>
        /// Returns the set of entity IDs tracked by this event (enemies + boss).
        /// Used by Region.PurgeNativeNpcs to avoid killing vampire event entities.
        /// </summary>
        public HashSet<ulong> GetTrackedEntityIds()
        {
            var ids = new HashSet<ulong>(_enemyEntityIds);
            if (_bossEntityId != Entity.InvalidId)
                ids.Add(_bossEntityId);
            return ids;
        }

        /// <summary>
        /// Cleans up all controllers. Called when the region shuts down.
        /// </summary>
        public void Shutdown()
        {
            _centerpiece?.Destroy();
            _centerpiece = null;

            foreach (var controller in _controllers)
            {
                try { IncursionLogCollator.EndSession(controller.EntityId); } catch { }
                try { controller?.Dispose(); } catch { }
            }
            _controllers.Clear();
            _enemyEntityIds.Clear();
            Logger.Info($"[VampireBloodRitual] Event shut down.");
        }

        #endregion

        // ------------------------------------------------------------------
        // Spawning helpers
        // ------------------------------------------------------------------

        #region Spawning 

        /// <summary>
        /// Spawns an incursion enemy of type T at an exact position.
        /// </summary>
        private Agent SpawnEnemyAtPosition<T>(Vector3 spawnPos) where T : IncursionEnemyController
        {
            return SpawnEnemyAtPosition(spawnPos, typeof(T));
        }

        /// <summary>
        /// Spawns an incursion enemy of the specified controller type at an exact position.
        /// Non-generic overload used for runtime type selection (e.g. random miniboss pool).
        /// </summary>
        private Agent SpawnEnemyAtPosition(Vector3 spawnPos, Type controllerType)
        {
            spawnPos = RegionLocation.ProjectToFloor(_region, spawnPos);
            if (spawnPos == Vector3.Zero) return null;

            var cell = _region.GetCellAtPosition(spawnPos);
            if (cell == null) return null;

            // Create the controller via Activator (controllerType has a (Game) constructor)
            var controller = (IncursionEnemyController)System.Activator.CreateInstance(controllerType, _game);

            // Determine combat body: boss-type enemies override EntityRef in ApplyRenderSkin
            // For avatar-type enemies, use the default combat body
            PrototypeId entityRef = GameDatabase.GetPrototypeRefByName(CombatBodyProtoName);
            if (controller.RenderBossRef != PrototypeId.Invalid)
                entityRef = controller.RenderBossRef;

            var manager = _region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = entityRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Apply visual/collision scale override (e.g. 25% bigger for vampire bosses/mids)
            float configScale = 1.5f;
            spec.BoundsScaleOverride = controller.VisualScaleOverride > 0f
                ? controller.VisualScaleOverride
                : configScale;

            // Apply render skin (avatar costume, nameplate, etc.)
            ApplyRenderSkin(spec, controller);

            int level = _region.GetAreaLevel(cell.Area);
            spec.Properties[PropertyEnum.CharacterLevel] = level;
            spec.Properties[PropertyEnum.CombatLevel] = level;
            spec.Properties[PropertyEnum.VariationSeed] = _game.Random.Next(1, 10000);

            // Hide minimap markers for all vampire event enemies
            spec.Properties[PropertyEnum.MapTracking] = false;

            spec.Spawn();

            // After spawn, hide minimap markers (boss prototypes re-enable MapTracking on spawn).
            // Aggro is handled by the controller's WakeRadius system - enemies only chase
            // after a player enters their wake radius, then chase forever.
            if (spec.ActiveEntity is Agent spawnedAgent)
            {
                spawnedAgent.Properties[PropertyEnum.MapTracking] = false;
            }

            var entity = spec.ActiveEntity;
            if (entity == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                controller.Dispose();
                Logger.Warn($"[VampireBloodRitual] Failed to spawn {controllerType.Name} at {spawnPos}.");
                return null;
            }

            if (entity is Agent agent)
            {
                controller.Start(agent);
                _controllers.Add(controller);
                _enemyEntityIds.Add(agent.Id);

                // Register with IncursionManager so the damage pipeline can look up
                // the controller's outgoing damage scale for these entities.
                _game.IncursionManager?.RegisterController(controller);

                // Begin per-encounter log session (same as IncursionManager).
                string logName = controller.LogTrueName ?? controller.GetLabel() ?? controller.GetType().Name;
                IncursionLogCollator.BeginSession(agent.Id, controller.LogFilePrefix, logName);

                // Boss and TeamUp enemies (RenderAvatarRef == Invalid) need an invisible
                // nameplate proxy for the red prestige nameplate, same as IncursionManager.
                // Skip the proxy for enemies that opt out (e.g. trash-tier thralls) to
                // avoid visual bugs from the unreliable IsClientEntityHidden flag.
                // The display name for proxy-less enemies is set via ClientRenderPlayerName
                // on the spawn spec in ApplyRenderSkin (boss path).
                if (controller.RenderAvatarRef == PrototypeId.Invalid && controller.NeedsNameplateProxy)
                    _game.IncursionManager?.SpawnNameplateProxy(_region, agent, controller);

                Logger.Info($"[VampireBloodRitual] Spawned {controllerType.Name} (id={agent.Id}) at {spawnPos}.");
                return agent;
            }

            controller.Dispose();
            return null;
        }

        /// <summary>
        /// Spawns the Vampire Blood Lord boss at the ritual site.
        /// </summary>
        private void SpawnBoss(Vector3 position)
        {
            Agent boss = SpawnEnemyAtPosition<CalamityEnemyVampireBossBloodLord>(position);
            if (boss != null)
                _bossEntityId = boss.Id;
        }

        /// <summary>
        /// Spawns the ritual centerpiece: a floating amulet model with looping VFX
        /// and a blue prestige nameplate reading "Amulet of Quiox".
        /// </summary>
        private void SpawnCenterpiece(Vector3 position)
        {
            // Resolve the VFX asset from PowerVisualsGlobals
            var globalsProto = GameDatabase.PowerVisualsGlobalsPrototype;
            AssetId vfxAsset = AssetId.Invalid;
            if (globalsProto != null)
                vfxAsset = globalsProto.InfinityPowerPointEarnedClass;

            if (vfxAsset == AssetId.Invalid)
                Logger.Warn("[VampireBloodRitual] InfinityPowerPointEarnedClass VFX asset not found, centerpiece will have no VFX.");

            _centerpiece = new RitualCenterpiece(
                game: _game,
                region: _region,
                displayName: "Amulet of Quiox",
                prestigeLevel: 2,       // blue nameplate
                itemProtoPath: "Entity/Items/Artifacts/Prototypes/Tier1Artifacts/Art050.prototype",
                hoverHeight: 100f,
                boundsScale: 3.0f,
                vfxAssetId: vfxAsset,
                vfxIntervalMs: 1000
            );

            _centerpiece.Spawn(position);
        }

        #endregion

        // ------------------------------------------------------------------
        // Render skin application (mirrors IncursionManager.ApplyRenderSkin)
        // ------------------------------------------------------------------

        #region Render Skin 

        private void ApplyRenderSkin(SpawnSpec spec, IncursionEnemyController controller)
        {
            // Boss render path
            PrototypeId bossRef = controller.RenderBossRef;
            if (bossRef != PrototypeId.Invalid)
            {
                spec.EntityRef = bossRef;

                // Set custom display name for the boss entity (same as IncursionManager).
                // This is especially important for trash-tier enemies that skip the
                // nameplate proxy - without this, they'd have no visible name at all.
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

                return;
            }

            // Avatar render path
            PrototypeId renderRef = controller.RenderAvatarRef;
            if (renderRef != PrototypeId.Invalid)
            {
                var avatarProto = renderRef.As<AvatarPrototype>();
                if (avatarProto == null) return;

                spec.ClientRenderPrototypeRef = renderRef;

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

                // Apply costume from controller's costume table or default
                PrototypeId costumeRef = controller.RenderCostumeRef;
                if (costumeRef == PrototypeId.Invalid || costumeRef.As<CostumePrototype>() == null)
                    costumeRef = avatarProto.GetStartingCostumeForPlatform(Platforms.PC);

                if (costumeRef != PrototypeId.Invalid)
                    spec.Properties[PropertyEnum.CostumeCurrent] = costumeRef;

                return;
            }

            // Team-Up render path
            PrototypeId teamupRef = controller.RenderTeamupRef;
            if (teamupRef != PrototypeId.Invalid)
            {
                spec.ClientRenderPrototypeRef = teamupRef;
                spec.ClientRenderPlayerName = controller.InvaderDisplayName;
            }
        }

        #endregion

        #endregion
    }
}
