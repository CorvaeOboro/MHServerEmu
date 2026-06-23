using System;
using System.Collections.Generic;
using System.Linq;
using MHServerEmu.Core.Collisions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Dialog;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.InteractNearbyAuto;
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
        private readonly EventPointer<ItemAutoPickupEvent> _itemAutoPickupEvent = new();
        private readonly EventPointer<InteractNearbyAutoEvent> _interactNearbyAutoEvent = new();
        private readonly EventPointer<ChestAutoOpenEvent> _chestAutoOpenEvent = new();

        #region Item Auto-Pickup

        /// <summary>
        /// Periodic per-player tick that vacuums up nearby <see cref="Item"/> entities flagged as
        /// currency (Eternity Splinters, Cube Shards, etc.). Controlled by config.ini
        /// </summary>
        private void DoItemAutoPickupTick()
        {
            //  the live config toggle so admins can flip it off without a restart.
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.ItemAutoPickupEnable == false)
                return;

            Avatar avatar = CurrentAvatar;
            Region region = avatar?.Region;
            if (avatar == null || avatar.IsInWorld == false || region == null)
            {
                // Player may be transitioning regions or dead; just try again next tick.
                ScheduleItemAutoPickupEvent();
                return;
            }

            float radius = Math.Max(1f, customOptions.ItemAutoPickupRadius);
            Sphere volume = new(avatar.RegionLocation.Position, radius);

            // Gather candidates into a list first so we can safely mutate the world (destroy items)
            // outside the spatial-partition iteration. Same pattern as Pet vacuum.
            using var candidatesHandle = ListPool<Item>.Instance.Get(out List<Item> candidates);
            HashSet<Item> pickedUp = new();
            foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
            {
                if (worldEntity is not Item item)
                    continue;

                // Instanced loot: skip items belonging to other players.
                ulong restrictedToPlayerGuid = item.Properties[PropertyEnum.RestrictedToPlayerGuid];
                if (restrictedToPlayerGuid != 0 && restrictedToPlayerGuid != DatabaseUniqueId)
                    continue;

                // Only consider items currently on the ground (root-owner). Already-inventoried
                // items would have a non-root owner.
                if (item.IsRootOwner == false)
                    continue;

                // Currency-only filter: ItemPrototype.IsCurrency is true when the item carries
                // PropertyEnum.ItemCurrency (Eternity Splinters, Cube Shards).
                // Additional hardcoded seasonal/event currencies are explicitly included by exact proto path.
                string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                bool isHardcodedCurrency =
                    protoName == "Entity/Items/CurrencyItems/SeasonalLE/Seasonal/Anniversary/BirthdayCakeSlice2016.prototype" ||
                    protoName == "Entity/Items/CurrencyItems/SeasonalLE/Recurring/AgentNikkisSpecialKetchup.prototype";
                if (item.Prototype is not ItemPrototype itemProto ||
                    (itemProto.IsCurrency == false && isHardcodedCurrency == false))
                    continue;

                candidates.Add(item);
            }

            foreach (Item item in candidates)
            {
                // AcquireCurrencyItem returns true only after successfully applying currency props.
                bool acquired = AcquireCurrencyItem(item);
                if (acquired == false)
                {
                    // Fallback: hardcoded seasonal/event currency items may not have consumable
                    // currency properties, so pick them up as regular inventory items.
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                    bool isHardcodedCurrency =
                        protoName == "Entity/Items/CurrencyItems/SeasonalLE/Seasonal/Anniversary/BirthdayCakeSlice2016.prototype" ||
                        protoName == "Entity/Items/CurrencyItems/SeasonalLE/Recurring/AgentNikkisSpecialKetchup.prototype";
                    if (isHardcodedCurrency)
                        acquired = TryAutoPickupToInventory(item) == InventoryResult.Success;
                }

                if (acquired == false)
                    continue;

                pickedUp.Add(item);
                avatar.TryActivateOnLootPickupProcs(item);
                item.Destroy();
            }

            // --- Crafting ingredient pickup (additive, runs after currency loop) ---
            if (customOptions.ItemAutoPickupCraftingIngredientEnable)
            {
                bool itemAutoPickupCraftingIngredientLogging = customOptions.ItemAutoPickupCraftingIngredientLoggingEnable;
                if (itemAutoPickupCraftingIngredientLogging)
                    Logger.Trace($"[ItemAutoPickupCrafting] Starting crafting ingredient scan for player [{this}]. radius={radius}, toStash={customOptions.ItemAutoPickupCraftingIngredientToStash}");
                candidates.Clear();
                int scanned = 0;
                int filteredOut = 0;
                foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
                {
                    if (worldEntity is not Item item)
                        continue;

                    scanned++;
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);

                    ulong restrictedToPlayerGuid = item.Properties[PropertyEnum.RestrictedToPlayerGuid];
                    if (restrictedToPlayerGuid != 0 && restrictedToPlayerGuid != DatabaseUniqueId)
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Skipping [{protoName}] \u2014 instanced loot for another player.");
                        filteredOut++;
                        continue;
                    }

                    if (item.IsRootOwner == false)
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Skipping [{protoName}] \u2014 not a root-owned item (already in inventory).");
                        filteredOut++;
                        continue;
                    }

                    // Only auto-pickup specific crafting ingredients by exact prototype path.
                    bool isWhitelistedCraftingIngredient =
                        protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Astral.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Genome.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Nano.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/ElementProtos/Elements/Ionic.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/SpiritOfIvaldi.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/SpiritOfYmir.prototype" ||
                        protoName == "Entity/Items/Crafting/Ingredients/UnstableMolecule.prototype";
                    if (isWhitelistedCraftingIngredient == false)
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Skipping [{protoName}] \u2014 not a whitelisted crafting ingredient.");
                        filteredOut++;
                        continue;
                    }

                    if (pickedUp.Contains(item))
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Skipping [{protoName}] \u2014 already picked up by currency loop.");
                        filteredOut++;
                        continue;
                    }

                    if (itemAutoPickupCraftingIngredientLogging)
                        Logger.Trace($"[ItemAutoPickupCrafting] Candidate accepted: [{protoName}]");
                    candidates.Add(item);
                }

                if (itemAutoPickupCraftingIngredientLogging)
                    Logger.Trace($"[ItemAutoPickupCrafting] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");

                foreach (Item item in candidates)
                {
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                    InventoryResult result;
                    if (customOptions.ItemAutoPickupCraftingIngredientToStash)
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Attempting stash pickup for [{protoName}]...");
                        result = TryAutoPickupToStash(item);
                    }
                    else
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Attempting general inventory pickup for [{protoName}]...");
                        result = TryAutoPickupToInventory(item);
                    }

                    if (result == InventoryResult.Success)
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Info($"[ItemAutoPickupCrafting] Successfully picked up [{protoName}] for player [{this}].");
                        avatar.TryActivateOnLootPickupProcs(item);
                        item.Destroy();
                    }
                    else
                    {
                        if (itemAutoPickupCraftingIngredientLogging)
                            Logger.Trace($"[ItemAutoPickupCrafting] Pickup failed for [{protoName}] \u2014 result={result}. Leaving on ground.");
                    }
                }
            }

            // --- Relic pickup (additive, runs after currency and crafting loops) ---
            if (customOptions.ItemAutoPickupRelicEnable)
            {
                bool itemAutoPickupRelicLogging = customOptions.ItemAutoPickupRelicLoggingEnable;
                if (itemAutoPickupRelicLogging)
                    Logger.Trace($"[ItemAutoPickupRelic] Starting relic scan for player [{this}]. radius={radius}, toStash={customOptions.ItemAutoPickupRelicToStash}, equipSameType={customOptions.ItemAutoPickupRelicEquipIfSameTypeEquippedEnable}");
                candidates.Clear();
                int scanned = 0;
                int filteredOut = 0;
                foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
                {
                    if (worldEntity is not Item item)
                        continue;

                    scanned++;
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);

                    ulong restrictedToPlayerGuid = item.Properties[PropertyEnum.RestrictedToPlayerGuid];
                    if (restrictedToPlayerGuid != 0 && restrictedToPlayerGuid != DatabaseUniqueId)
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Trace($"[ItemAutoPickupRelic] Skipping [{protoName}] \u2014 instanced loot for another player.");
                        filteredOut++;
                        continue;
                    }

                    if (item.IsRootOwner == false)
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Trace($"[ItemAutoPickupRelic] Skipping [{protoName}] \u2014 not a root-owned item.");
                        filteredOut++;
                        continue;
                    }

                    if (item.Prototype is not RelicPrototype)
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Trace($"[ItemAutoPickupRelic] Skipping [{protoName}] \u2014 not a RelicPrototype.");
                        filteredOut++;
                        continue;
                    }

                    if (pickedUp.Contains(item))
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Trace($"[ItemAutoPickupRelic] Skipping [{protoName}] \u2014 already picked up by an earlier loop.");
                        filteredOut++;
                        continue;
                    }

                    if (itemAutoPickupRelicLogging)
                        Logger.Trace($"[ItemAutoPickupRelic] Candidate accepted: [{protoName}]");
                    candidates.Add(item);
                }

                if (itemAutoPickupRelicLogging)
                    Logger.Trace($"[ItemAutoPickupRelic] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");

                foreach (Item item in candidates)
                {
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                    InventoryResult result = InventoryResult.Invalid;

                    // Optional: try to stack onto an already-equipped relic of the same type
                    Item equippedRelic = GetEquippedRelicForStacking(avatar, item);
                    if (customOptions.ItemAutoPickupRelicEquipIfSameTypeEquippedEnable && equippedRelic != null)
                    {
                        Inventory relicInv = GetAvatarEquipmentInventoryForSlot(avatar, EquipmentInvUISlot.Relic);
                        if (relicInv != null)
                        {
                            if (itemAutoPickupRelicLogging)
                                Logger.Trace($"[ItemAutoPickupRelic] Attempting equip-to-stack for [{protoName}] into avatar relic slot...");
                            ulong? stackEntityId = InvalidId;
                            result = item.ChangeInventoryLocation(relicInv, Inventory.InvalidSlot, ref stackEntityId, true);
                            if (itemAutoPickupRelicLogging)
                                Logger.Trace($"[ItemAutoPickupRelic] Equip-to-stack result for [{protoName}]: {result}");
                        }
                    }

                    // If equip-to-stack was not attempted or failed, fall back to inventory/stash
                    if (result != InventoryResult.Success)
                    {
                        if (customOptions.ItemAutoPickupRelicToStash)
                        {
                            if (itemAutoPickupRelicLogging)
                                Logger.Trace($"[ItemAutoPickupRelic] Attempting stash pickup for [{protoName}]...");
                            result = TryAutoPickupToStash(item);
                        }
                        else
                        {
                            if (itemAutoPickupRelicLogging)
                                Logger.Trace($"[ItemAutoPickupRelic] Attempting general inventory pickup for [{protoName}]...");
                            result = TryAutoPickupToInventory(item);
                        }
                    }

                    if (result == InventoryResult.Success)
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Info($"[ItemAutoPickupRelic] Successfully picked up [{protoName}] for player [{this}].");
                        avatar.TryActivateOnLootPickupProcs(item);
                        item.Destroy();
                    }
                    else
                    {
                        if (itemAutoPickupRelicLogging)
                            Logger.Trace($"[ItemAutoPickupRelic] Pickup failed for [{protoName}] \u2014 result={result}. Leaving on ground.");
                    }
                }
            }

            // --- Rune pickup (additive, runs after all prior loops) ---
            if (customOptions.ItemAutoPickupRuneEnable)
            {
                bool itemAutoPickupRuneLogging = customOptions.ItemAutoPickupRuneLoggingEnable;
                if (itemAutoPickupRuneLogging)
                    Logger.Trace($"[ItemAutoPickupRune] Starting rune scan for player [{this}]. radius={radius}, toStash={customOptions.ItemAutoPickupRuneToStash}");
                candidates.Clear();
                int scanned = 0;
                int filteredOut = 0;
                foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
                {
                    if (worldEntity is not Item item)
                        continue;

                    scanned++;
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);

                    ulong restrictedToPlayerGuid = item.Properties[PropertyEnum.RestrictedToPlayerGuid];
                    if (restrictedToPlayerGuid != 0 && restrictedToPlayerGuid != DatabaseUniqueId)
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Skipping [{protoName}] \u2014 instanced loot for another player.");
                        filteredOut++;
                        continue;
                    }

                    if (item.IsRootOwner == false)
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Skipping [{protoName}] \u2014 not a root-owned item.");
                        filteredOut++;
                        continue;
                    }

                    // Runes are identified by path since there is no RunePrototype class
                    if (protoName.Contains("/Runewords/Glyphs/RunewordGlyph") == false &&
                        protoName.Contains("/Runewords/Glyphs/OnslaughtRune") == false)
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Skipping [{protoName}] \u2014 not a recognized rune item.");
                        filteredOut++;
                        continue;
                    }

                    if (pickedUp.Contains(item))
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Skipping [{protoName}] \u2014 already picked up by an earlier loop.");
                        filteredOut++;
                        continue;
                    }

                    if (itemAutoPickupRuneLogging)
                        Logger.Trace($"[ItemAutoPickupRune] Candidate accepted: [{protoName}]");
                    candidates.Add(item);
                }

                if (itemAutoPickupRuneLogging)
                    Logger.Trace($"[ItemAutoPickupRune] Scan complete. scanned={scanned}, filteredOut={filteredOut}, candidates={candidates.Count}");

                foreach (Item item in candidates)
                {
                    string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                    InventoryResult result;
                    if (customOptions.ItemAutoPickupRuneToStash)
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Attempting stash pickup for [{protoName}]...");
                        result = TryAutoPickupToStash(item);
                    }
                    else
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Attempting general inventory pickup for [{protoName}]...");
                        result = TryAutoPickupToInventory(item);
                    }

                    if (result == InventoryResult.Success)
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Info($"[ItemAutoPickupRune] Successfully picked up [{protoName}] for player [{this}].");
                        avatar.TryActivateOnLootPickupProcs(item);
                        item.Destroy();
                    }
                    else
                    {
                        if (itemAutoPickupRuneLogging)
                            Logger.Trace($"[ItemAutoPickupRune] Pickup failed for [{protoName}] \u2014 result={result}. Leaving on ground.");
                    }
                }
            }

            ScheduleItemAutoPickupEvent();
        }

        /// <summary>
        /// Attempts to move an item into the player's general inventory.
        /// </summary>
        private InventoryResult TryAutoPickupToInventory(Item item)
        {
            Inventory inventory = GetInventory(InventoryConvenienceLabel.General);
            if (inventory == null)
                return InventoryResult.NoAvailableInventory;

            ulong? stackEntityId = InvalidId;
            return item.ChangeInventoryLocation(inventory, Inventory.InvalidSlot, ref stackEntityId, true);
        }

        /// <summary>
        /// Attempts to move an item into the first unlocked stash tab that accepts it,
        /// falling back to the general inventory if no stash tab works.
        /// </summary>
        private InventoryResult TryAutoPickupToStash(Item item)
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
                    if (result == InventoryResult.Success)
                        return result;
                }
            }

            // Fall back to general inventory
            return TryAutoPickupToInventory(item);
        }

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

        private void ScheduleItemAutoPickupEvent()
        {
            if (_itemAutoPickupEvent.IsValid) return;
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null) return;
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.ItemAutoPickupEnable == false) return;

            // Clamp the interval to a sane floor so misconfiguration can't busy-loop the scheduler.
            int intervalMs = Math.Max(50, customOptions.ItemAutoPickupIntervalMs);
            scheduler.ScheduleEvent(_itemAutoPickupEvent, TimeSpan.FromMilliseconds(intervalMs), _pendingEvents);
            _itemAutoPickupEvent.Get().Initialize(this);
        }

        #endregion

        #region Interact Nearby 

        /// <summary>
        /// Periodic tick that automatically activates mission objectives and nearby civilians
        /// when the player's avatar moves within interaction range.
        /// </summary>
        private void DoInteractNearbyAutoTick()
        {
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.InteractNearbyAutoEnable == false)
                return;

            bool interactNearbyAutoLogging = customOptions.InteractNearbyAutoLoggingEnable;

            // Parse comma-separated whitelist / blacklist once per tick
            string[] whitelist = string.IsNullOrWhiteSpace(customOptions.InteractNearbyAutoWhitelist)
                ? Array.Empty<string>()
                : customOptions.InteractNearbyAutoWhitelist.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            string[] blacklist = string.IsNullOrWhiteSpace(customOptions.InteractNearbyAutoBlacklist)
                ? Array.Empty<string>()
                : customOptions.InteractNearbyAutoBlacklist.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

            Avatar avatar = CurrentAvatar;
            Region region = avatar?.Region;
            if (avatar == null || avatar.IsAliveInWorld == false || region == null)
            {
                if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] Tick skipped: avatar={{avatar?.ToString()}} region={{region?.ToString()}}");
                if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] Tick skipped: avatar alive={{avatar?.IsAliveInWorld ?? false}} region={{region?.ToString()}}");
                ScheduleInteractNearbyAutoEvent();
                return;
            }

            float baseRange = GameDatabase.GlobalsPrototype?.InteractRange ?? 400f;
            float radius = baseRange + 200f; // generous padding for spatial query
            Sphere volume = new(avatar.RegionLocation.Position, radius);

            int scanned = 0;
            int filtered = 0;
            int blacklisted = 0;
            int outOfRange = 0;
            int noInteract = 0;
            int wrongMethod = 0;
            int activated = 0;

            if (interactNearbyAutoLogging)
            {
                Logger.Trace($"[InteractNearbyAuto] Tick start for [{this}] avatar=[{avatar}] region=[{region}] radius={radius:F0}");
                InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] Tick start: avatar=[{avatar}] region=[{region}] radius={radius:F0} pos=({avatar.RegionLocation.Position.X:F0},{avatar.RegionLocation.Position.Y:F0},{avatar.RegionLocation.Position.Z:F0})");
            }

            foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
            {
                scanned++;
                if (worldEntity == null || worldEntity == avatar) continue;
                if (worldEntity.IsInWorld == false) continue;

                string entityName = GameDatabase.GetFormattedPrototypeName(worldEntity.PrototypeDataRef);
                ulong entityId = worldEntity.Id;

                // Hardcoded blacklist: NEVER auto-activate these entities (overrides everything)
                if (blacklist.Length > 0 && blacklist.Any(b => entityName.Contains(b, StringComparison.OrdinalIgnoreCase)))
                {
                    blacklisted++;
                    if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — blacklisted");
                    if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — blacklisted (matches one of: {string.Join(", ", blacklist)})");
                    continue;
                }

                // Pre-compute type flags for logging (used even when whitelisted)
                bool isMissionObjective = worldEntity.MissionPrototype != PrototypeId.Invalid;
                bool isCivilian = worldEntity is Agent agent && agent.IsHostileToPlayers() == false;

                // Whitelist bypass: if the entity matches the whitelist, skip all type filtering
                bool isWhitelisted = whitelist.Length > 0 && whitelist.Any(w => entityName.Contains(w, StringComparison.OrdinalIgnoreCase));
                if (isWhitelisted == false)
                {
                    // Not whitelisted — apply normal type filters
                    if (worldEntity is Item)
                    {
                        if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — is an Item (not whitelisted)");
                        if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — is an Item (not whitelisted)");
                        continue; // exclude items / pickups unless explicitly whitelisted
                    }

                    // Exclude stashes — they are non-hostile agents but not "civilians" in the gameplay sense
                    if (worldEntity.Properties[PropertyEnum.OpenPlayerStash])
                    {
                        if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — is a stash (OpenPlayerStash)");
                        if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — is a stash (OpenPlayerStash)");
                        continue;
                    }

                    // Focus: mission objectives, non-hostile agents (civilians), or interactable world objects (consoles)
                    bool isInteractableWorldObject = worldEntity is WorldEntity
                        && worldEntity is not Agent
                        && worldEntity is not Item
                        && worldEntity is not Hotspot;
                    if (isMissionObjective == false && isCivilian == false && isInteractableWorldObject == false)
                    {
                        filtered++;
                        if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — not mission objective, civilian, or interactable world object (MissionProto={worldEntity.MissionPrototype})");
                        if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — not mission/civilian/worldObject  MissionProto={worldEntity.MissionPrototype}  EntityType={worldEntity.GetType().Name}");
                        continue;
                    }
                }
                else if (interactNearbyAutoLogging)
                {
                    Logger.Trace($"[InteractNearbyAuto] WHITELISTED [{entityName}#{entityId}] — bypassing type filter");
                    InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] WHITELISTED [{entityName}#{entityId}] — bypassing type filter");
                }

                if (avatar.InInteractRange(worldEntity, InteractionMethod.Use) == false)
                {
                    outOfRange++;
                    if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — out of interact range");
                    if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — out of range");
                    continue;
                }

                InteractData interactData = new();
                var interactionStatus = InteractionManager.CallGetInteractionStatus(
                    new EntityDesc(worldEntity), avatar,
                    InteractionOptimizationFlags.None, InteractionFlags.Default, ref interactData);

                if (interactionStatus == InteractionMethod.None)
                {
                    noInteract++;
                    if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — CallGetInteractionStatus returned None");
                    if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — interactionStatus=None");
                    continue;
                }

                // Only auto-trigger Use or Converse interactions (never PickUp)
                if (interactionStatus.HasFlag(InteractionMethod.Use) == false
                    && interactionStatus.HasFlag(InteractionMethod.Converse) == false)
                {
                    wrongMethod++;
                    if (interactNearbyAutoLogging) Logger.Trace($"[InteractNearbyAuto] SKIP [{entityName}#{entityId}] — interactionStatus={interactionStatus} (needs Use or Converse)");
                    if (interactNearbyAutoLogging) InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] — interactionStatus={interactionStatus} (needs Use|Converse)");
                    continue;
                }

                activated++;
                if (interactNearbyAutoLogging)
                {
                    Logger.Info($"[InteractNearbyAuto] ACTIVATE [{entityName}#{entityId}] — interactionStatus={interactionStatus} isMission={isMissionObjective} isCivilian={isCivilian}");
                    InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] ACTIVATE [{entityName}#{entityId}] — interactionStatus={interactionStatus} isMission={isMissionObjective} isCivilian={isCivilian} missionRef={worldEntity.MissionPrototype}");
                }
                avatar.UseInteractableObject(worldEntity.Id, PrototypeId.Invalid);
            }

            if (interactNearbyAutoLogging)
            {
                string summary = $"[InteractNearbyAuto] Tick end for [{this}]: scanned={scanned} filtered={filtered} blacklisted={blacklisted} outOfRange={outOfRange} noInteract={noInteract} wrongMethod={wrongMethod} activated={activated}";
                Logger.Trace(summary);
                InteractObjectAutomaticLogCollator.WriteLine(Id, $"[InteractNearbyAuto_AUTO] {summary}");
            }

            ScheduleInteractNearbyAutoEvent();
        }

        private void ScheduleInteractNearbyAutoEvent()
        {
            if (_interactNearbyAutoEvent.IsValid) return;
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null) return;
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.InteractNearbyAutoEnable == false) return;

            int intervalMs = Math.Max(50, customOptions.InteractNearbyAutoIntervalMs);
            scheduler.ScheduleEvent(_interactNearbyAutoEvent, TimeSpan.FromMilliseconds(intervalMs), _pendingEvents);
            _interactNearbyAutoEvent.Get().Initialize(this);
        }


        #endregion

        #region Chest Open Auto
        private void DoChestAutoOpenTick()
        {
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.ItemChestAutoOpenEnable == false)
            {
                Logger.Trace("DoChestAutoOpenTick(): Mod disabled or config missing.");
                return;
            }

            bool itemChestAutoOpenLogging = customOptions.ItemChestAutoOpenLoggingEnable;

            Avatar avatar = CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
            {
                if (itemChestAutoOpenLogging)
                    Logger.Trace($"DoChestAutoOpenTick(): Avatar null or not in world. avatar=[{avatar}]. Rescheduling.");
                ScheduleChestAutoOpenEvent();
                return;
            }

            Inventory generalInv = GetInventory(InventoryConvenienceLabel.General);
            if (generalInv == null)
            {
                if (itemChestAutoOpenLogging)
                    Logger.Trace("DoChestAutoOpenTick(): General inventory is null. Rescheduling.");
                ScheduleChestAutoOpenEvent();
                return;
            }

            if (itemChestAutoOpenLogging)
                Logger.Trace($"DoChestAutoOpenTick(): Scanning General inventory with {generalInv.Count} item(s) for player [{this}].");

            var whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string configWhitelist = customOptions.ItemChestAutoOpenWhitelist;
            if (string.IsNullOrWhiteSpace(configWhitelist) == false)
            {
                foreach (string part in configWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = part.Trim();
                    if (trimmed.Length > 0)
                        whitelist.Add(trimmed);
                }
            }
            else
            {
                whitelist.Add("Chest");
                whitelist.Add("Crate");
                whitelist.Add("LootBox");
                whitelist.Add("Giftbox");
                whitelist.Add("GiftBox");
            }

            if (itemChestAutoOpenLogging)
                Logger.Trace($"DoChestAutoOpenTick(): Whitelist patterns ({whitelist.Count}): [{string.Join(", ", whitelist)}]");

            foreach (var entry in generalInv)
            {
                Item item = Game.EntityManager.GetEntity<Item>(entry.Id);
                if (item == null)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Trace($"DoChestAutoOpenTick(): Skipping slot {entry.Slot} — entity not found for Id={entry.Id}.");
                    continue;
                }

                string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
                if (itemChestAutoOpenLogging)
                    Logger.Trace($"DoChestAutoOpenTick(): Evaluating item [{protoName}] at slot {entry.Slot}.");

                if (item.Prototype is not ItemPrototype itemProto)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Trace($"DoChestAutoOpenTick(): Skipping [{protoName}] — not an ItemPrototype.");
                    continue;
                }

                if (itemProto.IsUsable == false)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Trace($"DoChestAutoOpenTick(): Skipping [{protoName}] — IsUsable is false.");
                    continue;
                }

                bool isWhitelisted = false;
                string matchedPattern = null;
                foreach (string pattern in whitelist)
                {
                    if (protoName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        isWhitelisted = true;
                        matchedPattern = pattern;
                        break;
                    }
                }

                if (isWhitelisted == false)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Trace($"DoChestAutoOpenTick(): Skipping [{protoName}] — no whitelist match.");
                    continue;
                }

                if (itemChestAutoOpenLogging)
                    Logger.Trace($"DoChestAutoOpenTick(): [{protoName}] matched whitelist pattern '{matchedPattern}'.");

                bool canUse = item.CanUse(avatar, checkPower: true, checkInventory: false);
                if (canUse == false)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Trace($"DoChestAutoOpenTick(): Skipping [{protoName}] — CanUse returned false.");
                    continue;
                }

                if (itemChestAutoOpenLogging)
                    Logger.Trace($"DoChestAutoOpenTick(): Attempting to open [{protoName}] for player [{this}]...");
                bool opened = item.InteractWithAvatar(avatar);
                if (opened)
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Info($"DoChestAutoOpenTick(): Successfully opened [{protoName}] for player [{this}].");
                }
                else
                {
                    if (itemChestAutoOpenLogging)
                        Logger.Warn($"DoChestAutoOpenTick(): InteractWithAvatar returned false for [{protoName}].");
                }

                ScheduleChestAutoOpenEvent();
                return;
            }

            if (itemChestAutoOpenLogging)
                Logger.Trace("DoChestAutoOpenTick(): No openable chests found in General inventory. Rescheduling next scan.");
            ScheduleChestAutoOpenEvent();
        }

        private void ScheduleChestAutoOpenEvent()
        {
            if (_chestAutoOpenEvent.IsValid)
            {
                Logger.Trace("ScheduleChestAutoOpenEvent(): Event already valid, not rescheduling.");
                return;
            }
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null)
            {
                Logger.Trace("ScheduleChestAutoOpenEvent(): Scheduler is null.");
                return;
            }
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.ItemChestAutoOpenEnable == false)
            {
                Logger.Trace("ScheduleChestAutoOpenEvent(): Mod disabled or config missing.");
                return;
            }

            bool itemChestAutoOpenLogging = customOptions.ItemChestAutoOpenLoggingEnable;
            int cooldownMs = Math.Max(500, customOptions.ItemChestAutoOpenCooldownMs);
            if (itemChestAutoOpenLogging)
                Logger.Trace($"ScheduleChestAutoOpenEvent(): Scheduling next tick in {cooldownMs}ms for player [{this}].");
            scheduler.ScheduleEvent(_chestAutoOpenEvent, TimeSpan.FromMilliseconds(cooldownMs), _pendingEvents);
            _chestAutoOpenEvent.Get().Initialize(this);
        }

        #endregion

        private class ItemAutoPickupEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoItemAutoPickupTick();
        }

        private class ChestAutoOpenEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoChestAutoOpenTick();
        }

        private class InteractNearbyAutoEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoInteractNearbyAutoTick();
        }
    }
}
