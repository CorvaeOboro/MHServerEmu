using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Missions;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Commands.Implementations
{
    public partial class MissionCommands
    {
        [Command("list")]
        [CommandDescription("List all missions tracked by the player's MissionManager with state, type, and tracker flags.")]
        [CommandUsage("mission list")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var manager = playerConnection.Player?.MissionManager;
            if (manager == null) return "Mission manager is null.";

            var missions = manager.GetAllMissions().Where(m => m != null).OrderBy(m => m.State).ThenBy(m => m.PrototypeName).ToList();
            if (missions.Count == 0) return "No missions in MissionManager.";

            CommandHelper.SendMessage(client, $"Player missions ({missions.Count}):", true);
            var lines = missions.Select(m =>
            {
                string type = GetMissionTypeString(m);
                string tracker = m.Prototype is MissionPrototype mp ? $"tracker={mp.ShowInMissionTracker}" : "";
                string suspended = m.IsSuspended ? " [SUSPENDED]" : "";
                return $"{m.State,-10} {type,-12} {m.PrototypeName}{suspended} {tracker}";
            });
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        [Command("dailies")]
        [CommandDescription("List all daily missions with state, type, completion count, and reset frequency.")]
        [CommandUsage("mission dailies")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Dailies(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var manager = playerConnection.Player?.MissionManager;
            if (manager == null) return "Mission manager is null.";

            var player = playerConnection.Player;
            var dailies = manager.GetAllMissions()
                .Where(m => m != null && m.IsDailyMission)
                .OrderBy(m => m.State)
                .ThenBy(m => m.PrototypeName)
                .ToList();

            if (dailies.Count == 0) return "No daily missions found.";

            CommandHelper.SendMessage(client, $"Daily missions ({dailies.Count}):", true);
            var lines = dailies.Select(m =>
            {
                var dailyProto = m.DailyMissionPrototype;
                string dailyType = dailyProto?.Type.ToString() ?? "?";
                string resetFreq = dailyProto?.ResetFrequency.ToString() ?? "?";
                string day = dailyProto?.Day.ToString() ?? "?";
                PropertyId propId = new(PropertyEnum.SharedQuestCompletionCount, m.PrototypeDataRef);
                int completionCount = player.Properties[propId];
                string sharedQuest = m.IsSharedQuest ? " [SharedQuest]" : "";
                string suspended = m.IsSuspended ? " [SUSPENDED]" : "";
                return $"{m.State,-10} type={dailyType,-9} reset={resetFreq,-7} day={day,-9} completionCount={completionCount}{sharedQuest}{suspended} {m.PrototypeName}";
            });
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        [Command("sharedquests")]
        [CommandDescription("List all shared quest missions with state, completion count, and suspended status.")]
        [CommandUsage("mission sharedquests")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string SharedQuests(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var manager = playerConnection.Player?.MissionManager;
            if (manager == null) return "Mission manager is null.";

            var player = playerConnection.Player;
            var sharedQuests = manager.GetAllMissions()
                .Where(m => m != null && m.IsSharedQuest)
                .OrderBy(m => m.State)
                .ThenBy(m => m.PrototypeName)
                .ToList();

            if (sharedQuests.Count == 0) return "No shared quest missions found.";

            CommandHelper.SendMessage(client, $"Shared Quest missions ({sharedQuests.Count}):", true);
            var lines = sharedQuests.Select(m =>
            {
                PropertyId propId = new(PropertyEnum.SharedQuestCompletionCount, m.PrototypeDataRef);
                int completionCount = player.Properties[propId];
                string suspended = m.IsSuspended ? " [SUSPENDED]" : "";
                var dailyProto = m.DailyMissionPrototype;
                string dailyType = dailyProto?.Type.ToString() ?? "?";
                return $"{m.State,-10} type={dailyType,-9} completionCount={completionCount}{suspended} {m.PrototypeName}";
            });
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        [Command("tracker")]
        [CommandDescription("Show the player's mission tracker filter state (which categories are shown/hidden).")]
        [CommandUsage("mission tracker")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Tracker(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var player = playerConnection.Player;
            if (player == null) return "Player not found.";

            var filters = new List<PrototypeId>();
            foreach (var f in GameDatabase.DataDirectory.IteratePrototypesInHierarchy<MissionTrackerFilterPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                filters.Add(f);
            filters.Sort((a, b) => GameDatabase.GetPrototype<MissionTrackerFilterPrototype>(a).DisplayOrder.CompareTo(GameDatabase.GetPrototype<MissionTrackerFilterPrototype>(b).DisplayOrder));

            if (filters.Count == 0) return "No MissionTrackerFilter prototypes found.";

            CommandHelper.SendMessage(client, $"Mission Tracker Filters ({filters.Count}):", true);
            var lines = filters.Select(filterRef =>
            {
                var filterProto = GameDatabase.GetPrototype<MissionTrackerFilterPrototype>(filterRef);
                bool isEnabled = player.Properties[PropertyEnum.MissionTrackerFilter, filterRef];
                string status = isEnabled ? "ON " : "OFF";
                return $"{status} filterType={filterProto.FilterType,-20} default={filterProto.DisplayByDefault} order={filterProto.DisplayOrder} {GameDatabase.GetFormattedPrototypeName(filterRef)}";
            });
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        [Command("hide")]
        [CommandDescription("Suspend a mission to hide it from the Mission Tracker UI. Works on completed shared quests.")]
        [CommandUsage("mission hide [pattern]")]
        [CommandUserLevel(AccountUserLevel.Admin)]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Hide(string[] @params, NetClient client)
        {
            string errorMessage = GetPlayerMissionFromPattern(client, @params[0], out List<Mission> missionsFound);
            if (errorMessage != null) return errorMessage;

            if (missionsFound.Count == 1)
            {
                var mission = missionsFound[0];
                if (mission.IsSuspended) return $"{mission.PrototypeName} is already suspended.";
                mission.SetSuspendedState(true);
                return $"{mission.PrototypeName} suspended (hidden from tracker). State={mission.State}";
            }

            CommandHelper.SendMessage(client, $"Multiple matches found :", true);
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", missionsFound.Select(k => k.PrototypeName)), false);
            return string.Empty;
        }

        [Command("unhide")]
        [CommandDescription("Unsuspend a mission to show it again in the Mission Tracker UI.")]
        [CommandUsage("mission unhide [pattern]")]
        [CommandUserLevel(AccountUserLevel.Admin)]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Unhide(string[] @params, NetClient client)
        {
            string errorMessage = GetPlayerMissionFromPattern(client, @params[0], out List<Mission> missionsFound);
            if (errorMessage != null) return errorMessage;

            if (missionsFound.Count == 1)
            {
                var mission = missionsFound[0];
                if (mission.IsSuspended == false) return $"{mission.PrototypeName} is not suspended.";
                mission.SetSuspendedState(false);
                return $"{mission.PrototypeName} unsuspended (visible in tracker). State={mission.State}";
            }

            CommandHelper.SendMessage(client, $"Multiple matches found :", true);
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", missionsFound.Select(k => k.PrototypeName)), false);
            return string.Empty;
        }

        [Command("trackerfilter")]
        [CommandDescription("Toggle a mission tracker filter on or off. Usage: trackerfilter [FilterType] [on|off]")]
        [CommandUsage("mission trackerfilter [FilterType] [on|off]")]
        [CommandUserLevel(AccountUserLevel.Admin)]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string TrackerFilter(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var player = playerConnection.Player;
            if (player == null) return "Player not found.";

            string filterTypeName = @params[0];
            bool enable = @params[1].Equals("on", StringComparison.OrdinalIgnoreCase) || @params[1].Equals("true", StringComparison.OrdinalIgnoreCase);

            if (Enum.TryParse<UIMissionTrackerFilterTypeEnum>(filterTypeName, true, out var targetFilterType) == false)
                return $"Unknown filter type: {filterTypeName}. Valid types: {string.Join(", ", Enum.GetNames(typeof(UIMissionTrackerFilterTypeEnum)))}";

            PrototypeId filterRef = PrototypeId.Invalid;
            foreach (var f in GameDatabase.DataDirectory.IteratePrototypesInHierarchy<MissionTrackerFilterPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
            {
                if (GameDatabase.GetPrototype<MissionTrackerFilterPrototype>(f).FilterType == targetFilterType)
                {
                    filterRef = f;
                    break;
                }
            }

            if (filterRef == PrototypeId.Invalid)
                return $"No MissionTrackerFilterPrototype found for FilterType={targetFilterType}.";

            player.Properties[PropertyEnum.MissionTrackerFilter, filterRef] = enable;
            return $"Mission tracker filter {targetFilterType} ({GameDatabase.GetFormattedPrototypeName(filterRef)}) set to {(enable ? "ON" : "OFF")}.";
        }

        [Command("dumpprops")]
        [CommandDescription("Dump mission-related properties for the player (SharedQuestCompletionCount, MissionRewardReceived, etc.)")]
        [CommandUsage("mission dumpprops [pattern]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string DumpProps(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found";

            var player = playerConnection.Player;
            if (player == null) return "Player not found.";

            var manager = player.MissionManager;
            if (manager == null) return "Mission manager is null.";

            string pattern = @params.Length > 0 ? @params[0] : null;
            var missions = manager.GetAllMissions().Where(m => m != null).ToList();
            if (pattern != null)
                missions = missions.Where(m => m.PrototypeName.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();

            if (missions.Count == 0) return "No missions found matching pattern.";

            CommandHelper.SendMessage(client, $"Mission properties ({missions.Count}):", true);
            var lines = missions.Select(m =>
            {
                PropertyId sqPropId = new(PropertyEnum.SharedQuestCompletionCount, m.PrototypeDataRef);
                int sqCount = player.Properties[sqPropId];
                bool rewardReceived = player.Properties[PropertyEnum.MissionRewardReceived, m.PrototypeDataRef];
                bool avatarRewardReceived = false;
                var avatar = player.CurrentAvatar;
                if (avatar != null)
                    avatarRewardReceived = avatar.Properties[PropertyEnum.MissionRewardReceived, m.PrototypeDataRef];
                return $"state={m.State,-10} sqCount={sqCount} rewardReceived={rewardReceived} avatarRewardReceived={avatarRewardReceived} suspended={m.IsSuspended} {m.PrototypeName}";
            });
            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        private static string GetMissionTypeString(Mission m)
        {
            if (m.IsSharedQuest) return "SharedQuest";
            if (m.IsDailyMission) return "Daily";
            if (m.IsLegendaryMission) return "Legendary";
            if (m.IsAdvancedMission) return "Advanced";
            if (m.IsOpenMission) return "OpenMission";
            if (m.IsLoreMission) return "Lore";
            if (m.IsAccountMission) return "Account";
            return "Mission";
        }

        private string GetPlayerMissionFromPattern(NetClient client, string pattern, out List<Mission> missionsFound)
        {
            missionsFound = new();

            PlayerConnection playerConnection = (PlayerConnection)client;
            var manager = playerConnection?.Player?.MissionManager;
            if (manager == null) return "Mission manager is null.";

            missionsFound.AddRange(manager.FindMissionsByPattern(pattern));
            if (missionsFound.Count == 0) return $"No mission found matching '{pattern}'";
            return null;
        }
    }
}
