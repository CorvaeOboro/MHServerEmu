#region Item Pickup Auto
// =============================================================================
// MOD Item Pickup Auto
// =============================================================================
//   picks up specific nearby items on a timer.
//
//   Currency , event items , Crafting ingredients , Relics , Runes
//   scans a radius around the player's hero.  Matching items are moved to the player's inventory or stash.
//
//   Currency is consumed directly and removed from the world.
//   Crafting ingredients and runes go to inventory or stash based on config.
//   Relics try to stack onto an equipped relic first, otherwise inventory or stash.
//
//  Config.ini :
//   ModItemPickupAutoEnable, ModItemPickupAutoRadius, ModItemPickupAutoIntervalMs
//   ModItemPickupAutoCraftingIngredientEnable, ...ToStash, ...LoggingEnable
//   ModItemPickupAutoRelicEnable, ...ToStash, ...EquipIfSameTypeEquippedEnable, ...LoggingEnable
//   ModItemPickupAutoRuneEnable, ...ToStash, ...LoggingEnable
//
//  VERSION:: 20260711
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using MHServerEmu.Core.Collisions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private readonly EventPointer<ModItemPickupAutoEvent> _modItemPickupAutoEvent = new();

        #region Item Groups

        private static bool IsHardcodedAutoPickupCurrency(Item item)
        {
            string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
            return protoName == "Entity/Items/CurrencyItems/SeasonalLE/Seasonal/Anniversary/BirthdayCakeSlice2016.prototype" ||
                   protoName == "Entity/Items/CurrencyItems/SeasonalLE/Recurring/AgentNikkisSpecialKetchup.prototype";
        }

        private static bool IsAutoPickupCurrencyCandidate(Item item)
        {
            return (item.Prototype is ItemPrototype itemProto && itemProto.IsCurrency) || IsHardcodedAutoPickupCurrency(item);
        }

        private static bool IsAutoPickupCraftingIngredient(Item item)
        {
            string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
            return protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Astral.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Genome.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Nano.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Ionic.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/SpiritOfIvaldi.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/SpiritOfYmir.prototype" ||
                   protoName == "Entity/Items/Crafting/Ingredients/UnstableMolecule.prototype";
        }

        private static bool IsAutoPickupRune(Item item)
        {
            string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
            return protoName.Contains("/Runewords/Glyphs/RunewordGlyph") ||
                   protoName.Contains("/Runewords/Glyphs/OnslaughtRune");
        }

        #endregion
