
#region Interact Nearby Auto
// =============================================================================
// MOD Interact Nearby Auto
// =============================================================================
//   interacts with nearby mission objectives, civilians, and
//   interactable world objects on a timer.
//
//   Scans a radius around the player's hero.
//   Targets must support Use or Converse interaction methods.
//   Items and stashes are excluded unless explicitly whitelisted.
//
//  Config.ini :
//   ModInteractNearbyAutoEnable, ModInteractNearbyAutoIntervalMs
//   ModInteractNearbyAutoLoggingEnable
//   ModInteractNearbyAutoWhitelist, ModInteractNearbyAutoBlacklist
//
//  VERSION:: 20260711
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using MHServerEmu.Core.Collisions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Dialog;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Logging;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private readonly EventPointer<ModInteractNearbyAutoEvent> _modInteractNearbyAutoEvent = new();

        #region Tick

        /// <summary>
        /// Periodic tick that automatically activates mission objectives and nearby civilians
        /// when the player's avatar moves within interaction range.
        /// </summary>
        private void DoModInteractNearbyAutoTick()
        {
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.ModInteractNearbyAutoEnable == false)
                return;

            bool modInteractNearbyAutoLogging = customOptions.ModInteractNearbyAutoLoggingEnable;

            // Parse comma-separated whitelist / blacklist once per tick
            ParseModInteractNearbyAutoLists(
                customOptions.ModInteractNearbyAutoWhitelist,
                customOptions.ModInteractNearbyAutoBlacklist,
                out string[] whitelist,
                out string[] blacklist);

            Avatar avatar = CurrentAvatar;
            Region region = avatar?.Region;
            if (avatar == null || avatar.IsAliveInWorld == false || region == null)
            {
                if (modInteractNearbyAutoLogging) Logger.Trace($"[ModInteractNearbyAuto] Tick skipped: avatar={{avatar?.ToString()}} region={{region?.ToString()}}");
                if (modInteractNearbyAutoLogging) ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] Tick skipped: avatar alive={{avatar?.IsAliveInWorld ?? false}} region={{region?.ToString()}}");
                ScheduleModInteractNearbyAutoEvent();
                return;
            }

            float baseRange = GameDatabase.GlobalsPrototype?.InteractRange ?? 400f;
            float radius = baseRange + 200f; // generous padding for spatial query
            Sphere volume = new(avatar.RegionLocation.Position, radius);

            int scanned = 0;
            int filtered = 0;
            int blacklisted = 0;
            int invisible = 0;
            int outOfRange = 0;
            int noInteract = 0;
            int wrongMethod = 0;
            int activated = 0;

            if (modInteractNearbyAutoLogging)
            {
                Logger.Trace($"[ModInteractNearbyAuto] Tick start for [{this}] avatar=[{avatar}] region=[{region}] radius={radius:F0}");
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] Tick start: avatar=[{avatar}] region=[{region}] radius={radius:F0} pos=({avatar.RegionLocation.Position.X:F0},{avatar.RegionLocation.Position.Y:F0},{avatar.RegionLocation.Position.Z:F0})");
            }

            foreach (WorldEntity worldEntity in region.IterateEntitiesInVolume(volume, new()))
            {
                scanned++;
                if (worldEntity == null || worldEntity == avatar || worldEntity.IsInWorld == false)
                    continue;

                string entityName = GameDatabase.GetFormattedPrototypeName(worldEntity.PrototypeDataRef);
                AutoInteractResult result = TryAutoActivateEntity(worldEntity, entityName, avatar, whitelist, blacklist, modInteractNearbyAutoLogging);
                switch (result)
                {
                    case AutoInteractResult.Blacklisted: blacklisted++; break;
                    case AutoInteractResult.Filtered: filtered++; break;
                    case AutoInteractResult.Invisible: invisible++; break;
                    case AutoInteractResult.OutOfRange: outOfRange++; break;
                    case AutoInteractResult.NoInteract: noInteract++; break;
                    case AutoInteractResult.WrongMethod: wrongMethod++; break;
                    case AutoInteractResult.Activated: activated++; break;
                }
            }

            if (modInteractNearbyAutoLogging)
            {
                string summary = $"[ModInteractNearbyAuto] Tick end for [{this}]: scanned={scanned} filtered={filtered} blacklisted={blacklisted} invisible={invisible} outOfRange={outOfRange} noInteract={noInteract} wrongMethod={wrongMethod} activated={activated}";
                Logger.Trace(summary);
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] {summary}");
            }

            ScheduleModInteractNearbyAutoEvent();
        }

        #endregion

        #region Helpers

        private enum AutoInteractResult
        {
            Blacklisted,
            Filtered,
            Invisible,
            OutOfRange,
            NoInteract,
            WrongMethod,
            Activated
        }

        private static void ParseModInteractNearbyAutoLists(string whitelistCsv, string blacklistCsv, out string[] whitelist, out string[] blacklist)
        {
            whitelist = string.IsNullOrWhiteSpace(whitelistCsv)
                ? Array.Empty<string>()
                : whitelistCsv.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            blacklist = string.IsNullOrWhiteSpace(blacklistCsv)
                ? Array.Empty<string>()
                : blacklistCsv.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }

        private AutoInteractResult TryAutoActivateEntity(WorldEntity worldEntity, string entityName, Avatar avatar, string[] whitelist, string[] blacklist, bool logging)
        {
            ulong entityId = worldEntity.Id;

            #region Blacklist Filter

            // Hardcoded blacklist: NEVER auto-activate these entities (overrides everything)
            if (blacklist.Length > 0 && blacklist.Any(b => entityName.Contains(b, StringComparison.OrdinalIgnoreCase)))
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - blacklisted");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - blacklisted (matches one of: {string.Join(", ", blacklist)})");
                }
                return AutoInteractResult.Blacklisted;
            }

            // Pre-compute type flags for logging (used even when whitelisted)
            bool isMissionObjective = worldEntity.MissionPrototype != PrototypeId.Invalid;
            bool isCivilian = worldEntity is Agent agent && agent.IsHostileToPlayers() == false;

            // Whitelist bypass: if the entity matches the whitelist, skip all type filtering
            bool isWhitelisted = whitelist.Length > 0 && whitelist.Any(w => entityName.Contains(w, StringComparison.OrdinalIgnoreCase));
            if (isWhitelisted == false)
            {
                // Not whitelisted - apply normal type filters
                if (worldEntity is Item)
                {
                    if (logging)
                    {
                        Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - is an Item (not whitelisted)");
                        ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - is an Item (not whitelisted)");
                    }
                    return AutoInteractResult.Filtered;
                }

                // Exclude stashes - they are non-hostile agents but not "civilians" in the gameplay sense
                if (worldEntity.Properties[PropertyEnum.OpenPlayerStash])
                {
                    if (logging)
                    {
                        Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - is a stash (OpenPlayerStash)");
                        ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - is a stash (OpenPlayerStash)");
                    }
                    return AutoInteractResult.Filtered;
                }

                // Focus: mission objectives, non-hostile agents (civilians), or interactable world objects (consoles)
                bool isInteractableWorldObject = worldEntity is WorldEntity
                    && worldEntity is not Agent
                    && worldEntity is not Item
                    && worldEntity is not Hotspot;
                if (isMissionObjective == false && isCivilian == false && isInteractableWorldObject == false)
                {
                    if (logging)
                    {
                        Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - not mission objective, civilian, or interactable world object (MissionProto={worldEntity.MissionPrototype})");
                        ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - not mission/civilian/worldObject  MissionProto={worldEntity.MissionPrototype}  EntityType={worldEntity.GetType().Name}");
                    }
                    return AutoInteractResult.Filtered;
                }
            }
            else if (logging)
            {
                Logger.Trace($"[ModInteractNearbyAuto] WHITELISTED [{entityName}#{entityId}] - bypassing type filter");
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] WHITELISTED [{entityName}#{entityId}] - bypassing type filter");
            }

            #endregion

            #region Chest Visibility 

            // Detect chest-like entities by prototype name so we can enforce visibility on them even if they are mission objectives
            bool looksLikeChest = entityName.Contains("Chest", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("Bounty", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("Crate", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("LootBox", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("Giftbox", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("GiftBox", StringComparison.OrdinalIgnoreCase)
                               || entityName.Contains("Commendation", StringComparison.OrdinalIgnoreCase);

            // Visibility diagnostics (always log for chests to help debug hidden vs visible)
            bool hasVisibleProperty = worldEntity.Properties.HasProperty(PropertyEnum.Visible);
            bool visiblePropertyValue = worldEntity.Properties[PropertyEnum.Visible];
            bool defaultRuntimeVisibility = worldEntity.DefaultRuntimeVisibility;
            bool dormancy = worldEntity.Properties[PropertyEnum.Dormant];
            bool enabled = worldEntity.Properties[PropertyEnum.Enabled];
            var entityState = worldEntity.Properties[PropertyEnum.EntityState];
            var interactable = worldEntity.Properties[PropertyEnum.Interactable];
            int interactableUsesLeft = worldEntity.Properties[PropertyEnum.InteractableUsesLeft];

            if (looksLikeChest)
            {
                // Unconditional INFO-level logging for chests so diagnostics are always captured
                string diag = $"[ModInteractNearbyAuto] CHEST_DIAGNOSTIC [{entityName}#{entityId}] hasVisibleProp={hasVisibleProperty} visibleProp={visiblePropertyValue} defaultRuntimeVis={defaultRuntimeVisibility} dormancy={dormancy} enabled={enabled} entityState={entityState} interactable={interactable} interactableUsesLeft={interactableUsesLeft} isMission={isMissionObjective} isCivilian={isCivilian}";
                Logger.Info(diag);
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] {diag}");
            }
            else if (logging)
            {
                Logger.Trace($"[ModInteractNearbyAuto] VISIBILITY [{entityName}#{entityId}] hasVisibleProp={hasVisibleProperty} visibleProp={visiblePropertyValue} defaultRuntimeVis={defaultRuntimeVisibility} dormancy={dormancy} enabled={enabled} isMission={isMissionObjective} isCivilian={isCivilian}");
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] VISIBILITY [{entityName}#{entityId}] hasVisibleProp={hasVisibleProperty} visibleProp={visiblePropertyValue} defaultRuntimeVis={defaultRuntimeVisibility} dormancy={dormancy} enabled={enabled} isMission={isMissionObjective} isCivilian={isCivilian}");
            }

            // Gate 1: Chest-like entities - skip ONLY if explicitly marked invisible (HasProperty && value == false).
            // We avoid DefaultRuntimeVisibility because most prototypes have VisibleByDefault=false, which would
            // incorrectly block visible chests that had SetVisible(true) called (true == global default causes
            // PropertyCollection to remove the property, making HasProperty false).
            if (looksLikeChest && hasVisibleProperty && visiblePropertyValue == false)
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - CHEST explicitly invisible");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - CHEST explicitly invisible");
                }
                return AutoInteractResult.Invisible;
            }

            // Gate 2: Other non-mission / non-civilian world objects
            if (isMissionObjective == false && isCivilian == false && visiblePropertyValue == false)
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - invisible (not yet visible)");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - invisible (not yet visible)");
                }
                return AutoInteractResult.Invisible;
            }

            #endregion

            #region Activation Checks

            if (avatar.InInteractRange(worldEntity, InteractionMethod.Use) == false)
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - out of interact range");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - out of range");
                }
                return AutoInteractResult.OutOfRange;
            }

            InteractData interactData = new();
            var interactionStatus = InteractionManager.CallGetInteractionStatus(
                new EntityDesc(worldEntity), avatar,
                InteractionOptimizationFlags.None, InteractionFlags.Default, ref interactData);

            if (interactionStatus == InteractionMethod.None)
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - CallGetInteractionStatus returned None");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - interactionStatus=None");
                }
                return AutoInteractResult.NoInteract;
            }

            // Only auto-trigger Use or Converse interactions (never PickUp)
            if (interactionStatus.HasFlag(InteractionMethod.Use) == false
                && interactionStatus.HasFlag(InteractionMethod.Converse) == false)
            {
                if (logging)
                {
                    Logger.Trace($"[ModInteractNearbyAuto] SKIP [{entityName}#{entityId}] - interactionStatus={interactionStatus} (needs Use or Converse)");
                    ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] SKIP [{entityName}#{entityId}] - interactionStatus={interactionStatus} (needs Use|Converse)");
                }
                return AutoInteractResult.WrongMethod;
            }

            if (logging)
            {
                Logger.Info($"[ModInteractNearbyAuto] ACTIVATE [{entityName}#{entityId}] - interactionStatus={interactionStatus} isMission={isMissionObjective} isCivilian={isCivilian}");
                ModInteractNearbyAutoLogCollator.WriteLine(Id, $"[ModInteractNearbyAuto_AUTO] ACTIVATE [{entityName}#{entityId}] - interactionStatus={interactionStatus} isMission={isMissionObjective} isCivilian={isCivilian} missionRef={worldEntity.MissionPrototype}");
            }
            avatar.UseInteractableObject(worldEntity.Id, PrototypeId.Invalid);
            return AutoInteractResult.Activated;

            #endregion
        }

        #endregion

        #region Scheduling

        private void ScheduleModInteractNearbyAutoEvent()
        {
            if (_modInteractNearbyAutoEvent.IsValid) return;
            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null) return;
            var customOptions = Game.CustomGameOptions;
            if (customOptions == null || customOptions.ModInteractNearbyAutoEnable == false) return;

            int intervalMs = Math.Max(50, customOptions.ModInteractNearbyAutoIntervalMs);
            scheduler.ScheduleEvent(_modInteractNearbyAutoEvent, TimeSpan.FromMilliseconds(intervalMs), _pendingEvents);
            _modInteractNearbyAutoEvent.Get().Initialize(this);
        }

        #endregion



        private class ModInteractNearbyAutoEvent : CallMethodEvent<Player>
        {
            protected override CallbackDelegate GetCallback() => (player) => player.DoModInteractNearbyAutoTick();
        }
    }
}
#endregion