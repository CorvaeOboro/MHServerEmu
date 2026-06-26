using MHServerEmu.Core.Extensions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
using MHServerEmu.Games.Properties;

/*
    DANGER ROOM SCENARIO ITEMS
    ==========================
    The following item prototypes are identified as Danger Room scenario portals
    by the TryGetDangerRoomCategory() filter (contains "DangerRoom" + "PortalTo",
    excludes recipes/crates/relics/etc.):

    --- Random Theme Portals (fixed rarity, combinable) ---
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomRandomThemeNoAffixesWhite.prototype   (Common)
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomRandomThemeNoAffixesGreen.prototype   (Uncommon)
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomRandomThemeNoAffixesBlue.prototype    (Rare)
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomRandomThemeNoAffixesPurple.prototype  (Epic)
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomRandomThemeNoAffixesYellow.prototype (Legendary)

    --- Named Scenario Portals (combinable, result of all combines) ---
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioAIMFacility.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioBambooForest.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioBroodCaves.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioMarsh.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioMilitary.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioSubway.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioTraining.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/PortalToDangerRoomScenarioTrainyard.prototype

    --- Static Challenge Portals (ignored by combine) ---
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioAIM.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioAIMCosmic.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioBroodShip.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioBroodShipCosmic.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioCableFight.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioCableFightCosmic.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioDoop.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioDoopCosmic.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioSubway.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioSubwayCosmic.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioTrainyard.prototype
    Entity/Items/Consumables/Prototypes/DangerRoom/StaticChallenges/PortalToDRStaticScenarioTrainyardCosmic.prototype

    --- Excluded (not scenarios) ---
    CraftCommonDRPortal / CraftUncommonDRPortal / CraftRareDRPortal / CraftEpicDRPortal / CraftCosmicDRPortal / RerollCosmicDRPortal
    (These are crafting recipes, not scenario items.)

    COMBINE BEHAVIOUR
    -----------------
    Random Theme + Random Theme -> random Named Scenario (next tier)
    Named Scenario + Named Scenario -> random Named Scenario (next tier)
    Static Challenges are never combined.
*/

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private static readonly Logger DangerRoomLogger = LogManager.CreateLogger();

        private static readonly object _cacheLock = new();
        private static Dictionary<int, PrototypeId> _rarityProtoByTier;
        private static List<PrototypeId> _namedScenarioProtos;

        private enum DangerRoomCategory
        {
            None,
            Random,
            Named,
            Static
        }

        public string CombineDangerRoomScenarios(int maxTier)
        {
            if (Game.CustomGameOptions.DangerRoomCombineCommandEnable == false)
                return "Danger Room auto-combine is disabled by server settings.";

            BuildRarityCache();

            Inventory generalInv = GetInventory(InventoryConvenienceLabel.General);
            if (generalInv == null)
                return "General inventory not found.";

            EntityManager em = Game.EntityManager;
            // Group by (category, tier) so Random and Named items each combine within their own group
            Dictionary<(DangerRoomCategory Category, int Tier), List<Item>> scenariosByCategoryAndTier = new();

            foreach (var entry in generalInv)
            {
                Item item = em.GetEntity<Item>(entry.Id);
                if (item == null || item.IsScheduledToDestroy) continue;

                DangerRoomCategory category = TryGetDangerRoomCategory(item);
                if (category == DangerRoomCategory.None || category == DangerRoomCategory.Static)
                    continue;

                int tier = item.RarityPrototype?.Tier ?? 0;
                var key = (category, tier);
                if (!scenariosByCategoryAndTier.TryGetValue(key, out var list))
                {
                    list = new List<Item>();
                    scenariosByCategoryAndTier[key] = list;
                }
                list.Add(item);
            }

            if (scenariosByCategoryAndTier.Count == 0)
                return "No Danger Room scenarios found in your general inventory.";

            int combined = 0;

            // Process tiers from lowest to highest
            var sortedKeys = scenariosByCategoryAndTier.Keys.OrderBy(k => k.Tier).ToList();

            foreach (var key in sortedKeys)
            {
                int tier = key.Tier;
                if (maxTier > 0 && tier >= maxTier)
                    continue;

                if (!_rarityProtoByTier.TryGetValue(tier + 1, out PrototypeId nextRarityRef))
                    continue;

                if (_namedScenarioProtos == null || _namedScenarioProtos.Count == 0)
                    continue;

                if (!scenariosByCategoryAndTier.TryGetValue(key, out var items))
                    continue;

                while (items.Count >= 2)
                {
                    Item a = items[0];
                    Item b = items[1];

                    PrototypeId nextProtoRef = _namedScenarioProtos[Game.Random.Next(0, _namedScenarioProtos.Count)];

                    if (!TryCreateScenarioItem(nextProtoRef, nextRarityRef, out Item newItem))
                    {
                        DangerRoomLogger.Warn($"CombineDangerRoomScenarios(): Failed to create item {nextProtoRef.GetName()} with rarity {nextRarityRef.GetName()} for player {this}");
                        break;
                    }

                    if (a.CurrentStackSize > 1)
                        a.DecrementStack(1);
                    else
                        a.Destroy();

                    if (b.CurrentStackSize > 1)
                        b.DecrementStack(1);
                    else
                        b.Destroy();

                    items.RemoveRange(0, 2);
                    combined++;
                }
            }

            if (combined == 0)
                return "No Danger Room scenarios could be combined.";

            return $"Combined {combined} Danger Room scenario pair(s).";
        }

        private static void BuildRarityCache()
        {
            lock (_cacheLock)
            {
                if (_rarityProtoByTier != null) return;

                _rarityProtoByTier = new Dictionary<int, PrototypeId>();
                foreach (PrototypeId rarityRef in DataDirectory.Instance.IteratePrototypesInHierarchy<RarityPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                {
                    RarityPrototype rarityProto = GameDatabase.GetPrototype<RarityPrototype>(rarityRef);
                    if (rarityProto == null) continue;
                    _rarityProtoByTier[rarityProto.Tier] = rarityRef;
                }

                _namedScenarioProtos = new List<PrototypeId>();
                foreach (PrototypeId protoRef in DataDirectory.Instance.IteratePrototypesInHierarchy<ItemPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                {
                    string protoName = protoRef.GetName();
                    if (string.IsNullOrEmpty(protoName)) continue;

                    if (!protoName.Contains("DangerRoom", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!protoName.Contains("PortalTo", StringComparison.OrdinalIgnoreCase)) continue;

                    // Exclude recipes, crates, relics, etc.
                    string[] excluded = { "Recipe", "Crate", "Relic", "Tournament", "Gift", "Box", "Daily", "RandomDungeon", "RandomMaxAffixDungeon" };
                    bool skip = false;
                    foreach (string ex in excluded)
                    {
                        if (protoName.Contains(ex, StringComparison.OrdinalIgnoreCase))
                        { skip = true; break; }
                    }
                    if (skip) continue;

                    // Only cache Named scenarios (skip Random themes and Static challenges)
                    if (protoName.Contains("RandomTheme", StringComparison.OrdinalIgnoreCase)) continue;
                    if (protoName.Contains("StaticChallenge", StringComparison.OrdinalIgnoreCase) ||
                        protoName.Contains("StaticChallenges", StringComparison.OrdinalIgnoreCase)) continue;

                    _namedScenarioProtos.Add(protoRef);
                }

                DangerRoomLogger.Info($"Built rarity cache: {_rarityProtoByTier.Count} tier(s), {_namedScenarioProtos.Count} named scenario(s).");
            }
        }

        private bool TryCreateScenarioItem(PrototypeId itemProtoRef, PrototypeId rarityProtoRef, out Item item)
        {
            item = null;

            ItemPrototype itemProto = GameDatabase.GetPrototype<ItemPrototype>(itemProtoRef);
            if (itemProto == null)
                return false;

            if (DataDirectory.Instance.PrototypeIsAbstract(itemProtoRef))
                return false;

            using ItemResolver resolver = ObjectPoolManager.Instance.Get<ItemResolver>();
            resolver.Initialize(Game.Random);
            resolver.SetContext(LootContext.Crafting, this);

            AvatarPrototype avatarProto = CurrentAvatar?.AvatarPrototype;

            using DropFilterArguments filterArgs = ObjectPoolManager.Instance.Get<DropFilterArguments>();
            DropFilterArguments.Initialize(filterArgs, LootContext.Crafting);
            filterArgs.ItemProto = itemProto;
            filterArgs.Level = 1;
            filterArgs.RollFor = resolver.ResolveAvatarPrototype(avatarProto, true, 1f)?.DataRef ?? PrototypeId.Invalid;
            filterArgs.Rarity = rarityProtoRef;
            filterArgs.Slot = itemProto.GetInventorySlotForAgent(avatarProto);

            if (itemProto.MakeRestrictionsDroppable(filterArgs, RestrictionTestFlags.All, out _) == false)
                return false;

            ItemSpec itemSpec = new(filterArgs.ItemProto.DataRef, filterArgs.Rarity, filterArgs.Level, 0,
                Array.Empty<AffixSpec>(), resolver.Random.Next());

            if (LootUtilities.UpdateAffixes(resolver, filterArgs, AffixCountBehavior.Roll, itemSpec, null).HasFlag(MutationResults.Error))
            {
                DangerRoomLogger.Warn($"TryCreateScenarioItem(): Failed to update affixes for {itemProto}");
                return false;
            }

            using EntitySettings settings = ObjectPoolManager.Instance.Get<EntitySettings>();
            settings.EntityRef = itemProtoRef;
            settings.ItemSpec = itemSpec;

            item = Game.EntityManager.CreateEntity(settings) as Item;
            if (item == null)
                return false;

            item.Properties[PropertyEnum.InventoryStackCount] = 1;

            InventoryResult result = AcquireItem(item, PrototypeId.Invalid);
            if (result != InventoryResult.Success)
            {
                item.Destroy();
                item = null;
                return false;
            }

            return true;
        }

        private static DangerRoomCategory TryGetDangerRoomCategory(Item item)
        {
            if (item?.ItemPrototype == null) return DangerRoomCategory.None;

            string protoName = item.PrototypeDataRef.GetName();
            if (string.IsNullOrEmpty(protoName)) return DangerRoomCategory.None;

            // Danger Room scenario portals contain "DangerRoom" and "PortalTo" in their path.
            // We exclude recipes, crates, relics, tournament rewards, gifts, and generic random dungeons.
            if (!protoName.Contains("DangerRoom", StringComparison.OrdinalIgnoreCase))
                return DangerRoomCategory.None;

            if (!protoName.Contains("PortalTo", StringComparison.OrdinalIgnoreCase))
                return DangerRoomCategory.None;

            string[] excluded = { "Recipe", "Crate", "Relic", "Tournament", "Gift", "Box", "Daily", "RandomDungeon", "RandomMaxAffixDungeon" };
            foreach (string ex in excluded)
            {
                if (protoName.Contains(ex, StringComparison.OrdinalIgnoreCase))
                    return DangerRoomCategory.None;
            }

            if (protoName.Contains("StaticChallenge", StringComparison.OrdinalIgnoreCase) ||
                protoName.Contains("StaticChallenges", StringComparison.OrdinalIgnoreCase))
                return DangerRoomCategory.Static;

            if (protoName.Contains("RandomTheme", StringComparison.OrdinalIgnoreCase))
                return DangerRoomCategory.Random;

            return DangerRoomCategory.Named;
        }
    }
}