// =============================================================================


        #region Tick

        /// <summary>
        /// Periodic per-player tick that vacuums up nearby <see cref="Item"/> entities flagged as
        /// currency (Eternity Splinters, Cube Shards, etc.). Controlled by config.ini
        /// </summary>
        private void DoModItemPickupAutoTick()
        {
            //  the live config toggle so admins can flip it off without a restart.
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.ModItemPickupAutoEnable == false)
                return;

            Avatar avatar = CurrentAvatar;
            Region region = avatar?.Region;
            if (avatar == null || avatar.IsInWorld == false || region == null)
            {
                // Player may be transitioning regions or dead; just try again next tick.
                ScheduleModItemPickupAutoEvent();
                return;
            }

            float radius = Math.Max(1f, customOptions.ModItemPickupAutoRadius);
            Sphere volume = new(avatar.RegionLocation.Position, radius);

            // Gather candidates into a list first so we can safely mutate the world (destroy items)
            // outside the spatial-partition iteration. Same pattern as Pet vacuum.
            using var candidatesHandle = ListPool<Item>.Instance.Get(out List<Item> candidates);
            HashSet<Item> pickedUp = new();

            PickupCurrencyItems(region, volume, pickedUp, candidates, avatar);

            if (customOptions.ModItemPickupAutoCraftingIngredientEnable)
                PickupCraftingIngredients(region, volume, pickedUp, candidates, avatar, customOptions);

            if (customOptions.ModItemPickupAutoRelicEnable)
                PickupRelics(region, volume, pickedUp, candidates, avatar, customOptions);

            if (customOptions.ModItemPickupAutoRuneEnable)
                PickupRunes(region, volume, pickedUp, candidates, avatar, customOptions);

            ScheduleModItemPickupAutoEvent();
        }

        #endregion

        #region Currency Pickup

        private void PickupCurrencyItems(Region region, Sphere volume, HashSet<Item> pickedUp, List<Item> candidates, Avatar avatar)
        {
            ScanAutoPickupCandidates(region, volume, pickedUp, IsAutoPickupCurrencyCandidate, "ModItemPickupAutoCurrency", false, candidates, out _, out _);

            foreach (Item item in candidates)
            {
                // AcquireCurrencyItem returns true only after successfully applying currency props.
                bool acquired = AcquireCurrencyItem(item);
                bool movedToInventory = false;
                if (acquired == false)
                {
                    // Fallback: hardcoded seasonal/event currency items may not have consumable
                    // currency properties, so pick them up as regular inventory items.
                    if (IsHardcodedAutoPickupCurrency(item))
                    {
                        acquired = TryAutoPickupToInventory(item) == InventoryResult.Success;
                        movedToInventory = acquired;
                    }
                }

                if (acquired == false)
                    continue;

                pickedUp.Add(item);
                avatar.TryActivateOnLootPickupProcs(item);

                // Only destroy world entities that were consumed in place. Hardcoded currencies
                // that had to be moved into inventory must stay there.
                if (movedToInventory == false)
                {
                    if (item.IsDestroyed)
                        Logger.Warn($"[ModItemPickupAutoCurrency] Currency item [{GameDatabase.GetPrototypeName(item.Prototype.DataRef)}] is already destroyed before calling Destroy(). This may indicate a double-destroy.");
                    item.Destroy();
                }
            }
        }

        #endregion

        #region Crafting Ingredient Pickup

        private void PickupCraftingIngredients(Region region, Sphere volume, HashSet<Item> pickedUp, List<Item> candidates, Avatar avatar, CustomGameOptionsConfig customOptions)
        {
            bool logging = customOptions.ModItemPickupAutoCraftingIngredientLoggingEnable;
            if (logging)
                Logger.Trace($"[ModItemPickupAutoCrafting] Starting crafting ingredient scan for player [{this}]. radius={customOptions.ModItemPickupAutoRadius}, toStash={customOptions.ModItemPickupAutoCraftingIngredientToStash}");
            ScanAutoPickupCandidates(region, volume, pickedUp, IsAutoPickupCraftingIngredient, "ModItemPickupAutoCrafting", logging, candidates, out int scanned, out int filteredOut);
            if (logging)
                Logger.Trace($"[ModItemPickupAutoCrafting] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");
            PickupCandidatesToStashOrInventory(candidates, avatar, customOptions.ModItemPickupAutoCraftingIngredientToStash, "ModItemPickupAutoCrafting", logging);
        }

        #endregion

        #region Relic Pickup

        private void PickupRelics(Region region, Sphere volume, HashSet<Item> pickedUp, List<Item> candidates, Avatar avatar, CustomGameOptionsConfig customOptions)
        {
            bool logging = customOptions.ModItemPickupAutoRelicLoggingEnable;
            if (logging)
                Logger.Trace($"[ModItemPickupAutoRelic] Starting relic scan for player [{this}]. radius={customOptions.ModItemPickupAutoRadius}, toStash={customOptions.ModItemPickupAutoRelicToStash}, equipSameType={customOptions.ModItemPickupAutoRelicEquipIfSameTypeEquippedEnable}");
            ScanAutoPickupCandidates(region, volume, pickedUp, item => item.Prototype is RelicPrototype, "ModItemPickupAutoRelic", logging, candidates, out int scanned, out int filteredOut);
            if (logging)
                Logger.Trace($"[ModItemPickupAutoRelic] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");

            foreach (Item item in candidates)
            {
                string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                InventoryResult result = InventoryResult.Invalid;

                // Optional: try to stack onto an already-equipped relic of the same type
                Item equippedRelic = GetEquippedRelicForStacking(avatar, item);
                if (customOptions.ModItemPickupAutoRelicEquipIfSameTypeEquippedEnable && equippedRelic != null)
                {
                    Inventory relicInv = GetAvatarEquipmentInventoryForSlot(avatar, EquipmentInvUISlot.Relic);
                    if (relicInv != null)
                    {
                        if (logging)
                            Logger.Trace($"[ModItemPickupAutoRelic] Attempting equip-to-stack for [{protoName}] into avatar relic slot...");
                        ulong? stackEntityId = InvalidId;
                        result = item.ChangeInventoryLocation(relicInv, Inventory.InvalidSlot, ref stackEntityId, true);
                        LogAutoPickupMoveResult(item, relicInv, result, stackEntityId, "ModItemPickupAutoRelic", logging);
                    }
                }

                // If equip-to-stack was not attempted or failed, fall back to inventory/stash
                if (result != InventoryResult.Success)
                {
                    if (customOptions.ModItemPickupAutoRelicToStash)
                    {
                        if (logging)
                            Logger.Trace($"[ModItemPickupAutoRelic] Attempting stash pickup for [{protoName}]...");
                        result = TryAutoPickupToStash(item, "ModItemPickupAutoRelic", logging);
                    }
                    else
                    {
                        if (logging)
                            Logger.Trace($"[ModItemPickupAutoRelic] Attempting general inventory pickup for [{protoName}]...");
                        result = TryAutoPickupToInventory(item, "ModItemPickupAutoRelic", logging);
                    }
                }

                if (result == InventoryResult.Success)
                {
                    if (logging)
                        Logger.Info($"[ModItemPickupAutoRelic] Successfully picked up [{protoName}] for player [{this}].");
                    avatar.TryActivateOnLootPickupProcs(item);
                }
                else
                {
                    if (logging)
                        Logger.Trace($"[ModItemPickupAutoRelic] Pickup failed for [{protoName}] - result={result}. Leaving on ground.");
                }
            }
        }

        #endregion

        #region Rune Pickup

        private void PickupRunes(Region region, Sphere volume, HashSet<Item> pickedUp, List<Item> candidates, Avatar avatar, CustomGameOptionsConfig customOptions)
        {
            bool logging = customOptions.ModItemPickupAutoRuneLoggingEnable;
            if (logging)
                Logger.Trace($"[ModItemPickupAutoRune] Starting rune scan for player [{this}]. radius={customOptions.ModItemPickupAutoRadius}, toStash={customOptions.ModItemPickupAutoRuneToStash}");
            ScanAutoPickupCandidates(region, volume, pickedUp, IsAutoPickupRune, "ModItemPickupAutoRune", logging, candidates, out int scanned, out int filteredOut);
            if (logging)
                Logger.Trace($"[ModItemPickupAutoRune] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");
            PickupCandidatesToStashOrInventory(candidates, avatar, customOptions.ModItemPickupAutoRuneToStash, "ModItemPickupAutoRune", logging);
        }

        #endregion

        #region Scanning

        private void ScanAutoPickupCandidates(Region region, Sphere volume, HashSet<Item> pickedUp, Predicate<Item> filter,
            string logPrefix, bool loggingEnabled, List<Item> candidates, out int scanned, out int filteredOut)
        {
            scanned = 0;
            filteredOut = 0;
            candidates.Clear();

            foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
            {
                if (worldEntity is not Item item)
                    continue;

                scanned++;
                string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);

                ulong restrictedToPlayerGuid = item.Properties[PropertyEnum.RestrictedToPlayerGuid];
                if (restrictedToPlayerGuid != 0 && restrictedToPlayerGuid != DatabaseUniqueId)
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Skipping [{protoName}] - instanced loot for another player.");
                    filteredOut++;
                    continue;
                }

                if (item.IsRootOwner == false)
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Skipping [{protoName}] - not a root-owned item.");
                    filteredOut++;
                    continue;
                }

                if (pickedUp.Contains(item))
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Skipping [{protoName}] - already picked up by an earlier loop.");
                    filteredOut++;
                    continue;
                }

                if (filter(item) == false)
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Skipping [{protoName}] - not a matching item.");
                    filteredOut++;
                    continue;
                }

                if (loggingEnabled)
                    Logger.Trace($"[{logPrefix}] Candidate accepted: [{protoName}]");
                candidates.Add(item);
            }
        }

        #endregion

        #region Pickup

        private void PickupCandidatesToStashOrInventory(List<Item> candidates, Avatar avatar, bool toStash, string logPrefix, bool loggingEnabled)
        {
            foreach (Item item in candidates)
            {
                string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                InventoryResult result;
                if (toStash)
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Attempting stash pickup for [{protoName}]...");
                    result = TryAutoPickupToStash(item, logPrefix, loggingEnabled);
                }
                else
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Attempting general inventory pickup for [{protoName}]...");
                    result = TryAutoPickupToInventory(item, logPrefix, loggingEnabled);
                }

                if (result == InventoryResult.Success)
                {
                    if (loggingEnabled)
                        Logger.Info($"[{logPrefix}] Successfully picked up [{protoName}] for player [{this}].");
                    avatar.TryActivateOnLootPickupProcs(item);
                }
                else
                {
                    if (loggingEnabled)
                        Logger.Trace($"[{logPrefix}] Pickup failed for [{protoName}] - result={result}. Leaving on ground.");
                }
            }
        }

        /// <summary>
        /// Logs the outcome of an inventory move and warns if the item appears to have been
        /// prematurely destroyed or ended up in an unexpected inventory.
        /// </summary>
        private void LogAutoPickupMoveResult(Item item, Inventory destination, InventoryResult result, ulong? stackEntityId, string logPrefix, bool loggingEnabled)
        {
            if (loggingEnabled == false) return;

            string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
            if (result != InventoryResult.Success)
            {
                Logger.Trace($"[{logPrefix}] Move result for [{protoName}]: {result}");
                return;
            }

            bool isDestroyed = item.IsDestroyed;
            bool isScheduledToDestroy = item.IsScheduledToDestroy;
            PrototypeId actualInventoryRef = item.InventoryLocation.InventoryRef;
            PrototypeId expectedInventoryRef = destination?.PrototypeDataRef ?? PrototypeId.Invalid;
            bool isInExpectedInventory = actualInventoryRef == expectedInventoryRef;
            bool wasStacked = stackEntityId != InvalidId;

            Logger.Trace($"[{logPrefix}] Move success for [{protoName}]: destroyed={isDestroyed}, scheduledToDestroy={isScheduledToDestroy}, inExpectedInventory={isInExpectedInventory}, wasStacked={wasStacked}, stackEntityId={stackEntityId}");

            // The earlier deletion bug: item.Destroy() called after a successful non-stacking move.
            if (isDestroyed && wasStacked == false)
                Logger.Warn($"[{logPrefix}] Item [{protoName}] was destroyed after a successful move but was not reported as stacked. This may indicate premature destruction.");

            if (isDestroyed == false && isInExpectedInventory == false && wasStacked == false)
                Logger.Warn($"[{logPrefix}] Item [{protoName}] is not destroyed but is not in the expected inventory [{GameDatabase.GetPrototypeName(expectedInventoryRef)}], actual=[{GameDatabase.GetPrototypeName(actualInventoryRef)}].");
        }

        /// <summary>
        /// Attempts to move an item into the player's general inventory.
        /// </summary>
        private InventoryResult TryAutoPickupToInventory(Item item, string logPrefix = null, bool loggingEnabled = false)
        {
            Inventory inventory = GetInventory(InventoryConvenienceLabel.General);
            if (inventory == null)
                return InventoryResult.NoAvailableInventory;

            ulong? stackEntityId = InvalidId;
            InventoryResult result = item.ChangeInventoryLocation(inventory, Inventory.InvalidSlot, ref stackEntityId, true);
            LogAutoPickupMoveResult(item, inventory, result, stackEntityId, logPrefix, loggingEnabled);
            return result;
        }

        /// <summary>
        /// Attempts to move an item into the first unlocked stash tab that accepts it,
        /// falling back to the general inventory if no stash tab works.
        /// </summary>
        private InventoryResult TryAutoPickupToStash(Item item, string logPrefix = null, bool loggingEnabled = false)
        {
            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true))
            {
                foreach (PrototypeId stashRef in stashRefs)
                {
                    Inventory stashInv = GetInventoryByRef(stashRef);
                    if (stashInv == null) continue;

                    if (stashInv.Prototype.AllowEntity(item.Prototype) == false)
                        continue;

                    ulong? stackEntityId = InvalidId;
                    InventoryResult result = item.ChangeInventoryLocation(stashInv, Inventory.InvalidSlot, ref stackEntityId, true);
                    LogAutoPickupMoveResult(item, stashInv, result, stackEntityId, logPrefix, loggingEnabled);
                    if (result == InventoryResult.Success)
                        return result;
                }
            }

            // Fall back to general inventory
            return TryAutoPickupToInventory(item, logPrefix, loggingEnabled);
        }

        #endregion

        #region Relic Helpers

        /// <summary>
        /// Returns the avatar's equipment inventory that corresponds to the specified <see cref="EquipmentInvUISlot"/>.
        /// </summary>
        private Inventory GetAvatarEquipmentInventoryForSlot(Avatar avatar, EquipmentInvUISlot slot)
        {
            AvatarPrototype avatarProto = avatar?.AvatarPrototype;
            if (avatarProto?.EquipmentInventories == null)
                return null;

            foreach (AvatarEquipInventoryAssignmentPrototype assignment in avatarProto.EquipmentInventories)
            {
                if (assignment.UISlot == slot)
                    return avatar.GetInventoryByRef(assignment.Inventory);
            }

            return null;
        }

        /// <summary>
        /// Returns the equipped <see cref="Item"/> that matches the given relic prototype and has room
        /// for the new relic to stack onto it. Returns <see langword="null"/> if no such equipped relic exists
        /// or if the stack is already at max capacity.
        /// </summary>
        private Item GetEquippedRelicForStacking(Avatar avatar, Item relic)
        {
            Inventory relicInv = GetAvatarEquipmentInventoryForSlot(avatar, EquipmentInvUISlot.Relic);
            if (relicInv == null)
                return null;

            foreach (var entry in relicInv)
            {
                if (entry.ProtoRef != relic.PrototypeDataRef)
                    continue;

                Item equippedRelic = Game.EntityManager.GetEntity<Item>(entry.Id);
                if (equippedRelic != null && relic.CanStackOnto(equippedRelic, isAdding: true))
                    return equippedRelic;
            }

            return null;
        }

        #endregion

        #region Scheduling

        private void ScheduleModItemPickupAutoEvent()
        {
            if (_modItemPickupAutoEvent.IsValid) return;
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null) return;
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.ModItemPickupAutoEnable == false) return;

            // Clamp the interval to a sane floor so misconfiguration can't busy-loop the scheduler.
            int intervalMs = Math.Max(50, customOptions.ModItemPickupAutoIntervalMs);
            scheduler.ScheduleEvent(_modItemPickupAutoEvent, TimeSpan.FromMilliseconds(intervalMs), _pendingEvents);
            _modItemPickupAutoEvent.Get().Initialize(this);
        }

        #endregion

        #endregion

        private class ModItemPickupAutoEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoModItemPickupAutoTick();
        }
    }
}
