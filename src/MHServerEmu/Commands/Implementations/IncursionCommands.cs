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
    public class IncursionCommands : CommandGroup
    {
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

            var (entity, reason) = game.IncursionManager.ForceIncursionForAvatar(avatar);
            if (entity == null) return $"Incursion failed: {reason}";

            return $"Invader spawned: {entity.PrototypeName} (id {entity.Id}).";
        }

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

        [Command("status")]
        [CommandDescription("Shows the current incursion system state and configuration.")]
        [CommandUsage("incursion status")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Status(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            return IncursionManager.GetStatusString();
        }

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

        [Command("trial")]
        [CommandDescription("Starts or stops an incursion trial: a 1v1 gauntlet against every incursion enemy type.")]
        [CommandUsage("incursion trial [stop]")]
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

            Player player = playerConnection.Player;
            return game.IncursionManager.StartTrial(player);
        }

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
    }
}
