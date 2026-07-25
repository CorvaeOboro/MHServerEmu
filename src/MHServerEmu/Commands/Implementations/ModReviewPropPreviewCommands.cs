// =============================================================================
//  MOD ReviewPropPreview - commands
// =============================================================================
//  Feature:    Dev tool for exploring and previewing game props, doodads,
//              and decorative entities in-world. The "single prop explorer"
//              counterpart to ReviewDecoPrefab's "multi-prop arranger".
//
//  Commands:   !prop search <pattern>       = search prop prototypes by name
//              !prop spawn <path|cat> [sc]  = spawn a prop (by path or random from category)
//              !prop near [radius]          = list nearby entities with prop names
//              !prop info [entityId]        = show prop rendering info
//              !prop list [category]        = list props by category
//              !prop cleanup                = destroy all spawned props
//              !prop rotate <id> <degrees>  = rotate a spawned prop
//              !prop scale <id> <factor>    = delete + respawn at new scale
//
//  Logging:    Always on when ReviewPropPreviewLoggingEnable=true in Config.ini.
//              Full prototype lists from search/list/near are collated to
//              Data/ReviewPropPreviewLog.txt for later analysis.
//
//  Categories: destructible, decoration, prop, transition, hotspot,
//              spawner, kismet, agent, item, worldentity
// =============================================================================

