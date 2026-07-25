// =============================================================================
//  MOD LootFilter - commands
// =============================================================================
//  Feature:    Server-side per-player loot filtering of additonal item types set via commands
//  Items  :    Ring, Medal, Insignia (slot-based); Team-Up Gear, Catalysts
//              (type-based); Uru-Forged (boolean toggle).
//  Commands:   !filter list | set | clear | clearall | rarities
//  Target :    Each command accepts an optional target: global (default), "me"
//              (current character), or a specific named character .
//              rarities by short name (e.g. "epic").
//  Example :   "!filter set ring uncommon me"  =  ( only epic+ rings on current )
//  Example :   "!filter set teamup epic global" = ( only cosmic teamupgear across all characters )
// =============================================================================

using System.Text;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("filter")]
    [CommandGroupDescription("Manage personal loot filters for Ring, Medal, Insignia, Team-Up Gear, Catalysts, Uru-Forged, and gear slots slot1-slot5.")]
    public class ModLootFilterCommands : CommandGroup
    {
        #region short names

        private static bool? ParseBoolean(string token)
        {
            return token.ToLowerInvariant() switch
            {
                "on" or "true" or "yes" or "all" => true,
                "off" or "false" or "no" or "none" => false,
                _ => null,
            };
        }

        private static string GetCurrentAvatarName(Player player)
        {
            Avatar avatar = player.CurrentAvatar;
            if (avatar == null) return null;
            return ModLootFilterHelper.GetAvatarShortName(avatar.PrototypeDataRef);
        }

        /// <summary>
        /// Resolves the target section to read/write. <paramref name="target"/> may be
        /// null/"global" (global defaults), "me" (the active character), or a character name.
        /// </summary>
        private static ModLootFilterSection ResolveSection(Player player, string target, bool create, out string scopeLabel)
        {
            if (string.IsNullOrEmpty(target) || target.Equals("global", StringComparison.OrdinalIgnoreCase))
            {
                scopeLabel = "global";
                return player.LootFilter.Global;
            }

            string charName = target.Equals("me", StringComparison.OrdinalIgnoreCase)
                ? GetCurrentAvatarName(player)
                : target;

            if (string.IsNullOrEmpty(charName))
            {
                scopeLabel = null;
                return null;
            }

            scopeLabel = charName;
            return player.LootFilter.GetCharacterSection(charName, create);
        }

        #endregion

        #region  list

        [Command("list")]
        [CommandDescription("Shows current loot filter thresholds and boolean toggles.")]
        [CommandUsage("filter list")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string List(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            string avatarName = GetCurrentAvatarName(player);
            ModLootFilterSection charSection = player.LootFilter.GetCharacterSection(avatarName);

            var sb = new StringBuilder();
            sb.Append($"Loot filter (effective for {avatarName ?? "current character"}):\n");
            foreach (var kvp in ModLootFilterHelper.FilterNameMap)
            {
                // Only show canonical keys, skip aliases
                if (kvp.Key != kvp.Value) continue;

                string key = kvp.Key;
                if (ModLootFilterHelper.BooleanFilters.Contains(key))
                {
                    bool global = player.LootFilter.Global.Booleans.TryGetValue(key, out bool gv) && gv;
                    bool effective = ModLootFilterHelper.GetEffectiveBoolean(player.LootFilter, key, avatarName);
                    sb.Append($"  {key}: {(effective ? "ON" : "OFF")}  (global: {(global ? "ON" : "OFF")})\n");
                }
                else
                {
                    string global = ModLootFilterHelper.GetFormattedThreshold(player.LootFilter.Global.Thresholds, key);
                    string charText = charSection != null
                        ? ModLootFilterHelper.GetFormattedThreshold(charSection.Thresholds, key)
                        : "(none)";
                    PrototypeId effectiveRef = ModLootFilterHelper.GetEffectiveThreshold(player.LootFilter, key, avatarName);
                    string effective;
                    if (effectiveRef == PrototypeId.Invalid)
                        effective = "(none)";
                    else if (effectiveRef == GameDatabase.LootGlobalsPrototype?.RarityUnique)
                        effective = "unique (avatar-restricted only)";
                    else
                        effective = GameDatabase.GetFormattedPrototypeName(effectiveRef);
                    sb.Append($"  {key}: {effective}  (global: {global}, char: {charText})\n");
                }
            }

            if (player.LootFilter.Global.ExactItems.Count > 0)
            {
                sb.Append("\nExact item filters (global):\n");
                foreach (string item in player.LootFilter.Global.ExactItems)
                    sb.Append($"  {item}\n");
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region  set

        [Command("set")]
        [CommandDescription("Sets a rarity threshold or boolean toggle for an item type. Use 'unique' to filter avatar-restricted uniques. Optional target: global (default), me, or a character name.")]
        [CommandUsage("filter set <type> <rarity|on/off|unique> [global|me|<character>]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(2)]
        public string Set(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            string typeToken = @params[0].ToLower();
            string valueToken = @params[1];
            string target = @params.Length > 2 ? @params[2] : null;

            if (ModLootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: slot1-slot5, ring, medal, insignia, teamup, catalyst, uruforged.";

            ModLootFilterSection section = ResolveSection(player, target, create: true, out string scopeLabel);
            if (section == null)
                return $"Could not resolve target '{target}'. Use global, me, or a character name.";

            // Boolean filters (e.g. uruforged)
            if (ModLootFilterHelper.BooleanFilters.Contains(filterKey))
            {
                bool? boolValue = ParseBoolean(valueToken);
                if (boolValue == null)
                    return $"Invalid value '{valueToken}' for boolean filter '{filterKey}'. Use on/off, true/false, yes/no, all/none.";

                section.Booleans[filterKey] = boolValue.Value;
                ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter set [{scopeLabel}]: {filterKey} -> {(boolValue.Value ? "ON" : "OFF")}.";
            }

            // Rarity threshold filters
            PrototypeId rarityRef = ModLootFilterHelper.ResolveRarityByName(valueToken);
            if (rarityRef == PrototypeId.Invalid)
                return $"Unknown rarity '{valueToken}'. Use '!filter rarities' to see valid names.";

            section.Thresholds[filterKey] = rarityRef;
            ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);

            string rarityName = GameDatabase.GetFormattedPrototypeName(rarityRef);
            return $"Filter set [{scopeLabel}]: {filterKey} -> {rarityName}. Items at or below this rarity will not drop.";
        }

        #endregion

        #region  clear

        [Command("clear")]
        [CommandDescription("Removes the filter setting for an item type. Optional target: global (default), me, or a character name.")]
        [CommandUsage("filter clear <type> [global|me|<character>]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Clear(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            string typeToken = @params[0].ToLower();
            string target = @params.Length > 1 ? @params[1] : null;

            if (ModLootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: slot1-slot5, ring, medal, insignia, teamup, catalyst, uruforged.";

            ModLootFilterSection section = ResolveSection(player, target, create: false, out string scopeLabel);
            if (section == null)
                return $"No '{scopeLabel ?? target}' settings exist.";

            bool removed = ModLootFilterHelper.BooleanFilters.Contains(filterKey)
                ? section.Booleans.Remove(filterKey)
                : section.Thresholds.Remove(filterKey);

            if (removed)
            {
                ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter cleared [{scopeLabel}] for {filterKey}.";
            }

            return $"No filter was set [{scopeLabel}] for {filterKey}.";
        }

        #endregion

        #region  clearall

        [Command("clearall")]
        [CommandDescription("Removes all custom loot filter settings (global defaults and every character override).")]
        [CommandUsage("filter clearall")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string ClearAll(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            int thresholdCount = player.LootFilter.Global.Thresholds.Count;
            int boolCount = player.LootFilter.Global.Booleans.Count;
            foreach (var section in player.LootFilter.Characters.Values)
            {
                thresholdCount += section.Thresholds.Count;
                boolCount += section.Booleans.Count;
            }

            player.LootFilter.Global.Thresholds.Clear();
            player.LootFilter.Global.Booleans.Clear();
            player.LootFilter.Characters.Clear();
            ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
            return $"Cleared {thresholdCount} threshold(s) and {boolCount} boolean toggle(s) across global + all characters.";
        }

        #endregion

        #region  rarities

        [Command("rarities")]
        [CommandDescription("Lists all valid rarity names you can use with '!filter set'.")]
        [CommandUsage("filter rarities")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Rarities(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("Valid rarity names (case-insensitive):\n");
            foreach (var kvp in ModLootFilterHelper.GetRarityMap())
            {
                string displayName = GameDatabase.GetFormattedPrototypeName(kvp.Value);
                sb.Append($"  {kvp.Key}  ({displayName})\n");
            }
            return sb.ToString().TrimEnd();
        }

        #endregion

        #region  exact

        [Command("exact")]
        [CommandDescription("Manage exact-item prototype filters. Usage: !filter exact add <path> | remove <path> | list | clear")]
        [CommandUsage("filter exact add|remove|list|clear")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Exact(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            if (@params.Length == 0)
                return "Usage: !filter exact add <path> | remove <path> | list | clear";

            string sub = @params[0].ToLowerInvariant();
            switch (sub)
            {
                case "add":
                case "remove":
                    if (@params.Length < 2)
                        return $"Usage: !filter exact {sub} <full/prototype/path.prototype>";

                    string path = @params[1];
                    if (sub == "add")
                    {
                        if (player.LootFilter.Global.ExactItems.Add(path))
                        {
                            ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                            return $"Added exact-item filter: {path}";
                        }
                        return $"Exact-item filter already contains: {path}";
                    }
                    else
                    {
                        if (player.LootFilter.Global.ExactItems.Remove(path))
                        {
                            ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                            return $"Removed exact-item filter: {path}";
                        }
                        return $"Exact-item filter did not contain: {path}";
                    }

                case "list":
                    if (player.LootFilter.Global.ExactItems.Count == 0)
                        return "No exact-item filters set.";
                    var listSb = new StringBuilder();
                    listSb.Append("Exact item filters (global):\n");
                    foreach (string item in player.LootFilter.Global.ExactItems)
                        listSb.Append($"  {item}\n");
                    return listSb.ToString().TrimEnd();

                case "clear":
                    int count = player.LootFilter.Global.ExactItems.Count;
                    player.LootFilter.Global.ExactItems.Clear();
                    ModLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                    return $"Cleared {count} exact-item filter(s).";

                default:
                    return "Unknown subcommand. Usage: !filter exact add <path> | remove <path> | list | clear";
            }
        }

        #endregion
    }
}
