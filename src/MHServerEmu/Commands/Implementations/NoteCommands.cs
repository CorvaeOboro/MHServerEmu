using System.Text;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.DatabaseAccess.Models;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Logging;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("note")]
    [CommandGroupDescription("Area design note placement tool. Marks world positions for enemy placement, event design, etc.")]
    public class NoteCommands : CommandGroup
    {
        [Command("place")]
        [CommandDescription("Places a note at your current position with the given category. Spawns an invisible nameplate marker.")]
        [CommandUsage("note place <category> [comment...]\nCategories: boss, mob, miniboss, or any custom word.\nExample: !note place boss good spawn for vampire lord")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Place(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game == null) return "Game not found.";

            Avatar avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsAliveInWorld == false)
                return "Avatar not found or not alive in world.";

            string category = @params[0];
            string comment = null;
            if (@params.Length > 1)
                comment = string.Join(' ', @params.Skip(1));

            string label = AreaNoteCollator.PlaceNote(avatar, category, comment);
            if (label == null)
                return "Failed to place note. Are you in a valid region?";

            string pos = avatar.RegionLocation.Position.ToStringNames();
            string commentPart = string.IsNullOrEmpty(comment) ? "" : $". Comment: \"{comment}\"";
            return $"Placed {label} at {pos}{commentPart}";
        }

        [Command("list")]
        [CommandDescription("Lists all notes in the current region.")]
        [CommandUsage("note list")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game == null) return "Game not found.";

            Avatar avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
                return "Avatar not found.";

            Region region = avatar.Region;
            if (region == null) return "No region found.";

            var notes = AreaNoteCollator.GetNotesForRegion(region.Id);
            if (notes.Count == 0)
                return $"No notes in '{region.PrototypeName}'.";

            var lines = new List<string>();
            lines.Add($"Notes in '{region.PrototypeName}' ({notes.Count}):");
            foreach (var note in notes)
            {
                string commentPart = string.IsNullOrEmpty(note.Comment) ? "" : $" - {note.Comment}";
                lines.Add($"  {note.Label} [{note.Category}] ({note.X:F0}, {note.Y:F0}, {note.Z:F0}){commentPart}");
            }

            CommandHelper.SendMessageSplit(client, string.Join("\r\n", lines), false);
            return string.Empty;
        }

        [Command("clear")]
        [CommandDescription("Clears all notes in the current region and despawns their markers.")]
        [CommandUsage("note clear")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Clear(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game == null) return "Game not found.";

            Avatar avatar = playerConnection.Player?.CurrentAvatar;
            if (avatar == null || avatar.IsInWorld == false)
                return "Avatar not found.";

            Region region = avatar.Region;
            if (region == null) return "No region found.";

            int count = AreaNoteCollator.ClearNotesForRegion(game, region.Id);
            return count > 0
                ? $"Cleared {count} note(s) from '{region.PrototypeName}'."
                : $"No notes to clear in '{region.PrototypeName}'.";
        }

        [Command("remove")]
        [CommandDescription("Removes a specific note by label (e.g. NOTE_BOSS_01) and despawns its marker.")]
        [CommandUsage("note remove <label>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Remove(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = (PlayerConnection)client;
            Game game = playerConnection.Game;
            if (game == null) return "Game not found.";

            string label = @params[0];
            bool removed = AreaNoteCollator.RemoveNote(game, label);
            return removed
                ? $"Removed note '{label}'."
                : $"Note '{label}' not found.";
        }

        [Command("save")]
        [CommandDescription("Flushes all notes to a JSON file in Logs/AreaNotes/.")]
        [CommandUsage("note save")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Save(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            string result = AreaNoteCollator.FlushToJson();
            return result.StartsWith("Error") ? result : $"Saved notes to: {result}";
        }

        [Command("status")]
        [CommandDescription("Shows a summary of all placed notes across all regions.")]
        [CommandUsage("note status")]
        [CommandInvokerType(CommandInvokerType.Any)]
        public string Status(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            return AreaNoteCollator.GetSummary();
        }

        [Command("clearall")]
        [CommandDescription("Clears ALL notes across all regions and despawns their markers.")]
        [CommandUsage("note clearall")]
        [CommandInvokerType(CommandInvokerType.Any)]
        [CommandUserLevel(AccountUserLevel.Admin)]
        public string ClearAll(string[] @params, NetClient client)
        {
            if (HasAccess(client, out string accessError) == false) return accessError;

            PlayerConnection playerConnection = client as PlayerConnection;
            Game game = playerConnection?.Game;

            int count = AreaNoteCollator.ClearAll(game);
            return $"Cleared all {count} note(s) across all regions.";
        }

        private static bool HasAccess(NetClient client, out string error)
        {
            error = null;

            if (client == null)
                return true;

            DBAccount account = CommandHelper.GetClientAccount(client);
            if (account != null && account.UserLevel >= AccountUserLevel.Admin)
                return true;

            error = "You need admin privileges to use note commands.";
            return false;
        }
    }
}
