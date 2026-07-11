#region Chest Open Auto
// =============================================================================
// MOD Chest Open Auto
// =============================================================================
//   opens usable chest-like items in the player's general inventory on a timer.
//
//   Scans inventory for items matching a whitelist.
//   Matching items that are usable ( lvl requirment )  are used, which consumes or opens them.
//   Whitelist defaults to Chest, Crate, LootBox, Giftbox, GiftBox.
//
//  Config.ini :
//   ModChestOpenAutoEnable, ModChestOpenAutoIntervalMs, ModChestOpenAutoLoggingEnable
//   ModChestOpenAutoWhitelist
//
//  VERSION:: 20260711
// =============================================================================

using System;
using System.Collections.Generic;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private readonly EventPointer<ModChestOpenAutoEvent> _modChestOpenAutoEvent = new();

        #region Tick

        private void DoModChestOpenAutoTick()
        {
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.ModChestOpenAutoEnable == false)
            {
                Logger.Trace("DoModChestOpenAutoTick(): Mod disabled or config missing.");
                return;
            }

            bool itemModChestOpenAutoLogging = customOptions.ModChestOpenAutoLoggingEnable;

            Avatar avatar = CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
            {
                if (itemModChestOpenAutoLogging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Avatar null or not in world. avatar=[{avatar}]. Rescheduling.");
                ScheduleModChestOpenAutoEvent();
                return;
            }

            Inventory generalInv = GetInventory(InventoryConvenienceLabel.General);
            if (generalInv == null)
            {
                if (itemModChestOpenAutoLogging)
                    Logger.Trace("DoModChestOpenAutoTick(): General inventory is null. Rescheduling.");
                ScheduleModChestOpenAutoEvent();
                return;
            }

            if (itemModChestOpenAutoLogging)
                Logger.Trace($"DoModChestOpenAutoTick(): Scanning General inventory with {generalInv.Count} item(s) for player [{this}].");

            HashSet<string> whitelist = GetModChestOpenAutoWhitelist(customOptions.ModChestOpenAutoWhitelist);

            if (itemModChestOpenAutoLogging)
                Logger.Trace($"DoModChestOpenAutoTick(): Whitelist patterns ({whitelist.Count}): [{string.Join(", ", whitelist)}]");

            foreach (var entry in generalInv)
            {
                Item item = Game.EntityManager.GetEntity<Item>(entry.Id);
                if (TryOpenChestItem(item, entry.Slot, avatar, whitelist, itemModChestOpenAutoLogging))
                {
                    ScheduleModChestOpenAutoEvent();
                    return;
                }
            }

            if (itemModChestOpenAutoLogging)
                Logger.Trace("DoModChestOpenAutoTick(): No openable chests found in General inventory. Rescheduling next scan.");
            ScheduleModChestOpenAutoEvent();
        }

        #endregion

        #region Helpers

        private static HashSet<string> GetModChestOpenAutoWhitelist(string configWhitelist)
        {
            var whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
            return whitelist;
        }

        private bool TryOpenChestItem(Item item, uint slot, Avatar avatar, HashSet<string> whitelist, bool logging)
        {
            if (item == null)
            {
                if (logging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Skipping slot {slot} - entity not found.");
                return false;
            }

            string protoName = GameDatabase.GetPrototypeName(item.Prototype.DataRef);
            if (logging)
                Logger.Trace($"DoModChestOpenAutoTick(): Evaluating item [{protoName}] at slot {slot}.");

            if (item.Prototype is not ItemPrototype itemProto)
            {
                if (logging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Skipping [{protoName}] - not an ItemPrototype.");
                return false;
            }

            if (itemProto.IsUsable == false)
            {
                if (logging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Skipping [{protoName}] - IsUsable is false.");
                return false;
            }

            string matchedPattern = null;
            foreach (string pattern in whitelist)
            {
                if (protoName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    matchedPattern = pattern;
                    break;
                }
            }

            if (matchedPattern == null)
            {
                if (logging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Skipping [{protoName}] - no whitelist match.");
                return false;
            }

            if (logging)
                Logger.Trace($"DoModChestOpenAutoTick(): [{protoName}] matched whitelist pattern '{matchedPattern}'.");

            bool canUse = item.CanUse(avatar, checkPower: true, checkInventory: false);
            if (canUse == false)
            {
                if (logging)
                    Logger.Trace($"DoModChestOpenAutoTick(): Skipping [{protoName}] - CanUse returned false.");
                return false;
            }

            if (logging)
                Logger.Trace($"DoModChestOpenAutoTick(): Attempting to open [{protoName}] for player [{this}]...");
            bool opened = item.InteractWithAvatar(avatar);
            if (opened)
            {
                if (logging)
                    Logger.Info($"DoModChestOpenAutoTick(): Successfully opened [{protoName}] for player [{this}].");
            }
            else
            {
                if (logging)
                    Logger.Warn($"DoModChestOpenAutoTick(): InteractWithAvatar returned false for [{protoName}].");
            }

            return true;
        }

        #endregion

        #region Scheduling

        private void ScheduleModChestOpenAutoEvent()
        {
            if (_modChestOpenAutoEvent.IsValid)
            {
                Logger.Trace("ScheduleModChestOpenAutoEvent(): Event already valid, not rescheduling.");
                return;
            }
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null)
            {
                Logger.Trace("ScheduleModChestOpenAutoEvent(): Scheduler is null.");
                return;
            }
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.ModChestOpenAutoEnable == false)
            {
                Logger.Trace("ScheduleModChestOpenAutoEvent(): Mod disabled or config missing.");
                return;
            }

            bool itemModChestOpenAutoLogging = customOptions.ModChestOpenAutoLoggingEnable;
            int cooldownMs = Math.Max(500, customOptions.ModChestOpenAutoCooldownMs);
            if (itemModChestOpenAutoLogging)
                Logger.Trace($"ScheduleModChestOpenAutoEvent(): Scheduling next tick in {cooldownMs}ms for player [{this}].");
            scheduler.ScheduleEvent(_modChestOpenAutoEvent, TimeSpan.FromMilliseconds(cooldownMs), _pendingEvents);
            _modChestOpenAutoEvent.Get().Initialize(this);
        }

        #endregion

        #endregion

        private class ModChestOpenAutoEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoModChestOpenAutoTick();
        }
    }
}
