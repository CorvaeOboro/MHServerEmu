// =============================================================================
//  MOD DeathRecap - commands
// =============================================================================
//  Feature:    Re-display the last death recap in chat after respawning.
//  Commands:   !recap
//  Config :    DeathRecapEnable in Config.ini
// =============================================================================

using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("recap")]
    [CommandGroupDescription("Re-display your last death recap in chat.")]
    [CommandGroupFlags(CommandGroupFlags.SingleCommand)]
    public class ModDeathRecapCommands : CommandGroup
    {
        #region recap

        [DefaultCommand]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Recap(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            if (playerConnection == null) return "PlayerConnection not found.";

            var player = playerConnection.Player;
            if (player == null) return "Player not found.";

            if (player.ResendLastDeathRecap())
                return string.Empty;

            return "No death recap available. Die first, then use !recap.";
        }

        #endregion
    }
}
