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
    /// Per-player loot filter settings for non-gear slots and special item types.
    /// Stored in a local JSON file on the server, not synced to the game client.
    /// </summary>
    public class PlayerLootFilter
    {
        public Dictionary<string, PrototypeId> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles loading and saving <see cref="PlayerLootFilter"/> to disk.
    /// </summary>
    public static class PlayerLootFilterStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly string BaseDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "PlayerLootFilters");

        /// <summary>
        /// JSON DTO for backward-compatible serialization of <see cref="PlayerLootFilter"/>.
        /// </summary>
        private class PersistedFilter
        {
            public Dictionary<string, ulong> Thresholds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, bool> Booleans { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        public static PlayerLootFilter Load(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            if (File.Exists(path) == false)
                return new PlayerLootFilter();

            try
            {
                string json = File.ReadAllText(path);

                // Try new PersistedFilter format first
                var persisted = JsonSerializer.Deserialize<PersistedFilter>(json);
                if (persisted != null)
                {
                    var filter = new PlayerLootFilter();
                    if (persisted.Booleans != null)
                        foreach (var kvp in persisted.Booleans)
                            filter.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

                    if (persisted.Thresholds != null)
                    {
                        foreach (var kvp in persisted.Thresholds)
                        {
                            string key = kvp.Key;
                            if (Enum.TryParse<EquipmentInvUISlot>(key, out EquipmentInvUISlot slot))
                                key = slot.ToString().ToLowerInvariant();
                            else
                                key = key.ToLowerInvariant();

                            filter.Thresholds[key] = (PrototypeId)kvp.Value;
                        }
                    }
                    return filter;
                }

                // Fallback: old flat dictionary format (all ulong thresholds)
                var raw = JsonSerializer.Deserialize<Dictionary<string, ulong>>(json);
                if (raw == null) return new PlayerLootFilter();

                var legacyFilter = new PlayerLootFilter();
                foreach (var kvp in raw)
                {
                    string key = kvp.Key;
                    if (Enum.TryParse<EquipmentInvUISlot>(key, out EquipmentInvUISlot slot))
                        key = slot.ToString().ToLowerInvariant();
                    else
                        key = key.ToLowerInvariant();

                    legacyFilter.Thresholds[key] = (PrototypeId)kvp.Value;
                }
                return legacyFilter;
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to load loot filter for player {playerDbId}: {e.Message}");
                return new PlayerLootFilter();
            }
        }

        public static void Save(ulong playerDbId, PlayerLootFilter filter)
        {
            try
            {
                Directory.CreateDirectory(BaseDir);
                var persisted = new PersistedFilter();
                foreach (var kvp in filter.Thresholds)
                    persisted.Thresholds[kvp.Key.ToLowerInvariant()] = (ulong)kvp.Value;
                foreach (var kvp in filter.Booleans)
                    persisted.Booleans[kvp.Key.ToLowerInvariant()] = kvp.Value;

                string json = JsonSerializer.Serialize(persisted, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetPath(playerDbId), json);
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to save loot filter for player {playerDbId}: {e.Message}");
            }
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
            if (player.Game?.CustomGameOptions?.EnableLootFilters == false) return;

            bool enableCharacterFilter = player.Game?.CustomGameOptions?.EnableCharacterSpecificLootFilter == true;
            int removed = summary.ItemSpecs.RemoveAll(itemSpec =>
            {
                bool shouldFilter = ShouldFilter(player.LootFilter, itemSpec, avatarProtoRef, enableCharacterFilter, out string reason);
                if (shouldFilter)
                {
                    ItemPrototype itemProto = itemSpec.ItemProtoRef.As<ItemPrototype>();
                    string protoName = itemProto?.DataRef.GetName() ?? "unknown";
                    Logger.Trace($"[LootFilter] Removed [{protoName}] — reason: {reason}");
                }
                return shouldFilter;
            });
            if (removed > 0)
                Logger.Trace($"[LootFilter] Removed {removed} filtered item(s) for player [{player}]");
        }

        private static bool ShouldFilter(PlayerLootFilter filter, ItemSpec itemSpec, PrototypeId avatarProtoRef, bool enableCharacterFilter, out string reason)
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
                Logger.Trace($"[LootFilter] Unmatched item [{itemProto.GetType().Name}] protoName=[{itemProto.DataRef.GetName()}]");
                return false;
            }

            // Boolean filters (e.g. uruforged) - on/off, no rarity threshold
            if (BooleanFilters.Contains(filterKey))
            {
                if (filter.Booleans.TryGetValue(filterKey, out bool enabled) == false || enabled == false)
                    return false;

                if (filterKey == "uruforged" && itemSpec.RarityProtoRef == GameDatabase.LootGlobalsPrototype.RarityUruForged)
                {
                    reason = "uruforged boolean filter";
                    return true;
                }

                return false;
            }

            if (filter.Thresholds.TryGetValue(filterKey, out PrototypeId thresholdRef) == false || thresholdRef == PrototypeId.Invalid)
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

        // --- Command helpers ---

        private static Dictionary<string, PrototypeId> _rarityNameMap;
        private static readonly object _rarityMapLock = new();

        private static void EnsureRarityMapBuilt()
        {
            lock (_rarityMapLock)
            {
                if (_rarityNameMap != null) return;

                _rarityNameMap = new(StringComparer.OrdinalIgnoreCase);
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
                }
            }
        }

        public static PrototypeId ResolveRarityByName(string name)
        {
            EnsureRarityMapBuilt();
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
