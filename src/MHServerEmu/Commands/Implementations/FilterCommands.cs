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
    [CommandGroupDescription("Manage personal loot filters for Ring, Medal, Insignia, Team-Up Gear, Catalysts, and Uru-Forged.")]
    public class FilterCommands : CommandGroup
    {
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
            return LootFilterHelper.GetAvatarShortName(avatar.PrototypeDataRef);
        }

        /// <summary>
        /// Resolves the target section to read/write. <paramref name="target"/> may be
        /// null/"global" (global defaults), "me" (the active character), or a character name.
        /// </summary>
        private static LootFilterSection ResolveSection(Player player, string target, bool create, out string scopeLabel)
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
            LootFilterSection charSection = player.LootFilter.GetCharacterSection(avatarName);

            var sb = new StringBuilder();
            sb.Append($"Loot filter (effective for {avatarName ?? "current character"}):\n");
            foreach (var kvp in LootFilterHelper.FilterNameMap)
            {
                // Only show canonical keys, skip aliases
                if (kvp.Key != kvp.Value) continue;

                string key = kvp.Key;
                if (LootFilterHelper.BooleanFilters.Contains(key))
                {
                    bool global = player.LootFilter.Global.Booleans.TryGetValue(key, out bool gv) && gv;
                    bool effective = LootFilterHelper.GetEffectiveBoolean(player.LootFilter, key, avatarName);
                    sb.Append($"  {key}: {(effective ? "ON" : "OFF")}  (global: {(global ? "ON" : "OFF")})\n");
                }
                else
                {
                    string global = LootFilterHelper.GetFormattedThreshold(player.LootFilter.Global.Thresholds, key);
                    string charText = charSection != null
                        ? LootFilterHelper.GetFormattedThreshold(charSection.Thresholds, key)
                        : "(none)";
                    PrototypeId effectiveRef = LootFilterHelper.GetEffectiveThreshold(player.LootFilter, key, avatarName);
                    string effective = effectiveRef != PrototypeId.Invalid
                        ? GameDatabase.GetFormattedPrototypeName(effectiveRef)
                        : "(none)";
                    sb.Append($"  {key}: {effective}  (global: {global}, char: {charText})\n");
                }
            }
            return sb.ToString().TrimEnd();
        }

        [Command("set")]
        [CommandDescription("Sets a rarity threshold or boolean toggle for an item type. Optional target: global (default), me, or a character name.")]
        [CommandUsage("filter set <type> <rarity|on/off> [global|me|<character>]")]
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

            if (LootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: ring, medal, insignia, teamup, catalyst, uruforged.";

            LootFilterSection section = ResolveSection(player, target, create: true, out string scopeLabel);
            if (section == null)
                return $"Could not resolve target '{target}'. Use global, me, or a character name.";

            // Boolean filters (e.g. uruforged)
            if (LootFilterHelper.BooleanFilters.Contains(filterKey))
            {
                bool? boolValue = ParseBoolean(valueToken);
                if (boolValue == null)
                    return $"Invalid value '{valueToken}' for boolean filter '{filterKey}'. Use on/off, true/false, yes/no, all/none.";

                section.Booleans[filterKey] = boolValue.Value;
                PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter set [{scopeLabel}]: {filterKey} -> {(boolValue.Value ? "ON" : "OFF")}.";
            }

            // Rarity threshold filters
            PrototypeId rarityRef = LootFilterHelper.ResolveRarityByName(valueToken);
            if (rarityRef == PrototypeId.Invalid)
                return $"Unknown rarity '{valueToken}'. Use '!filter rarities' to see valid names.";

            section.Thresholds[filterKey] = rarityRef;
            PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);

            string rarityName = GameDatabase.GetFormattedPrototypeName(rarityRef);
            return $"Filter set [{scopeLabel}]: {filterKey} -> {rarityName}. Items at or below this rarity will not drop.";
        }

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

            if (LootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: ring, medal, insignia, teamup, catalyst, uruforged.";

            LootFilterSection section = ResolveSection(player, target, create: false, out string scopeLabel);
            if (section == null)
                return $"No '{scopeLabel ?? target}' settings exist.";

            bool removed = LootFilterHelper.BooleanFilters.Contains(filterKey)
                ? section.Booleans.Remove(filterKey)
                : section.Thresholds.Remove(filterKey);

            if (removed)
            {
                PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter cleared [{scopeLabel}] for {filterKey}.";
            }

            return $"No filter was set [{scopeLabel}] for {filterKey}.";
        }

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
            PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
            return $"Cleared {thresholdCount} threshold(s) and {boolCount} boolean toggle(s) across global + all characters.";
        }

        [Command("rarities")]
        [CommandDescription("Lists all valid rarity names you can use with '!filter set'.")]
        [CommandUsage("filter rarities")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Rarities(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.Append("Valid rarity names (case-insensitive):\n");
            foreach (var kvp in LootFilterHelper.GetRarityMap())
            {
                string displayName = GameDatabase.GetFormattedPrototypeName(kvp.Value);
                sb.Append($"  {kvp.Key}  ({displayName})\n");
            }
            return sb.ToString().TrimEnd();
        }
    }
}
