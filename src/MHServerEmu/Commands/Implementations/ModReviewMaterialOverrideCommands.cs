// =============================================================================
//  MOD ReviewMaterialOverride - commands
// =============================================================================
//  Feature:    Dev tool for researching server-side material / texture / model
//              overrides on entities, avatars, and props.  Spawns test entities
//              using the SpawnSpec pipeline (same as Incursion mod) with various
//              override techniques applied at creation time.
//
//  KEY FINDING: Post-spawn property changes (CostumeCurrent, etc.) are replicated
//  via NetMessageSetProperty but the client does NOT re-build visual pawns on
//  live property updates.  Overrides MUST be set on SpawnSpec BEFORE Spawn().
//
//  KEY FINDING: BoundsScaleOverride only scales server-side collision bounds.
//  The client receives it via archive but may not visually scale avatar-rendered
//  entities.  Scale works reliably for non-avatar props/entities.
//
//  KEY FINDING: Avatar rendering (ClientRenderPrototypeRef + CostumeCurrent) is
//  the same technique as the Incursion mod.  The costume/render tests duplicate
//  Incursion's proven approach.  Unique to this mod: condition UnrealClass,
//  EntityState appearance, and prop material/texture exploration.
//
//  Commands:   !mat test [technique]  = spawn test entities with overrides
//              !mat spawn <type> ...  = fine-grained spawn with custom params
//              !mat cleanup           = destroy all test-spawned entities
//              !mat info [entityId]   = dump rendering chain for an entity
//              !mat search <pattern>  = search all game assets
//              !mat props             = search + spawn prop prototypes
//              !mat reset             = reset avatar + cleanup spawned entities
//
//  Logging:    Always on when ReviewMaterialOverrideLoggingEnable=true in Config.ini.
//              Actions collated to Data/ReviewMaterialOverrideLog.txt.
//
//  Techniques: costume  = spawn entity rendered as Silver Surfer (chrome)
//              render   = spawn entity rendered as She-Hulk (model swap)
//              condition= spawn entity + apply condition with UnrealClass
//              state    = spawn prop + set EntityState appearance
//              scale    = spawn entity at 3x scale (positive control)
// =============================================================================

