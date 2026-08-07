#region Item Merge Best Affix
// =============================================================================
// MOD Item Merge Best Affix
// =============================================================================
//   merges pairs of identical items (same prototype, rarity, item level) into a
//   single new copy whose affix values are the maximum of each corresponding
//   affix from both source items.
//
//   Cost: 2 source items + 1 Cosmic Essence (configurable cost item, or free).
//
//   Commands (see ModItemMergeBestAffixCommands.cs):
//     !merge any            -> merge one pair of any eligible items
//     !merge <itemType>     -> merge one pair of a specific item type (e.g. ring)
//     !merge <rarity>       -> merge one pair of a specific rarity (e.g. unique)
//     !merge dryrun         -> show what would be merged (no consumption)
//     !merge dryrun <filter>-> dry run with a type/rarity filter
//
//   Only scans the player's personal general inventory. Does NOT touch
//   equipped items or stash.
//
//   Processes only ONE merge per command call.
//
//  VERSION:: 20260805
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MHServerEmu.Core.Extensions;
using MHServerEmu.Core.Helpers;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.System.Random;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Properties.Evals;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private static readonly Logger ItemMergeBestAffixLogger = LogManager.CreateLogger();

        /// <summary>
        /// Filter mode for the merge command.
        /// </summary>
        private enum MergeFilterMode
        {
            None,       // no filter - merge any eligible pair
            Rarity,     // filter by rarity name
            ItemType,   // filter by item type keyword in prototype name
            DryRun,     // dry run - no consumption, report only
        }

        /// <summary>
        /// Parsed filter for a merge command invocation.
        /// </summary>
        private struct MergeFilter
        {
            public MergeFilterMode Mode;
            public string FilterValue;     // rarity name or item type keyword
            public bool IsDryRun;

            public static MergeFilter None => new() { Mode = MergeFilterMode.None };

            public bool Matches(Item item)
            {
                if (Mode == MergeFilterMode.None || Mode == MergeFilterMode.DryRun)
                    return true;

                if (Mode == MergeFilterMode.Rarity)
                {
                    RarityPrototype rarityProto = item.RarityPrototype;
                    if (rarityProto == null) return false;
                    string rarityName = rarityProto.DataRef.GetName();
                    string fileName = System.IO.Path.GetFileName(rarityName);
                    if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                        fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);
                    return fileName.Equals(FilterValue, StringComparison.OrdinalIgnoreCase);
                }

                if (Mode == MergeFilterMode.ItemType)
                {
                    string protoName = item.PrototypeDataRef.GetName();
                    return protoName != null && protoName.Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
                }

                return true;
            }
        }

        #region Command 

        /// <summary>
        /// Main entry point for the !merge command.
        /// Processes at most one merge per call.
        /// </summary>
        public string ModItemMergeBestAffix(string[] @params)
        {
            if (Game.CustomGameOptions.ModItemMergeBestAffixCommandEnable == false)
                return "Item merge is disabled by server settings.";

            // Parse the filter from params
            MergeFilter filter = ParseMergeFilter(@params);
            if (filter.Mode == MergeFilterMode.Rarity && filter.FilterValue == null)
                return $"Unknown rarity. Valid names: {GetValidRarityNamesForMerge()}.";

            // Resolve the currency (Cosmic Essence) prototype
            bool isFreeMerge = Game.CustomGameOptions.ModItemMergeBestAffixFree;
            PrototypeId currencyRef = ResolveMergeCurrencyProtoRef();
            if (isFreeMerge == false && currencyRef == PrototypeId.Invalid)
                return "Merge currency item is not configured correctly on the server.";

            // Dry run: build a full diagnostic report showing ALL items and rejection reasons
            if (filter.IsDryRun)
            {
                int availableCurrency = isFreeMerge ? int.MaxValue : CountCurrencyInInventory(currencyRef);
                return BuildDryRunReport(filter, currencyRef, availableCurrency);
            }

            // Scan the personal inventory for mergeable items
            List<Item> candidates = CollectMergeCandidates(filter);
            if (candidates.Count == 0)
            {
                string filterDesc = filter.Mode == MergeFilterMode.None ? "" : $" matching filter '{filter.FilterValue}'";
                return $"No mergeable items found{filterDesc} in your general inventory. Run !merge dryrun for a full diagnostic.";
            }

            // Group candidates by (prototype, rarity, item level)
            // Use raw long values for the key to avoid any enum/struct equality edge cases.
            Dictionary<(long Proto, long Rarity, int Level), List<Item>> groups = new();
            foreach (Item item in candidates)
            {
                var key = ((long)item.PrototypeDataRef,
                           item.Properties[PropertyEnum.ItemRarity].RawLong,
                           (int)item.Properties[PropertyEnum.ItemLevel]);
                if (!groups.TryGetValue(key, out var list))
                {
                    list = new List<Item>();
                    groups[key] = list;
                }
                list.Add(item);
            }

            // Find the first group with at least 2 items
            Item sourceA = null;
            Item sourceB = null;
            foreach (var pair in groups)
            {
                if (pair.Value.Count >= 2)
                {
                    sourceA = pair.Value[0];
                    sourceB = pair.Value[1];
                    break;
                }
            }

            if (sourceA == null || sourceB == null)
                return "No mergeable item pairs found (need at least 2 of the same item). Run !merge dryrun for a full diagnostic.";

            // Check currency (skip if free merge is enabled)
            if (isFreeMerge == false)
            {
                int currencyCount = CountCurrencyInInventory(currencyRef);
                if (currencyCount < 1)
                {
                    string currencyName = currencyRef.GetName();
                    return $"You need 1 {GetShortPrototypeName(currencyName)} to merge items. You have {currencyCount}.";
                }
            }

            // Execute the merge
            if (!TryCreateMergedItem(sourceA, sourceB, out Item mergedItem, out string error))
            {
                ItemMergeBestAffixLogger.Warn($"ModItemMergeBestAffix(): Failed to create merged item for {sourceA.PrototypeDataRef.GetName()}: {error}");
                return $"Merge failed: {error}";
            }

            // Consume source items
            DestroyOrDecrementItem(sourceA);
            DestroyOrDecrementItem(sourceB);

            // Consume 1 currency (skip if free merge is enabled)
            if (isFreeMerge == false)
            {
                if (!TryConsumeCurrencyItem(currencyRef, 1))
                {
                    // This should not happen since we checked above, but handle gracefully
                    ItemMergeBestAffixLogger.Error($"ModItemMergeBestAffix(): Failed to consume currency after creating merged item.");
                }
            }

            // Write merge result JSON dump (if logging is enabled)
            if (Game.CustomGameOptions.ModItemMergeBestAffixLoggingEnable)
            {
                string resultPath = WriteMergeResultJsonDump(sourceA, sourceB, mergedItem, currencyRef);
                if (resultPath != null)
                    ItemMergeBestAffixLogger.Info($"Merge result dump written to: {resultPath}");
            }

            string itemName = GetShortPrototypeName(sourceA.PrototypeDataRef.GetName());
            if (isFreeMerge)
                return $"Merged 2x {itemName} into 1 with best affix rolls. (Free merge - no currency consumed.)";
            else
                return $"Merged 2x {itemName} into 1 with best affix rolls. 1 {GetShortPrototypeName(currencyRef.GetName())} consumed.";
        }

        #endregion

        #region Filter Parsing

        private MergeFilter ParseMergeFilter(string[] @params)
        {
            if (@params == null || @params.Length == 0)
                return MergeFilter.None;

            string first = @params[0].Trim();

            // dryrun / dry-run / preview
            if (first.Equals("dryrun", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("dry-run", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("preview", StringComparison.OrdinalIgnoreCase))
            {
                if (@params.Length > 1)
                {
                    string second = @params[1].Trim();
                    MergeFilter subFilter = ParseSingleFilter(second);
                    subFilter.IsDryRun = true;
                    if (subFilter.Mode == MergeFilterMode.None)
                        subFilter.Mode = MergeFilterMode.DryRun;
                    return subFilter;
                }
                return new MergeFilter { Mode = MergeFilterMode.DryRun, IsDryRun = true };
            }

            return ParseSingleFilter(first);
        }

        private MergeFilter ParseSingleFilter(string value)
        {
            // Check if it's a rarity name
            PrototypeId rarityRef = ResolveRarityByNameForMerge(value);
            if (rarityRef != PrototypeId.Invalid)
            {
                return new MergeFilter { Mode = MergeFilterMode.Rarity, FilterValue = value };
            }

            // Otherwise treat it as an item type keyword
            return new MergeFilter { Mode = MergeFilterMode.ItemType, FilterValue = value };
        }

        #endregion

        #region Rarity 

        private static Dictionary<string, PrototypeId> _mergeRarityNameMap;
        private static readonly object _mergeRarityMapLock = new();

        private static void EnsureMergeRarityMapBuilt()
        {
            lock (_mergeRarityMapLock)
            {
                if (_mergeRarityNameMap != null) return;

                _mergeRarityNameMap = new(StringComparer.OrdinalIgnoreCase);
                foreach (PrototypeId rarityRef in DataDirectory.Instance.IteratePrototypesInHierarchy<RarityPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                {
                    string fullName = rarityRef.GetName();
                    string fileName = System.IO.Path.GetFileName(fullName);

                    if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                        fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);

                    _mergeRarityNameMap[fileName] = rarityRef;

                    // Also map without the R# prefix (e.g. "R1Common" -> "Common")
                    string suffix = System.Text.RegularExpressions.Regex.Replace(fileName, @"^R\d+", "");
                    if (string.IsNullOrEmpty(suffix) == false)
                        _mergeRarityNameMap[suffix] = rarityRef;
                }
            }
        }

        private static PrototypeId ResolveRarityByNameForMerge(string name)
        {
            EnsureMergeRarityMapBuilt();
            if (string.IsNullOrWhiteSpace(name))
                return PrototypeId.Invalid;

            name = name.Trim();
            if (name.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".prototype".Length);

            if (_mergeRarityNameMap.TryGetValue(name, out PrototypeId rarityRef))
                return rarityRef;
            return PrototypeId.Invalid;
        }

        private static string GetValidRarityNamesForMerge()
        {
            EnsureMergeRarityMapBuilt();
            var names = new List<string>(_mergeRarityNameMap.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(", ", names);
        }

        #endregion

        #region Currency use

        private PrototypeId ResolveMergeCurrencyProtoRef()
        {
            string protoName = Game.CustomGameOptions.ModItemMergeBestAffixCurrencyProtoName;
            if (string.IsNullOrWhiteSpace(protoName))
                return PrototypeId.Invalid;

            PrototypeId refId = GameDatabase.GetPrototypeRefByName(protoName);
            if (refId == PrototypeId.Invalid) return PrototypeId.Invalid;

            // Verify it resolves to an actual item prototype
            ItemPrototype itemProto = GameDatabase.GetPrototype<ItemPrototype>(refId);
            if (itemProto == null) return PrototypeId.Invalid;

            return refId;
        }

        #endregion

        #region Inventory Scan

        /// <summary>
        /// Collects items from the player's personal general inventory that are
        /// eligible for merging. Does NOT scan equipment or stash.
        /// </summary>
        private List<Item> CollectMergeCandidates(MergeFilter filter)
        {
            List<Item> candidates = new();
            EntityManager em = Game.EntityManager;

            // Scan General inventory
            CollectFromInventory(GetInventory(InventoryConvenienceLabel.General), em, filter, candidates);

            return candidates;
        }

        private void CollectFromInventory(Inventory inventory, EntityManager em, MergeFilter filter, List<Item> candidates)
        {
            if (inventory == null) return;

            foreach (var entry in inventory)
            {
                Item item = em.GetEntity<Item>(entry.Id);
                if (item == null || item.IsScheduledToDestroy || item.IsDestroyed) continue;
                if (CheckEligibilityReason(item, filter) != null) continue;

                candidates.Add(item);
            }
        }

        /// <summary>
        /// Single source of truth for item eligibility. Returns null if the item is eligible
        /// for merging, or a human-readable rejection reason. Both the merge path and the
        /// dry run path use this method to avoid logic drift.
        /// </summary>
        private string CheckEligibilityReason(Item item, MergeFilter filter)
        {
            if (item?.ItemPrototype == null)
                return "null item or prototype";

            if (item.IsCurrencyItem())
                return "currency item";

            // Items need either rolled affix properties OR built-in properties to be mergeable.
            // Regular gear has AffixPropertiesEntries (from OnAffixAdded).
            // Unique items have PropertiesBuiltIn (rolled via OnBuiltInPropertyRoll).
            bool hasAffixProps = item.AffixPropertiesEntries.Count > 0;
            bool hasBuiltInProps = item.ItemPrototype.PropertiesBuiltIn.HasValue();
            if (hasAffixProps == false && hasBuiltInProps == false)
                return "no rollable stats (AffixPropertiesEntries=0, PropertiesBuiltIn=empty)";

            if (Game.CustomGameOptions.ModItemMergeBestAffixSkipBoundItems && item.IsBoundToCharacter)
                return "bound to character (ModItemMergeBestAffixSkipBoundItems=true)";

            if (filter.Matches(item) == false)
                return $"filter mismatch (mode={filter.Mode}, value={filter.FilterValue})";

            return null;
        }

        #endregion

        #region Currency 

        private int CountCurrencyInInventory(PrototypeId currencyProtoRef)
        {
            if (currencyProtoRef == PrototypeId.Invalid) return 0;

            int count = 0;
            EntityManager em = Game.EntityManager;

            void CountInInventory(Inventory inv)
            {
                if (inv == null) return;
                foreach (var entry in inv)
                {
                    Item item = em.GetEntity<Item>(entry.Id);
                    if (item == null || item.IsScheduledToDestroy || item.IsDestroyed) continue;
                    if (item.PrototypeDataRef == currencyProtoRef)
                        count += item.CurrentStackSize;
                }
            }

            CountInInventory(GetInventory(InventoryConvenienceLabel.General));

            return count;
        }

        /// <summary>
        /// Consumes the specified number of a physical item from the personal inventory.
        /// Returns false if not enough are available.
        /// </summary>
        private bool TryConsumeCurrencyItem(PrototypeId currencyProtoRef, int count)
        {
            if (currencyProtoRef == PrototypeId.Invalid || count <= 0) return false;

            Inventory generalInv = GetInventory(InventoryConvenienceLabel.General);

            EntityManager em = Game.EntityManager;
            int remaining = count;
            List<Item> toConsume = new();

            // Collect matching items from the general inventory
            void CollectFrom(Inventory inv)
            {
                if (inv == null) return;
                foreach (var entry in inv)
                {
                    if (remaining <= 0) return;
                    Item item = em.GetEntity<Item>(entry.Id);
                    if (item == null || item.IsScheduledToDestroy || item.IsDestroyed) continue;
                    if (item.PrototypeDataRef != currencyProtoRef) continue;

                    toConsume.Add(item);
                    remaining -= item.CurrentStackSize;
                }
            }

            CollectFrom(generalInv);

            if (remaining > 0) return false;  // Not enough

            // Consume
            remaining = count;
            foreach (Item item in toConsume)
            {
                if (remaining <= 0) break;
                int take = Math.Min(item.CurrentStackSize, remaining);
                if (item.CurrentStackSize > take)
                    item.DecrementStack(take);
                else
                    item.Destroy();
                remaining -= take;
            }

            return true;
        }

        #endregion

        #region Item Consume

        private static void DestroyOrDecrementItem(Item item)
        {
            if (item == null || item.IsDestroyed) return;

            if (item.CurrentStackSize > 1)
                item.DecrementStack(1);
            else
                item.Destroy();
        }

        #endregion

        #region Merged Item 

        // Seed search parameters are read from CustomGameOptionsConfig:
        //   ModItemMergeBestAffixSeedSearchCount - total seeds to try (default 100000)
        //   ModItemMergeBestAffixSeedBatchSize   - seeds per batch, with 1ms yield between batches

        /// <summary>
        /// Creates a new merged item from sourceA and sourceB. Instead of overriding property
        /// values after creation (which would be lost on save/reload since properties are re-rolled
        /// from the seed), this method searches for seeds that naturally produce values closest
        /// to the maximum of each corresponding property from both sources.
        /// </summary>
        private bool TryCreateMergedItem(Item sourceA, Item sourceB, out Item mergedItem, out string error)
        {
            mergedItem = null;
            error = null;

            // Validate eligibility
            if (AreMergeable(sourceA, sourceB) == false)
            {
                error = "items are not mergeable (different prototype, rarity, or level).";
                return false;
            }

            ItemSpec sourceSpec = sourceA.ItemSpec;

            // Step 1: Find the best main seed (for PropertiesBuiltIn rolling)
            int bestItemSeed = FindBestItemSeed(sourceA, sourceB, sourceSpec);

            // Step 2: Find the best affix seeds (for rolled affix properties)
            List<AffixSpec> bestAffixSpecs = FindBestAffixSpecs(sourceA, sourceB, sourceSpec);

            // Step 3: Build the ItemSpec with the best seeds
            ItemSpec newSpec = new ItemSpec(
                sourceSpec.ItemProtoRef,
                sourceSpec.RarityProtoRef,
                sourceSpec.ItemLevel,
                0,
                bestAffixSpecs,
                bestItemSeed,
                sourceSpec.EquippableBy
            );

            // Step 4: Create the entity with the best seeds
            using EntitySettings settings = ObjectPoolManager.Instance.Get<EntitySettings>();
            settings.EntityRef = sourceSpec.ItemProtoRef;
            settings.ItemSpec = newSpec;

            mergedItem = Game.EntityManager.CreateEntity(settings) as Item;
            if (mergedItem == null)
            {
                error = "failed to create merged item entity.";
                return false;
            }

            mergedItem.Properties[PropertyEnum.InventoryStackCount] = 1;

            // Inherit binding state (if either source is bound, the result is bound)
            if (sourceA.IsBoundToCharacter || sourceB.IsBoundToCharacter)
            {
                PrototypeId boundAgent = sourceA.IsBoundToCharacter
                    ? sourceA.BoundAgentProtoRef
                    : sourceB.BoundAgentProtoRef;
                mergedItem.ItemSpec.SetBindingState(true, boundAgent);
            }

            // Add to player inventory
            InventoryResult result = AcquireItem(mergedItem, PrototypeId.Invalid);
            if (result != InventoryResult.Success)
            {
                mergedItem.Destroy();
                mergedItem = null;
                error = $"failed to add merged item to inventory ({result}).";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Searches for the item seed that produces PropertiesBuiltIn values closest to
        /// the maximum of each corresponding property from sourceA and sourceB.
        /// Instead of creating temporary entities, this directly simulates the GRandom rolling
        /// logic (GRandom(seed) -> NextFloat() for each PickInRange entry) which is orders of
        /// magnitude faster. The min/max ranges are evaluated once from the source item's properties.
        /// </summary>
        private int FindBestItemSeed(Item sourceA, Item sourceB, ItemSpec sourceSpec)
        {
            ItemPrototype itemProto = sourceA.ItemPrototype;
            if (itemProto?.PropertiesBuiltIn.HasValue() != true)
                return sourceSpec.Seed;

            // Precompute the min/max ranges and desired values for each PickInRange property.
            // The ranges are evaluated from the source item's properties (item level, etc.) and
            // don't change between seeds, so we only eval them once.
            // For negative ranges (valueMax <= 0, e.g. cost reduction), more negative is better,
            // so the desired value is the min of both sources. For positive ranges, higher is better.
            var rollEntries = new List<(PropertyId propId, PropertyDataType dataType, float valueMin, float valueMax, bool rollAsInteger, float desiredVal)>();

            using EvalContextData evalContext = ObjectPoolManager.Instance.Get<EvalContextData>();
            evalContext.SetReadOnlyVar_PropertyCollectionPtr(EvalContext.Entity, sourceA.Properties);

            foreach (PropertyEntryPrototype propEntry in itemProto.PropertiesBuiltIn)
            {
                if (propEntry is not PropertyPickInRangeEntryPrototype pickInRange) continue;
                PropertyId propId = pickInRange.Prop;
                if (sourceA.Properties.HasProperty(propId) == false) continue;
                if (sourceB.Properties.HasProperty(propId) == false) continue;

                PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum);
                if (propInfo.DataType != PropertyDataType.Real && propInfo.DataType != PropertyDataType.Integer)
                    continue;

                float valueMin = pickInRange.ValueMin != null ? Eval.RunFloat(pickInRange.ValueMin, evalContext) : 0f;
                float valueMax = pickInRange.ValueMax != null ? Eval.RunFloat(pickInRange.ValueMax, evalContext) : 0f;

                float valA = propInfo.DataType == PropertyDataType.Integer
                    ? (int)sourceA.Properties[propId]
                    : (float)sourceA.Properties[propId];
                float valB = propInfo.DataType == PropertyDataType.Integer
                    ? (int)sourceB.Properties[propId]
                    : (float)sourceB.Properties[propId];

                // For negative ranges (e.g. cost reduction), more negative is better -> take min.
                // For positive ranges, higher is better -> take max.
                float desiredVal = (valueMax <= 0f) ? Math.Min(valA, valB) : Math.Max(valA, valB);
                rollEntries.Add((propId, propInfo.DataType, valueMin, valueMax, pickInRange.RollAsInteger, desiredVal));
            }

            if (rollEntries.Count == 0)
                return sourceSpec.Seed;

            // Search for the best seed by simulating GRandom directly.
            // Score = sum of squared absolute differences from desired values.
            // This penalizes both below AND above the desired value, so the seed
            // naturally rolls closest to the desired value without overshooting.
            // The search is processed in batches with a 1ms yield between batches
            // to avoid CPU spikes on the server.
            int searchCount = Game.CustomGameOptions.ModItemMergeBestAffixSeedSearchCount;
            int batchSize = Game.CustomGameOptions.ModItemMergeBestAffixSeedBatchSize;
            int bestSeed = sourceSpec.Seed;
            float bestScore = float.MaxValue;

            for (int i = 0; i < searchCount; i++)
            {
                int candidateSeed = i + 1;
                GRandom random = new(candidateSeed);

                float score = 0f;
                foreach (var (_, dataType, valueMin, valueMax, rollAsInteger, desiredVal) in rollEntries)
                {
                    float randomMult = random.NextFloat();
                    float rolledVal = SimulateRoll(randomMult, valueMin, valueMax, rollAsInteger, dataType);
                    float diff = rolledVal - desiredVal;
                    score += diff * diff;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestSeed = candidateSeed;
                    if (score == 0f) break;
                }

                // Yield between batches to avoid CPU spikes
                if (batchSize > 0 && (i + 1) % batchSize == 0)
                    System.Threading.Thread.Sleep(1);
            }

            ItemMergeBestAffixLogger.Info($"FindBestItemSeed(): Best seed={bestSeed}, score={bestScore:F2} (searched {searchCount} seeds)");
            return bestSeed;
        }

        /// <summary>
        /// Searches for the target affix seeds for each rolled affix. Directly simulates the GRandom
        /// rolling logic for affix PropertyEntries without creating temporary entities.
        /// The affix rolling sequence is: GRandom(affixSeed) -> NextFloat() for PowerBoost (if any),
        /// then NextFloat() for PowerGrantRank (if any), then NextFloat() for each PropertyEntry.
        /// </summary>
        private List<AffixSpec> FindBestAffixSpecs(Item sourceA, Item sourceB, ItemSpec sourceSpec)
        {
            var result = new List<AffixSpec>();
            foreach (AffixSpec spec in sourceSpec.AffixSpecs)
                result.Add(new AffixSpec(spec));

            for (int affixIdx = 0; affixIdx < result.Count; affixIdx++)
            {
                AffixSpec affixSpec = result[affixIdx];
                if (affixSpec.AffixProto == null) continue;
                if (affixSpec.AffixProto.Position == AffixPosition.Metadata) continue;

                AffixPrototype affixProto = affixSpec.AffixProto;

                // Collect desired max values from both sources for this affix
                AffixPropertiesCopyEntry? entryA = FindAffixEntry(sourceA, affixProto);
                AffixPropertiesCopyEntry? entryB = FindAffixEntry(sourceB, affixProto);
                if (entryA == null && entryB == null) continue;

                // Precompute the rolling entries from the affix prototype's PropertyEntries.
                // These are the PickInRange entries that get rolled with random.NextFloat().
                // We also store the range direction (valueMax <= 0 means negative range = more negative is better).
                var affixRollEntries = new List<(PropertyId propId, PropertyDataType dataType, float valueMin, float valueMax, bool rollAsInteger)>();

                if (affixProto.PropertyEntries != null)
                {
                    using EvalContextData affixEvalContext = ObjectPoolManager.Instance.Get<EvalContextData>();
                    affixEvalContext.SetReadOnlyVar_PropertyCollectionPtr(EvalContext.Entity, sourceA.Properties);

                    foreach (PropertyPickInRangeEntryPrototype propEntry in affixProto.PropertyEntries)
                    {
                        PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propEntry.Prop.Enum);
                        if (propInfo.DataType != PropertyDataType.Real && propInfo.DataType != PropertyDataType.Integer)
                            continue;

                        float valueMin = propEntry.ValueMin != null ? Eval.RunFloat(propEntry.ValueMin, affixEvalContext) : 0f;
                        float valueMax = propEntry.ValueMax != null ? Eval.RunFloat(propEntry.ValueMax, affixEvalContext) : 0f;

                        affixRollEntries.Add((propEntry.Prop, propInfo.DataType, valueMin, valueMax, propEntry.RollAsInteger));
                    }
                }

                if (affixRollEntries.Count == 0) continue;

                // Build a lookup from propId -> (rangeMax) so we can determine direction for desired value
                var rangeLookup = new Dictionary<PropertyId, float>();
                foreach (var (propId, _, _, valueMax, _) in affixRollEntries)
                    rangeLookup[propId] = valueMax;

                // Build list of (propId, dataType, desiredVal) from both sources.
                // For negative ranges (valueMax <= 0), more negative is better -> take min.
                // For positive ranges, higher is better -> take max.
                var desiredVals = new List<(PropertyId propId, float desiredVal)>();
                var allProps = new HashSet<PropertyId>();
                if (entryA?.Properties != null)
                    foreach (var kvp in entryA.Value.Properties)
                        if (kvp.Key.Enum != PropertyEnum.ItemLevel)  // Skip ItemLevel, it's not rolled
                            allProps.Add(kvp.Key);
                if (entryB?.Properties != null)
                    foreach (var kvp in entryB.Value.Properties)
                        if (kvp.Key.Enum != PropertyEnum.ItemLevel)
                            allProps.Add(kvp.Key);

                foreach (PropertyId propId in allProps)
                {
                    PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum);
                    if (propInfo.DataType != PropertyDataType.Real && propInfo.DataType != PropertyDataType.Integer)
                        continue;

                    float valA = 0f, valB = 0f;
                    bool hasA = entryA?.Properties?.HasProperty(propId) == true;
                    bool hasB = entryB?.Properties?.HasProperty(propId) == true;
                    if (!hasA && !hasB) continue;

                    if (hasA)
                        valA = propInfo.DataType == PropertyDataType.Integer
                            ? (int)entryA.Value.Properties[propId]
                            : (float)entryA.Value.Properties[propId];
                    if (hasB)
                        valB = propInfo.DataType == PropertyDataType.Integer
                            ? (int)entryB.Value.Properties[propId]
                            : (float)entryB.Value.Properties[propId];

                    // Determine direction from the range. If we don't have range info, default to max.
                    float rangeMax = 0f;
                    bool hasRange = rangeLookup.TryGetValue(propId, out rangeMax);
                    float desiredVal = (hasRange && rangeMax <= 0f) ? Math.Min(valA, valB) : Math.Max(valA, valB);
                    desiredVals.Add((propId, desiredVal));
                }

                if (desiredVals.Count == 0) continue;

                // Search for the target affix seed by simulating GRandom.
                // Score = sum of squared absolute differences from desired values.
                // Processed in batches with a 1ms yield between batches to avoid CPU spikes.
                int searchCount = Game.CustomGameOptions.ModItemMergeBestAffixSeedSearchCount;
                int batchSize = Game.CustomGameOptions.ModItemMergeBestAffixSeedBatchSize;
                int bestAffixSeed = affixSpec.Seed;
                float bestScore = float.MaxValue;

                for (int i = 0; i < searchCount; i++)
                {
                    int candidateSeed = i + 1;
                    GRandom random = new(candidateSeed);

                    // Simulate the rolling sequence:
                    // 1. PowerBoost (if AffixPowerModifierPrototype with PowerBoostMax > 0) - consumes one NextFloat()
                    // 2. PowerGrantRank (if AffixPowerModifierPrototype with PowerGrantRankMax > 0) - consumes one NextFloat()
                    // 3. Each PropertyEntry - consumes one NextFloat()
                    // We need to consume the same number of NextFloat() calls as the real code.
                    // NOTE: If the affix is an AffixPowerModifierPrototype, the PowerBoost/PowerGrantRank
                    // rolls happen before the PropertyEntries and consume extra NextFloat() calls.

                    AffixPowerModifierPrototype powerModProto = affixProto as AffixPowerModifierPrototype;
                    if (powerModProto != null)
                    {
                        // Eval PowerBoostMax and PowerGrantRankMax to check if they consume NextFloat()
                        using EvalContextData powerEvalContext = ObjectPoolManager.Instance.Get<EvalContextData>();
                        powerEvalContext.SetReadOnlyVar_PropertyCollectionPtr(EvalContext.Entity, sourceA.Properties);
                        powerEvalContext.SetVar_Int(EvalContext.Var1, (int)sourceA.Properties[PropertyEnum.ItemLevel]);
                        powerEvalContext.SetVar_Int(EvalContext.Var2, 0);

                        int powerBoostMax = Eval.RunInt(powerModProto.PowerBoostMax, powerEvalContext);
                        if (powerBoostMax > 0)
                            random.NextFloat();  // Consume the NextFloat() for PowerBoost

                        int powerGrantMaxRank = Eval.RunInt(powerModProto.PowerGrantRankMax, powerEvalContext);
                        if (powerGrantMaxRank > 0)
                            random.NextFloat();  // Consume the NextFloat() for PowerGrantRank
                    }

                    // Now roll each PropertyEntry
                    float score = 0f;
                    foreach (var (propId, dataType, valueMin, valueMax, rollAsInteger) in affixRollEntries)
                    {
                        float randomMult = random.NextFloat();
                        float rolledVal = SimulateRoll(randomMult, valueMin, valueMax, rollAsInteger, dataType);

                        // Find the desired value for this property
                        float propDesiredVal = 0f;
                        bool found = false;
                        foreach (var (dPropId, dVal) in desiredVals)
                        {
                            if (dPropId == propId) { propDesiredVal = dVal; found = true; break; }
                        }
                        if (!found) continue;

                        float diff = rolledVal - propDesiredVal;
                        score += diff * diff;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestAffixSeed = candidateSeed;
                        if (score == 0f) break;
                    }

                    // Yield between batches to avoid CPU spikes
                    if (batchSize > 0 && (i + 1) % batchSize == 0)
                        System.Threading.Thread.Sleep(1);
                }

                result[affixIdx].Seed = bestAffixSeed;
                ItemMergeBestAffixLogger.Info($"FindBestAffixSpecs(): Affix {affixProto.DataRef.GetName()}, best seed={bestAffixSeed}, score={bestScore:F2} (searched {searchCount} seeds)");
            }

            return result;
        }

        /// <summary>
        /// Simulates the property rolling logic from Item.cs without creating an entity.
        /// Replicates GenerateFloatWithinRange / GenerateTruncatedFloatWithinRange / GenerateIntWithinRange.
        /// </summary>
        private static float SimulateRoll(float randomMult, float min, float max, bool rollAsInteger, PropertyDataType dataType)
        {
            if (dataType == PropertyDataType.Real)
            {
                if (rollAsInteger)
                {
                    // GenerateTruncatedFloatWithinRange
                    float result = ((max - min + 1f) * randomMult) + min;
                    result = MathHelper.ClampNoThrow(result, min, max);
                    return MathF.Floor(result);
                }
                else
                {
                    // GenerateFloatWithinRange
                    return ((max - min) * randomMult) + min;
                }
            }
            else // Integer
            {
                // GenerateIntWithinRange = (int)GenerateTruncatedFloatWithinRange
                float result = ((max - min + 1f) * randomMult) + min;
                result = MathHelper.ClampNoThrow(result, min, max);
                return (int)MathF.Floor(result);
            }
        }

        /// <summary>
        /// Finds the affix property entry on an item that matches the given affix prototype.
        /// </summary>
        private static AffixPropertiesCopyEntry? FindAffixEntry(Item item, AffixPrototype affixProto)
        {
            if (item == null || affixProto == null) return null;

            IReadOnlyList<AffixPropertiesCopyEntry> entries = item.AffixPropertiesEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                AffixPropertiesCopyEntry entry = entries[i];
                if (entry.AffixProto != null && entry.AffixProto.DataRef == affixProto.DataRef)
                    return entry;
            }
            return null;
        }

        /// <summary>
        /// Returns the maximum of two property values based on their data type.
        /// For non-numeric types, returns the first value.
        /// </summary>
        private static PropertyValue TakeMaxValue(PropertyValue a, PropertyValue b, PropertyDataType dataType)
        {
            return dataType switch
            {
                PropertyDataType.Integer => Math.Max((int)a, (int)b),
                PropertyDataType.Real => Math.Max((float)a, (float)b),
                PropertyDataType.Boolean => (bool)a || (bool)b,
                _ => a,
            };
        }

        #endregion

        #region Eligibility 

        /// <summary>
        /// Returns true if two items are mergeable: same prototype, same rarity, same item level.
        /// </summary>
        private static bool AreMergeable(Item a, Item b)
        {
            if (a == null || b == null) return false;
            if (a.PrototypeDataRef != b.PrototypeDataRef) return false;
            if (a.Properties[PropertyEnum.ItemRarity].RawLong != b.Properties[PropertyEnum.ItemRarity].RawLong) return false;
            if (a.Properties[PropertyEnum.ItemLevel].RawLong != b.Properties[PropertyEnum.ItemLevel].RawLong) return false;
            return true;
        }

        #endregion

        #region Dry Run 

        /// <summary>
        /// Builds a dry-run diagnostic report. Scans ALL items in the general inventory,
        /// shows each item's eligibility status and rejection reason, groups eligible items,
        /// and writes a detailed JSON dump to a file for debugging.
        /// </summary>
        private string BuildDryRunReport(MergeFilter filter, PrototypeId currencyRef, int availableCurrency)
        {
            StringBuilder sb = new();
            EntityManager em = Game.EntityManager;
            Inventory generalInv = GetInventory(InventoryConvenienceLabel.General);

            // Collect all items for both the chat report and the JSON dump
            List<Item> allItems = new();
            if (generalInv != null)
            {
                foreach (var entry in generalInv)
                {
                    Item item = em.GetEntity<Item>(entry.Id);
                    if (item == null || item.IsScheduledToDestroy || item.IsDestroyed) continue;
                    allItems.Add(item);
                }
            }

            // 1. Inventory counts
            sb.Append("[Merge Dry Run] Inventory counts:\n");
            sb.Append($"  General:        {generalInv?.Count ?? 0} items ({allItems.Count} valid)\n");
            sb.Append($"  AvatarInPlay:   {GetInventory(InventoryConvenienceLabel.AvatarInPlay)?.Count ?? 0} items (skipped - equipped)\n");

            int stashCount = 0;
            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true))
            {
                foreach (var stashRef in stashRefs)
                {
                    Inventory stashInv = GetInventoryByRef(stashRef);
                    if (stashInv != null) stashCount += stashInv.Count;
                }
            }
            sb.Append($"  Stash:          {stashCount} items (skipped - stash)\n");
            if (Game.CustomGameOptions.ModItemMergeBestAffixFree)
                sb.Append($"  Currency:       FREE (no cost)\n");
            else
                sb.Append($"  Currency:       {availableCurrency} {GetShortPrototypeName(currencyRef.GetName())}\n");
            sb.Append('\n');

            // 2. Scan ALL items, group eligible ones, show only rejections in chat
            // Use long for the key to avoid any enum/struct equality edge cases.
            Dictionary<(long Proto, long Rarity, int Level), List<Item>> eligibleGroups = new();
            int eligibleCount = 0;
            int rejectedCount = 0;

            foreach (Item item in allItems)
            {
                string itemName = GetShortPrototypeName(item.PrototypeDataRef.GetName());
                string rarityName = GetShortPrototypeName(item.RarityPrototype?.DataRef.GetName() ?? "None");
                int level = (int)item.Properties[PropertyEnum.ItemLevel];

                // Check eligibility and get rejection reason
                string reason = CheckEligibilityReason(item, filter);

                if (reason == null)
                {
                    eligibleCount++;
                    // Group by (prototype, rarity, item level) using raw long values
                    long protoRaw = (long)item.PrototypeDataRef;
                    long rarityRaw = item.Properties[PropertyEnum.ItemRarity].RawLong;
                    int levelVal = (int)item.Properties[PropertyEnum.ItemLevel];
                    var key = (protoRaw, rarityRaw, levelVal);

                    if (!eligibleGroups.TryGetValue(key, out var list))
                    {
                        list = new List<Item>();
                        eligibleGroups[key] = list;
                    }
                    list.Add(item);
                }
                else
                {
                    rejectedCount++;
                    sb.Append($"  [SKIP] {itemName} ({rarityName}, Lv{level}) - {reason}\n");
                }
            }

            if (rejectedCount > 0)
                sb.Append('\n');

            // 3. Debug: show group details
            sb.Append($"[Merge Dry Run] Eligible: {eligibleCount}, Rejected: {rejectedCount}, Groups: {eligibleGroups.Count}\n");
            foreach (var grp in eligibleGroups)
            {
                string protoName = GetShortPrototypeName(((PrototypeId)grp.Key.Proto).GetName());
                sb.Append($"  Group: {protoName}, rarity={grp.Key.Rarity}, level={grp.Key.Level} -> {grp.Value.Count} item(s)\n");
            }
            sb.Append('\n');

            // 4. Show pairs with comparison
            int pairCount = 0;
            foreach (var pair in eligibleGroups)
            {
                if (pair.Value.Count < 2) continue;
                int pairs = pair.Value.Count / 2;
                pairCount += pairs;

                Item a = pair.Value[0];
                Item b = pair.Value[1];
                string itemName = GetShortPrototypeName(a.PrototypeDataRef.GetName());
                string rarityName = GetShortPrototypeName(a.RarityPrototype?.DataRef.GetName() ?? "");
                int level = (int)a.Properties[PropertyEnum.ItemLevel];

                sb.Append($"[Merge Dry Run] Pair: {itemName} ({rarityName}, Lv{level})\n");
                sb.Append($"  Available: {pair.Value.Count} copies -> {pairs} pair(s) can be merged\n");

                AppendAffixComparison(sb, a, b);
                sb.Append('\n');
            }

            // 5. Summary
            sb.Append($"[Merge Dry Run] Found {pairCount} viable pair(s).\n");
            if (pairCount == 0)
                sb.Append("  No merges would be performed.\n");
            else if (availableCurrency < 1)
                sb.Append("  Would merge 0 pair(s) (insufficient currency).\n");
            else
                sb.Append($"  Would merge 1 pair per command call. Run !merge {(filter.Mode == MergeFilterMode.None ? "any" : filter.FilterValue)} to execute.\n");

            // 5. Write detailed JSON dump to file for debugging (if logging is enabled)
            if (Game.CustomGameOptions.ModItemMergeBestAffixLoggingEnable)
            {
                string jsonPath = WriteDryRunJsonDump(allItems, filter, currencyRef, availableCurrency, eligibleCount, rejectedCount);
                if (jsonPath != null)
                    sb.Append($"  JSON dump: {jsonPath}\n");
                else
                    sb.Append("  (Failed to write JSON dump)\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes a detailed JSON dump of all items to Logs/ModItemMergeBestAffix/MergeItemAffixDryRun_<timestamp>.json.
        /// Includes full item.Properties, ItemSpec, AffixSpecs, AffixPropertiesEntries,
        /// and prototype PropertiesBuiltIn definitions for understanding how stats are stored.
        /// </summary>
        private string WriteDryRunJsonDump(List<Item> items, MergeFilter filter, PrototypeId currencyRef,
            int availableCurrency, int eligibleCount, int rejectedCount)
        {
            try
            {
                string dir = System.IO.Path.Combine(FileHelper.ServerRoot, "Logs", "ModItemMergeBestAffix");
                System.IO.Directory.CreateDirectory(dir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"MergeItemAffixDryRun_{timestamp}.json";
                string path = System.IO.Path.Combine(dir, fileName);

                var itemEntries = new List<object>();

                foreach (Item item in items)
                {
                    try
                    {
                        string reason = CheckEligibilityReason(item, filter);
                        var itemJson = BuildItemJson(item);
                        itemEntries.Add(new
                        {
                            status = reason == null ? "ELIGIBLE" : "REJECTED",
                            rejectionReason = reason ?? "",
                            item = itemJson,
                        });
                    }
                    catch (Exception itemEx)
                    {
                        ItemMergeBestAffixLogger.Warn($"WriteDryRunJsonDump(): Failed to dump item {item.PrototypeDataRef.GetName()} (entityId={item.Id}): {itemEx}");
                        itemEntries.Add(new
                        {
                            status = "DUMP_ERROR",
                            rejectionReason = itemEx.ToString(),
                            prototype = item.PrototypeDataRef.GetName(),
                            entityId = item.Id.ToString(),
                        });
                    }
                }

                var export = new
                {
                    exportedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    player = this.ToString(),
                    filter = new { mode = filter.Mode.ToString(), value = filter.FilterValue ?? "(none)" },
                    currency = currencyRef.GetName(),
                    currencyAvailable = availableCurrency,
                    eligible = eligibleCount,
                    rejected = rejectedCount,
                    totalItems = items.Count,
                    items = itemEntries,
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(export, options);
                System.IO.File.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                ItemMergeBestAffixLogger.Warn($"WriteDryRunJsonDump(): Failed to write JSON dump: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Writes a JSON dump of a completed merge to Logs/ModItemMergeBestAffix/MergeResult_<timestamp>.json.
        /// Shows the full source A, source B, and merged item with all affix/property values
        /// so the result can be verified.
        /// </summary>
        private string WriteMergeResultJsonDump(Item sourceA, Item sourceB, Item mergedItem, PrototypeId currencyRef)
        {
            try
            {
                string dir = System.IO.Path.Combine(FileHelper.ServerRoot, "Logs", "ModItemMergeBestAffix");
                System.IO.Directory.CreateDirectory(dir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"MergeResult_{timestamp}.json";
                string path = System.IO.Path.Combine(dir, fileName);

                var export = new
                {
                    exportedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    player = this.ToString(),
                    currency = currencyRef.GetName(),
                    sourceA = BuildItemJson(sourceA),
                    sourceB = BuildItemJson(sourceB),
                    merged = BuildItemJson(mergedItem),
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(export, options);
                System.IO.File.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                ItemMergeBestAffixLogger.Warn($"WriteMergeResultJsonDump(): Failed to write JSON dump: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Builds a JSON-serializable object with full item details: prototype, rarity, item level,
        /// ItemSpec, AffixSpecs, AffixPropertiesEntries, PropertiesBuiltIn, and all item.Properties.
        /// Used by both the dry run dump and the merge result dump.
        /// </summary>
        private object BuildItemJson(Item item)
        {
            ItemPrototype itemProto = item.ItemPrototype;
            RarityPrototype rarityProto = item.RarityPrototype;
            ItemSpec spec = item.ItemSpec;

            // Enumerate ALL properties on the item (this is where unique stats live)
            var allProperties = new List<object>();
            foreach (KeyValuePair<PropertyId, PropertyValue> kvp in item.Properties)
            {
                string propName;
                string propIdStr;
                string dataTypeStr;
                string valueStr;
                try
                {
                    PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(kvp.Key.Enum);
                    propName = propInfo.PropertyName;
                    propIdStr = kvp.Key.ToString();
                    dataTypeStr = propInfo.DataType.ToString();
                    valueStr = FormatPropertyValue(kvp.Value, propInfo.DataType);
                }
                catch
                {
                    propName = $"(error: enum={kvp.Key.Enum})";
                    propIdStr = kvp.Key.Raw.ToString();
                    dataTypeStr = "Unknown";
                    valueStr = kvp.Value.RawLong.ToString();
                }

                allProperties.Add(new
                {
                    name = propName,
                    propertyId = propIdStr,
                    dataType = dataTypeStr,
                    value = valueStr,
                    rawLong = kvp.Value.RawLong,
                });
            }

            // PropertiesBuiltIn from the prototype (defines which stats are rolled for uniques)
            var propsBuiltIn = new List<object>();
            if (itemProto?.PropertiesBuiltIn != null)
            {
                foreach (PropertyEntryPrototype propEntry in itemProto.PropertiesBuiltIn)
                {
                    if (propEntry == null) continue;

                    var pickInRange = propEntry as PropertyPickInRangeEntryPrototype;
                    var setEntry = propEntry as PropertySetEntryPrototype;

                    PropertyId propId = pickInRange?.Prop ?? setEntry?.Prop ?? default;
                    string propName = "(null)";
                    string propIdStr;
                    try
                    {
                        if (propId.Enum != PropertyEnum.Invalid)
                        {
                            propName = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum).PropertyName;
                            propIdStr = propId.ToString();
                        }
                        else
                        {
                            propIdStr = propId.Raw.ToString();
                        }
                    }
                    catch
                    {
                        propName = $"(error: enum={propId.Enum})";
                        propIdStr = propId.Raw.ToString();
                    }

                    propsBuiltIn.Add(new
                    {
                        type = pickInRange != null ? "PickInRange" : setEntry != null ? "Set" : "Unknown",
                        property = propIdStr,
                        propertyName = propName,
                        rollAsInteger = pickInRange?.RollAsInteger,
                    });
                }
            }

            // AffixPropertiesEntries (the _affixProperties list - used by regular gear)
            var affixEntries = new List<object>();
            foreach (AffixPropertiesCopyEntry entry in item.AffixPropertiesEntries)
            {
                var entryProps = new List<object>();
                if (entry.Properties != null)
                {
                    foreach (KeyValuePair<PropertyId, PropertyValue> kvp in entry.Properties)
                    {
                        string epName;
                        string epIdStr;
                        string epValStr;
                        try
                        {
                            PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(kvp.Key.Enum);
                            epName = propInfo.PropertyName;
                            epIdStr = kvp.Key.ToString();
                            epValStr = FormatPropertyValue(kvp.Value, propInfo.DataType);
                        }
                        catch
                        {
                            epName = $"(error: enum={kvp.Key.Enum})";
                            epIdStr = kvp.Key.Raw.ToString();
                            epValStr = kvp.Value.RawLong.ToString();
                        }

                        entryProps.Add(new
                        {
                            name = epName,
                            propertyId = epIdStr,
                            value = epValStr,
                        });
                    }
                }

                string powerModIdStr;
                try { powerModIdStr = entry.PowerModifierPropertyId.ToString(); }
                catch { powerModIdStr = entry.PowerModifierPropertyId.Raw.ToString(); }

                affixEntries.Add(new
                {
                    affix = entry.AffixProto?.DataRef.GetName() ?? "(null)",
                    levelRequirement = entry.LevelRequirement,
                    powerModifierPropertyId = powerModIdStr,
                    properties = entryProps,
                });
            }

            // AffixSpecs from ItemSpec
            var affixSpecs = new List<object>();
            foreach (AffixSpec affixSpec in spec.AffixSpecs)
            {
                affixSpecs.Add(new
                {
                    affix = affixSpec.AffixProto?.DataRef.GetName() ?? "(null)",
                    seed = affixSpec.Seed,
                    scope = affixSpec.ScopeProtoRef.GetName(),
                });
            }

            // Built-in affixes from prototype and rarity
            var protoAffixesBuiltIn = new List<object>();
            if (itemProto?.AffixesBuiltIn != null)
            {
                foreach (AffixEntryPrototype affixEntry in itemProto.AffixesBuiltIn)
                {
                    if (affixEntry == null) continue;
                    AffixPrototype affixProto = affixEntry.Affix.As<AffixPrototype>();
                    protoAffixesBuiltIn.Add(new
                    {
                        affix = affixEntry.Affix.GetName(),
                        position = affixProto?.Position.ToString() ?? "(null)",
                        power = affixEntry.Power.GetName(),
                    });
                }
            }

            var rarityAffixesBuiltIn = new List<object>();
            if (rarityProto?.AffixesBuiltIn != null)
            {
                foreach (AffixEntryPrototype affixEntry in rarityProto.AffixesBuiltIn)
                {
                    if (affixEntry == null) continue;
                    AffixPrototype affixProto = affixEntry.Affix.As<AffixPrototype>();
                    rarityAffixesBuiltIn.Add(new
                    {
                        affix = affixEntry.Affix.GetName(),
                        position = affixProto?.Position.ToString() ?? "(null)",
                        power = affixEntry.Power.GetName(),
                    });
                }
            }

            return new
            {
                prototype = item.PrototypeDataRef.GetName(),
                entityId = item.Id.ToString(),
                rarity = rarityProto?.DataRef.GetName() ?? "(null)",
                itemLevel = (int)item.Properties[PropertyEnum.ItemLevel],
                stackSize = item.CurrentStackSize,
                isBoundToCharacter = item.IsBoundToCharacter,
                boundAgent = item.BoundAgentProtoRef.GetName(),
                isCurrencyItem = item.IsCurrencyItem(),
                seed = spec.Seed,
                equippableBy = spec.EquippableBy.GetName(),
                itemSpec = new
                {
                    itemProtoRef = spec.ItemProtoRef.GetName(),
                    rarityProtoRef = spec.RarityProtoRef.GetName(),
                    itemLevel = spec.ItemLevel,
                    seed = spec.Seed,
                    equippableBy = spec.EquippableBy.GetName(),
                    affixSpecsCount = spec.AffixSpecs.Count,
                    affixSpecs = affixSpecs,
                },
                affixPropertiesEntriesCount = item.AffixPropertiesEntries.Count,
                affixPropertiesEntries = affixEntries,
                propertiesBuiltInCount = itemProto?.PropertiesBuiltIn?.Length ?? 0,
                propertiesBuiltIn = propsBuiltIn,
                prototypeAffixesBuiltIn = protoAffixesBuiltIn,
                rarityAffixesBuiltIn = rarityAffixesBuiltIn,
                allPropertiesCount = allProperties.Count,
                allProperties = allProperties,
            };
        }

        private void AppendAffixComparison(StringBuilder sb, Item a, Item b)
        {
            IReadOnlyList<AffixPropertiesCopyEntry> entriesA = a.AffixPropertiesEntries;
            IReadOnlyList<AffixPropertiesCopyEntry> entriesB = b.AffixPropertiesEntries;

            // Affix-based stats (regular gear)
            if (entriesA.Count > 0 || entriesB.Count > 0)
            {
                sb.Append("  Source A affixes:\n");
                AppendAffixValues(sb, entriesA, "    ");

                sb.Append("  Source B affixes:\n");
                AppendAffixValues(sb, entriesB, "    ");

                sb.Append("  Predicted result (max of each):\n");
                AppendPredictedMaxAffixes(sb, entriesA, entriesB, "    ");
            }

            // Built-in property stats (unique items)
            AppendBuiltInPropertyComparison(sb, a, b, "    ");
        }

        /// <summary>
        /// Shows a comparison of PropertiesBuiltIn values between two items.
        /// Only PickInRange entries are shown since Set entries are fixed (not rolled).
        /// </summary>
        private void AppendBuiltInPropertyComparison(StringBuilder sb, Item a, Item b, string indent)
        {
            ItemPrototype itemProto = a.ItemPrototype;
            if (itemProto?.PropertiesBuiltIn == null || itemProto.PropertiesBuiltIn.HasValue() == false)
                return;

            sb.Append("  Built-in rolled properties (PropertiesBuiltIn):\n");

            // Source A values
            sb.Append($"{indent}Source A:\n");
            foreach (PropertyEntryPrototype propEntry in itemProto.PropertiesBuiltIn)
            {
                if (propEntry is not PropertyPickInRangeEntryPrototype pickInRange) continue;
                PropertyId propId = pickInRange.Prop;
                if (a.Properties.HasProperty(propId) == false) continue;

                PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum);
                string valueStr = FormatPropertyValue(a.Properties[propId], propInfo.DataType);
                sb.Append($"{indent}  {propInfo.PropertyName} [{propId}]: {valueStr}\n");
            }

            // Source B values
            sb.Append($"{indent}Source B:\n");
            foreach (PropertyEntryPrototype propEntry in itemProto.PropertiesBuiltIn)
            {
                if (propEntry is not PropertyPickInRangeEntryPrototype pickInRange) continue;
                PropertyId propId = pickInRange.Prop;
                if (b.Properties.HasProperty(propId) == false) continue;

                PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum);
                string valueStr = FormatPropertyValue(b.Properties[propId], propInfo.DataType);
                sb.Append($"{indent}  {propInfo.PropertyName} [{propId}]: {valueStr}\n");
            }

            // Predicted max
            sb.Append($"{indent}Predicted result (max of each):\n");
            foreach (PropertyEntryPrototype propEntry in itemProto.PropertiesBuiltIn)
            {
                if (propEntry is not PropertyPickInRangeEntryPrototype pickInRange) continue;
                PropertyId propId = pickInRange.Prop;
                if (a.Properties.HasProperty(propId) == false || b.Properties.HasProperty(propId) == false) continue;

                PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(propId.Enum);
                PropertyValue valA = a.Properties[propId];
                PropertyValue valB = b.Properties[propId];
                PropertyValue maxVal = TakeMaxValue(valA, valB, propInfo.DataType);

                string aStr = FormatPropertyValue(valA, propInfo.DataType);
                string bStr = FormatPropertyValue(valB, propInfo.DataType);
                string maxStr = FormatPropertyValue(maxVal, propInfo.DataType);
                sb.Append($"{indent}  {propInfo.PropertyName}: max({aStr}, {bStr}) = {maxStr}\n");
            }
        }

        private void AppendAffixValues(StringBuilder sb, IReadOnlyList<AffixPropertiesCopyEntry> entries, string indent)
        {
            if (entries.Count == 0)
            {
                sb.Append($"{indent}(no affixes)\n");
                return;
            }

            foreach (AffixPropertiesCopyEntry entry in entries)
            {
                if (entry.AffixProto == null) continue;
                string affixName = GetShortPrototypeName(entry.AffixProto.DataRef.GetName());

                if (entry.Properties == null)
                {
                    sb.Append($"{indent}{affixName}: (no properties)\n");
                    continue;
                }

                foreach (KeyValuePair<PropertyId, PropertyValue> kvp in entry.Properties)
                {
                    PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(kvp.Key.Enum);
                    string valueStr = FormatPropertyValue(kvp.Value, propInfo.DataType);
                    sb.Append($"{indent}{affixName} [{propInfo.PropertyName}]: {valueStr}\n");
                }
            }
        }

        private void AppendPredictedMaxAffixes(StringBuilder sb, IReadOnlyList<AffixPropertiesCopyEntry> entriesA, IReadOnlyList<AffixPropertiesCopyEntry> entriesB, string indent)
        {
            // For each affix in A, find the matching one in B and show the max
            foreach (AffixPropertiesCopyEntry entryA in entriesA)
            {
                if (entryA.AffixProto == null || entryA.Properties == null) continue;

                string affixName = GetShortPrototypeName(entryA.AffixProto.DataRef.GetName());

                // Find matching entry in B by affix prototype DataRef
                AffixPropertiesCopyEntry? matchB = null;
                foreach (AffixPropertiesCopyEntry eb in entriesB)
                {
                    if (eb.AffixProto != null && eb.AffixProto.DataRef == entryA.AffixProto.DataRef)
                    {
                        matchB = eb;
                        break;
                    }
                }

                foreach (KeyValuePair<PropertyId, PropertyValue> kvp in entryA.Properties)
                {
                    PropertyInfo propInfo = GameDatabase.PropertyInfoTable.LookupPropertyInfo(kvp.Key.Enum);
                    PropertyValue valA = kvp.Value;
                    PropertyValue? valB = null;

                    if (matchB != null && matchB.Value.Properties != null && matchB.Value.Properties.HasProperty(kvp.Key))
                        valB = matchB.Value.Properties[kvp.Key];

                    string aStr = FormatPropertyValue(valA, propInfo.DataType);
                    string bStr = valB.HasValue ? FormatPropertyValue(valB.Value, propInfo.DataType) : "-";
                    string maxStr = valB.HasValue
                        ? FormatPropertyValue(TakeMaxValue(valA, valB.Value, propInfo.DataType), propInfo.DataType)
                        : aStr;

                    sb.Append($"{indent}{affixName} [{propInfo.PropertyName}]: max({aStr}, {bStr}) = {maxStr}\n");
                }
            }
        }

        private static string FormatPropertyValue(PropertyValue value, PropertyDataType dataType)
        {
            return dataType switch
            {
                PropertyDataType.Integer => ((int)value).ToString(),
                PropertyDataType.Real => ((float)value).ToString("F2"),
                PropertyDataType.Boolean => (bool)value ? "true" : "false",
                _ => value.RawLong.ToString(),
            };
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the short file name from a full prototype path (e.g. "Entity/Items/.../Foo.prototype" -> "Foo").
        /// </summary>
        private static string GetShortPrototypeName(string fullProtoPath)
        {
            if (string.IsNullOrEmpty(fullProtoPath)) return "Unknown";
            string fileName = System.IO.Path.GetFileName(fullProtoPath);
            if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);
            return fileName;
        }

        #endregion

        #endregion
    }
}
