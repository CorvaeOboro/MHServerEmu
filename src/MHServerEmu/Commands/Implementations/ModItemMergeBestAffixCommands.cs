// =============================================================================
//  MOD Item Merge Best Affix - commands
// =============================================================================
//  Feature:    Merges pairs of identical items (same prototype, rarity, item
//              level) into a single new copy whose affix values are the
//              maximum of each corresponding affix from both source items.
//              Costs 1 Cosmic Essence (configurable) per merge, or free if
//              ModItemMergeBestAffixFree is enabled. Only scans the player's
//              personal general inventory. Does NOT touch equipped items or stash.
//  Commands:   !merge any | <itemType> | <rarity> | dryrun [filter]
//  Access  :   In-game client only. Gated by ModItemMergeBestAffixCommandEnable.
//  Example :   "!merge any"        = merge one pair of any eligible items
//  Example :   "!merge unique"     = merge one pair of unique-rarity items
//  Example :   "!merge ring"       = merge one pair of ring items
//  Example :   "!merge dryrun"     = preview what would be merged (no cost)
//  Example :   "!merge dryrun cosmic" = preview cosmic-rarity merges only
// =============================================================================

using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("merge")]
    [CommandGroupDescription("Item merging - combines two identical items into one with the best affix rolls. Costs 1 Cosmic Essence per merge (or free).")]
    public class ModItemMergeBestAffixCommands : CommandGroup
    {
        [Command("any")]
        [CommandDescription("Merges one pair of any eligible items (same name, rarity, grade) taking the best affix rolls.")]
        [CommandUsage("merge any")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string MergeAny(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            // "any" was already consumed by the dispatcher, so @params is empty here.
            // Pass null to indicate "no filter, not a dry run".
            return player.ModItemMergeBestAffix(null);
        }

        [Command("dryrun")]
        [CommandDescription("Shows what would be merged without consuming anything. Optionally filter by item type or rarity.")]
        [CommandUsage("merge dryrun [filter]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string MergeDryRun(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;

            // "dryrun" was consumed by the dispatcher. @params contains the optional filter.
            // Prepend "dryrun" so the parser in ModItemMergeBestAffix knows this is a dry run.
            string[] allParams;
            if (@params != null && @params.Length > 0)
            {
                allParams = new string[@params.Length + 1];
                allParams[0] = "dryrun";
                System.Array.Copy(@params, 0, allParams, 1, @params.Length);
            }
            else
            {
                allParams = new string[] { "dryrun" };
            }

            return player.ModItemMergeBestAffix(allParams);
        }

        /// <summary>
        /// Fallback for dynamic filters: !merge unique, !merge ring, !merge cosmic, etc.
        /// The first param is the filter value (rarity name or item type keyword).
        /// </summary>
        [DefaultCommand]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string MergeFiltered(string[] @params, NetClient client)
        {
            if (@params == null || @params.Length == 0)
                return Fallback(@params, client);

            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;
            return player.ModItemMergeBestAffix(@params);
        }
    }
}
