// =============================================================================
//  MOD ReviewDecoPrefab - commands
// =============================================================================
//  Feature:    Dev tool for creating saved arrangements of props + VFX —
//              "deco prefabs" — such as a ring of floating infinity stones
//              each playing a looping VFX. Designed for designers and creatives
//              to explore spawnable props, VFX, animations, and powers, then
//              assemble them into reusable arrangements.
//
//  Commands:   !deco ring [radius] [count] [vfx]  = spawn ring of props+VFX
//              !deco spawn <path> [scale] [vfx]   = spawn single prop
//              !deco vfx <id> <assetName>          = play VFX on entity
//              !deco vfxloop <id> <asset> [sec]    = looping VFX on entity
//              !deco move <id> <x> <y> <z>         = move entity
//              !deco orbit <id> <radius> [speed]   = orbit entity
//              !deco cleanup                       = destroy all + stop loops
//              !deco save <name>                   = save arrangement to JSON
//              !deco load <name>                   = load arrangement from JSON
//              !deco list                          = list saved arrangements
//              !deco explore <type> [pattern]      = search props/vfx/powers/assets
//              !deco info [entityId]               = entity rendering + position
//
//  Logging:    Always on when ReviewDecoPrefabLoggingEnable=true in Config.ini.
//              Actions collated to Data/ReviewDecoPrefabLog.txt.
//
//  Infinity Stones: 6 gems (Mind, Power, Reality, Soul, Space, Time)
//  each with a corresponding VFX from PowerVisualsGlobalsPrototype.
// =============================================================================

