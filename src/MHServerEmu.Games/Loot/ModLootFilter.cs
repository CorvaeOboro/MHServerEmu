// =============================================================================
//  MOD LootFilter  
// =============================================================================
//  Feature:    Server-side per-player loot filtering of additonal item types set via commands
//  Items  :    Ring, Medal, Insignia (slot-based); Team-Up Gear, Catalysts
//              (type-based); Uru-Forged (boolean toggle).
//  Pipeline:   Runs in LootManager.SpawnLootFromSummary() BEFORE the vanilla
//              LootVaporizer. Filtered items are removed (no credits, no PetTech XP).
//  Commands:   !filter list | set | clear | clearall | rarities
//  Target :    Each command accepts an optional target: global (default), "me"
//              (current character), or a specific named character .
//              rarities by short name (e.g. "epic").
//  Storage:    Per-player JSON on server in Data/PlayerLootFilters/<dbId>.json.
// =============================================================================

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
    #region Data variables

    public class ModLootFilterSection
    {
        public Dictionary<string, PrototypeId> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ExactItems { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Per-player loot filter settings for non-gear slots and special item types.
    /// Stored in a human-editable JSON file on the server..
    /// Contains a <see cref="Global"/> default block plus optional per-character overrides.
    /// At drop time the effective threshold for each item type is the HIGHER rarity tier
    /// between the global value and the active character's override (escalation).
    /// </summary>
    public class ModLootFilter
    {
        /// <summary>Global defaults applied to every character.</summary>
        public ModLootFilterSection Global { get; set; } = new();

        /// <summary>Per-character overrides, keyed by avatar short name (e.g. "Rogue", "ScarletWitch"). Case-insensitive.</summary>
        public Dictionary<string, ModLootFilterSection> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the override section for the given avatar short name, optionally creating it.
        /// </summary>
        public ModLootFilterSection GetCharacterSection(string avatarName, bool create = false)
        {
            if (string.IsNullOrEmpty(avatarName))
                return null;

            if (Characters.TryGetValue(avatarName, out ModLootFilterSection section))
                return section;

            if (create)
            {
                section = new ModLootFilterSection();
                Characters[avatarName] = section;
                return section;
            }

            return null;
        }
    }

    #endregion

    /// <summary>
    /// Handles loading and saving <see cref="ModLootFilter"/> to disk.
    /// </summary>
    public static class ModLootFilterStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly string BaseDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "PlayerLootFilters");
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        #region Persisted Data

        /// <summary>
        /// Human-editable JSON DTO. Rarities are stored by NAME (e.g. "epic") rather than
        /// long prototype ids so the file can be hand-edited by the player ( or potential external tool )
        /// </summary>
        private class PersistedSection
        {
            public Dictionary<string, string> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> ExactItems { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private class PersistedFilter
        {
            public PersistedSection Global { get; set; } = new();
            public Dictionary<string, PersistedSection> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Load

        public static ModLootFilter Load(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            if (File.Exists(path) == false)
                return new ModLootFilter();

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return new ModLootFilter();

                var persisted = JsonSerializer.Deserialize<PersistedFilter>(json) ?? new PersistedFilter();
                var filter = new ModLootFilter();

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
            catch (Exception e)
            {
                Logger.Warn($"Failed to load loot filter for player {playerDbId}: {e.Message}");
                return new ModLootFilter();
            }
        }

        #endregion

        #region Save

        private static ModLootFilterSection SectionFromPersisted(PersistedSection persisted)
        {
            var section = new ModLootFilterSection();
            if (persisted == null)
                return section;

            if (persisted.Booleans != null)
                foreach (var kvp in persisted.Booleans)
                    section.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

            if (persisted.ExactItems != null)
                foreach (string item in persisted.ExactItems)
                    if (string.IsNullOrWhiteSpace(item) == false)
                        section.ExactItems.Add(item);

            if (persisted.Thresholds != null)
            {
                foreach (var kvp in persisted.Thresholds)
                {
                    string key = NormalizeKey(kvp.Key);
                    PrototypeId rarityRef = ModLootFilterHelper.ResolveRarityByName(kvp.Value);
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

        private static string NormalizeKey(string key)
        {
            if (Enum.TryParse<EquipmentInvUISlot>(key, out EquipmentInvUISlot slot))
                return slot.ToString().ToLowerInvariant();
            return key.ToLowerInvariant();
        }

        public static void Save(ulong playerDbId, ModLootFilter filter)
        {
            try
            {
                Directory.CreateDirectory(BaseDir);

                var persisted = new PersistedFilter
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

        private static PersistedSection SectionToPersisted(ModLootFilterSection section)
        {
            var persisted = new PersistedSection();
            if (section == null)
                return persisted;

            foreach (var kvp in section.Thresholds)
            {
                if (kvp.Value == PrototypeId.Invalid)
                    continue;
                persisted.Thresholds[kvp.Key.ToLowerInvariant()] = ModLootFilterHelper.GetRarityShortName(kvp.Value);
            }
            foreach (var kvp in section.Booleans)
                persisted.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

            foreach (string item in section.ExactItems)
                persisted.ExactItems.Add(item);

            return persisted;
        }

        private static string GetPath(ulong playerDbId) => Path.Combine(BaseDir, $"{playerDbId}.json");

        #endregion
    }

    /// <summary>
    /// Helper for applying loot filters to <see cref="LootResultSummary"/>.
    /// </summary>
    public static class ModLootFilterHelper
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static readonly HashSet<EquipmentInvUISlot> FilterableSlots = new()
        {
            EquipmentInvUISlot.Ring,
            EquipmentInvUISlot.Medal,
            EquipmentInvUISlot.Insignia
        };

        private static readonly HashSet<EquipmentInvUISlot> GearSlots = new()
        {
            EquipmentInvUISlot.Gear01,
            EquipmentInvUISlot.Gear02,
            EquipmentInvUISlot.Gear03,
            EquipmentInvUISlot.Gear04,
            EquipmentInvUISlot.Gear05
        };

        #region Filter apply

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
                        ModLootFilterLogCollator.WriteLine(player.Id, $"[LootFilter] Removed [{protoName}] - reason: {reason}");
                    }
                }
                return shouldFilter;
            });
            if (removed > 0 && lootFilterLogging)
            {
                int vaporized = summary.VaporizedItemSpecs.Count;
                string summaryMsg = $"[LootFilter] Summary: {removed} item(s) removed by LootFilter (pure removal, no credits/XP), {vaporized} item(s) vaporized (converted to credits/PetTech XP) for player [{player}]";
                Logger.Trace(summaryMsg);
                ModLootFilterLogCollator.WriteLine(player.Id, summaryMsg);
            }
        }

        private static bool ShouldFilter(ModLootFilter filter, ItemSpec itemSpec, PrototypeId avatarProtoRef, string avatarName, bool enableCharacterFilter, ulong playerId, bool lootFilterLogging, out string reason)
        {
            reason = null;
            ItemPrototype itemProto = itemSpec.ItemProtoRef.As<ItemPrototype>();
            if (itemProto == null) return false;

            // Global exact-item filter: delete specific prototype paths regardless of rarity or slot
            if (filter.Global.ExactItems.Count > 0)
            {
                string itemProtoName = GameDatabase.GetPrototypeName(itemSpec.ItemProtoRef);
                if (string.IsNullOrEmpty(itemProtoName) == false && filter.Global.ExactItems.Contains(itemProtoName))
                {
                    reason = $"ExactItemFilter ({itemProtoName})";
                    return true;
                }
            }

            // Character-specific filter: remove items bound to other characters, keep Any-Hero items
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
            else if (GearSlots.Contains(slot))
            {
                filterKey = $"slot{(int)slot}";   // Gear01 -> slot1, Gear02 -> slot2, ... Gear05 -> slot5
            }

            // 2. Type-based check (TeamUpGear, Catalyst, UruForged slot)
            if (filterKey == null)
            {
                if (itemProto is TeamUpGearPrototype)
                    filterKey = "teamup";
                else if (IsCatalystPrototype(itemProto))
                    filterKey = "catalyst";
            }

            // 3. Uru-Forged slot check (boolean toggle, identified by equipment slot)
            if (filterKey == null && slot == EquipmentInvUISlot.UruForged)
                filterKey = "uruforged";

            if (filterKey == null)
            {
                if (lootFilterLogging)
                {
                    string msg = $"[LootFilter] Unmatched item [{itemProto.GetType().Name}] slot=[{slot}] protoName=[{itemProto.DataRef.GetName()}]";
                    Logger.Trace(msg);
                    ModLootFilterLogCollator.WriteLine(playerId, msg);
                }
                return false;
            }

            // Boolean filters (e.g. uruforged) - on/off, no rarity threshold.
            if (BooleanFilters.Contains(filterKey))
            {
                if (GetEffectiveBoolean(filter, filterKey, avatarName) == false)
                    return false;

                reason = $"{filterKey} boolean filter (ON)";
                return true;
            }

            // Effective threshold = HIGHER rarity tier of global vs the active character's override.
            PrototypeId thresholdRef = GetEffectiveThreshold(filter, filterKey, avatarName);
            if (thresholdRef == PrototypeId.Invalid)
                return false;

            RarityPrototype itemRarity = itemSpec.RarityProtoRef.As<RarityPrototype>();
            if (itemRarity == null)
            {
                reason = "Rarity lookup failed (null rarity prototype)";
                return false;
            }

            // Special case: threshold is set to the Unique rarity -> only delete unique items that are
            // avatar-restricted to the current character. Any-Hero uniques (EquippableBy Invalid) are preserved.
            PrototypeId rarityUnique = GameDatabase.LootGlobalsPrototype?.RarityUnique ?? PrototypeId.Invalid;
            if (thresholdRef == rarityUnique)
            {
                if (itemSpec.RarityProtoRef != rarityUnique)
                    return false;

                if (itemSpec.EquippableBy == PrototypeId.Invalid)
                    return false; // Any-Hero unique: never delete

                if (itemSpec.EquippableBy != avatarProtoRef)
                    return false; // Bound to another character (already handled by character-specific mode, or simply not ours)

                reason = $"{filterKey} unique filter (restricted to {GameDatabase.GetPrototypeName(itemSpec.EquippableBy)}, current avatar is {GameDatabase.GetPrototypeName(avatarProtoRef)})";
                return true;
            }

            RarityPrototype thresholdRarity = thresholdRef.As<RarityPrototype>();
            if (thresholdRarity == null)
            {
                reason = "Rarity lookup failed (null threshold rarity prototype)";
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

        #endregion

        #region highest threshold

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
        public static PrototypeId GetEffectiveThreshold(ModLootFilter filter, string filterKey, string avatarName)
        {
            if (filter == null) return PrototypeId.Invalid;

            PrototypeId result = PrototypeId.Invalid;
            if (filter.Global.Thresholds.TryGetValue(filterKey, out PrototypeId globalRef))
                result = globalRef;

            ModLootFilterSection charSection = filter.GetCharacterSection(avatarName);
            if (charSection != null && charSection.Thresholds.TryGetValue(filterKey, out PrototypeId charRef))
                result = HigherTier(result, charRef);

            return result;
        }

        /// <summary>
        /// Computes the effective boolean toggle for an item type: global OR the active character's override.
        /// </summary>
        public static bool GetEffectiveBoolean(ModLootFilter filter, string filterKey, string avatarName)
        {
            if (filter == null) return false;

            bool result = filter.Global.Booleans.TryGetValue(filterKey, out bool globalVal) && globalVal;

            ModLootFilterSection charSection = filter.GetCharacterSection(avatarName);
            if (charSection != null && charSection.Booleans.TryGetValue(filterKey, out bool charVal) && charVal)
                result = true;

            return result;
        }

        #endregion

        #region Rarity names

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

        #endregion

        #region short names

        public static readonly HashSet<string> BooleanFilters = new(StringComparer.OrdinalIgnoreCase)
        {
            "uruforged",
        };

        public static readonly Dictionary<string, string> FilterNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["slot1"] = "slot1",
            ["gear01"] = "slot1",
            ["slot2"] = "slot2",
            ["gear02"] = "slot2",
            ["slot3"] = "slot3",
            ["gear03"] = "slot3",
            ["slot4"] = "slot4",
            ["gear04"] = "slot4",
            ["slot5"] = "slot5",
            ["gear05"] = "slot5",
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

        #endregion
    }
}
