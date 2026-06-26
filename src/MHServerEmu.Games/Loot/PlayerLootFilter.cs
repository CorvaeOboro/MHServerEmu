using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Calligraphy;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.Loot.Specs;

namespace MHServerEmu.Games.Loot
{
    /// <summary>
    /// A single set of loot filter thresholds/toggles (used for the global defaults
    /// and for each per-character override block).
    /// </summary>
    public class LootFilterSection
    {
        public Dictionary<string, PrototypeId> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Per-player loot filter settings for non-gear slots and special item types.
    /// Stored in a human-editable JSON file on the server, not synced to the game client.
    /// Contains a <see cref="Global"/> default block plus optional per-character overrides.
    /// At drop time the effective threshold for each item type is the HIGHER rarity tier
    /// between the global value and the active character's override (escalation).
    /// </summary>
    public class PlayerLootFilter
    {
        /// <summary>Global defaults applied to every character.</summary>
        public LootFilterSection Global { get; set; } = new();

        /// <summary>Per-character overrides, keyed by avatar short name (e.g. "Rogue", "ScarletWitch"). Case-insensitive.</summary>
        public Dictionary<string, LootFilterSection> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the override section for the given avatar short name, optionally creating it.
        /// </summary>
        public LootFilterSection GetCharacterSection(string avatarName, bool create = false)
        {
            if (string.IsNullOrEmpty(avatarName))
                return null;

            if (Characters.TryGetValue(avatarName, out LootFilterSection section))
                return section;

            if (create)
            {
                section = new LootFilterSection();
                Characters[avatarName] = section;
                return section;
            }

            return null;
        }
    }

    /// <summary>
    /// Handles loading and saving <see cref="PlayerLootFilter"/> to disk.
    /// </summary>
    public static class PlayerLootFilterStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly string BaseDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "PlayerLootFilters");
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        /// <summary>
        /// Human-editable JSON DTO. Rarities are stored by NAME (e.g. "epic") rather than
        /// opaque prototype ids so the file can be hand-edited by the player.
        /// </summary>
        private class PersistedSection
        {
            public Dictionary<string, string> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private class PersistedFilterV2
        {
            public PersistedSection Global { get; set; } = new();
            public Dictionary<string, PersistedSection> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        public static PlayerLootFilter Load(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            if (File.Exists(path) == false)
                return new PlayerLootFilter();

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return new PlayerLootFilter();

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // New v2 format: { "Global": {...}, "Characters": {...} }
                if (root.ValueKind == JsonValueKind.Object &&
                    (root.TryGetProperty("Global", out _) || root.TryGetProperty("Characters", out _)))
                {
                    return LoadV2(json);
                }

                // Legacy formats migrate into the Global section.
                return LoadLegacy(root);
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to load loot filter for player {playerDbId}: {e.Message}");
                return new PlayerLootFilter();
            }
        }

        private static PlayerLootFilter LoadV2(string json)
        {
            var persisted = JsonSerializer.Deserialize<PersistedFilterV2>(json) ?? new PersistedFilterV2();
            var filter = new PlayerLootFilter();

            filter.Global = SectionFromPersisted(persisted.Global);
            if (persisted.Characters != null)
            {
                foreach (var kvp in persisted.Characters)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                        continue;
                    filter.Characters[kvp.Key] = SectionFromPersisted(kvp.Value);
                }
            }
            return filter;
        }

        private static LootFilterSection SectionFromPersisted(PersistedSection persisted)
        {
            var section = new LootFilterSection();
            if (persisted == null)
                return section;

            if (persisted.Booleans != null)
                foreach (var kvp in persisted.Booleans)
                    section.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

            if (persisted.Thresholds != null)
            {
                foreach (var kvp in persisted.Thresholds)
                {
                    string key = NormalizeKey(kvp.Key);
                    PrototypeId rarityRef = LootFilterHelper.ResolveRarityByName(kvp.Value);
                    if (rarityRef == PrototypeId.Invalid)
                    {
                        Logger.Warn($"Loot filter: unknown rarity '{kvp.Value}' for '{key}' (ignored).");
                        continue;
                    }
                    section.Thresholds[key] = rarityRef;
                }
            }
            return section;
        }

        private static PlayerLootFilter LoadLegacy(JsonElement root)
        {
            // Old formats stored thresholds as ulong prototype ids, either under a
            // "Thresholds"/"Booleans" object or as a flat dictionary at the root.
            var filter = new PlayerLootFilter();

            JsonElement thresholdsElem = root;
            JsonElement booleansElem = default;
            bool hasBooleans = false;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Thresholds", out JsonElement t))
            {
                thresholdsElem = t;
                hasBooleans = root.TryGetProperty("Booleans", out booleansElem);
            }

            if (thresholdsElem.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty prop in thresholdsElem.EnumerateObject())
                {
                    if (prop.Value.ValueKind != JsonValueKind.Number)
                        continue;
                    string key = NormalizeKey(prop.Name);
                    filter.Global.Thresholds[key] = (PrototypeId)prop.Value.GetUInt64();
                }
            }

