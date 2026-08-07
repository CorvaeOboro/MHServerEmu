// =============================================================================
//  MOD Incursion - commands
// =============================================================================
//  Feature:    Hostile invader spawn system. Periodically spawns enemies near
//              players, supports static enemy override, 1v1 trial gauntlets,
//              and a per-player/per-character hunt tracker.
//  Commands:   !incursion now | spawn | start | stop | status | debug |
//              enemy | trial | hunt
//  Config :    IncursionCommandsRequireAdmin in Config.ini
//  Example :   "!incursion now"              = spawn a random invader near you
//  Example :   "!incursion enemy Venom"      = set the static invader to Venom
//  Example :   "!incursion trial boss"       = start a boss-only trial gauntlet
//  Example :   "!incursion hunt reset all"   = reset all hunt data
// =============================================================================

using System.Linq;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Config;
using MHServerEmu.Core.Network;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Calligraphy;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Populations;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("incursion")]
    [CommandGroupDescription("Controls the incursion system.")]
    public class ModIncursionCommands : CommandGroup
    {
        #region now

        [Command("now")]
        [CommandDescription("Spawns a hostile invader near your avatar. In-game only.")]
        [CommandUsage("incursion now")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Now(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game?.IncursionManager == null) return "Incursion manager not available.";

            Avatar avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false) return "Avatar not found or not alive in world.";

            var (entity, reason) = game.IncursionManager.ForceIncursionForAvatar(avatar, playerConnection.Player);
            if (entity == null) return $"Incursion failed: {reason}";

            return $"Invader spawned: {entity.PrototypeName} (id {entity.Id}).";
        }

        #endregion

        #region spawn

        [Command("spawn")]
        [CommandDescription("Spawns a specific incursion invader by name pattern near your avatar. In-game only.")]
        [CommandUsage("incursion spawn <pattern>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Spawn(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game?.IncursionManager == null) return "Incursion manager not available.";

            Avatar avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false) return "Avatar not found or not alive in world.";

            var (entity, reason) = game.IncursionManager.ForceSpawnByPattern(avatar, @params[0]);
            if (entity == null) return $"Spawn failed: {reason}";

            return $"Invader spawned: {entity.PrototypeName} (id {entity.Id}).";
        }

        #endregion

        #region start

        [Command("start")]
        [CommandDescription("Enables incursion spawning process-wide.")]
        [CommandUsage("incursion start")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Start(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            bool changed = IncursionManager.EnableSpawning();
            return changed ? "Incursion spawning enabled." : "Incursion spawning was already enabled.";
        }

        #endregion

        #region stop

        [Command("stop")]
        [CommandDescription("Disables incursion spawning process-wide.")]
        [CommandUsage("incursion stop")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Stop(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            bool changed = IncursionManager.DisableSpawning();
            return changed ? "Incursion spawning disabled." : "Incursion spawning was already disabled.";
        }

        #endregion

        #region status

        [Command("status")]
        [CommandDescription("Shows the current incursion system state and configuration.")]
        [CommandUsage("incursion status")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Status(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            return IncursionManager.GetStatusString();
        }

        #endregion

        #region debug

        [Command("debug")]
        [CommandDescription("Toggles verbose incursion enemy diagnostics.")]
        [CommandUsage("incursion debug [on|off]")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Debug(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            bool enabled;
            if (@params != null && @params.Length > 0)
            {
                string arg = @params[0].ToLowerInvariant();
                if (arg is "on" or "true" or "1")
                    enabled = true;
                else if (arg is "off" or "false" or "0")
                    enabled = false;
                else
                    return "Usage: incursion debug [on|off]";
            }
            else
            {
                enabled = IncursionEnemyController.VerboseLogging == false;
            }

            IncursionEnemyController.VerboseLogging = enabled;
            return $"Incursion enemy verbose logging {(enabled ? "enabled" : "disabled")}.";
        }

        #endregion

        #region enemy

        [Command("enemy")]
        [CommandDescription("Sets the invader prototype by name pattern (searches agent prototypes). Works in-game and from the server console.")]
        [CommandUsage("incursion enemy [pattern]")]
        [CommandInvokerType(CommandInvokerType.Any)]
        [CommandParamCount(1)]
        public string Enemy(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            var (enemyRef, message) = ResolveEnemy(@params[0]);
            if (enemyRef == PrototypeId.Invalid) return message;

            return IncursionManager.SetEnemyStatic(enemyRef);
        }

        #endregion

        #region trial

        [Command("trial")]
        [CommandDescription("Starts or stops an incursion trial: a 1v1 gauntlet against incursion enemy types. Optionally filter by mode.")]
        [CommandUsage("incursion trial [stop|all|avatar|teamup|boss]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Trial(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game?.IncursionManager == null) return "Incursion manager not available.";

            if (@params != null && @params.Length > 0 && @params[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                game.IncursionManager.EndTrial("Stopped by player.");
                return "Incursion trial stopped.";
            }

            string mode = "all";
            if (@params != null && @params.Length > 0)
                mode = @params[0];

            Player player = playerConnection.Player;
            return game.IncursionManager.StartTrial(player, mode);
        }

        #endregion

        #region hunt

        [Command("hunt")]
        [CommandDescription("Shows incursion hunt completion status, or resets hunt data. Hunt tracks unique enemy encounters per player and per character.")]
        [CommandUsage("incursion hunt [reset [all]]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Hunt(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player == null) return "Player not found.";

            // Subcommand: reset
            if (@params != null && @params.Length > 0 && @params[0].Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                bool resetAll = @params.Length > 1 && @params[1].Equals("all", StringComparison.OrdinalIgnoreCase);
                return IncursionManager.ResetHuntData(player, resetAll);
            }

            // Default: show status
            return IncursionManager.GetHuntStatusString(player);
        }

        #endregion

        #region helpers

        /// <summary>
        /// Resolves an agent prototype from a name pattern.
        /// </summary>
        private static (PrototypeId, string) ResolveEnemy(string pattern)
        {
            const int MaxMatches = 10;

            var matches = GameDatabase.SearchPrototypes(pattern,
                DataFileSearchFlags.SortMatchesByName | DataFileSearchFlags.CaseInsensitive,
                HardcodedBlueprints.Agent).ToList();

            if (matches.Count == 0)
                return (PrototypeId.Invalid, $"No agent prototypes match '{pattern}'.");

            if (matches.Count > 1)
            {
                var names = matches.Take(MaxMatches).Select(GameDatabase.GetPrototypeName);
                string header = matches.Count <= MaxMatches
                    ? $"Found {matches.Count} matches for '{pattern}':"
                    : $"Found {matches.Count} matches for '{pattern}', first {MaxMatches}:";
                return (PrototypeId.Invalid, header + "\r\n" + string.Join("\r\n", names));
            }

            return (matches[0], null);
        }

        /// <summary>
        /// Returns true if the invoker may use incursion commands. Server console invocations
        /// (client == null) are always allowed. In-game invocations require admin only when
        /// the IncursionCommandsRequireAdmin config option is enabled.
        /// </summary>
        private static bool HasAccess(NetClient client, out string error)
        {
            error = null;

            if (client == null)
                return true;

            var options = ConfigManager.Instance.GetConfig<CustomGameOptionsConfig>();
            if (options.IncursionCommandsRequireAdmin == false)
                return true;

            DBAccount account = CommandHelper.GetClientAccount(client);
            if (account != null && account.UserLevel >= AccountUserLevel.Admin)
                return true;

            error = "You do not have enough privileges to use incursion commands (IncursionCommandsRequireAdmin is enabled).";
            return false;
        }

        #endregion
    }
}