using System.Text;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Collisions;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Network;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Common;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Navi;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("prop")]
    [CommandGroupDescription("Dev tool for exploring and previewing game props. Subcommands: search, spawn, near, info, list, cleanup, rotate, scale.")]
    [CommandGroupUserLevel(AccountUserLevel.Admin)]
    public class ModReviewPropPreviewCommands : CommandGroup
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static bool FileLoggingEnabled => ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>().ReviewPropPreviewLoggingEnable;

        // --- Tracked spawned entities ---------------------------------------

        private class SpawnedPropInfo
        {
            public ulong EntityId;
            public PrototypeId ProtoRef;
            public string PrototypePath;
            public Vector3 Position;
            public float Scale;
            public Orientation Orientation;
        }

        private static readonly List<SpawnedPropInfo> _spawnedProps = new();

        // --- Category definitions -------------------------------------------

        private record PropCategory(string Name, string SearchPattern, string Description);

        private static readonly PropCategory[] _categories =
        {
            new("destructible", "Destructible", "Destructible props (barrels, crates, breakable objects)"),
            new("decoration",   "Decoration",   "Decorative entities (banners, statues, ornaments)"),
            new("prop",         "Prop",         "General props"),
            new("transition",   "Transition",   "Region transitions (portals, doors)"),
            new("hotspot",      "Hotspot",      "Hotspot entities (area triggers)"),
            new("spawner",      "Spawner",      "Spawner entities"),
            new("kismet",       "Kismet",       "Kismet sequence entities (cutscene objects)"),
            new("agent",        "Agent",        "Agent entities (NPCs, mobs)"),
            new("item",         "Item",         "Item entities"),
            new("worldentity",  "WorldEntity",  "All world entities (broad)"),
        };

        // Broad default search patterns for prop discovery
        private static readonly string[] _defaultSearchPatterns =
        {
            "Barrel", "Chest", "Crystal", "Pyramid", "Obelisk", "Beacon",
            "Destructible", "Door", "Switch", "Statue", "Pillar", "Terminal",
            "Doodad", "Prop", "Decoration", "Banner", "Rune", "Stone", "Gem", "Orb",
            "Altar", "Crate", "Lever", "Button", "Column", "Generator",
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
                return "Your avatar must be in the world to use prop preview commands.";
            return null;
        }

        private static void LogProp(string category, string message)
        {
            string line = $"[ReviewPropPreview] [{category}] {message}";
            Logger.Info(line);
            if (FileLoggingEnabled)
                System.IO.File.AppendAllText("Data/ReviewPropPreviewLog.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {line}\n");
        }

        private static void LogPropFullList(string category, string header, IEnumerable<string> lines)
        {
            if (FileLoggingEnabled == false) return;
            var sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ReviewPropPreview] [{category}] {header}");
            foreach (string line in lines)
                sb.AppendLine($"  {line}");
            System.IO.File.AppendAllText("Data/ReviewPropPreviewLog.txt", sb.ToString());
        }

        // --- Spawn pipeline --------------------------------------------------

        private const float SpawnSearchRadius = 128f;
        private const float MaxSpawnDistance = 1500f;

        /// <summary>
        /// Validates or finds a navmesh-valid spawn position near the given target position.
        /// If the target is blocked, searches outward in rings up to MaxSpawnDistance.
        /// </summary>
        private static Vector3 ValidateSpawnPosition(Region region, Vector3 position, WorldEntityPrototype entityProto)
        {
            PathFlags pathFlags = Region.GetPathFlagsForEntity(entityProto);
            var posFlags = PositionCheckFlags.CanBeBlockedEntity | PositionCheckFlags.CanPathTo;
            var blockFlags = BlockingCheckFlags.CheckSpawns;

            Bounds bounds = new(entityProto.Bounds, position);
            if (region.IsLocationClear(ref bounds, pathFlags, posFlags, blockFlags))
                return bounds.Center;

            float minDistance;
            float maxDistance = 0.0f;
            bool spawnFound = false;
            Vector3 spawnPosition = position;

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

        private static WorldEntity SpawnProp(Avatar avatar, PrototypeId protoRef, Vector3 position, float scale = 1f, Orientation orientation = default)
        {
            var region = avatar.Region;
            if (region == null)
            {
                LogProp("spawn", "FAIL: avatar.Region is null");
                return null;
            }

            var popManager = region.PopulationManager;
            if (popManager == null)
            {
                LogProp("spawn", "FAIL: region.PopulationManager is null");
                return null;
            }

            // Validate spawn position on navmesh
            var entityProto = protoRef.As<WorldEntityPrototype>();
            Vector3 spawnPos = entityProto != null
                ? ValidateSpawnPosition(region, position, entityProto)
                : position;
            spawnPos = RegionLocation.ProjectToFloor(region, spawnPos);

            var group = popManager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPos, orientation);

            var spec = popManager.CreateSpawnSpec(group);
            spec.EntityRef = protoRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;
            spec.BoundsScaleOverride = scale;

            int level = avatar.CharacterLevel;
            spec.Properties[PropertyEnum.CharacterLevel] = level;
            spec.Properties[PropertyEnum.CombatLevel] = level;
            spec.Properties[PropertyEnum.VariationSeed] = avatar.Game.Random.Next(1, 10000);

            spec.Spawn();

            var entity = spec.ActiveEntity;
            if (entity == null)
            {
                popManager.RemoveSpawnGroup(group.Id);
                LogProp("spawn", $"FAIL: Spawn() returned null for {GameDatabase.GetPrototypeName(protoRef)}");
                return null;
            }

            _spawnedProps.Add(new SpawnedPropInfo
            {
                EntityId = entity.Id,
                ProtoRef = protoRef,
                PrototypePath = GameDatabase.GetPrototypeName(protoRef),
                Position = spawnPos,
                Scale = scale,
                Orientation = orientation,
            });

            LogProp("spawn", $"Spawned id=0x{entity.Id:X}, proto={GameDatabase.GetPrototypeName(protoRef)}, " +
                $"pos={spawnPos.ToStringNames()}, scale={scale}, orient={orientation}");

            return entity;
        }

        // --- Cleanup --------------------------------------------------------

        private static int CleanupAll(Game game)
        {
            int count = 0;
            foreach (var info in _spawnedProps)
            {
                var entity = game.EntityManager.GetEntity<WorldEntity>(info.EntityId);
                if (entity != null)
                {
                    entity.ScheduleDestroyEvent(TimeSpan.Zero);
                    count++;
                }
            }
            _spawnedProps.Clear();
            return count;
        }

        // --- Prototype classification ---------------------------------------

        private static string GetPrototypeCategory(PrototypeId protoRef)
        {
            if (protoRef.As<DestructiblePropPrototype>() != null) return "destructible";
            if (protoRef.As<PropPrototype>() != null) return "prop";
            if (protoRef.As<TransitionPrototype>() != null) return "transition";
            if (protoRef.As<HotspotPrototype>() != null) return "hotspot";
            if (protoRef.As<SpawnerPrototype>() != null) return "spawner";
            if (protoRef.As<KismetSequenceEntityPrototype>() != null) return "kismet";
            if (protoRef.As<AgentPrototype>() != null) return "agent";
            if (protoRef.As<ItemPrototype>() != null) return "item";
            if (protoRef.As<WorldEntityPrototype>() != null) return "worldentity";
            return "unknown";
        }

        // --- Default command ------------------------------------------------

        [DefaultCommand]
        [CommandDescription("Shows available prop preview commands.")]
        [CommandUsage("prop [search|spawn|near|info|list|cleanup|rotate|scale] ...")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public override string Fallback(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("ReviewPropPreview commands:\n");
            sb.Append("  !prop search <pattern>     - Search prop prototypes by name\n");
            sb.Append("  !prop spawn <path|cat> [s] - Spawn a prop by path or random from category\n");
            sb.Append("  !prop near [radius]        - List nearby entities with prop names & IDs\n");
            sb.Append("  !prop info [entityId]      - Show prop rendering info (UnrealClass, bounds, states)\n");
            sb.Append("  !prop list [category]      - List props by category (destructible, decoration, prop, etc.)\n");
            sb.Append("  !prop cleanup              - Destroy all spawned props\n");
            sb.Append("  !prop rotate <id> <deg>    - Rotate a spawned prop\n");
            sb.Append("  !prop scale <id> <factor>  - Delete + respawn at new scale\n");
            sb.Append($"\nSpawned props: {_spawnedProps.Count}");
            sb.Append($"\nFile logging: {(FileLoggingEnabled ? "ON" : "OFF")} (Config.ini: ReviewPropPreviewLoggingEnable)");
            return sb.ToString();
        }

        // --- search ---------------------------------------------------------

        [Command("search")]
        [CommandDescription("Searches prop prototypes by name pattern. Shows prototype path, UnrealClass, and category.")]
        [CommandUsage("prop search <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Search(string[] @params, NetClient client)
        {
            string pattern = @params[0];
            var matches = GameDatabase.SearchPrototypes(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No prototypes found matching '{pattern}'.";

            const int MaxResults = 30;
            var lines = new List<string> { $"Prototype search '{pattern}' ({matches.Count} matches, showing {Math.Min(matches.Count, MaxResults)}):" };

            // Build full list for logging (not truncated)
            var fullLogLines = new List<string>();

            foreach (var protoRef in matches.Take(MaxResults))
            {
                string name = GameDatabase.GetPrototypeName(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                string category = GetPrototypeCategory(protoRef);
                lines.Add($"  [{category}] {name} -> {unrealName}");
            }

            // Full list for file log
            foreach (var protoRef in matches)
            {
                string name = GameDatabase.GetPrototypeName(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                string category = GetPrototypeCategory(protoRef);
                fullLogLines.Add($"[{category}] {name} -> {unrealName}");
            }

            if (matches.Count > MaxResults)
                lines.Add($"  ... and {matches.Count - MaxResults} more (see log for full list)");

            lines.Add("  Use !prop spawn <path> to spawn any of these.");
            LogProp("search", $"Pattern='{pattern}', matches={matches.Count}");
            LogPropFullList("search", $"Full results for '{pattern}' ({matches.Count} matches):", fullLogLines);
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- spawn ----------------------------------------------------------

        [Command("spawn")]
        [CommandDescription("Spawns a prop in front of the avatar. Accepts a prototype path or a category name for random spawn.")]
        [CommandUsage("prop spawn <protoPath|category> [scale]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Spawn(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string arg = @params[0];
            float scale = 1f;
            if (@params.Length > 1 && float.TryParse(@params[1], out float s)) scale = s;

            // First try to resolve as a prototype path
            PrototypeId protoRef = GameDatabase.GetPrototypeRefByName(arg);
            string spawnSource = $"path={arg}";

            // If not found, try to resolve as a category name for random spawn
            if (protoRef == PrototypeId.Invalid)
            {
                string categoryName = arg.ToLowerInvariant();
                var catDef = Array.Find(_categories, c => c.Name == categoryName);
                if (catDef != null)
                {
                    var matches = GameDatabase.SearchPrototypes(catDef.SearchPattern,
                        DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

                    var filtered = matches.Where(m => GetPrototypeCategory(m) == categoryName).ToList();
                    if (filtered.Count == 0)
                        return $"No prototypes found in category '{categoryName}'.";

                    protoRef = filtered[avatar.Game.Random.Next(filtered.Count)];
                    spawnSource = $"category={categoryName} (random pick from {filtered.Count})";
                    LogProp("spawn", $"Random category spawn: category='{categoryName}', picked={GameDatabase.GetPrototypeName(protoRef)}, pool={filtered.Count}");
                }
            }

            if (protoRef == PrototypeId.Invalid)
                return $"Prototype '{arg}' not found and not a valid category. Use !prop search <pattern> or !prop list <category>.";

            Vector3 pos = avatar.RegionLocation.Position + avatar.Forward * 300f;
            var entity = SpawnProp(avatar, protoRef, pos, scale);

            if (entity != null)
            {
                AssetId worldAsset = entity.GetEntityWorldAsset();
                return $"Spawned: id=0x{entity.Id:X}, proto={GameDatabase.GetPrototypeName(protoRef)}, {spawnSource}, scale={scale}, " +
                       $"UnrealClass={GetAssetDisplayName(worldAsset)}";
            }

            return $"FAIL: Spawn returned null for {spawnSource}.";
        }

        // --- near ------------------------------------------------------------

        [Command("near")]
        [CommandDescription("Lists nearby entities with their entity IDs, prototype names, categories, and distances.")]
        [CommandUsage("prop near [radius]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Near(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            float radius = 1000f;
            if (@params.Length > 0 && float.TryParse(@params[0], out float r)) radius = r;

            var region = avatar.Region;
            if (region == null)
                return "Avatar is not in a region.";

            Vector3 avatarPos = avatar.RegionLocation.Position;
            var sphere = new Sphere(avatarPos, radius);

            var nearbyEntities = new List<(WorldEntity entity, float dist)>();
            foreach (var entity in region.IterateEntitiesInVolume(sphere, new(EntityRegionSPContextFlags.UnrestrictedPartitions)))
            {
                if (entity == avatar) continue;
                float dist = Vector3.Distance(avatarPos, entity.RegionLocation.Position);
                nearbyEntities.Add((entity, dist));
            }

            nearbyEntities.Sort((a, b) => a.dist.CompareTo(b.dist));

            const int MaxResults = 30;
            var lines = new List<string> { $"=== Nearby Entities (radius={radius:F0}, found={nearbyEntities.Count}) ===" };
            var fullLogLines = new List<string>();

            foreach (var (entity, dist) in nearbyEntities.Take(MaxResults))
            {
                var protoRef = entity.PrototypeDataRef;
                string protoName = GameDatabase.GetPrototypeName(protoRef);
                string category = GetPrototypeCategory(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                bool tracked = _spawnedProps.Any(i => i.EntityId == entity.Id);

                string line = $"  0x{entity.Id:X} [{category}] {protoName} -> {unrealName} (dist={dist:F0}){(tracked ? " *spawned" : "")}";
                lines.Add(line);
                fullLogLines.Add(line);
            }

            // Add all remaining to full log
            for (int i = MaxResults; i < nearbyEntities.Count; i++)
            {
                var entity = nearbyEntities[i].entity;
                var dist = nearbyEntities[i].dist;
                var protoRef = entity.PrototypeDataRef;
                string protoName = GameDatabase.GetPrototypeName(protoRef);
                string category = GetPrototypeCategory(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                bool tracked = _spawnedProps.Any(j => j.EntityId == entity.Id);
                fullLogLines.Add($"  0x{entity.Id:X} [{category}] {protoName} -> {unrealName} (dist={dist:F0}){(tracked ? " *spawned" : "")}");
            }

            if (nearbyEntities.Count > MaxResults)
                lines.Add($"  ... and {nearbyEntities.Count - MaxResults} more (see log for full list)");

            lines.Add("  Use !prop info <entityId> for details, !prop rotate <id> <deg> to rotate.");

            LogProp("near", $"Radius={radius:F0}, found={nearbyEntities.Count} entities near avatar at {avatarPos.ToStringNames()}");
            LogPropFullList("near", $"Full nearby entity list (radius={radius:F0}, {nearbyEntities.Count} entities):", fullLogLines);
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- info ------------------------------------------------------------

        [Command("info")]
        [CommandDescription("Shows rendering and bounds info for your avatar or a specified entity.")]
        [CommandUsage("prop info [entityId]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Info(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            WorldEntity target = avatar;
            if (@params.Length > 0 && ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
            {
                target = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
                if (target == null)
                    return $"Entity 0x{entityId:X} not found.";
            }

            var protoRef = target.PrototypeDataRef;
            var worldProto = protoRef.As<WorldEntityPrototype>();
            string category = GetPrototypeCategory(protoRef);

            var lines = new List<string> { $"=== Prop Info: {target} ===" };

            lines.Add($"  Id: 0x{target.Id:X}");
            lines.Add($"  Prototype: {GameDatabase.GetPrototypeName(protoRef)}");
            lines.Add($"  Category: {category}");
            lines.Add($"  Position: {target.RegionLocation.Position.ToStringNames()}");
            lines.Add($"  Orientation: {target.RegionLocation.Orientation}");

            // Rendering chain
            AssetId worldAsset = target.GetEntityWorldAsset();
            AssetId originalAsset = target.GetOriginalWorldAsset();
            lines.Add($"  WorldAsset: {GetAssetDisplayName(worldAsset)} [{GetAssetTypeName(worldAsset)}]");
            lines.Add($"  OriginalAsset: {GetAssetDisplayName(originalAsset)} [{GetAssetTypeName(originalAsset)}]");

            if (worldProto != null)
            {
                lines.Add($"  Prototype UnrealClass: {GetAssetDisplayName(worldProto.UnrealClass)} [{GetAssetTypeName(worldProto.UnrealClass)}]");
                lines.Add($"  MarvelModelRenderClass: {GetAssetDisplayName(worldProto.MarvelModelRenderClass)} [{GetAssetTypeName(worldProto.MarvelModelRenderClass)}]");
                lines.Add($"  SnapToFloorOnSpawn: {worldProto.SnapToFloorOnSpawn}");
                lines.Add($"  VisibleByDefault: {worldProto.VisibleByDefault}");
            }

            lines.Add($"  ClientPrototypeRefOverride: {(target.ClientPrototypeRefOverride != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(target.ClientPrototypeRefOverride) : "(none)")}");
            lines.Add($"  IsClientRenderedAsAvatar: {target.IsClientRenderedAsAvatar}");
            lines.Add($"  IsInWorld: {target.IsInWorld}");

            // Bounds info
            if (worldProto?.Bounds != null)
            {
                lines.Add($"  Bounds: type={worldProto.Bounds.GetType().Name}");
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
            else
            {
                lines.Add("  EntityState: (none)");
            }

            // Keywords
            if (worldProto?.Keywords != null && worldProto.Keywords.Length > 0)
            {
                var keywordNames = new List<string>();
                foreach (var kwRef in worldProto.Keywords)
                    keywordNames.Add(GameDatabase.GetPrototypeName(kwRef));
                lines.Add($"  Keywords: {string.Join(", ", keywordNames)}");
            }

            // Check if tracked by us
            var info = _spawnedProps.FirstOrDefault(i => i.EntityId == target.Id);
            if (info != null)
            {
                lines.Add($"  Tracked: yes, scale={info.Scale}, spawned at {info.Position.ToStringNames()}");
            }
            else
            {
                lines.Add("  Tracked: no (not spawned by !prop)");
            }

            LogProp("info", $"Entity=0x{target.Id:X}, proto={GameDatabase.GetPrototypeName(protoRef)}, category={category}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- list ------------------------------------------------------------

        [Command("list")]
        [CommandDescription("Lists props by category. Without argument, shows all categories with counts. With category, shows prototypes.")]
        [CommandUsage("prop list [category]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            // No category: show category overview with broad search
            if (@params.Length == 0)
            {
                var overviewLines = new List<string> { "=== Prop Categories ===" };
                overviewLines.Add("  Categories: destructible, decoration, prop, transition, hotspot, spawner, kismet, agent, item, worldentity");
                overviewLines.Add("  Use !prop list <category> to search for prototypes in that category.");
                overviewLines.Add("  Use !prop search <pattern> for custom pattern searches.");
                overviewLines.Add($"\n  Currently spawned props: {_spawnedProps.Count}");

                // Also list tracked props if any
                if (_spawnedProps.Count > 0)
                {
                    overviewLines.Add("\n  Spawned props:");
                    foreach (var info in _spawnedProps)
                        overviewLines.Add($"    0x{info.EntityId:X} {info.PrototypePath} scale={info.Scale}");
                }

                CommandHelper.SendMessages(client, overviewLines);
                return string.Empty;
            }

            string category = @params[0].ToLowerInvariant();

            // Find the category definition
            var catDef = Array.Find(_categories, c => c.Name == category);
            if (catDef == null)
                return $"Unknown category '{category}'. Available: destructible, decoration, prop, transition, hotspot, spawner, kismet, agent, item, worldentity";

            var matches = GameDatabase.SearchPrototypes(catDef.SearchPattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No prototypes found for category '{category}' (pattern '{catDef.SearchPattern}').";

            // Filter to only those that actually match the category type
            var filtered = matches.Where(m => GetPrototypeCategory(m) == category).ToList();

            const int MaxResults = 30;
            var catLines = new List<string> { $"Category '{category}' — {catDef.Description}" };
            catLines.Add($"  {filtered.Count} prototypes found (showing {Math.Min(filtered.Count, MaxResults)}):");

            var fullLogLines = new List<string>();

            foreach (var protoRef in filtered.Take(MaxResults))
            {
                string name = GameDatabase.GetPrototypeName(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                catLines.Add($"    {name} -> {unrealName}");
            }

            // Full list for file log
            foreach (var protoRef in filtered)
            {
                string name = GameDatabase.GetPrototypeName(protoRef);
                var worldProto = protoRef.As<WorldEntityPrototype>();
                AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                fullLogLines.Add($"{name} -> {unrealName}");
            }

            if (filtered.Count > MaxResults)
                catLines.Add($"  ... and {filtered.Count - MaxResults} more (see log for full list)");

            catLines.Add("  Use !prop spawn <path> to spawn any of these, or !prop spawn <category> for a random one.");
            LogProp("list", $"Category='{category}', matches={filtered.Count}");
            LogPropFullList("list", $"Full prototype list for category '{category}' ({filtered.Count} entries):", fullLogLines);
            CommandHelper.SendMessages(client, catLines);
            return string.Empty;
        }

        // --- cleanup ---------------------------------------------------------

        [Command("cleanup")]
        [CommandDescription("Destroys all props spawned by !prop spawn.")]
        [CommandUsage("prop cleanup")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Cleanup(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            int count = CleanupAll(avatar.Game);
            LogProp("cleanup", $"Destroyed {count} props");
            return $"Cleaned up {count} spawned props.";
        }

        // --- rotate ----------------------------------------------------------

        [Command("rotate")]
        [CommandDescription("Rotates a spawned prop to the specified angle in degrees (0-360).")]
        [CommandUsage("prop rotate <entityId> <degrees>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string Rotate(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            if (!float.TryParse(@params[1], out float degrees))
                return "Invalid degrees value. Usage: !prop rotate <id> <degrees>";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            float radians = degrees * (float)(Math.PI / 180.0);
            var orientation = new Orientation(radians, 0f, 0f);

            var result = entity.ChangeRegionPosition(null, orientation, ChangePositionFlags.ForceUpdate);

            // Update tracked info
            var info = _spawnedProps.FirstOrDefault(i => i.EntityId == entityId);
            if (info != null) info.Orientation = orientation;

            LogProp("rotate", $"Rotated 0x{entityId:X} to {degrees:F1}°, result={result}");
            return $"Rotated 0x{entityId:X} to {degrees:F1}°, result={result}";
        }

        // --- scale -----------------------------------------------------------

        [Command("scale")]
        [CommandDescription("Deletes and respawns a prop at a new scale (scale can't be changed live).")]
        [CommandUsage("prop scale <entityId> <factor>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string Scale(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            if (!float.TryParse(@params[1], out float factor))
                return "Invalid scale factor. Usage: !prop scale <id> <factor>";

            var info = _spawnedProps.FirstOrDefault(i => i.EntityId == entityId);
            if (info == null)
                return $"Entity 0x{entityId:X} is not a !prop-spawned entity. Scale only works on props spawned by !prop spawn.";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found (may already be destroyed).";

            // Remember position and orientation
            Vector3 pos = entity.RegionLocation.Position;
            Orientation orient = entity.RegionLocation.Orientation;

            // Destroy the old entity
            entity.ScheduleDestroyEvent(TimeSpan.Zero);
            _spawnedProps.Remove(info);

            // Respawn at new scale
            var newEntity = SpawnProp(avatar, info.ProtoRef, pos, factor, orient);

            if (newEntity != null)
            {
                LogProp("scale", $"Respawned 0x{entityId:X} as 0x{newEntity.Id:X} at scale {factor}");
                return $"Respawned: old=0x{entityId:X}, new=0x{newEntity.Id:X}, scale={factor}";
            }

            return $"FAIL: Could not respawn at scale {factor}. Old entity was destroyed.";
        }

    }
}
