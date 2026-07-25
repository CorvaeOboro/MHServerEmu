// =============================================================================
//  MOD ReviewVFXPreview - commands
// =============================================================================
//  Feature:    Dev tool for previewing VFX (visual effects).  Plays
//              NetMessagePlayPowerVisuals on the player's avatar and writes
//              the resolved asset name to chat.
//
//  Three sources of VFX:
//    1. Globals  - the 16 AssetId fields in PowerVisualsGlobalsPrototype
//    2. Discovered - VFX candidates found by the IncursionVFX discovery script
//       (agent spawn FX, power visuals, teleport/portal/beam effects)
//    3. Arbitrary  - any asset name in the game can be played via playasset
//
//  Commands:   !vfx show [category] [count] | list [category] | play <name> |
//              playasset <AssetName> | find <pattern> | categories |
//              search <pattern> | all [category] | discovered [count]
//  Categories: teleport, loot, achievement, mission, omega, infinity, pettech,
//              discovered-fire, discovered-teleport, discovered-beam
//  Example :   "!vfx show"            =  random category, 3 random VFX played on avatar
//  Example :   "!vfx show infinity 5" =  5 random infinity VFX
//  Example :   "!vfx play LootVaporizedClass" = play that specific global VFX
//  Example :   "!vfx playasset MarvelAgent_FireGiant_MeteorSpawn" = play any asset
//  Example :   "!vfx find FireSpawn"  =  search all game assets for "FireSpawn"
//  Example :   "!vfx discovered 3"    =  play 3 random discovered VFX
// =============================================================================

