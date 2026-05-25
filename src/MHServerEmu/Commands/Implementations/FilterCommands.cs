using System.Text;
using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
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

            var thresholds = player.LootFilter.Thresholds;
            var booleans = player.LootFilter.Booleans;
            var sb = new StringBuilder();
            sb.AppendLine("Current loot filter settings:");
            foreach (var kvp in LootFilterHelper.FilterNameMap)
            {
                // Only show canonical keys, skip aliases
                if (kvp.Key != kvp.Value) continue;

                if (LootFilterHelper.BooleanFilters.Contains(kvp.Key))
                {
                    bool enabled = booleans.TryGetValue(kvp.Key, out bool val) && val;
                    sb.AppendLine($"  {kvp.Key}: {(enabled ? "ON" : "OFF")}");
                }
                else
                {
                    string rarityName = LootFilterHelper.GetFormattedThreshold(thresholds, kvp.Key);
                    sb.AppendLine($"  {kvp.Key}: {rarityName}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        [Command("set")]
        [CommandDescription("Sets a rarity threshold or boolean toggle for an item type.")]
        [CommandUsage("filter set <type> <rarity>   or   filter set <type> on/off")]
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

            if (LootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: ring, medal, insignia, teamup, catalyst, uruforged.";

            // Boolean filters (e.g. uruforged)
            if (LootFilterHelper.BooleanFilters.Contains(filterKey))
            {
                bool? boolValue = ParseBoolean(valueToken);
                if (boolValue == null)
                    return $"Invalid value '{valueToken}' for boolean filter '{filterKey}'. Use on/off, true/false, yes/no, all/none.";

                player.LootFilter.Booleans[filterKey] = boolValue.Value;
                PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter set: {filterKey} -> {(boolValue.Value ? "ON" : "OFF")}.";
            }

            // Rarity threshold filters
            PrototypeId rarityRef = LootFilterHelper.ResolveRarityByName(valueToken);
            if (rarityRef == PrototypeId.Invalid)
                return $"Unknown rarity '{valueToken}'. Use '!filter rarities' to see valid names.";

            player.LootFilter.Thresholds[filterKey] = rarityRef;
            PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);

            string rarityName = GameDatabase.GetFormattedPrototypeName(rarityRef);
            return $"Filter set: {filterKey} -> {rarityName}. Items at or below this rarity will not drop.";
        }

        [Command("clear")]
        [CommandDescription("Removes the filter setting for an item type.")]
        [CommandUsage("filter clear <type>")]
        [CommandInvokerType(CommandInvokerType.Client)]
        [CommandParamCount(1)]
        public string Clear(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            string typeToken = @params[0].ToLower();
            if (LootFilterHelper.FilterNameMap.TryGetValue(typeToken, out string filterKey) == false)
                return $"Unknown type '{typeToken}'. Valid: ring, medal, insignia, teamup, catalyst, uruforged.";

            if (LootFilterHelper.BooleanFilters.Contains(filterKey))
            {
                if (player.LootFilter.Booleans.Remove(filterKey))
                {
                    PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                    return $"Filter cleared for {filterKey}.";
                }
                return $"No filter was set for {filterKey}.";
            }

            if (player.LootFilter.Thresholds.Remove(filterKey))
            {
                PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
                return $"Filter cleared for {filterKey}.";
            }

            return $"No filter was set for {filterKey}.";
        }

        [Command("clearall")]
        [CommandDescription("Removes all custom loot filter settings.")]
        [CommandUsage("filter clearall")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string ClearAll(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            if (player?.LootFilter == null)
                return "Loot filters are not available right now.";

            int thresholdCount = player.LootFilter.Thresholds.Count;
            int boolCount = player.LootFilter.Booleans.Count;
            player.LootFilter.Thresholds.Clear();
            player.LootFilter.Booleans.Clear();
            PlayerLootFilterStorage.Save(player.DatabaseUniqueId, player.LootFilter);
            return $"Cleared {thresholdCount} threshold(s) and {boolCount} boolean toggle(s).";
        }

        [Command("rarities")]
        [CommandDescription("Lists all valid rarity names you can use with '!filter set'.")]
        [CommandUsage("filter rarities")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Rarities(string[] @params, NetClient client)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Valid rarity names (case-insensitive):");
            foreach (var kvp in LootFilterHelper.GetRarityMap())
            {
                string displayName = GameDatabase.GetFormattedPrototypeName(kvp.Value);
                sb.AppendLine($"  {kvp.Key}  ({displayName})");
            }
            return sb.ToString().TrimEnd();
        }
    }
}