using System.Text;
using Gazillion;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Network;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Common;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Navi;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("deco")]
    [CommandGroupDescription("Dev tool for deco prefabs: spawn props+VFX, save/load arrangements, explore assets.")]
    [CommandGroupUserLevel(AccountUserLevel.Admin)]
    public class ModReviewDecoPrefabCommands : CommandGroup
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static bool FileLoggingEnabled => ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>().ReviewDecoPrefabLoggingEnable;

        // --- Tracked spawned entities + loops --------------------------------

        private class SpawnedEntityInfo
        {
            public ulong EntityId;
            public string PrototypePath;
            public Vector3 Position;
            public float Scale;
            public string VfxAssetName;
            public float VfxLoopInterval;
            public string Label;
        }

        private static readonly List<SpawnedEntityInfo> _spawnedInfos = new();
        private static readonly Dictionary<ulong, EventPointer<VfxLoopEvent>> _vfxLoops = new();
        private static readonly Dictionary<ulong, EventPointer<OrbitEvent>> _orbits = new();

        // --- Infinity stone VFX mapping -------------------------------------

        private record InfinityStoneInfo(string Name, string VfxPropertyName, InfinityGem Gem);

        private static readonly InfinityStoneInfo[] _infinityStones =
        {
            new("Mind",    "InfinityMindPointEarnedClass",    InfinityGem.Mind),
            new("Power",   "InfinityPowerPointEarnedClass",   InfinityGem.Power),
            new("Reality", "InfinityRealityPointEarnedClass", InfinityGem.Reality),
            new("Soul",    "InfinitySoulPointEarnedClass",    InfinityGem.Soul),
            new("Space",   "InfinitySpacePointEarnedClass",   InfinityGem.Space),
            new("Time",    "InfinityTimePointEarnedClass",    InfinityGem.Time),
        };

        // Default prop to use for infinity stones (a small visible entity)
        private const string DefaultPropPath = "Entity/Props/Destructibles/Barrel.prototype";

        // --- Helpers --------------------------------------------------------

        private static string GetAssetDisplayName(AssetId assetId)
        {
            string name = GameDatabase.GetAssetName(assetId);
            return string.IsNullOrEmpty(name) ? assetId.ToString() : name;
        }

        private static string GetAssetTypeName(AssetId assetId)
        {
            var typeId = GameDatabase.DataDirectory.AssetDirectory.GetAssetTypeRef(assetId);
            return typeId == AssetTypeId.Invalid ? "Unknown" : GameDatabase.GetAssetTypeName(typeId);
        }

        private static string ValidateAvatar(NetClient client, out PlayerConnection playerConnection, out Avatar avatar)
        {
            playerConnection = (PlayerConnection)client;
            avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
                return "Your avatar must be in the world to use deco commands.";
            return null;
        }

        private static void LogDeco(string category, string message)
        {
            string line = $"[ReviewDecoPrefab] [{category}] {message}";
            Logger.Info(line);
            if (FileLoggingEnabled)
                System.IO.File.AppendAllText("Data/ReviewDecoPrefabLog.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {line}\n");
        }

        // --- VFX playback (works on any entity) ------------------------------

        private static void PlayVfxOnEntity(WorldEntity entity, AssetId assetId)
        {
            var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                .SetEntityId(entity.Id)
                .SetPowerAssetRef((ulong)assetId)
                .Build();

            entity.Game?.NetworkManager?.SendMessageToInterested(msg, entity, AOINetworkPolicyValues.AOIChannelProximity);
        }

        private static AssetId ResolveVfxAsset(string name)
        {
            // Try as a PowerVisualsGlobalsPrototype property name first
            var globalsProto = GameDatabase.PowerVisualsGlobalsPrototype;
            if (globalsProto != null)
            {
                var prop = typeof(PowerVisualsGlobalsPrototype).GetProperty(name,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop?.PropertyType == typeof(AssetId))
                    return (AssetId)prop.GetValue(globalsProto);
            }

            // Fall back to resolving as an arbitrary asset name
            return GameDatabase.StringRefManager.GetDataRefByName(name);
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

            // Search outward from the target position
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

        private static WorldEntity SpawnPropEntity(Avatar avatar, string label,
            PrototypeId entityRef, Vector3 position, float scale = 1f,
            string vfxAssetName = null, float vfxLoopInterval = 0f)
        {
            var region = avatar.Region;
            if (region == null)
            {
                LogDeco("spawn", "FAIL: avatar.Region is null");
                return null;
            }

            var popManager = region.PopulationManager;
            if (popManager == null)
            {
                LogDeco("spawn", "FAIL: region.PopulationManager is null");
                return null;
            }

            // Validate spawn position on navmesh
            var entityProto = entityRef.As<WorldEntityPrototype>();
            Vector3 spawnPos = entityProto != null
                ? ValidateSpawnPosition(region, position, entityProto)
                : position;
            spawnPos = RegionLocation.ProjectToFloor(region, spawnPos);

            var group = popManager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

            var spec = popManager.CreateSpawnSpec(group);
            spec.EntityRef = entityRef;
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
                LogDeco("spawn", $"FAIL: Spawn() returned null for {GameDatabase.GetPrototypeName(entityRef)}");
                return null;
            }

            // Track the spawned entity
            var info = new SpawnedEntityInfo
            {
                EntityId = entity.Id,
                PrototypePath = GameDatabase.GetPrototypeName(entityRef),
                Position = spawnPos,
                Scale = scale,
                Label = label,
            };

            // Start VFX loop if requested
            if (!string.IsNullOrEmpty(vfxAssetName))
            {
                info.VfxAssetName = vfxAssetName;
                info.VfxLoopInterval = vfxLoopInterval > 0 ? vfxLoopInterval : 3f;

                AssetId vfxAsset = ResolveVfxAsset(vfxAssetName);
                if (vfxAsset != AssetId.Invalid)
                {
                    PlayVfxOnEntity(entity, vfxAsset);
                    if (info.VfxLoopInterval > 0)
                        StartVfxLoop(entity, vfxAsset, TimeSpan.FromSeconds(info.VfxLoopInterval));
                }
            }

            _spawnedInfos.Add(info);

            LogDeco("spawn", $"Spawned id=0x{entity.Id:X}, proto={info.PrototypePath}, " +
                $"pos={spawnPos.ToStringNames()}, scale={scale}, vfx={vfxAssetName ?? "(none)"}");

            return entity;
        }

        // --- VFX looping via scheduled events -------------------------------

        // Store VFX loop data keyed by entity ID so the event callback can access it
        private class VfxLoopData
        {
            public ulong EntityId;
            public AssetId VfxAssetId;
            public double LoopIntervalSec;
        }

        private static readonly Dictionary<ulong, VfxLoopData> _vfxLoopData = new();

        private class VfxLoopEvent : CallMethodEvent<WorldEntity>
        {
            private static readonly CallbackDelegate _callback = PlayVfxLoopCallback;

            protected override CallbackDelegate GetCallback() => _callback;

            private static void PlayVfxLoopCallback(WorldEntity entity)
            {
                if (entity == null || entity.IsInWorld == false) return;
                if (_vfxLoopData.TryGetValue(entity.Id, out var data) == false) return;

                // Re-play the VFX
                PlayVfxOnEntity(entity, data.VfxAssetId);

                // Reschedule
                var scheduler = entity.Game?.GameEventScheduler;
                if (scheduler != null && _vfxLoops.TryGetValue(entity.Id, out var pointer))
                    scheduler.ScheduleEvent(pointer, TimeSpan.FromSeconds(data.LoopIntervalSec));
            }
        }

        private static void StartVfxLoop(WorldEntity entity, AssetId vfxAsset, TimeSpan interval)
        {
            // Cancel existing loop if any
            StopVfxLoop(entity.Id);

            var scheduler = entity.Game?.GameEventScheduler;
            if (scheduler == null) return;

            _vfxLoopData[entity.Id] = new VfxLoopData
            {
                EntityId = entity.Id,
                VfxAssetId = vfxAsset,
                LoopIntervalSec = interval.TotalSeconds,
            };

            var pointer = new EventPointer<VfxLoopEvent>();
            scheduler.ScheduleEvent(pointer, interval);
            pointer.Get().Initialize(entity);

            _vfxLoops[entity.Id] = pointer;
            LogDeco("vfxloop", $"Started VFX loop on 0x{entity.Id:X}, asset={GetAssetDisplayName(vfxAsset)}, interval={interval.TotalSeconds:F1}s");
        }

        private static void StopVfxLoop(ulong entityId)
        {
            _vfxLoops.Remove(entityId);
            _vfxLoopData.Remove(entityId);
        }

        // --- Orbit motion via scheduled events ------------------------------

        // Store orbit data keyed by entity ID
        private class OrbitData
        {
            public ulong EntityId;
            public Vector3 Center;
            public float Radius;
            public double AngularSpeed; // radians per second
            public double CurrentAngle;
        }

        private static readonly Dictionary<ulong, OrbitData> _orbitData = new();

        private class OrbitEvent : CallMethodEvent<WorldEntity>
        {
            private static readonly CallbackDelegate _callback = OrbitTickCallback;

            protected override CallbackDelegate GetCallback() => _callback;

            private static void OrbitTickCallback(WorldEntity entity)
            {
                if (entity == null || entity.IsInWorld == false) return;
                if (_orbitData.TryGetValue(entity.Id, out var data) == false) return;

                data.CurrentAngle += data.AngularSpeed * 0.1; // 100ms tick
                float x = data.Center.X + data.Radius * (float)Math.Cos(data.CurrentAngle);
                float y = data.Center.Y + data.Radius * (float)Math.Sin(data.CurrentAngle);
                float z = data.Center.Z;

                entity.ChangeRegionPosition(new Vector3(x, y, z), null, ChangePositionFlags.None);

                // Reschedule
                var scheduler = entity.Game?.GameEventScheduler;
                if (scheduler != null && _orbits.TryGetValue(entity.Id, out var pointer))
                    scheduler.ScheduleEvent(pointer, TimeSpan.FromMilliseconds(100));
            }
        }

        private static void StartOrbit(WorldEntity entity, float radius, double secondsPerRevolution)
        {
            StopOrbit(entity.Id);

            var scheduler = entity.Game?.GameEventScheduler;
            if (scheduler == null) return;

            _orbitData[entity.Id] = new OrbitData
            {
                EntityId = entity.Id,
                Center = entity.RegionLocation.Position,
                Radius = radius,
                AngularSpeed = (2.0 * Math.PI) / secondsPerRevolution,
                CurrentAngle = 0,
            };

            var pointer = new EventPointer<OrbitEvent>();
            scheduler.ScheduleEvent(pointer, TimeSpan.FromMilliseconds(100));
            pointer.Get().Initialize(entity);

            _orbits[entity.Id] = pointer;
            LogDeco("orbit", $"Started orbit on 0x{entity.Id:X}, radius={radius}, speed={secondsPerRevolution:F1}s/rev");
        }

        private static void StopOrbit(ulong entityId)
        {
            _orbits.Remove(entityId);
            _orbitData.Remove(entityId);
        }

        // --- Cleanup --------------------------------------------------------

        private static int CleanupAll(Game game)
        {
            // Stop all loops and orbits
            _vfxLoops.Clear();
            _vfxLoopData.Clear();
            _orbits.Clear();
            _orbitData.Clear();

            // Destroy all spawned entities
            int count = 0;
            foreach (var info in _spawnedInfos)
            {
                var entity = game.EntityManager.GetEntity<WorldEntity>(info.EntityId);
                if (entity != null)
                {
                    entity.ScheduleDestroyEvent(TimeSpan.Zero);
                    count++;
                }
            }
            _spawnedInfos.Clear();
            return count;
        }

        // --- Default command ------------------------------------------------

        [DefaultCommand]
        [CommandDescription("Shows available deco prefab commands.")]
        [CommandUsage("deco [ring|spawn|vfx|vfxloop|move|orbit|cleanup|save|load|list|explore|info] ...")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public override string Fallback(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("ReviewDecoPrefab commands:\n");
            sb.Append("  !deco ring [radius] [count] [vfx]  - Spawn ring of props with looping VFX (default: infinity stones)\n");
            sb.Append("  !deco spawn <path> [scale] [vfx]   - Spawn a single prop with optional VFX\n");
            sb.Append("  !deco vfx <entityId> <assetName>   - Play a VFX on a spawned entity\n");
            sb.Append("  !deco vfxloop <id> <asset> [sec]    - Loop a VFX on entity (default interval: 3s)\n");
            sb.Append("  !deco move <id> <x> <y> <z>         - Move entity to coordinates\n");
            sb.Append("  !deco orbit <id> <radius> [speed]   - Orbit entity (speed in sec/rev, default 10)\n");
            sb.Append("  !deco cleanup                       - Destroy all deco entities + stop loops\n");
            sb.Append("  !deco save <name>                   - Save current arrangement to JSON\n");
            sb.Append("  !deco load <name>                   - Load a saved arrangement\n");
            sb.Append("  !deco list                          - List saved arrangements\n");
            sb.Append("  !deco explore <type> [pattern]      - Search: props, vfx, powers, assets, animations\n");
            sb.Append("  !deco info [entityId]               - Show entity rendering + position info\n");
            sb.Append($"\nSpawned entities: {_spawnedInfos.Count} | Active VFX loops: {_vfxLoops.Count} | Orbits: {_orbits.Count}");
            return sb.ToString();
        }

        // --- ring ------------------------------------------------------------

        [Command("ring")]
        [CommandDescription("Spawns a ring of props with looping VFX. Default: 6 infinity stones with matching VFX.")]
        [CommandUsage("deco ring [radius] [count] [vfxAssetName]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Ring(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            float radius = 300f;
            int count = 6;
            string vfxOverride = null;

            if (@params.Length > 0 && float.TryParse(@params[0], out float r)) radius = r;
            if (@params.Length > 1 && int.TryParse(@params[1], out int c)) count = Math.Clamp(c, 1, 20);
            if (@params.Length > 2) vfxOverride = @params[2];

            // Resolve prop prototype
            PrototypeId propRef = GameDatabase.GetPrototypeRefByName(DefaultPropPath);
            if (propRef == PrototypeId.Invalid)
            {
                // Fallback: search for any prop
                var matches = GameDatabase.SearchPrototypes("Barrel",
                    DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();
                if (matches.Count > 0)
                    propRef = matches[0];
                else
                    return $"Could not resolve default prop '{DefaultPropPath}'. Use !deco spawn <path> instead.";
            }

            Vector3 center = avatar.RegionLocation.Position + avatar.Forward * (radius + 200f);
            var lines = new List<string> { $"=== Spawning Ring: radius={radius}, count={count} ===" };

            for (int i = 0; i < count; i++)
            {
                double angle = (2.0 * Math.PI * i) / count;
                float x = center.X + radius * (float)Math.Cos(angle);
                float y = center.Y + radius * (float)Math.Sin(angle);
                float z = center.Z + 100f; // float slightly above ground

                // Determine VFX: use infinity stone mapping for count=6, or override
                string vfxName = null;
                string label = $"Stone_{i}";

                if (vfxOverride != null)
                {
                    vfxName = vfxOverride;
                    label = $"Ring_{i}";
                }
                else if (count == 6)
                {
                    var stone = _infinityStones[i];
                    vfxName = stone.VfxPropertyName;
                    label = stone.Name;
                }
                else
                {
                    // Cycle through infinity stone VFX
                    var stone = _infinityStones[i % _infinityStones.Length];
                    vfxName = stone.VfxPropertyName;
                    label = stone.Name;
                }

                var entity = SpawnPropEntity(avatar, label, propRef, new Vector3(x, y, z), 1.5f, vfxName, 3f);

                if (entity != null)
                    lines.Add($"  [{i}] {label}: id=0x{entity.Id:X}, vfx={vfxName}, pos=({x:F0},{y:F0},{z:F0})");
                else
                    lines.Add($"  [{i}] {label}: FAIL");
            }

            lines.Add($"  Total spawned: {_spawnedInfos.Count}. Use !deco cleanup to remove.");
            LogDeco("ring", $"Spawned ring of {count} at radius {radius}, center={center.ToStringNames()}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- spawn -----------------------------------------------------------

        [Command("spawn")]
        [CommandDescription("Spawns a single prop with optional scale and VFX.")]
        [CommandUsage("deco spawn <protoPath> [scale] [vfxAssetName]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Spawn(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string protoPath = @params[0];
            float scale = 1f;
            string vfxName = null;

            // Parse scale and vfx from remaining params
            for (int i = 1; i < @params.Length; i++)
            {
                if (float.TryParse(@params[i], out float s)) scale = s;
                else vfxName = @params[i];
            }

            PrototypeId protoRef = GameDatabase.GetPrototypeRefByName(protoPath);
            if (protoRef == PrototypeId.Invalid)
                return $"Prototype '{protoPath}' not found. Use !deco explore props <pattern> to search.";

            Vector3 pos = avatar.RegionLocation.Position + avatar.Forward * 300f;

            var entity = SpawnPropEntity(avatar, "spawn", protoRef, pos, scale, vfxName, 3f);

            if (entity != null)
            {
                LogDeco("spawn", $"Single spawn: id=0x{entity.Id:X}, proto={protoPath}, scale={scale}, vfx={vfxName ?? "(none)"}");
                return $"Spawned: id=0x{entity.Id:X}, proto={protoPath}, scale={scale}, vfx={vfxName ?? "(none)"}";
            }

            return "FAIL: Spawn returned null.";
        }

        // --- vfx -------------------------------------------------------------

        [Command("vfx")]
        [CommandDescription("Plays a VFX on a spawned entity by entity ID and asset name.")]
        [CommandUsage("deco vfx <entityId> <assetName>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string Vfx(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            string assetName = @params[1];
            AssetId assetId = ResolveVfxAsset(assetName);
            if (assetId == AssetId.Invalid)
                return $"VFX asset '{assetName}' not found. Use !deco explore vfx <pattern> to search.";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            PlayVfxOnEntity(entity, assetId);
            LogDeco("vfx", $"Played {assetName} on 0x{entityId:X}");
            return $"Playing VFX: {assetName} -> {GetAssetDisplayName(assetId)} on entity 0x{entityId:X}";
        }

        // --- vfxloop ---------------------------------------------------------

        [Command("vfxloop")]
        [CommandDescription("Starts a looping VFX on an entity. Replays at given interval (default 3 seconds).")]
        [CommandUsage("deco vfxloop <entityId> <assetName> [intervalSec]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string VfxLoop(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            string assetName = @params[1];
            float interval = 3f;
            if (@params.Length > 2 && float.TryParse(@params[2], out float iv)) interval = iv;

            AssetId assetId = ResolveVfxAsset(assetName);
            if (assetId == AssetId.Invalid)
                return $"VFX asset '{assetName}' not found.";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            PlayVfxOnEntity(entity, assetId);
            StartVfxLoop(entity, assetId, TimeSpan.FromSeconds(interval));

            return $"VFX loop started: {assetName} on 0x{entityId:X}, interval={interval:F1}s";
        }

        // --- move ------------------------------------------------------------

        [Command("move")]
        [CommandDescription("Moves a spawned entity to new coordinates (relative to region).")]
        [CommandUsage("deco move <entityId> <x> <y> <z>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(4)]
        public string Move(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            if (!float.TryParse(@params[1], out float x) || !float.TryParse(@params[2], out float y) || !float.TryParse(@params[3], out float z))
                return "Invalid coordinates. Usage: !deco move <id> <x> <y> <z>";

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            var newPos = new Vector3(x, y, z);
            var result = entity.ChangeRegionPosition(newPos, null, ChangePositionFlags.ForceUpdate);

            // Update tracked info
            var info = _spawnedInfos.FirstOrDefault(i => i.EntityId == entityId);
            if (info != null) info.Position = newPos;

            LogDeco("move", $"Moved 0x{entityId:X} to ({x:F0},{y:F0},{z:F0}), result={result}");
            return $"Moved 0x{entityId:X} to ({x:F0},{y:F0},{z:F0}), result={result}";
        }

        // --- orbit -----------------------------------------------------------

        [Command("orbit")]
        [CommandDescription("Makes an entity orbit its current position. Speed in seconds per revolution (default 10).")]
        [CommandUsage("deco orbit <entityId> <radius> [secondsPerRev]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string Orbit(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            if (!ulong.TryParse(@params[0], System.Globalization.NumberStyles.HexNumber, null, out ulong entityId))
                return $"Invalid entity ID: {@params[0]}";

            if (!float.TryParse(@params[1], out float radius))
                return "Invalid radius.";

            double secondsPerRev = 10.0;
            if (@params.Length > 2 && double.TryParse(@params[2], out double spr)) secondsPerRev = spr;

            var entity = avatar.Game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity == null)
                return $"Entity 0x{entityId:X} not found.";

            StartOrbit(entity, radius, secondsPerRev);
            return $"Orbit started: 0x{entityId:X}, radius={radius}, {secondsPerRev:F1}s/rev";
        }

        // --- cleanup ---------------------------------------------------------

        [Command("cleanup")]
        [CommandDescription("Destroys all deco-spawned entities and stops all VFX loops and orbits.")]
        [CommandUsage("deco cleanup")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Cleanup(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            int count = CleanupAll(avatar.Game);
            LogDeco("cleanup", $"Destroyed {count} entities");
            return $"Cleaned up {count} deco entities. All loops and orbits stopped.";
        }

        // --- save ------------------------------------------------------------

        [Command("save")]
        [CommandDescription("Saves the current arrangement to Data/ReviewDecoPrefabs/<name>.json.")]
        [CommandUsage("deco save <name>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Save(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string name = @params[0];
            string dir = "Data/ReviewDecoPrefabs";
            System.IO.Directory.CreateDirectory(dir);
            string path = System.IO.Path.Combine(dir, $"{name}.json");

            var json = new StringBuilder();
            json.Append("{\n");
            json.Append($"  \"name\": \"{name}\",\n");
            json.Append($"  \"entities\": [\n");

            for (int i = 0; i < _spawnedInfos.Count; i++)
            {
                var info = _spawnedInfos[i];
                json.Append("    {\n");
                json.Append($"      \"prototypePath\": \"{info.PrototypePath}\",\n");
                json.Append($"      \"position\": [{info.Position.X:F1}, {info.Position.Y:F1}, {info.Position.Z:F1}],\n");
                json.Append($"      \"scale\": {info.Scale:F2},\n");
                if (!string.IsNullOrEmpty(info.VfxAssetName))
                {
                    json.Append($"      \"vfxAssetName\": \"{info.VfxAssetName}\",\n");
                    json.Append($"      \"vfxLoopInterval\": {info.VfxLoopInterval:F1},\n");
                }
                json.Append($"      \"label\": \"{info.Label}\"\n");
                json.Append(i < _spawnedInfos.Count - 1 ? "    },\n" : "    }\n");
            }

            json.Append("  ]\n");
            json.Append("}\n");

            System.IO.File.WriteAllText(path, json.ToString());
            LogDeco("save", $"Saved {_spawnedInfos.Count} entities to {path}");
            return $"Saved {_spawnedInfos.Count} entities to {path}";
        }

        // --- load ------------------------------------------------------------

        [Command("load")]
        [CommandDescription("Loads a saved arrangement from Data/ReviewDecoPrefabs/<name>.json.")]
        [CommandUsage("deco load <name>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Load(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string name = @params[0];
            string path = System.IO.Path.Combine("Data/ReviewDecoPrefabs", $"{name}.json");

            if (!System.IO.File.Exists(path))
                return $"Saved arrangement '{name}' not found at {path}. Use !deco list to see available arrangements.";

            // Cleanup current entities first
            CleanupAll(avatar.Game);

            string json = System.IO.File.ReadAllText(path);
            var lines = new List<string> { $"=== Loading arrangement: {name} ===" };
            int spawned = 0;

            // Simple line-based parsing (avoid System.Text.Json dependency issues)
            string currentProto = null;
            float px = 0, py = 0, pz = 0;
            float scale = 1f;
            string vfxName = null;
            float vfxInterval = 3f;
            string label = null;

            foreach (string line in json.Split('\n'))
            {
                string trimmed = line.Trim().TrimEnd(',');

                if (trimmed.Contains("\"prototypePath\""))
                    currentProto = ExtractJsonValue(trimmed, "prototypePath");
                else if (trimmed.Contains("\"scale\"") && float.TryParse(ExtractJsonValue(trimmed, "scale"), out float s))
                    scale = s;
                else if (trimmed.Contains("\"vfxAssetName\""))
                    vfxName = ExtractJsonValue(trimmed, "vfxAssetName");
                else if (trimmed.Contains("\"vfxLoopInterval\"") && float.TryParse(ExtractJsonValue(trimmed, "vfxLoopInterval"), out float vi))
                    vfxInterval = vi;
                else if (trimmed.Contains("\"label\""))
                    label = ExtractJsonValue(trimmed, "label");
                else if (trimmed.Contains("\"position\""))
                {
                    var nums = ExtractJsonArray(trimmed);
                    if (nums.Count >= 3) { px = nums[0]; py = nums[1]; pz = nums[2]; }
                }
                else if (trimmed == "}" && currentProto != null)
                {
                    // End of entity block — spawn it
                    PrototypeId protoRef = GameDatabase.GetPrototypeRefByName(currentProto);
                    if (protoRef != PrototypeId.Invalid)
                    {
                        var entity = SpawnPropEntity(avatar, label ?? "loaded", protoRef,
                            new Vector3(px, py, pz), scale, vfxName, vfxInterval);
                        if (entity != null)
                        {
                            spawned++;
                            lines.Add($"  [{spawned}] {label}: id=0x{entity.Id:X}");
                        }
                    }

                    // Reset for next entity
                    currentProto = null;
                    vfxName = null;
                    label = null;
                    scale = 1f;
                    vfxInterval = 3f;
                }
            }

            lines.Add($"  Loaded {spawned} entities. Use !deco cleanup to remove.");
            LogDeco("load", $"Loaded {spawned} entities from {path}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        private static string ExtractJsonValue(string line, string key)
        {
            int idx = line.IndexOf(key);
            if (idx < 0) return null;
            int colon = line.IndexOf(':', idx);
            if (colon < 0) return null;
            string rest = line[(colon + 1)..].Trim().Trim('"', ',', ' ', '\n', '\r');
            return rest;
        }

        private static List<float> ExtractJsonArray(string line)
        {
            var result = new List<float>();
            int start = line.IndexOf('[');
            int end = line.IndexOf(']');
            if (start < 0 || end < 0) return result;
            string inner = line[(start + 1)..end];
            foreach (string part in inner.Split(','))
            {
                if (float.TryParse(part.Trim(), out float val))
                    result.Add(val);
            }
            return result;
        }

        // --- list ------------------------------------------------------------

        [Command("list")]
        [CommandDescription("Lists saved deco prefab arrangements.")]
        [CommandUsage("deco list")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            string dir = "Data/ReviewDecoPrefabs";
            if (!System.IO.Directory.Exists(dir))
                return "No saved arrangements found. Use !deco save <name> to create one.";

            var files = System.IO.Directory.GetFiles(dir, "*.json");
            if (files.Length == 0)
                return "No saved arrangements found.";

            var lines = new List<string> { $"Saved arrangements ({files.Length}):" };
            foreach (string file in files)
                lines.Add($"  {System.IO.Path.GetFileNameWithoutExtension(file)}");

            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- explore ---------------------------------------------------------

        [Command("explore")]
        [CommandDescription("Explores game assets: props, vfx, powers, assets, animations. Usage: deco explore <type> [pattern]")]
        [CommandUsage("deco explore <props|vfx|powers|assets|animations> [pattern]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Explore(string[] @params, NetClient client)
        {
            string type = @params[0].ToLowerInvariant();
            string pattern = @params.Length > 1 ? @params[1] : null;

            return type switch
            {
                "props" => ExploreProps(client, pattern),
                "vfx" => ExploreVfx(client, pattern),
                "powers" => ExplorePowers(client, pattern),
                "assets" => ExploreAssets(client, pattern),
                "animations" => ExploreAnimations(client, pattern),
                _ => $"Unknown explore type '{type}'. Use: props, vfx, powers, assets, animations"
            };
        }

        private static string ExploreProps(NetClient client, string pattern)
        {
            var patterns = pattern != null
                ? new[] { pattern }
                : new[] { "Barrel", "Chest", "Crystal", "Pyramid", "Obelisk", "Beacon",
                          "Destructible", "Door", "Switch", "Statue", "Pillar", "Terminal",
                          "Doodad", "Prop", "Decoration", "Banner", "Rune", "Stone", "Gem", "Orb" };

            var lines = new List<string> { "=== Prop Prototype Search ===" };
            int total = 0;

            foreach (string p in patterns)
            {
                var matches = GameDatabase.SearchPrototypes(p,
                    DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

                if (matches.Count == 0) continue;

                lines.Add($"\n  Pattern '{p}' ({matches.Count} matches):");
                foreach (var protoRef in matches.Take(5))
                {
                    string name = GameDatabase.GetPrototypeName(protoRef);
                    var worldProto = protoRef.As<WorldEntityPrototype>();
                    AssetId unreal = worldProto?.UnrealClass ?? AssetId.Invalid;
                    string unrealName = unreal != AssetId.Invalid ? GetAssetDisplayName(unreal) : "(none)";
                    bool isProp = protoRef.As<PropPrototype>() != null;
                    lines.Add($"    {(isProp ? "[PROP]" : "[ENT]")} {name} -> {unrealName}");
                    total++;
                }
                if (matches.Count > 5)
                    lines.Add($"    ... and {matches.Count - 5} more");
            }

            lines.Add($"\n  Total shown: {total}");
            lines.Add("  Use !deco spawn <path> to spawn any of these.");
            LogDeco("explore", $"Props: searched {patterns.Length} patterns, found {total}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        private static string ExploreVfx(NetClient client, string pattern)
        {
            var lines = new List<string> { "=== VFX Asset Search ===" };

            // Show infinity stone VFX from PowerVisualsGlobalsPrototype
            if (pattern == null || pattern.Contains("infinity", StringComparison.OrdinalIgnoreCase))
            {
                lines.Add("\n  Infinity Stone VFX (PowerVisualsGlobalsPrototype):");
                var globalsProto = GameDatabase.PowerVisualsGlobalsPrototype;
                if (globalsProto != null)
                {
                    foreach (var stone in _infinityStones)
                    {
                        var prop = typeof(PowerVisualsGlobalsPrototype).GetProperty(stone.VfxPropertyName);
                        if (prop != null)
                        {
                            var assetId = (AssetId)prop.GetValue(globalsProto);
                            lines.Add($"    {stone.Name,-10} -> {stone.VfxPropertyName} = {GetAssetDisplayName(assetId)}");
                        }
                    }
                }
            }

            // Search assets if pattern provided
            if (pattern != null)
            {
                var matches = GameDatabase.SearchAssets(pattern,
                    DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

                if (matches.Count > 0)
                {
                    lines.Add($"\n  Assets matching '{pattern}' ({matches.Count} total, showing {Math.Min(matches.Count, 20)}):");
                    foreach (var assetId in matches.Take(20))
                        lines.Add($"    {GetAssetDisplayName(assetId)} [{GetAssetTypeName(assetId)}]");
                    if (matches.Count > 20)
                        lines.Add($"    ... and {matches.Count - 20} more");
                }
            }

            lines.Add("\n  Use !deco vfx <entityId> <assetName> or !deco vfxloop to play.");
            LogDeco("explore", $"VFX: pattern={pattern ?? "(all)"}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        private static string ExplorePowers(NetClient client, string pattern)
        {
            if (pattern == null) return "Usage: !deco explore powers <pattern>";

            var matches = GameDatabase.SearchPrototypes(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            var lines = new List<string> { $"Power prototypes matching '{pattern}' ({matches.Count}):" };

            foreach (var protoRef in matches.Take(20))
            {
                string name = GameDatabase.GetPrototypeName(protoRef);
                var powerProto = protoRef.As<PowerPrototype>();
                string unrealClass = powerProto?.PowerUnrealClass != AssetId.Invalid
                    ? GetAssetDisplayName(powerProto.PowerUnrealClass) : "(none)";
                lines.Add($"  {name} -> UnrealClass: {unrealClass}");
            }

            if (matches.Count > 20)
                lines.Add($"  ... and {matches.Count - 20} more");

            LogDeco("explore", $"Powers: pattern={pattern}, matches={matches.Count}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        private static string ExploreAssets(NetClient client, string pattern)
        {
            if (pattern == null) return "Usage: !deco explore assets <pattern>";

            var matches = GameDatabase.SearchAssets(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No assets found matching '{pattern}'.";

            var lines = new List<string> { $"Game assets matching '{pattern}' ({matches.Count}, showing {Math.Min(matches.Count, 30)}):" };
            foreach (var assetId in matches.Take(30))
                lines.Add($"  {GetAssetDisplayName(assetId)} [{GetAssetTypeName(assetId)}]");

            if (matches.Count > 30)
                lines.Add($"  ... and {matches.Count - 30} more");

            LogDeco("explore", $"Assets: pattern={pattern}, matches={matches.Count}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        private static string ExploreAnimations(NetClient client, string pattern)
        {
            string searchPattern = pattern ?? "Animation";
            var matches = GameDatabase.SearchAssets(searchPattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No animation assets found matching '{searchPattern}'.";

            var lines = new List<string> { $"Animation assets matching '{searchPattern}' ({matches.Count}, showing {Math.Min(matches.Count, 30)}):" };
            foreach (var assetId in matches.Take(30))
                lines.Add($"  {GetAssetDisplayName(assetId)} [{GetAssetTypeName(assetId)}]");

            if (matches.Count > 30)
                lines.Add($"  ... and {matches.Count - 30} more");

            LogDeco("explore", $"Animations: pattern={searchPattern}, matches={matches.Count}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

        // --- info ------------------------------------------------------------

        [Command("info")]
        [CommandDescription("Shows rendering chain and position for your avatar or a specified entity.")]
        [CommandUsage("deco info [entityId]")]
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

            var lines = new List<string> { $"=== Entity Info: {target} ===" };

            lines.Add($"  Id: 0x{target.Id:X}");
            lines.Add($"  Prototype: {GameDatabase.GetPrototypeName(target.PrototypeDataRef)}");
            lines.Add($"  Position: {target.RegionLocation.Position.ToStringNames()}");
            lines.Add($"  Orientation: {target.RegionLocation.Orientation}");

            AssetId worldAsset = target.GetEntityWorldAsset();
            AssetId originalAsset = target.GetOriginalWorldAsset();
            lines.Add($"  WorldAsset: {GetAssetDisplayName(worldAsset)} [{GetAssetTypeName(worldAsset)}]");
            lines.Add($"  OriginalAsset: {GetAssetDisplayName(originalAsset)} [{GetAssetTypeName(originalAsset)}]");

            lines.Add($"  ClientPrototypeRefOverride: {(target.ClientPrototypeRefOverride != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(target.ClientPrototypeRefOverride) : "(none)")}");
            lines.Add($"  IsClientRenderedAsAvatar: {target.IsClientRenderedAsAvatar}");
            lines.Add($"  IsInWorld: {target.IsInWorld}");

            // Check if tracked by us
            var info = _spawnedInfos.FirstOrDefault(i => i.EntityId == target.Id);
            if (info != null)
            {
                lines.Add($"  Deco Label: {info.Label}");
                lines.Add($"  Deco VFX: {info.VfxAssetName ?? "(none)"}");
                lines.Add($"  VFX Loop Active: {_vfxLoops.ContainsKey(target.Id)}");
                lines.Add($"  Orbit Active: {_orbits.ContainsKey(target.Id)}");
            }

            LogDeco("info", $"Entity=0x{target.Id:X}, proto={GameDatabase.GetPrototypeName(target.PrototypeDataRef)}");
            CommandHelper.SendMessages(client, lines);
            return string.Empty;
        }

    }
}