using System.Reflection;
using System.Text;
using Gazillion;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Network;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("vfx")]
    [CommandGroupDescription("Dev tool for previewing VFX. Subcommands: show, list, play, playasset, find, categories, search, all, discovered.")]
    [CommandGroupUserLevel(AccountUserLevel.Admin)]
    public class ModReviewVFXPreviewCommands : CommandGroup
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        // --- VFX catalog ----------------------------------------------------

        /// <summary>
        /// A single VFX entry from the catalog.
        /// </summary>
        private record VfxEntry(string PropertyName, string Category, AssetId AssetId);

        /// <summary>
        /// All known VFX entries from PowerVisualsGlobalsPrototype (globals)
        /// plus discovered candidates from the IncursionVFX report.
        /// Lazy-initialized on first use because GameDatabase is not ready at static construction time.
        /// </summary>
        private static List<VfxEntry> _catalog;
        private static bool _catalogInitialized;

        /// <summary>
        /// Discovered VFX candidates from the IncursionVFX discovery report.
        /// These are asset names found in .sip data that are likely valid
        /// NetMessagePlayPowerVisuals targets (agent spawn FX, power visuals).
        /// </summary>
        private static readonly string[] _discoveredFire =
        {
            "MarvelAgent_FireGiant_MeteorSpawn",
            "MarvelAgent_FireDemon_Grunt_Archer_FireSpawn",
            "MarvelAgent_FireDemon_Grunt_Sword_FireSpawn",
            "MarvelAgent_FireDemonWingedEnc3_FlyingSpawn",
            "MarvelAgent_FireDemon_Grunt_Maul_FireSpawn",
            "MarvelAgent_FireDemonWinged_FlyingSpawn",
            "MarvelAgent_FireDemon_Grunt_Spearman_FireSpawn",
            "MarvelAgent_FireGiant_Spawn",
            "MarvelAgent_FireDemon_Grunt_Dual_FireSpawn",
            "MarvelAgent_FireDemonFlameTosser_Spawn",
            "MarvelAgent_FireGiantLimboBoss_Spawn",
            "MarvelAgent_FireGiant_SpawnObelisk",
        };

        private static readonly string[] _discoveredTeleport =
        {
            "MarvelAgent_SerpentManHunter_Spawn_TeleportIn",
            "MarvelAgent_HydraExoBeam_Spawn",
            "MarvelAgent_HydraExoBeam_DR_Teleport",
            "MarvelAgent_HandNinja_Spawn",
            "MarvelAgent_HandNinja_DR_Teleport",
            "MarvelAgent_SkrullMedic_TeleportSpawn",
            "MarvelAgent_AvengersWarSkrull_Spawn",
            "MarvelAgent_AsgardGuard_Combat_Male_DR_Teleport",
            "MarvelAgent_AsgardGuard_Combat_Female_DR_Teleport",
            "MarvelAgent_MindlessOne_Boss_PortalSpawn",
            "PowerDoctorStrange_TeleportHit",
            "PowerDeadpool_Teleport",
            "Power_SurturPortal_BanishToMonolith",
        };

        private static readonly string[] _discoveredBeam =
        {
            "PowerIronMan_UnibeamUpgradedHitFX",
            "PowerCyclops_ChanneledBeam",
            "PowerSilverSurfer_BigBeam",
            "PowerSkrullAgentCoulson_DestroyerBeam",
            "PowerRogue_StolenPower_MindlessOneBeam",
            "PowerBoss_IronMan_OneOffBeamLeft",
            "PowerChanneledEnergyBeam_HumanTorch",
            "PowerTeamup_Quake_ChanneledBeam",
            "PowerDrDoomPhase1Turret_ChanneledEnergyBeam",
            "PowerElektra_BamfDiveBombEnd",
        };

        /// <summary>
        /// Category -> list of entries lookup.
        /// Lazy-initialized alongside _catalog.
        /// </summary>
        private static Dictionary<string, List<VfxEntry>> _byCategory;

        private static void EnsureCatalogInitialized()
        {
            if (_catalogInitialized) return;
            _catalogInitialized = true;
            _catalog = BuildCatalog();
            _byCategory = BuildCategoryLookup(_catalog);
        }

        private static List<VfxEntry> BuildCatalog()
        {
            var result = new List<VfxEntry>();

            // 1. Globals from PowerVisualsGlobalsPrototype
            var proto = GameDatabase.PowerVisualsGlobalsPrototype;
            if (proto != null)
            {
                foreach (var prop in typeof(PowerVisualsGlobalsPrototype).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.PropertyType != typeof(AssetId)) continue;
                    var assetId = (AssetId)prop.GetValue(proto);
                    if (assetId == AssetId.Invalid) continue;

                    string category = DeriveCategory(prop.Name);
                    result.Add(new VfxEntry(prop.Name, category, assetId));
                }
            }

            // 2. Discovered VFX candidates from the IncursionVFX report
            AddDiscovered(result, _discoveredFire,      "discovered-fire");
            AddDiscovered(result, _discoveredTeleport,   "discovered-teleport");
            AddDiscovered(result, _discoveredBeam,       "discovered-beam");

            return result;
        }

        private static void AddDiscovered(List<VfxEntry> result, string[] names, string category)
        {
            foreach (string name in names)
            {
                AssetId assetId = GameDatabase.StringRefManager.GetDataRefByName(name);
                if (assetId == AssetId.Invalid) continue;
                result.Add(new VfxEntry(name, category, assetId));
            }
        }

        private static Dictionary<string, List<VfxEntry>> BuildCategoryLookup(List<VfxEntry> entries)
        {
            var dict = new Dictionary<string, List<VfxEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (dict.TryGetValue(entry.Category, out var list) == false)
                {
                    list = new();
                    dict[entry.Category] = list;
                }
                list.Add(entry);
            }
            return dict;
        }

        /// <summary>
        /// Maps a property name like "InfinityTimePointEarnedClass" to a category like "infinity".
        /// </summary>
        private static string DeriveCategory(string propName)
        {
            // Strip trailing "Class"
            string s = propName;
            if (s.EndsWith("Class", StringComparison.OrdinalIgnoreCase))
                s = s[..^5];

            return s.ToLowerInvariant() switch
            {
                "avatarleashteleport"      => "teleport",
                "lootvaporized"             => "loot",
                "achievementunlocked"       => "achievement",
                "dailymissioncomplete"      => "mission",
                "omegapointgained"          => "omega",
                var v when v.StartsWith("unlockpettech") => "pettech",
                var v when v.StartsWith("infinity")     => "infinity",
                _ => s.ToLowerInvariant(),
            };
        }

        // --- Helpers --------------------------------------------------------

        private static string GetAssetDisplayName(AssetId assetId)
        {
            string name = GameDatabase.GetAssetName(assetId);
            return string.IsNullOrEmpty(name) ? assetId.ToString() : name;
        }

        /// <summary>
        /// Resolves an asset name string to an AssetId via the StringRefManager.
        /// </summary>
        private static AssetId ResolveAssetByName(string name)
        {
            return GameDatabase.StringRefManager.GetDataRefByName(name);
        }

        /// <summary>
        /// Plays a single VFX on the given avatar.
        /// </summary>
        private static void PlayVfx(Avatar avatar, AssetId assetId)
        {
            var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                .SetEntityId(avatar.Id)
                .SetPowerAssetRef((ulong)assetId)
                .Build();

            avatar.Game?.NetworkManager?.SendMessageToInterested(msg, avatar, AOINetworkPolicyValues.AOIChannelProximity);
        }

        /// <summary>
        /// Validates that the player's avatar is in world. Returns null on success,
        /// or an error string if not ready.
        /// </summary>
        private static string ValidateAvatar(NetClient client, out PlayerConnection playerConnection, out Avatar avatar)
        {
            playerConnection = (PlayerConnection)client;
            avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
                return "Your avatar must be in the world to preview VFX.";
            return null;
        }

        // --- Default command ------------------------------------------------

        [DefaultCommand]
        [CommandDescription("Shows available VFX categories and usage.")]
        [CommandUsage("vfx [show|list|play|playasset|find|categories|search|all|discovered] ...")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public override string Fallback(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("VFX Preview commands:\n");
            sb.Append("  !vfx show [category] [count]  - Play random VFX from a category (default: random category, 3)\n");
            sb.Append("  !vfx list [category]          - List VFX entries in a category (or all)\n");
            sb.Append("  !vfx play <name>              - Play a VFX by property name or asset name\n");
            sb.Append("  !vfx playasset <AssetName>    - Play any game asset as VFX (resolved by name)\n");
            sb.Append("  !vfx find <pattern>           - Search ALL game assets (not just catalog)\n");
            sb.Append("  !vfx categories               - List all categories\n");
            sb.Append("  !vfx search <pattern>         - Search catalog VFX by name pattern\n");
            sb.Append("  !vfx all [category]           - Play every VFX in a category (or all)\n");
            sb.Append("  !vfx discovered [count]       - Play random discovered VFX (from IncursionVFX report)\n");
            EnsureCatalogInitialized();
            sb.Append($"\nAvailable categories: {string.Join(", ", _byCategory.Keys)}.\n");
            sb.Append($"Total catalog entries: {_catalog.Count}.");
            return sb.ToString();
        }

        // --- show -----------------------------------------------------------

        [Command("show")]
        [CommandDescription("Plays random VFX from a category on your avatar and writes names to chat. Default: random category, 3 VFX.")]
        [CommandUsage("vfx show [category] [count]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Show(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            var random = avatar.Game.Random;
            EnsureCatalogInitialized();

            // Resolve category
            string category = @params.Length > 0 ? @params[0] : null;
            if (string.IsNullOrEmpty(category))
            {
                var categories = _byCategory.Keys.ToList();
                category = categories[random.Next(categories.Count)];
            }

            if (_byCategory.TryGetValue(category, out var entries) == false)
                return $"Unknown category '{category}'. Available: {string.Join(", ", _byCategory.Keys)}.";

            // Resolve count
            int count = 3;
            if (@params.Length > 1 && int.TryParse(@params[1], out int requested))
                count = Math.Clamp(requested, 1, entries.Count);

            // Pick random entries
            var picked = entries.OrderBy(_ => random.Next()).Take(count).ToList();

            // Play and report
            var chatLines = new List<string> { $"Playing {picked.Count} VFX from [{category}]:" };
            foreach (var entry in picked)
            {
                PlayVfx(avatar, entry.AssetId);
                string assetName = GetAssetDisplayName(entry.AssetId);
                chatLines.Add($"  {entry.PropertyName} -> {assetName}");
                Logger.Info($"[VFXPreview] Played {entry.PropertyName} ({assetName}) on avatar {avatar.Id}.");
            }

            CommandHelper.SendMessages(client, chatLines);
            return string.Empty;
        }

        // --- list -----------------------------------------------------------

        [Command("list")]
        [CommandDescription("Lists VFX entries in a category (or all if no category given).")]
        [CommandUsage("vfx list [category]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            EnsureCatalogInitialized();
            string category = @params.Length > 0 ? @params[0] : null;

            IEnumerable<VfxEntry> entries = string.IsNullOrEmpty(category)
                ? _catalog
                : _byCategory.TryGetValue(category, out var list) ? list : null;

            if (entries == null)
                return $"Unknown category '{category}'. Available: {string.Join(", ", _byCategory.Keys)}.";

            var chatLines = new List<string>();
            string header = string.IsNullOrEmpty(category)
                ? $"All VFX entries ({_catalog.Count}):"
                : $"VFX entries in [{category}] ({entries.Count()}):";
            chatLines.Add(header);

            foreach (var entry in entries)
                chatLines.Add($"  {entry.PropertyName} -> {GetAssetDisplayName(entry.AssetId)}");

            CommandHelper.SendMessages(client, chatLines);
            return string.Empty;
        }

        // --- play (catalog or asset name) ----------------------------------

        [Command("play")]
        [CommandDescription("Plays a VFX by property name (e.g. LootVaporizedClass) or asset name (e.g. MarvelAgent_FireGiant_MeteorSpawn).")]
        [CommandUsage("vfx play <name>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Play(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string name = @params[0];
            EnsureCatalogInitialized();

            // Try catalog match first (property name or asset name)
            var entry = _catalog.FirstOrDefault(e =>
                e.PropertyName.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? _catalog.FirstOrDefault(e =>
                e.PropertyName.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (entry != null)
            {
                PlayVfx(avatar, entry.AssetId);
                string assetName = GetAssetDisplayName(entry.AssetId);
                Logger.Info($"[ReviewVFXPreview] Played {entry.PropertyName} ({assetName}) on avatar {avatar.Id}.");
                return $"Playing: {entry.PropertyName} -> {assetName}";
            }

            // Fall back to resolving as an arbitrary asset name
            AssetId assetId = ResolveAssetByName(name);
            if (assetId == AssetId.Invalid)
                return $"No VFX found matching '{name}'. Use '!vfx find <pattern>' to search all game assets.";

            PlayVfx(avatar, assetId);
            string resolvedName = GetAssetDisplayName(assetId);
            Logger.Info($"[VFXPreview] Played asset {name} ({resolvedName}) on avatar {avatar.Id}.");
            return $"Playing asset: {name} -> {resolvedName}";
        }

        // --- playasset (arbitrary asset name) -------------------------------

        [Command("playasset")]
        [CommandDescription("Plays any game asset as VFX by its exact asset name. Resolved via StringRefManager.")]
        [CommandUsage("vfx playasset <AssetName>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string PlayAsset(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            string name = @params[0];
            AssetId assetId = ResolveAssetByName(name);
            if (assetId == AssetId.Invalid)
                return $"Asset '{name}' not found. Use '!vfx find <pattern>' to search.";

            PlayVfx(avatar, assetId);
            string resolvedName = GetAssetDisplayName(assetId);
            Logger.Info($"[VFXPreview] Played asset {name} ({resolvedName}) on avatar {avatar.Id}.");
            return $"Playing asset: {name} -> {resolvedName}";
        }

        // --- find (search ALL game assets) ---------------------------------

        [Command("find")]
        [CommandDescription("Searches ALL game assets (not just catalog) by name pattern. Use this to discover new VFX.")]
        [CommandUsage("vfx find <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Find(string[] @params, NetClient client)
        {
            string pattern = @params[0];

            var matches = GameDatabase.SearchAssets(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive).ToList();

            if (matches.Count == 0)
                return $"No game assets found matching '{pattern}'.";

            const int MaxResults = 30;
            int displayCount = Math.Min(matches.Count, MaxResults);

            var chatLines = new List<string> { $"Game assets matching '{pattern}' ({matches.Count} total, showing {displayCount}):" };
            foreach (var assetId in matches.Take(MaxResults))
            {
                string assetName = GetAssetDisplayName(assetId);
                var typeId = GameDatabase.DataDirectory.AssetDirectory.GetAssetTypeRef(assetId);
                string typeName = GameDatabase.GetAssetTypeName(typeId);
                chatLines.Add($"  {assetName} [{typeName}]");
            }

            if (matches.Count > MaxResults)
                chatLines.Add($"  ... and {matches.Count - MaxResults} more.");

            chatLines.Add("Use '!vfx playasset <name>' to play any of these.");
            CommandHelper.SendMessages(client, chatLines);
            return string.Empty;
        }

        // --- categories -----------------------------------------------------

        [Command("categories")]
        [CommandDescription("Lists all VFX categories with their entry counts.")]
        [CommandUsage("vfx categories")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Categories(string[] @params, NetClient client)
        {
            EnsureCatalogInitialized();
            var sb = new StringBuilder();
            sb.Append("VFX categories:\n");
            foreach (var kvp in _byCategory)
                sb.Append($"  {kvp.Key} ({kvp.Value.Count} entries)\n");
            sb.Append($"\nTotal: {_catalog.Count} entries.");
            return sb.ToString().TrimEnd();
        }

        // --- search ---------------------------------------------------------

        [Command("search")]
        [CommandDescription("Searches VFX entries by property name pattern (case-insensitive).")]
        [CommandUsage("vfx search <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Search(string[] @params, NetClient client)
        {
            string pattern = @params[0];
            EnsureCatalogInitialized();

            var matches = _catalog.Where(e =>
                e.PropertyName.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                GetAssetDisplayName(e.AssetId).Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
                return $"No VFX found matching '{pattern}'.";

            var chatLines = new List<string> { $"VFX matching '{pattern}' ({matches.Count}):" };
            foreach (var entry in matches)
                chatLines.Add($"  [{entry.Category}] {entry.PropertyName} -> {GetAssetDisplayName(entry.AssetId)}");

            CommandHelper.SendMessages(client, chatLines);
            return string.Empty;
        }

        // --- all ------------------------------------------------------------

        [Command("all")]
        [CommandDescription("Plays every VFX in a category (or all categories) on your avatar.")]
        [CommandUsage("vfx all [category]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string All(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            EnsureCatalogInitialized();
            string category = @params.Length > 0 ? @params[0] : null;

            IEnumerable<VfxEntry> entries = string.IsNullOrEmpty(category)
                ? _catalog
                : _byCategory.TryGetValue(category, out var list) ? list : null;

            if (entries == null)
                return $"Unknown category '{category}'. Available: {string.Join(", ", _byCategory.Keys)}.";

            var entryList = entries.ToList();
            var chatLines = new List<string> { $"Playing all {entryList.Count} VFX{(string.IsNullOrEmpty(category) ? "" : $" from [{category}]")}:" };

            foreach (var entry in entryList)
            {
                PlayVfx(avatar, entry.AssetId);
                chatLines.Add($"  {entry.PropertyName} -> {GetAssetDisplayName(entry.AssetId)}");
            }

            CommandHelper.SendMessages(client, chatLines);
            Logger.Info($"[ReviewVFXPreview] Played all {entryList.Count} VFX on avatar {avatar.Id}.");
            return string.Empty;
        }

        // --- discovered -----------------------------------------------------

        [Command("discovered")]
        [CommandDescription("Plays random VFX from the IncursionVFX discovered candidates (fire, teleport, beam themes).")]
        [CommandUsage("vfx discovered [count]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Discovered(string[] @params, NetClient client)
        {
            string error = ValidateAvatar(client, out _, out var avatar);
            if (error != null) return error;

            EnsureCatalogInitialized();
            var discoveredEntries = _catalog.Where(e => e.Category.StartsWith("discovered")).ToList();
            if (discoveredEntries.Count == 0)
                return "No discovered VFX entries available (assets may not be loaded).";

            int count = 3;
            if (@params.Length > 0 && int.TryParse(@params[0], out int requested))
                count = Math.Clamp(requested, 1, discoveredEntries.Count);

            var random = avatar.Game.Random;
            var picked = discoveredEntries.OrderBy(_ => random.Next()).Take(count).ToList();

            var chatLines = new List<string> { $"Playing {picked.Count} discovered VFX:" };
            foreach (var entry in picked)
            {
                PlayVfx(avatar, entry.AssetId);
                string assetName = GetAssetDisplayName(entry.AssetId);
                chatLines.Add($"  [{entry.Category}] {entry.PropertyName} -> {assetName}");
                Logger.Info($"[ReviewVFXPreview] Played discovered {entry.PropertyName} ({assetName}) on avatar {avatar.Id}.");
            }

            CommandHelper.SendMessages(client, chatLines);
            return string.Empty;
        }
    }
}