using System.Text;
using Gazillion;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.Network;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Common;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Calligraphy;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Navi;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Powers.Conditions;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;
using MHServerEmu.Games.Loot;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("mat")]
    [CommandGroupDescription("Dev tool for researching material/texture/model overrides. Subcommands: test, info, search, props, reset.")]
    [CommandGroupUserLevel(AccountUserLevel.Admin)]
    public class ModReviewMaterialOverrideCommands : CommandGroup
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static bool FileLoggingEnabled => ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>().ReviewMaterialOverrideLoggingEnable;

        // Track test-spawned entities for cleanup
        private static readonly List<ulong> _spawnedEntityIds = new();

        // --- Hardcoded test defaults ----------------------------------------

        // Combat body — the invisible server-side entity that gets rendered as something else.
        // Same one the Incursion mod uses.
        private const string CombatBodyPath = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";

        // Silver Surfer — chrome material source
        private const string SilverSurferAvatarPath = "Entity/Characters/Avatars/Shipping/SilverSurfer.prototype";
        private const string SilverSurferCostumePath = "Entity/Items/Costumes/Prototypes/SilverSurfer/Classic.prototype";

        // She-Hulk — model swap test
        private const string SheHulkAvatarPath = "Entity/Characters/Avatars/Shipping/SheHulk.prototype";
        private const string SheHulkCostumePath = "Entity/Items/Costumes/Prototypes/SheHulk/ModernVU.prototype";

        // Prop search patterns — broadened for better coverage
        private static readonly string[] PropSearchPatterns =
        {
            "Chest", "Pyramid", "Obelisk", "Beacon", "Crystal", "Generator",
            "SkrullBeacon", "Destructible", "Barrel", "Crate", "Door", "Switch",
            "Lever", "Button", "Altar", "Statue", "Pillar", "Column", "Terminal"
        };

        // --- Helpers --------------------------------------------------------

        private static string GetAssetDisplayName(AssetId assetId)
        {
            string name = GameDatabase.GetAssetName(assetId);
            return string.IsNullOrEmpty(name) ? assetId.ToString() : name;
        }

        private static string GetAssetTypeName(AssetId assetId)
        {
            AssetTypeId typeId = GameDatabase.DataDirectory.AssetDirectory.GetAssetTypeRef(assetId);
            return typeId == AssetTypeId.Invalid ? "Unknown" : GameDatabase.GetAssetTypeName(typeId);
        }

        private static string ValidateAvatar(NetClient client, out PlayerConnection playerConnection, out Avatar avatar)
        {
            playerConnection = (PlayerConnection)client;
            avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
                return "Your avatar must be in the world to use material override commands.";
            return null;
        }

        private static void LogAttempt(string technique, string message)
        {
            string line = $"[ReviewMaterialOverride] [{technique}] {message}";
            Logger.Info(line);
            if (FileLoggingEnabled)
                System.IO.File.AppendAllText("Data/ReviewMaterialOverrideLog.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {line}\n");
        }

        // --- Spawn pipeline (same as Incursion mod) -------------------------

        private const float SpawnSearchRadius = 128f;
        private const float MaxSpawnDistance = 1500f;

        /// <summary>
        /// Finds a navmesh-valid spawn position near the avatar within MaxSpawnDistance units.
        /// Tries forward arc first, then full circle, then right next to the player.
        /// Same approach as IncursionManager.ChooseOpenSpawnPosition.
        /// </summary>
        private static Vector3 FindValidSpawnPosition(Region region, Avatar avatar, WorldEntityPrototype entityProto)
        {
            Vector3 playerPos = avatar.RegionLocation.Position;
            float playerYaw = avatar.Orientation.Yaw;
            float baseDistance = 300f + (float)(avatar.Game.Random.NextDouble() * 200f);
            PathFlags pathFlags = Region.GetPathFlagsForEntity(entityProto);
            var posFlags = PositionCheckFlags.CanBeBlockedEntity | PositionCheckFlags.CanPathTo;
            var blockFlags = BlockingCheckFlags.CheckSpawns;

            // 1) Forward arc: try angles centered on player's yaw (-45 to +45 degrees)
            for (int i = 0; i < 5; i++)
            {
                float angleOffset = (i - 2) * (MathF.PI / 8f);
                float angle = playerYaw + angleOffset;
                Vector3 origin = playerPos + new Vector3(MathF.Cos(angle) * baseDistance, MathF.Sin(angle) * baseDistance, 0f);
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, posFlags, blockFlags);
                if (candidate != origin) return candidate;
            }

            // 2) Wider fallback: sweep full circle in 8 steps
            float fallbackAngleOffset = (float)(avatar.Game.Random.NextDouble() * MathF.PI * 2f);
            for (int i = 0; i < 8; i++)
            {
                float angle = fallbackAngleOffset + (i * MathF.PI / 4f);
                Vector3 origin = playerPos + new Vector3(MathF.Cos(angle) * baseDistance, MathF.Sin(angle) * baseDistance, 0f);
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, posFlags, blockFlags);
                if (candidate != origin) return candidate;
            }

            // 3) Last resort: try right next to the player
            {
                Vector3 origin = playerPos;
                Bounds bounds = new(entityProto.Bounds, origin);
                Vector3 candidate = ChooseSpawnPosition(region, origin, ref bounds, pathFlags, posFlags, blockFlags);
                if (candidate != origin) return candidate;
            }

            return Vector3.Zero;
        }

        private static Vector3 ChooseSpawnPosition(Region region, Vector3 position, ref Bounds bounds,
            PathFlags pathFlags, PositionCheckFlags posFlags, BlockingCheckFlags blockFlags)
        {
            Vector3 spawnPosition = position;

            if (region.IsLocationClear(ref bounds, pathFlags, posFlags, blockFlags))
                return bounds.Center;

            float minDistance;
            float maxDistance = 0.0f;
            bool spawnFound = false;

            while (spawnFound == false)
            {
                minDistance = maxDistance;
                maxDistance += SpawnSearchRadius;
                if (maxDistance > MaxSpawnDistance) return position;
                spawnFound = region.ChooseRandomPositionNearPoint(ref bounds, pathFlags, posFlags, blockFlags,
                    minDistance, maxDistance, out spawnPosition);
            }

            return spawnPosition;
        }

        /// <summary>
        /// Spawns a test entity near the avatar using the SpawnSpec pipeline.
        /// Overrides (ClientRenderPrototypeRef, CostumeCurrent, BoundsScaleOverride)
        /// are applied BEFORE Spawn() so they get baked into the entity create message.
        /// Uses navmesh-valid position finding to ensure entities spawn within reach.
        /// </summary>
        private static WorldEntity SpawnTestEntity(Avatar avatar, string label,
            PrototypeId entityRef, PrototypeId renderAvatarRef = PrototypeId.Invalid,
            PrototypeId costumeRef = PrototypeId.Invalid, float scale = 1f)
        {
            var region = avatar.Region;
            if (region == null)
            {
                LogAttempt(label, "FAIL: avatar.Region is null");
                return null;
            }

            var popManager = region.PopulationManager;
            if (popManager == null)
            {
                LogAttempt(label, "FAIL: region.PopulationManager is null");
                return null;
            }

            // Get entity prototype for bounds + path flags
            var entityProto = entityRef.As<WorldEntityPrototype>();
            if (entityProto == null)
            {
                LogAttempt(label, $"FAIL: Could not resolve WorldEntityPrototype for {GameDatabase.GetPrototypeName(entityRef)}");
                return null;
            }

            // Find a navmesh-valid spawn position near the avatar
            Vector3 spawnPos = FindValidSpawnPosition(region, avatar, entityProto);
            if (spawnPos == Vector3.Zero)
            {
                LogAttempt(label, $"FAIL: Could not find valid spawn position within {MaxSpawnDistance} units of avatar at {avatar.RegionLocation.Position.ToStringNames()}");
                return null;
            }

            // Project to floor
            spawnPos = RegionLocation.ProjectToFloor(region, spawnPos);

            var group = popManager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

            var spec = popManager.CreateSpawnSpec(group);
            spec.EntityRef = entityRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;
            spec.BoundsScaleOverride = scale;

            // Apply render override BEFORE Spawn() — this is the critical step.
            // The override gets baked into NetMessageEntityCreate archive data.
            if (renderAvatarRef != PrototypeId.Invalid)
            {
                spec.ClientRenderPrototypeRef = renderAvatarRef;

                // Custom name for the rendered entity
                spec.ClientRenderPlayerName = label;

                if (costumeRef != PrototypeId.Invalid)
                    spec.Properties[PropertyEnum.CostumeCurrent] = costumeRef;
            }

            int level = avatar.CharacterLevel;
            spec.Properties[PropertyEnum.CharacterLevel] = level;
            spec.Properties[PropertyEnum.CombatLevel] = level;
            spec.Properties[PropertyEnum.VariationSeed] = avatar.Game.Random.Next(1, 10000);

            spec.Spawn();

            var entity = spec.ActiveEntity;
            if (entity == null)
            {
                popManager.RemoveSpawnGroup(group.Id);
                LogAttempt(label, $"FAIL: Spawn() returned null entity for {GameDatabase.GetPrototypeName(entityRef)}");
                return null;
            }

            _spawnedEntityIds.Add(entity.Id);

            // Log full rendering diagnostics
            AssetId worldAsset = entity.GetEntityWorldAsset();
            AssetId originalAsset = entity.GetOriginalWorldAsset();
            PrototypeId appliedCostume = entity.Properties[PropertyEnum.CostumeCurrent];
            string renderInfo = entity.ClientPrototypeRefOverride != PrototypeId.Invalid
                ? GameDatabase.GetPrototypeName(entity.ClientPrototypeRefOverride) : "(self)";
            string costumeInfo = appliedCostume != PrototypeId.Invalid
                ? GameDatabase.GetPrototypeName(appliedCostume) : "(none)";

            string diagLine = $"Spawned id=0x{entity.Id:X}, proto={GameDatabase.GetPrototypeName(entity.PrototypeDataRef)}, " +
                $"renderAs={renderInfo}, costume={costumeInfo}, " +
                $"worldAsset={GetAssetDisplayName(worldAsset)} [{GetAssetTypeName(worldAsset)}], " +
                $"originalAsset={GetAssetDisplayName(originalAsset)} [{GetAssetTypeName(originalAsset)}], " +
                $"scale={scale:0.0}, isClientRenderedAsAvatar={entity.IsClientRenderedAsAvatar}, " +
                $"pos={spawnPos.ToStringNames()}";

            LogAttempt(label, diagLine);

            return entity;
        }

        /// <summary>
        /// Destroys all test-spawned entities and clears the tracking list.
        /// </summary>
        private static int CleanupSpawnedEntities(Game game)
        {
            int cleaned = 0;
            foreach (ulong entityId in _spawnedEntityIds)
            {
                var entity = game.EntityManager.GetEntity<WorldEntity>(entityId);
                if (entity != null)
                {
                    entity.ScheduleDestroyEvent(TimeSpan.Zero);
                    cleaned++;
                }
            }
            _spawnedEntityIds.Clear();
            return cleaned;
        }

        // --- Default command ------------------------------------------------

        [DefaultCommand]
        [CommandDescription("Shows available material override commands.")]
        [CommandUsage("mat [test|spawn|cleanup|info|search|props|reset] ...")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public override string Fallback(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("Material Override commands:\n");
            sb.Append("  !mat test [technique]   - Spawn test entities with overrides (costume|render|condition|state|scale|all)\n");
            sb.Append("  !mat spawn <type> ...   - Fine-grained spawn: avatar <path> [costume] [scale] | boss <path> [scale] | prop <path> [scale]\n");
            sb.Append("  !mat cleanup            - Destroy all test-spawned entities\n");
            sb.Append("  !mat info [entityId]    - Show rendering chain for an entity\n");
            sb.Append("  !mat search <pattern>   - Search all game assets by name pattern\n");
            sb.Append("  !mat props              - Search + spawn prop prototypes\n");
            sb.Append("  !mat reset              - Reset avatar + cleanup spawned entities\n");
            sb.Append("  !mat crossmat <avatar> <costume> [scale] - Spawn avatar body with DIFFERENT avatar's costume (material swap test)\n");
            sb.Append("  !mat item <itemPath> [scale]  - Spawn an item as decorative in-world model (like loot drops)\n");
            sb.Append("  !mat costumes <avatarPath>    - List costumes for an avatar (shows CostumeUnrealClass assets)\n");
            sb.Append("  !mat applycostume <id> <path> - Apply costume to existing entity (post-spawn test)\n");
            sb.Append("\nTechniques: costume=Silver Surfer chrome, render=She-Hulk model swap, condition=UnrealClass, state=EntityState, scale=3x prop control");
            sb.Append("\nCross-material: crossmat tests whether costume material (CostumeUnrealClass) transfers across avatar models");
            return sb.ToString();
        }

        // --- test -----------------------------------------------------------

        [Command("test")]
        [CommandDescription("Spawns test entities with various override techniques applied at creation time.")]
        [CommandUsage("mat test [costume|render|condition|state|scale|all]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Test(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string technique = @params.Length > 0 ? @params[0].ToLowerInvariant() : "all";
            var results = new List<string> { $"=== Material Override Test: {technique} ===" };
            results.Add($"  Spawned entities so far: {_spawnedEntityIds.Count}");

            if (technique is "all" or "costume")
                results.AddRange(TestCrossAvatarCostume(avatar));

            if (technique is "all" or "render")
                results.AddRange(TestClientPrototypeRefOverride(avatar));

            if (technique is "all" or "condition")
                results.AddRange(TestConditionUnrealClass(avatar));

            if (technique is "all" or "state")
                results.AddRange(TestEntityStateAppearance(avatar));

            if (technique is "all" or "scale")
                results.AddRange(TestBoundsScale(avatar));

            results.Add($"  Total spawned entities: {_spawnedEntityIds.Count}");
            results.Add("  Use !mat cleanup to destroy test entities.");
            results.Add("=== Test complete. Check chat and log for results. ===");
            CommandHelper.SendMessages(client, results);
            return string.Empty;
        }

        // --- Test: Cross-avatar costume override (spawn-based) -------------

        private List<string> TestCrossAvatarCostume(Avatar avatar)
        {
            var lines = new List<string>();
            lines.Add("\n--- Technique: Cross-Avatar Costume Override (spawn) ---");
            lines.Add("  NOTE: This duplicates the Incursion mod's avatar rendering approach.");
            lines.Add("  The entity will look like Silver Surfer — same as Incursion spawn.");
            lines.Add("  Useful for testing different costumes/avatars via !mat spawn avatar.");

            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyPath);
            if (combatBodyRef == PrototypeId.Invalid)
            {
                lines.Add($"  FAIL: Could not resolve combat body '{CombatBodyPath}'.");
                LogAttempt("costume", $"Failed to resolve combat body: {CombatBodyPath}");
                return lines;
            }

            PrototypeId surferAvatarRef = GameDatabase.GetPrototypeRefByName(SilverSurferAvatarPath);
            PrototypeId surferCostumeRef = GameDatabase.GetPrototypeRefByName(SilverSurferCostumePath);

            if (surferAvatarRef == PrototypeId.Invalid || surferCostumeRef == PrototypeId.Invalid)
            {
                lines.Add("  FAIL: Could not resolve Silver Surfer avatar/costume.");
                LogAttempt("costume", "Failed to resolve Silver Surfer avatar or costume");
                return lines;
            }

            var costumeProto = surferCostumeRef.As<CostumePrototype>();
            AssetId costumeUnreal = costumeProto?.CostumeUnrealClass ?? AssetId.Invalid;

            lines.Add($"  Combat body: {GameDatabase.GetPrototypeName(combatBodyRef)}");
            lines.Add($"  Render as: {GameDatabase.GetPrototypeName(surferAvatarRef)} (Silver Surfer)");
            lines.Add($"  Costume: {GameDatabase.GetPrototypeName(surferCostumeRef)}");
            lines.Add($"  CostumeUnrealClass: {GetAssetDisplayName(costumeUnreal)} [{GetAssetTypeName(costumeUnreal)}]");

            var entity = SpawnTestEntity(avatar, "costume", combatBodyRef, surferAvatarRef, surferCostumeRef, 1.5f);

            if (entity != null)
            {
                lines.Add($"  SPAWNED: id=0x{entity.Id:X}, IsClientRenderedAsAvatar={entity.IsClientRenderedAsAvatar}");
                lines.Add($"  GetEntityWorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                lines.Add("  => Check in-game: a Silver Surfer (chrome) entity should be visible.");
            }
            else
            {
                lines.Add("  FAIL: SpawnTestEntity returned null.");
            }

            return lines;
        }

        // --- Test: ClientPrototypeRefOverride (spawn-based) ----------------

        private List<string> TestClientPrototypeRefOverride(Avatar avatar)
        {
            var lines = new List<string>();
            lines.Add("\n--- Technique: ClientPrototypeRefOverride (spawn) ---");
            lines.Add("  NOTE: This duplicates the Incursion mod's avatar rendering approach.");
            lines.Add("  The entity will look like She-Hulk — same as Incursion spawn.");
            lines.Add("  Useful for testing different avatars via !mat spawn avatar <path>.");

            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyPath);
            PrototypeId sheHulkRef = GameDatabase.GetPrototypeRefByName(SheHulkAvatarPath);
            PrototypeId sheHulkCostumeRef = GameDatabase.GetPrototypeRefByName(SheHulkCostumePath);

            if (combatBodyRef == PrototypeId.Invalid || sheHulkRef == PrototypeId.Invalid)
            {
                lines.Add("  FAIL: Could not resolve combat body or She-Hulk avatar.");
                LogAttempt("render", "Failed to resolve combat body or She-Hulk avatar");
                return lines;
            }

            if (sheHulkCostumeRef == PrototypeId.Invalid)
            {
                var sheHulkProto = sheHulkRef.As<AvatarPrototype>();
                sheHulkCostumeRef = sheHulkProto?.GetStartingCostumeForPlatform(Platforms.PC) ?? PrototypeId.Invalid;
            }

            var costumeProto = sheHulkCostumeRef.As<CostumePrototype>();
            AssetId costumeUnreal = costumeProto?.CostumeUnrealClass ?? AssetId.Invalid;

            lines.Add($"  Combat body: {GameDatabase.GetPrototypeName(combatBodyRef)}");
            lines.Add($"  Render as: {GameDatabase.GetPrototypeName(sheHulkRef)} (She-Hulk)");
            lines.Add($"  Costume: {GameDatabase.GetPrototypeName(sheHulkCostumeRef)}");
            lines.Add($"  CostumeUnrealClass: {GetAssetDisplayName(costumeUnreal)} [{GetAssetTypeName(costumeUnreal)}]");

            var entity = SpawnTestEntity(avatar, "render", combatBodyRef, sheHulkRef, sheHulkCostumeRef, 1.5f);

            if (entity != null)
            {
                lines.Add($"  SPAWNED: id=0x{entity.Id:X}, IsClientRenderedAsAvatar={entity.IsClientRenderedAsAvatar}");
                lines.Add($"  GetEntityWorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                lines.Add("  => Check in-game: a She-Hulk entity should be visible (different from Silver Surfer).");
            }
            else
            {
                lines.Add("  FAIL: SpawnTestEntity returned null.");
            }

            return lines;
        }

        // --- Test: Condition UnrealClass (spawn + apply) -------------------

        private List<string> TestConditionUnrealClass(Avatar avatar)
        {
            var lines = new List<string>();
            lines.Add("\n--- Technique: Condition UnrealClass (spawn + apply) ---");

            var conditionMatches = GameDatabase.SearchPrototypes("Boost",
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            lines.Add($"  Found {conditionMatches.Count} condition prototypes matching 'Boost'.");

            ConditionPrototype foundCondition = null;
            PrototypeId foundConditionRef = PrototypeId.Invalid;
            foreach (var condRef in conditionMatches.Take(20))
            {
                var condProto = condRef.As<ConditionPrototype>();
                if (condProto?.UnrealClass != AssetId.Invalid)
                {
                    foundCondition = condProto;
                    foundConditionRef = condRef;
                    break;
                }
            }

            if (foundCondition == null)
            {
                lines.Add("  No condition with a UnrealClass found in first 20 results.");
                LogAttempt("condition", "No condition with UnrealClass found among Boost conditions");
                return lines;
            }

            AssetId condUnreal = foundCondition.UnrealClass;
            lines.Add($"  Found condition: {GameDatabase.GetPrototypeName(foundConditionRef)}");
            lines.Add($"  Condition UnrealClass: {GetAssetDisplayName(condUnreal)} [{GetAssetTypeName(condUnreal)}]");
            lines.Add($"  Condition has UnrealOverrides: {foundCondition.UnrealOverrides?.Length > 0}");

            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyPath);
            if (combatBodyRef == PrototypeId.Invalid)
            {
                lines.Add($"  FAIL: Could not resolve combat body '{CombatBodyPath}'.");
                return lines;
            }

            var entity = SpawnTestEntity(avatar, "condition", combatBodyRef, scale: 1.5f);
            if (entity == null)
            {
                lines.Add("  FAIL: SpawnTestEntity returned null.");
                return lines;
            }

            lines.Add($"  SPAWNED: id=0x{entity.Id:X}");

            try
            {
                var conditionCollection = entity.ConditionCollection;
                if (conditionCollection == null)
                {
                    lines.Add("  FAIL: Entity has no ConditionCollection.");
                    return lines;
                }

                Condition condition = ConditionCollection.AllocateCondition();
                ulong conditionId = conditionCollection.NextConditionId;
                bool initialized = condition.InitializeFromConditionPrototype(
                    conditionId, avatar.Game, entity.Id, entity.Id, entity.Id, foundCondition, TimeSpan.FromSeconds(60));

                if (initialized == false)
                {
                    lines.Add("  FAIL: Condition.InitializeFromConditionPrototype returned false.");
                    LogAttempt("condition", "InitializeFromConditionPrototype failed");
                    return lines;
                }

                bool added = conditionCollection.AddCondition(condition);
                lines.Add($"  Apply condition: AddCondition={added}, conditionId={conditionId}");
                LogAttempt("condition", $"AddCondition={added}, id={conditionId}, UnrealClass={GetAssetDisplayName(condUnreal)}");

                if (added)
                    lines.Add("  => Check in-game: did a visual effect appear on the spawned entity?");
                else
                    lines.Add("  NOTE: Condition was not added.");
            }
            catch (Exception ex)
            {
                lines.Add($"  ERROR: {ex.Message}");
                LogAttempt("condition", $"Exception: {ex.Message}");
            }

            return lines;
        }

        // --- Test: Entity State Appearance (spawn-based) -------------------

        private List<string> TestEntityStateAppearance(Avatar avatar)
        {
            var lines = new List<string>();
            lines.Add("\n--- Technique: Entity State Appearance (spawn) ---");

            // Search for EntityStatePrototype instances
            var stateMatches = GameDatabase.SearchPrototypes("Destroyed",
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            lines.Add($"  Found {stateMatches.Count} prototypes matching 'Destroyed'.");

            EntityStatePrototype foundState = null;
            PrototypeId foundStateRef = PrototypeId.Invalid;
            foreach (var stateRef in stateMatches.Take(30))
            {
                var stateProto = stateRef.As<EntityStatePrototype>();
                if (stateProto != null)
                {
                    foundState = stateProto;
                    foundStateRef = stateRef;
                    break;
                }
            }

            if (foundState == null)
            {
                lines.Add("  No EntityStatePrototype found in first 30 results.");
                LogAttempt("state", "No EntityStatePrototype found among 'Destroyed' matches");
                return lines;
            }

            lines.Add($"  Found state: {GameDatabase.GetPrototypeName(foundStateRef)}");
            lines.Add($"  State AppearanceEnum: {foundState.AppearanceEnum}");

            // Find a prop to spawn and apply the state to
            var propMatches = GameDatabase.SearchPrototypes("Destructible",
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            PrototypeId foundPropRef = PrototypeId.Invalid;
            foreach (var propRef in propMatches.Take(30))
            {
                var propProto = propRef.As<PropPrototype>();
                if (propProto != null && propProto.UnrealClass != AssetId.Invalid)
                {
                    foundPropRef = propRef;
                    break;
                }
            }

            if (foundPropRef == PrototypeId.Invalid)
            {
                lines.Add("  No PropPrototype with UnrealClass found.");
                return lines;
            }

            var propProto2 = foundPropRef.As<PropPrototype>();
            lines.Add($"  Found prop: {GameDatabase.GetPrototypeName(foundPropRef)}");
            lines.Add($"  Prop UnrealClass: {GetAssetDisplayName(propProto2.UnrealClass)} [{GetAssetTypeName(propProto2.UnrealClass)}]");

            // Spawn the prop
            var entity = SpawnTestEntity(avatar, "state", foundPropRef, scale: 1f);
            if (entity == null)
            {
                lines.Add("  FAIL: SpawnTestEntity returned null.");
                return lines;
            }

            lines.Add($"  SPAWNED: id=0x{entity.Id:X}");

            // Set the entity state
            try
            {
                entity.Properties[PropertyEnum.EntityState] = foundStateRef;
                lines.Add($"  SET EntityState={GameDatabase.GetPrototypeName(foundStateRef)}, AppearanceEnum={foundState.AppearanceEnum}");
                LogAttempt("state", $"Set EntityState={GameDatabase.GetPrototypeName(foundStateRef)}, AppearanceEnum={foundState.AppearanceEnum}");
            }
            catch (Exception ex)
            {
                lines.Add($"  ERROR setting state: {ex.Message}");
            }

            lines.Add("  => Check in-game: did the spawned prop's appearance change?");
            return lines;
        }

        // --- Test: BoundsScaleOverride (positive control, spawn-based) -----

        private List<string> TestBoundsScale(Avatar avatar)
        {
            var lines = new List<string>();
            lines.Add("\n--- Technique: BoundsScaleOverride (positive control, spawn) ---");
            lines.Add("  NOTE: BoundsScaleOverride scales server-side collision bounds. The client");
            lines.Add("  receives it but may not visually scale avatar-rendered entities.");
            lines.Add("  Using a prop (not avatar) where scale is reliably applied.");

            // Find a destructible prop to spawn at 3x scale
            var propMatches = GameDatabase.SearchPrototypes("Barrel",
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            PrototypeId propRef = PrototypeId.Invalid;
            foreach (var match in propMatches.Take(20))
            {
                var propProto = match.As<PropPrototype>();
                if (propProto != null && propProto.UnrealClass != AssetId.Invalid)
                {
                    propRef = match;
                    break;
                }
            }

            if (propRef == PrototypeId.Invalid)
            {
                lines.Add("  FAIL: Could not find a prop prototype to spawn.");
                return lines;
            }

            lines.Add($"  Prop: {GameDatabase.GetPrototypeName(propRef)}");
            lines.Add("  Scale: 3.0x (should be visibly large — confirms spawn pipeline works)");

            var entity = SpawnTestEntity(avatar, "scale", propRef, scale: 3.0f);

            if (entity != null)
            {
                lines.Add($"  SPAWNED: id=0x{entity.Id:X}, scale=3.0x");
                lines.Add("  => Check in-game: a 3x-size prop should be clearly visible.");
                lines.Add("  This confirms the spawn pipeline + BoundsScaleOverride work for props.");
            }
            else
            {
                lines.Add("  FAIL: SpawnTestEntity returned null.");
            }

            LogAttempt("scale", $"Spawned 3x scale prop {GameDatabase.GetPrototypeName(propRef)} as positive control");
            return lines;
        }

        // --- spawn ----------------------------------------------------------

        [Command("spawn")]
        [CommandDescription("Fine-grained spawn with custom params. Types: avatar, boss, prop.")]
        [CommandUsage("mat spawn avatar <avatarPath> [costumePath] [scale] | mat spawn boss <protoPath> [scale] | mat spawn prop <protoPath> [scale]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Spawn(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (@params.Length < 2)
                return "Usage: !mat spawn <avatar|boss|prop> <path> [costumePath] [scale]";

            string spawnType = @params[0].ToLowerInvariant();
            string protoPath = @params[1];
            float scale = 1.5f;

            // Parse optional scale from last param if it's a float
            int pathEndIndex = @params.Length;
            if (float.TryParse(@params[^1], out float parsedScale))
            {
                scale = parsedScale;
                pathEndIndex--;
            }

            var lines = new List<string> { $"=== Spawn: type={spawnType}, proto={protoPath}, scale={scale} ===" };

            PrototypeId protoRef = GameDatabase.GetPrototypeRefByName(protoPath);
            if (protoRef == PrototypeId.Invalid)
            {
                lines.Add($"  FAIL: Could not resolve prototype '{protoPath}'.");
                return string.Join("\n", lines);
            }

            if (spawnType == "avatar")
            {
                // Render a combat body as the specified avatar
                PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyPath);
                if (combatBodyRef == PrototypeId.Invalid)
                {
                    lines.Add($"  FAIL: Could not resolve combat body '{CombatBodyPath}'.");
                    return string.Join("\n", lines);
                }

                PrototypeId costumeRef = PrototypeId.Invalid;
                if (pathEndIndex > 2 && @params[2] != null)
                {
                    costumeRef = GameDatabase.GetPrototypeRefByName(@params[2]);
                    if (costumeRef == PrototypeId.Invalid)
                    {
                        var avatarProto = protoRef.As<AvatarPrototype>();
                        costumeRef = avatarProto?.GetStartingCostumeForPlatform(Platforms.PC) ?? PrototypeId.Invalid;
                        lines.Add($"  Costume path '{@params[2]}' not found, using starting costume.");
                    }
                }
                else
                {
                    var avatarProto = protoRef.As<AvatarPrototype>();
                    costumeRef = avatarProto?.GetStartingCostumeForPlatform(Platforms.PC) ?? PrototypeId.Invalid;
                }

                var entity = SpawnTestEntity(avatar, $"spawn:{spawnType}", combatBodyRef, protoRef, costumeRef, scale);
                if (entity != null)
                {
                    lines.Add($"  SPAWNED: id=0x{entity.Id:X}");
                    lines.Add($"  RenderAs: {GameDatabase.GetPrototypeName(protoRef)}");
                    lines.Add($"  Costume: {(costumeRef != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeRef) : "(none)")}");
                    lines.Add($"  WorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                }
                else
                    lines.Add("  FAIL: Spawn returned null.");
            }
            else if (spawnType == "boss" || spawnType == "prop")
            {
                // Spawn the entity directly (no render override)
                var entity = SpawnTestEntity(avatar, $"spawn:{spawnType}", protoRef, scale: scale);
                if (entity != null)
                {
                    lines.Add($"  SPAWNED: id=0x{entity.Id:X}");
                    lines.Add($"  Proto: {GameDatabase.GetPrototypeName(protoRef)}");
                    lines.Add($"  WorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                }
                else
                    lines.Add("  FAIL: Spawn returned null.");
            }
            else
            {
                lines.Add($"  Unknown spawn type '{spawnType}'. Use: avatar, boss, prop.");
            }

            LogAttempt("spawn", $"type={spawnType}, proto={protoPath}, scale={scale}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- cleanup --------------------------------------------------------

        [Command("cleanup")]
        [CommandDescription("Destroys all test-spawned entities.")]
        [CommandUsage("mat cleanup")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Cleanup(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            int count = CleanupSpawnedEntities(avatar.Game);
            LogAttempt("cleanup", $"Destroyed {count} test entities");
            return $"Cleaned up {count} test-spawned entities.";
        }

        // --- info -----------------------------------------------------------

        [Command("info")]
        [CommandDescription("Shows the rendering chain for your avatar or a specified entity.")]
        [CommandUsage("mat info [entityId]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Info(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out var playerConnection, out var avatar);
            if (error != null) return error;

            WorldEntity target = avatar;
            if (@params.Length > 0 && ulong.TryParse(@params[0], out ulong entityId))
            {
                target = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
                if (target == null)
                    return $"Entity 0x{entityId:X} not found.";
            }

            var lines = new List<string> { $"=== Rendering Info: {target} ===" };

            // Prototype info
            lines.Add($"  PrototypeDataRef: {GameDatabase.GetPrototypeName(target.PrototypeDataRef)}");
            lines.Add($"  ClientPrototypeRefOverride: {(target.ClientPrototypeRefOverride != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(target.ClientPrototypeRefOverride) : "(none)")}");
            lines.Add($"  GetClientPrototypeDataRef: {GameDatabase.GetPrototypeName(target.GetClientPrototypeDataRef())}");

            // World asset (UnrealClass)
            AssetId worldAsset = target.GetEntityWorldAsset();
            AssetId originalAsset = target.GetOriginalWorldAsset();
            lines.Add($"  GetEntityWorldAsset: {GetAssetDisplayName(worldAsset)} [{GetAssetTypeName(worldAsset)}]");
            lines.Add($"  GetOriginalWorldAsset: {GetAssetDisplayName(originalAsset)} [{GetAssetTypeName(originalAsset)}]");

            // Costume info
            if (target is Avatar targetAvatar)
            {
                PrototypeId costumeRef = targetAvatar.Properties[PropertyEnum.CostumeCurrent];
                lines.Add($"  CostumeCurrent: {(costumeRef != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeRef) : "(none)")}");

                var costumeProto = costumeRef.As<CostumePrototype>();
                if (costumeProto != null)
                {
                    lines.Add($"  CostumeUnrealClass: {GetAssetDisplayName(costumeProto.CostumeUnrealClass)} [{GetAssetTypeName(costumeProto.CostumeUnrealClass)}]");
                    lines.Add($"  Costume UsableBy: {GameDatabase.GetPrototypeName(costumeProto.UsableBy)}");
                }

                PrototypeId equippedCostume = targetAvatar.EquippedCostumeRef;
                lines.Add($"  EquippedCostumeRef: {(equippedCostume != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(equippedCostume) : "(none)")}");
            }

            // Entity state
            PrototypeId stateRef = target.Properties[PropertyEnum.EntityState];
            if (stateRef != PrototypeId.Invalid)
            {
                lines.Add($"  EntityState: {GameDatabase.GetPrototypeName(stateRef)}");
                var stateProto = stateRef.As<EntityStatePrototype>();
                if (stateProto != null)
                    lines.Add($"  EntityState AppearanceEnum: {stateProto.AppearanceEnum}");
            }

            // Conditions
            int conditionCount = 0;
            foreach (var cond in target.ConditionCollection)
                conditionCount++;
            lines.Add($"  Conditions: {conditionCount}");

            // IsClientRenderedAsAvatar
            lines.Add($"  IsClientRenderedAsAvatar: {target.IsClientRenderedAsAvatar}");

            LogAttempt("info", $"Entity={target}, WorldAsset={GetAssetDisplayName(worldAsset)}, Costume={(target is Avatar av ? GameDatabase.GetPrototypeName(av.Properties[PropertyEnum.CostumeCurrent]) : "N/A")}");

            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- search ---------------------------------------------------------

        [Command("search")]
        [CommandDescription("Searches all game assets by name pattern.")]
        [CommandUsage("mat search <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Search(string[] @params, NetClient client)
        {
            string pattern = @params[0];
            var matches = GameDatabase.SearchAssets(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No game assets found matching '{pattern}'.";

            const int MaxResults = 30;
            var lines = new List<string> { $"Game assets matching '{pattern}' ({matches.Count} total, showing {Math.Min(matches.Count, MaxResults)}):" };

            foreach (var assetId in matches.Take(MaxResults))
            {
                string assetName = GetAssetDisplayName(assetId);
                string typeName = GetAssetTypeName(assetId);
                lines.Add($"  {assetName} [{typeName}]");
            }

            if (matches.Count > MaxResults)
                lines.Add($"  ... and {matches.Count - MaxResults} more.");

            LogAttempt("search", $"Pattern='{pattern}', matches={matches.Count}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- props ----------------------------------------------------------

        [Command("props")]
        [CommandDescription("Searches for prop prototypes and spawns the first one found as a test.")]
        [CommandUsage("mat props [pattern]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Props(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            var lines = new List<string> { "=== Prop Prototype Search ===" };

            // If a specific pattern is provided, search only that; otherwise use defaults
            var patterns = @params.Length > 0
                ? new[] { @params[0] }
                : PropSearchPatterns;

            PrototypeId firstPropRef = PrototypeId.Invalid;
            string firstPropName = null;

            foreach (string pattern in patterns)
            {
                var matches = GameDatabase.SearchPrototypes(pattern,
                    DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

                if (matches.Count == 0) continue;

                lines.Add($"\n  Pattern '{pattern}' ({matches.Count} matches):");
                foreach (var protoRef in matches.Take(5))
                {
                    string protoName = GameDatabase.GetPrototypeName(protoRef);
                    var worldProto = protoRef.As<WorldEntityPrototype>();
                    AssetId unrealClass = worldProto?.UnrealClass ?? AssetId.Invalid;
                    string unrealName = unrealClass != AssetId.Invalid ? GetAssetDisplayName(unrealClass) : "(none)";
                    lines.Add($"    {protoName} -> UnrealClass: {unrealName}");

                    if (firstPropRef == PrototypeId.Invalid)
                    {
                        firstPropRef = protoRef;
                        firstPropName = protoName;
                    }
                }
                if (matches.Count > 5)
                    lines.Add($"    ... and {matches.Count - 5} more.");
            }

            // Spawn the first found prop as a test
            if (firstPropRef != PrototypeId.Invalid)
            {
                lines.Add($"\n  Spawning first found prop: {firstPropName}");
                var entity = SpawnTestEntity(avatar, "props", firstPropRef, scale: 1f);
                if (entity != null)
                {
                    lines.Add($"  SPAWNED: id=0x{entity.Id:X}");
                    lines.Add($"  WorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                    lines.Add("  => Check in-game: the prop should be visible near you.");
                }
                else
                    lines.Add("  FAIL: Spawn returned null.");
            }
            else
                lines.Add("  No props found to spawn.");

            LogAttempt("props", $"Searched {patterns.Length} pattern(s), spawned first found");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- reset ----------------------------------------------------------

        [Command("reset")]
        [CommandDescription("Resets avatar overrides and cleans up all test-spawned entities.")]
        [CommandUsage("mat reset")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Reset(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out var playerConnection, out var avatar);
            if (error != null) return error;

            var lines = new List<string> { "=== Resetting Material Overrides ===" };

            // Clean up spawned entities
            int cleaned = CleanupSpawnedEntities(avatar.Game);
            if (cleaned > 0)
                lines.Add($"  Destroyed {cleaned} test-spawned entities.");

            // Reset costume to original
            var avatarProto = avatar.AvatarPrototype;
            PrototypeId startingCostume = avatarProto.GetStartingCostumeForPlatform(Platforms.PC);
            if (startingCostume != PrototypeId.Invalid)
            {
                avatar.Properties[PropertyEnum.CostumeCurrent] = startingCostume;
                lines.Add($"  Reset CostumeCurrent to: {GameDatabase.GetPrototypeName(startingCostume)}");
                LogAttempt("reset", $"Reset CostumeCurrent to {GameDatabase.GetPrototypeName(startingCostume)}");
            }

            // Clear entity state
            PrototypeId stateRef = avatar.Properties[PropertyEnum.EntityState];
            if (stateRef != PrototypeId.Invalid)
            {
                avatar.Properties[PropertyEnum.EntityState] = PrototypeId.Invalid;
                lines.Add("  Cleared EntityState.");
                LogAttempt("reset", "Cleared EntityState");
            }

            // Remove all conditions from avatar
            int condCount = 0;
            var conditionIds = new List<ulong>();
            foreach (var cond in avatar.ConditionCollection)
                conditionIds.Add(cond.Id);

            foreach (ulong condId in conditionIds)
            {
                avatar.ConditionCollection.RemoveCondition(condId);
                condCount++;
            }

            if (condCount > 0)
            {
                lines.Add($"  Removed {condCount} conditions from avatar.");
                LogAttempt("reset", $"Removed {condCount} conditions");
            }

            lines.Add("  Reset complete.");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- crossmat --------------------------------------------------------

        [Command("crossmat")]
        [CommandDescription("Spawn a combat body rendered as one avatar but wearing a DIFFERENT avatar's costume. Tests cross-material transfer.")]
        [CommandUsage("mat crossmat <avatarPath> <costumePath> [scale]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string CrossMat(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string avatarPath = @params[0];
            string costumePath = @params[1];
            float scale = 1.5f;
            if (@params.Length > 2 && float.TryParse(@params[2], out float s)) scale = s;

            var lines = new List<string> { $"=== Cross-Material Test ===" };

            PrototypeId avatarRef = GameDatabase.GetPrototypeRefByName(avatarPath);
            PrototypeId costumeRef = GameDatabase.GetPrototypeRefByName(costumePath);

            if (avatarRef == PrototypeId.Invalid)
                return $"Avatar prototype '{avatarPath}' not found.";
            if (costumeRef == PrototypeId.Invalid)
                return $"Costume prototype '{costumePath}' not found.";

            var avatarProto = avatarRef.As<AvatarPrototype>();
            var costumeProto = costumeRef.As<CostumePrototype>();

            if (avatarProto == null)
                return $"'{avatarPath}' is not an AvatarPrototype.";
            if (costumeProto == null)
                return $"'{costumePath}' is not a CostumePrototype.";

            AssetId avatarUnreal = avatarProto.UnrealClass;
            AssetId costumeUnreal = costumeProto.CostumeUnrealClass;

            lines.Add($"  Render avatar: {GameDatabase.GetPrototypeName(avatarRef)}");
            lines.Add($"  Avatar UnrealClass: {GetAssetDisplayName(avatarUnreal)} [{GetAssetTypeName(avatarUnreal)}]");
            lines.Add($"  Costume: {GameDatabase.GetPrototypeName(costumeRef)}");
            lines.Add($"  Costume UnrealClass: {GetAssetDisplayName(costumeUnreal)} [{GetAssetTypeName(costumeUnreal)}]");
            lines.Add($"  Costume UsableBy: {GameDatabase.GetPrototypeName(costumeProto.UsableBy)}");
            lines.Add("");
            lines.Add("  TEST: ClientRenderPrototypeRef=avatar, CostumeCurrent=costume from DIFFERENT avatar.");
            lines.Add("  If the model shows the avatar's shape with the costume's material,");
            lines.Add("  then CostumeUnrealClass carries the material and can be cross-applied.");
            lines.Add("  If the model shows the costume's full model (not the avatar's),");
            lines.Add("  then CostumeUnrealClass IS the full model, not just material.");

            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyPath);
            if (combatBodyRef == PrototypeId.Invalid)
            {
                lines.Add($"  FAIL: Could not resolve combat body '{CombatBodyPath}'.");
                return string.Join("\n", lines);
            }

            var entity = SpawnTestEntity(avatar, "crossmat", combatBodyRef, avatarRef, costumeRef, scale);

            if (entity != null)
            {
                lines.Add($"  SPAWNED: id=0x{entity.Id:X}");
                lines.Add($"  IsClientRenderedAsAvatar: {entity.IsClientRenderedAsAvatar}");
                lines.Add($"  GetEntityWorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                lines.Add($"  Applied CostumeCurrent: {GameDatabase.GetPrototypeName(entity.Properties[PropertyEnum.CostumeCurrent])}");
                lines.Add("  => Check in-game: what model+material is displayed?");
            }
            else
                lines.Add("  FAIL: Spawn returned null.");

            LogAttempt("crossmat", $"avatar={avatarPath}, costume={costumePath}, scale={scale}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- item ------------------------------------------------------------

        [Command("item")]
        [CommandDescription("Spawns an item prototype as a decorative in-world model, replicating loot drop rendering.")]
        [CommandUsage("mat item <itemPath> [scale]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Item(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out var playerConnection, out var avatar);
            if (error != null) return error;

            string itemPath = @params[0];
            float scale = 1f;
            if (@params.Length > 1 && float.TryParse(@params[1], out float s)) scale = s;

            PrototypeId itemRef = GameDatabase.GetPrototypeRefByName(itemPath);
            if (itemRef == PrototypeId.Invalid)
                return $"Item prototype '{itemPath}' not found. Use !mat search <pattern> to find items.";

            var itemProto = itemRef.As<ItemPrototype>();
            if (itemProto == null)
                return $"'{itemPath}' is not an ItemPrototype.";

            var lines = new List<string> { $"=== Item Model Spawn ===" };
            lines.Add($"  Item: {GameDatabase.GetPrototypeName(itemRef)}");
            lines.Add($"  UnrealClass: {GetAssetDisplayName(itemProto.UnrealClass)} [{GetAssetTypeName(itemProto.UnrealClass)}]");

            // Spawn using EntitySettings directly (like LootManager.SpawnItemInternal does)
            var region = avatar.Region;
            if (region == null) return "Avatar is not in a region.";

            var entityProto = itemRef.As<WorldEntityPrototype>();
            Vector3 spawnPos = FindValidSpawnPosition(region, avatar, entityProto);
            if (spawnPos == Vector3.Zero)
                return $"Could not find valid spawn position within {MaxSpawnDistance} units.";
            spawnPos = RegionLocation.ProjectToFloor(region, spawnPos);

            using EntitySettings settings = ObjectPoolManager.Instance.Get<EntitySettings>();
            settings.EntityRef = itemRef;
            settings.RegionId = region.Id;
            settings.Position = spawnPos;
            settings.BoundsScaleOverride = scale;
            settings.ItemSpec = avatar.Game.LootManager.CreateItemSpec(itemRef, LootContext.CashShop, null);

            var entity = avatar.Game.EntityManager.CreateEntity(settings) as Item;

            if (entity != null)
            {
                _spawnedEntityIds.Add(entity.Id);
                lines.Add($"  SPAWNED: id=0x{entity.Id:X}");
                lines.Add($"  GetEntityWorldAsset: {GetAssetDisplayName(entity.GetEntityWorldAsset())}");
                lines.Add($"  GetOriginalWorldAsset: {GetAssetDisplayName(entity.GetOriginalWorldAsset())}");
                lines.Add($"  Position: {spawnPos.ToStringNames()}");
                lines.Add("  => Check in-game: the item's 3D model should be visible on the ground.");
                lines.Add("  This replicates how loot drops render. The UnrealClass drives the visual.");
                LogAttempt("item", $"Spawned item {itemPath}, id=0x{entity.Id:X}, UnrealClass={GetAssetDisplayName(itemProto.UnrealClass)}");
            }
            else
            {
                lines.Add("  FAIL: CreateEntity returned null.");
                LogAttempt("item", $"FAIL: CreateEntity returned null for {itemPath}");
            }

            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- costumes --------------------------------------------------------

        [Command("costumes")]
        [CommandDescription("Lists all costumes for an avatar, showing CostumeUnrealClass assets for material analysis.")]
        [CommandUsage("mat costumes <avatarPath>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Costumes(string[] @params, NetClient client)
        {
            string avatarPath = @params[0];
            PrototypeId avatarRef = GameDatabase.GetPrototypeRefByName(avatarPath);
            if (avatarRef == PrototypeId.Invalid)
                return $"Avatar prototype '{avatarPath}' not found.";

            var avatarProto = avatarRef.As<AvatarPrototype>();
            if (avatarProto == null)
                return $"'{avatarPath}' is not an AvatarPrototype.";

            var lines = new List<string> { $"=== Costumes for {GameDatabase.GetPrototypeName(avatarRef)} ===" };
            lines.Add($"  Avatar UnrealClass: {GetAssetDisplayName(avatarProto.UnrealClass)} [{GetAssetTypeName(avatarProto.UnrealClass)}]");

            // Get starting costume
            PrototypeId startingCostumeRef = avatarProto.GetStartingCostumeForPlatform(Platforms.PC);
            if (startingCostumeRef != PrototypeId.Invalid)
            {
                var startingCostume = startingCostumeRef.As<CostumePrototype>();
                if (startingCostume != null)
                {
                    lines.Add($"  Starting Costume: {GameDatabase.GetPrototypeName(startingCostumeRef)}");
                    lines.Add($"    CostumeUnrealClass: {GetAssetDisplayName(startingCostume.CostumeUnrealClass)} [{GetAssetTypeName(startingCostume.CostumeUnrealClass)}]");
                }
            }

            // Search for all costumes usable by this avatar
            string avatarName = GameDatabase.GetPrototypeName(avatarRef);
            string shortName = avatarName.Split('/').Last().Replace(".prototype", "");
            var costumeMatches = GameDatabase.SearchPrototypes($"Costumes/Prototypes/{shortName}",
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (costumeMatches.Count == 0)
            {
                // Broader search
                costumeMatches = GameDatabase.SearchPrototypes(shortName,
                    DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();
            }

            int costumeCount = 0;
            foreach (var costumeRef in costumeMatches)
            {
                var costumeProto = costumeRef.As<CostumePrototype>();
                if (costumeProto == null) continue;
                if (costumeProto.UsableBy != avatarRef) continue;

                costumeCount++;
                AssetId unreal = costumeProto.CostumeUnrealClass;
                lines.Add($"  {GameDatabase.GetPrototypeName(costumeRef)}");
                lines.Add($"    CostumeUnrealClass: {GetAssetDisplayName(unreal)} [{GetAssetTypeName(unreal)}]");
            }

            if (costumeCount == 0)
                lines.Add("  No costumes found for this avatar.");

            lines.Add($"\n  Total costumes: {costumeCount}");
            lines.Add("  Use !mat crossmat <avatarPath> <costumePath> to test cross-material.");
            LogAttempt("costumes", $"Avatar={avatarPath}, found {costumeCount} costumes");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- applycostume ----------------------------------------------------

        [Command("applycostume")]
        [CommandDescription("Applies a costume to an existing entity (post-spawn test). Expected to fail visually — documents why.")]
        [CommandUsage("mat applycostume <entityId> <costumePath>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string ApplyCostume(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            string costumePath = @params[1];
            PrototypeId costumeRef = GameDatabase.GetPrototypeRefByName(costumePath);
            if (costumeRef == PrototypeId.Invalid)
                return $"Costume prototype '{costumePath}' not found.";

            var costumeProto = costumeRef.As<CostumePrototype>();
            if (costumeProto == null)
                return $"'{costumePath}' is not a CostumePrototype.";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            var lines = new List<string> { $"=== Apply Costume (Post-Spawn) ===" };
            lines.Add($"  Entity: 0x{entity.Id:X} ({GameDatabase.GetPrototypeName(entity.PrototypeDataRef)})");
            lines.Add($"  Costume: {GameDatabase.GetPrototypeName(costumeRef)}");
            lines.Add($"  CostumeUnrealClass: {GetAssetDisplayName(costumeProto.CostumeUnrealClass)} [{GetAssetTypeName(costumeProto.CostumeUnrealClass)}]");

            // Record before state
            AssetId beforeAsset = entity.GetEntityWorldAsset();
            PrototypeId beforeCostume = entity.Properties[PropertyEnum.CostumeCurrent];
            lines.Add($"  BEFORE: WorldAsset={GetAssetDisplayName(beforeAsset)}, CostumeCurrent={GameDatabase.GetPrototypeName(beforeCostume)}");

            // Apply costume
            entity.Properties[PropertyEnum.CostumeCurrent] = costumeRef;

            // Record after state
            AssetId afterAsset = entity.GetEntityWorldAsset();
            PrototypeId afterCostume = entity.Properties[PropertyEnum.CostumeCurrent];
            lines.Add($"  AFTER:  WorldAsset={GetAssetDisplayName(afterAsset)}, CostumeCurrent={GameDatabase.GetPrototypeName(afterCostume)}");
            lines.Add($"  Asset changed: {beforeAsset != afterAsset}");
            lines.Add("");
            lines.Add("  NOTE: Post-spawn property changes are replicated via NetMessageSetProperty,");
            lines.Add("  but the client does NOT re-build visual pawns on live property updates.");
            lines.Add("  The server sees the new CostumeCurrent, but the client likely shows the old model.");
            lines.Add("  This confirms overrides MUST be set on SpawnSpec BEFORE Spawn().");

            LogAttempt("applycostume", $"Entity=0x{entityId:X}, costume={costumePath}, assetChanged={beforeAsset != afterAsset}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

    }
}
