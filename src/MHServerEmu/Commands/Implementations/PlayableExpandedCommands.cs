using System.Text;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Network;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.PlayableExpanded;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("playas")]
    [CommandGroupDescription("Play as an expanded character (PlayableExpanded - new playable characters built from Team-Up and other assets).")]
    public class PlayableExpandedCommands : CommandGroup
    {
        [DefaultCommand]
        [CommandDescription("Swaps your hero into the expanded character matching the name pattern. In-game only.")]
        [CommandUsage("playas <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Swap(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            if (@params == null || @params.Length == 0)
                return "Usage: playas <pattern> | off | me | list | status";

            Avatar avatar = GetAvatar(client, out string avatarError);
            if (avatar == null) return avatarError;

            var (character, message) = ExpandedCharacterRegistry.Resolve(@params[0]);
            if (character == null) return message;

            return avatar.EnterPlayableExpanded(character);
        }

        [Command("off")]
        [CommandDescription("Swaps back to your hero.")]
        [CommandUsage("playas off")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Off(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            Avatar avatar = GetAvatar(client, out string avatarError);
            if (avatar == null) return avatarError;

            return avatar.ExitPlayableExpanded();
        }

        [Command("me")]
        [CommandDescription("Restores your current hero's original powers. Use this if your powers were permanently messed up by previous playas sessions.")]
        [CommandUsage("playas me")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Me(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            Avatar avatar = GetAvatar(client, out string avatarError);
            if (avatar == null) return avatarError;

            // If still in playas mode, exit first so the restore works on the hero.
            if (avatar.IsPlayableExpandedActive)
                avatar.ExitPlayableExpanded();

            avatar.RestoreMyPowersExpanded();
            return "Your hero's powers have been restored. If you were in playas mode, it has been turned off.";
        }

        [Command("status")]
        [CommandDescription("Shows the current play-as state.")]
        [CommandUsage("playas status")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Status(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            Avatar avatar = GetAvatar(client, out string avatarError);
            if (avatar == null) return avatarError;

            if (avatar.IsPlayableExpandedActive == false)
                return "Not currently playing as an expanded character.";

            return $"Currently playing as {avatar.PlayableExpanded.Character?.DisplayName}.";
        }

        [Command("list")]
        [CommandDescription("Lists dedicated expanded characters and Team-Up prototypes matching an optional pattern.")]
        [CommandUsage("playas list [pattern]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            string pattern = @params != null && @params.Length > 0 ? @params[0] : null;

            StringBuilder sb = new();

            // Dedicated, hand-tuned characters first.
            sb.Append("Dedicated characters:\n");
            int dedicated = 0;
            foreach (ExpandedCharacter character in ExpandedCharacterRegistry.DedicatedCharacters)
            {
                if (pattern != null && character.DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                sb.Append($"  {character.DisplayName}\n");
                dedicated++;
            }

            if (dedicated == 0)
                sb.Append("  (none)\n");

            // Generic Team-Up fallbacks.
            const int MaxListed = 25;
            int generic = 0;
            StringBuilder genericSb = new();
            foreach (PrototypeId teamUpRef in GameDatabase.DataDirectory.IteratePrototypesInHierarchy<AgentTeamUpPrototype>(
                PrototypeIterateFlags.NoAbstractApprovedOnly))
            {
                string name = GameDatabase.GetPrototypeName(teamUpRef);
                if (pattern != null && name.Contains(pattern, StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                generic++;
                if (generic <= MaxListed)
                    genericSb.Append($"  {GetShortName(name)}\n");
            }

            sb.Append(generic <= MaxListed
                ? $"Team-Up assets (generic tuning), {generic}:\n"
                : $"Team-Up assets (generic tuning), {generic}, first {MaxListed}:\n");
            sb.Append(genericSb);

            return sb.ToString();
        }

        private static string GetShortName(string prototypeName)
        {
            int index = prototypeName.LastIndexOf('/');
            string shortName = index >= 0 ? prototypeName[(index + 1)..] : prototypeName;
            return shortName.Replace(".prototype", string.Empty);
        }

        private static Avatar GetAvatar(NetClient client, out string error)
        {
            error = null;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Avatar avatar = playerConnection?.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false)
            {
                error = "Avatar not found or not alive in world.";
                return null;
            }

            return avatar;
        }

        /// <summary>
        /// Returns true if the invoker may use playas commands. In-game invocations require
        /// admin only when the PlayableExpandedCommandsRequireAdmin config option is enabled.
        /// </summary>
        private static bool HasAccess(NetClient client, out string error)
        {
            error = null;

            if (client == null)
                return true;

            var options = ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>();
            if (options.PlayableExpandedCommandsRequireAdmin == false)
                return true;

            DBAccount account = CommandHelper.GetClientAccount(client);
            if (account != null && account.UserLevel >= AccountUserLevel.Admin)
                return true;

            error = "You do not have enough privileges to use playas commands (PlayableExpandedCommandsRequireAdmin is enabled).";
            return false;
        }
    }
}