            if (hasBooleans && booleansElem.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty prop in booleansElem.EnumerateObject())
                {
                    if (prop.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        filter.Global.Booleans[prop.Name.ToLowerInvariant()] = prop.Value.GetBoolean();
                }
            }

            return filter;
        }

        private static string NormalizeKey(string key)
        {
            if (Enum.TryParse<EquipmentInvUISlot>(key, out EquipmentInvUISlot slot))
                return slot.ToString().ToLowerInvariant();
            return key.ToLowerInvariant();
        }

        public static void Save(ulong playerDbId, PlayerLootFilter filter)
        {
            try
            {
                Directory.CreateDirectory(BaseDir);

                var persisted = new PersistedFilterV2
                {
                    Global = SectionToPersisted(filter.Global)
                };
                foreach (var kvp in filter.Characters)
                    persisted.Characters[kvp.Key] = SectionToPersisted(kvp.Value);

                string json = JsonSerializer.Serialize(persisted, WriteOptions);
                File.WriteAllText(GetPath(playerDbId), json);
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to save loot filter for player {playerDbId}: {e.Message}");
            }
        }

        private static PersistedSection SectionToPersisted(LootFilterSection section)
        {
            var persisted = new PersistedSection();
            if (section == null)
                return persisted;

            foreach (var kvp in section.Thresholds)
            {
                if (kvp.Value == PrototypeId.Invalid)
                    continue;
                persisted.Thresholds[kvp.Key.ToLowerInvariant()] = LootFilterHelper.GetRarityShortName(kvp.Value);
            }
            foreach (var kvp in section.Booleans)
                persisted.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

            return persisted;
        }

        private static string GetPath(ulong playerDbId) => Path.Combine(BaseDir, $"{playerDbId}.json");
    }

    /// <summary>
    /// Helper for applying loot filters to <see cref="LootResultSummary"/>.
    /// </summary>
    public static class LootFilterHelper
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static readonly HashSet<EquipmentInvUISlot> FilterableSlots = new()
        {
            EquipmentInvUISlot.Ring,
            EquipmentInvUISlot.Medal,
            EquipmentInvUISlot.Insignia
        };

        /// <summary>
        /// Removes items from the provided <see cref="LootResultSummary"/> that match
        /// the player's custom loot filter thresholds. Pure removal - no credits or PetTech XP.
        /// </summary>
        public static void ApplyFilters(Player player, LootResultSummary summary, PrototypeId avatarProtoRef)
        {
            if (player?.LootFilter == null) return;
            if (player.Game?.CustomGameOptions?.LootFilterEnable == false) return;

            bool enableCharacterFilter = player.Game?.CustomGameOptions?.LootFilterCharacterSpecificEnable == true;
            bool lootFilterLogging = player.Game?.CustomGameOptions?.LootFilterLoggingEnable == true;
            string avatarName = GetAvatarShortName(avatarProtoRef);
            int removed = summary.ItemSpecs.RemoveAll(itemSpec =>
            {
                bool shouldFilter = ShouldFilter(player.LootFilter, itemSpec, avatarProtoRef, avatarName, enableCharacterFilter, player.Id, lootFilterLogging, out string reason);
                if (shouldFilter)
                {
                    ItemPrototype itemProto = itemSpec.ItemProtoRef.As<ItemPrototype>();
                    string protoName = itemProto?.DataRef.GetName() ?? "unknown";
                    if (lootFilterLogging)
                    {
                        Logger.Trace($"[LootFilter] Removed [{protoName}] - reason: {reason}");
                        LootFilterLogCollator.WriteLine(player.Id, $"[LootFilter] Removed [{protoName}] - reason: {reason}");
                    }
                }
                return shouldFilter;
            });
            if (removed > 0 && lootFilterLogging)
            {
                Logger.Trace($"[LootFilter] Removed {removed} filtered item(s) for player [{player}]");
                LootFilterLogCollator.WriteLine(player.Id, $"[LootFilter] Removed {removed} filtered item(s) for player [{player}]");
            }
        }

        private static bool ShouldFilter(PlayerLootFilter filter, ItemSpec itemSpec, PrototypeId avatarProtoRef, string avatarName, bool enableCharacterFilter, ulong playerId, bool lootFilterLogging, out string reason)
        {
            reason = null;
            ItemPrototype itemProto = itemSpec.ItemProtoRef.As<ItemPrototype>();
            if (itemProto == null) return false;

            // Character-specific filter: drop items bound to other characters, keep Any-Hero items
            if (enableCharacterFilter &&
                avatarProtoRef != PrototypeId.Invalid &&
                itemSpec.EquippableBy != PrototypeId.Invalid &&
                itemSpec.EquippableBy != avatarProtoRef)
            {
                reason = $"CharacterSpecificLootFilter (bound to {GameDatabase.GetPrototypeName(itemSpec.EquippableBy)}, current avatar is {GameDatabase.GetPrototypeName(avatarProtoRef)})";
                return true;
            }

            string filterKey = null;

            // 1. Slot-based check (Ring, Medal, Insignia)
            AgentPrototype agentProto = avatarProtoRef.As<AgentPrototype>();
            EquipmentInvUISlot slot = itemProto.GetInventorySlotForAgent(agentProto);
            if (FilterableSlots.Contains(slot))
            {
                filterKey = slot.ToString().ToLowerInvariant();
            }

            // 2. Type-based check (TeamUpGear, Catalyst)
            if (filterKey == null)
            {
                if (itemProto is TeamUpGearPrototype)
                    filterKey = "teamup";
                else if (IsCatalystPrototype(itemProto))
                    filterKey = "catalyst";
            }

            if (filterKey == null)
            {
                if (lootFilterLogging)
                {
                    string msg = $"[LootFilter] Unmatched item [{itemProto.GetType().Name}] protoName=[{itemProto.DataRef.GetName()}]";
                    Logger.Trace(msg);
                    LootFilterLogCollator.WriteLine(playerId, msg);
                }
                return false;
            }

            // Boolean filters (e.g. uruforged) - on/off, no rarity threshold.
            // Effective = global OR active character's override.
            if (BooleanFilters.Contains(filterKey))
            {
                if (GetEffectiveBoolean(filter, filterKey, avatarName) == false)
                    return false;

                if (filterKey == "uruforged" && itemSpec.RarityProtoRef == GameDatabase.LootGlobalsPrototype.RarityUruForged)
                {
                    reason = "uruforged boolean filter";
                    return true;
                }

                return false;
            }

            // Effective threshold = HIGHER rarity tier of global vs the active character's override.
            PrototypeId thresholdRef = GetEffectiveThreshold(filter, filterKey, avatarName);
            if (thresholdRef == PrototypeId.Invalid)
                return false;

            RarityPrototype itemRarity = itemSpec.RarityProtoRef.As<RarityPrototype>();
            RarityPrototype thresholdRarity = thresholdRef.As<RarityPrototype>();
            if (itemRarity == null || thresholdRarity == null)
            {
                reason = "Rarity lookup failed (null rarity prototype)";
                return false;
            }

            if (itemRarity.Tier <= thresholdRarity.Tier)
            {
                reason = $"{filterKey} rarity tier {itemRarity.Tier} ({GameDatabase.GetPrototypeName(itemSpec.RarityProtoRef)}) <= threshold tier {thresholdRarity.Tier} ({GameDatabase.GetPrototypeName(thresholdRef)})";
                return true;
            }

            return false;
        }

        private static bool IsCatalystPrototype(ItemPrototype itemProto)
        {
            if (itemProto is not CostumeCorePrototype)
                return false;

            string protoName = itemProto.DataRef.GetName();

            return protoName.Contains("MysticalEnergiesCatalyst", StringComparison.OrdinalIgnoreCase)
                || protoName.Contains("AdvancedTechnologicalSystemsCatalyst", StringComparison.OrdinalIgnoreCase)
                || protoName.Contains("CosmicSpiritCatalyst", StringComparison.OrdinalIgnoreCase)
                || protoName.Contains("GeneticMutationCatalyst", StringComparison.OrdinalIgnoreCase)
                || protoName.Contains("RadioactiveIsotopeCatalyst", StringComparison.OrdinalIgnoreCase);
        }

        // --- Escalation helpers ---

        /// <summary>
        /// Returns the avatar's short name (e.g. "Rogue", "ScarletWitch") used as the
        /// per-character override key. Returns <see langword="null"/> if unresolved.
        /// </summary>
        public static string GetAvatarShortName(PrototypeId avatarProtoRef)
        {
            if (avatarProtoRef == PrototypeId.Invalid)
                return null;

            string fullName = GameDatabase.GetPrototypeName(avatarProtoRef);
            if (string.IsNullOrEmpty(fullName))
                return null;

            string fileName = Path.GetFileName(fullName);
            if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);
            return fileName;
        }

        /// <summary>
        /// Returns the threshold with the HIGHER rarity tier. <see cref="PrototypeId.Invalid"/> entries lose.
        /// </summary>
        private static PrototypeId HigherTier(PrototypeId a, PrototypeId b)
        {
            if (a == PrototypeId.Invalid) return b;
            if (b == PrototypeId.Invalid) return a;

            RarityPrototype ra = a.As<RarityPrototype>();
            RarityPrototype rb = b.As<RarityPrototype>();
            if (ra == null) return b;
            if (rb == null) return a;
            return ra.Tier >= rb.Tier ? a : b;
        }

        /// <summary>
        /// Computes the effective rarity threshold for an item type: the higher tier of the
        /// global default and the active character's override.
        /// </summary>
        public static PrototypeId GetEffectiveThreshold(PlayerLootFilter filter, string filterKey, string avatarName)
        {
            if (filter == null) return PrototypeId.Invalid;

            PrototypeId result = PrototypeId.Invalid;
            if (filter.Global.Thresholds.TryGetValue(filterKey, out PrototypeId globalRef))
                result = globalRef;

            LootFilterSection charSection = filter.GetCharacterSection(avatarName);
            if (charSection != null && charSection.Thresholds.TryGetValue(filterKey, out PrototypeId charRef))
                result = HigherTier(result, charRef);

            return result;
        }

        /// <summary>
        /// Computes the effective boolean toggle for an item type: global OR the active character's override.
        /// </summary>
        public static bool GetEffectiveBoolean(PlayerLootFilter filter, string filterKey, string avatarName)
        {
            if (filter == null) return false;

            bool result = filter.Global.Booleans.TryGetValue(filterKey, out bool globalVal) && globalVal;

            LootFilterSection charSection = filter.GetCharacterSection(avatarName);
            if (charSection != null && charSection.Booleans.TryGetValue(filterKey, out bool charVal) && charVal)
                result = true;

            return result;
        }

        // --- Command helpers ---

        private static Dictionary<string, PrototypeId> _rarityNameMap;
        private static Dictionary<PrototypeId, string> _rarityShortNameMap;
        private static readonly object _rarityMapLock = new();

        private static void EnsureRarityMapBuilt()
        {
            lock (_rarityMapLock)
            {
                if (_rarityNameMap != null) return;

                _rarityNameMap = new(StringComparer.OrdinalIgnoreCase);
                _rarityShortNameMap = new();
                foreach (PrototypeId rarityRef in DataDirectory.Instance.IteratePrototypesInHierarchy<RarityPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                {
                    string fullName = rarityRef.GetName();
                    string fileName = Path.GetFileName(fullName);

                    // Strip .prototype extension if present
                    if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                        fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);

                    _rarityNameMap[fileName] = rarityRef;

                    string suffix = Regex.Replace(fileName, @"^R\d+", "");
                    if (string.IsNullOrEmpty(suffix) == false)
                        _rarityNameMap[suffix] = rarityRef;

                    // Prefer the friendly suffix (e.g. "Epic") for serialization; fall back to file name.
                    _rarityShortNameMap[rarityRef] = string.IsNullOrEmpty(suffix) ? fileName : suffix;
                }
            }
        }

        /// <summary>
        /// Returns a human-friendly short rarity name (e.g. "Epic") for the given rarity proto,
        /// suitable for writing into the editable JSON file.
        /// </summary>
        public static string GetRarityShortName(PrototypeId rarityRef)
        {
            EnsureRarityMapBuilt();
            if (_rarityShortNameMap.TryGetValue(rarityRef, out string name))
                return name;
            return ((ulong)rarityRef).ToString();
        }

        public static PrototypeId ResolveRarityByName(string name)
        {
            EnsureRarityMapBuilt();
            if (string.IsNullOrWhiteSpace(name))
                return PrototypeId.Invalid;

            name = name.Trim();
            if (name.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".prototype".Length);

            if (_rarityNameMap.TryGetValue(name, out PrototypeId rarityRef))
                return rarityRef;
            return PrototypeId.Invalid;
        }

        public static IReadOnlyDictionary<string, PrototypeId> GetRarityMap()
        {
            EnsureRarityMapBuilt();
            return _rarityNameMap;
        }

        public static readonly HashSet<string> BooleanFilters = new(StringComparer.OrdinalIgnoreCase)
        {
            "uruforged",
        };

        public static readonly Dictionary<string, string> FilterNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ring"] = "ring",
            ["medal"] = "medal",
            ["insignia"] = "insignia",
            ["teamup"] = "teamup",
            ["team-up"] = "teamup",
            ["teamupgear"] = "teamup",
            ["catalyst"] = "catalyst",
            ["uruforged"] = "uruforged",
            ["uru"] = "uruforged",
            ["uru-forged"] = "uruforged",
        };

        public static string GetFormattedThreshold(Dictionary<string, PrototypeId> thresholds, string key)
        {
            if (thresholds.TryGetValue(key, out PrototypeId rarityRef) && rarityRef != PrototypeId.Invalid)
                return GameDatabase.GetFormattedPrototypeName(rarityRef);
            return "(none)";
        }
    }
}
