using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("dangerroom")]
    [CommandGroupDescription("Danger Room scenario management.")]
    public class DangerRoomCommands : CommandGroup
    {
        [Command("combine")]
        [CommandDescription("Combines lower-rarity Danger Room scenarios into higher-rarity ones.")]
        [CommandUsage("dangerroom combine [maxRarity]")]
        [CommandInvokerType(CommandInvokerType.Client)]
        public string Combine(string[] @params, NetClient client)
        {
            PlayerConnection playerConnection = (PlayerConnection)client;
            Player player = playerConnection.Player;

            int maxTier = GetDefaultMaxTier();

            if (@params != null && @params.Length > 0)
            {
                PrototypeId rarityRef = LootFilterHelper.ResolveRarityByName(@params[0]);
                if (rarityRef == PrototypeId.Invalid)
                    return $"Unknown rarity '{@params[0]}'. Valid names: {GetValidRarityNames()}.";

                RarityPrototype rarityProto = rarityRef.As<RarityPrototype>();
                if (rarityProto == null)
                    return $"Failed to resolve rarity prototype for '{@params[0]}'.";

                maxTier = rarityProto.Tier;
            }

            return player.CombineDangerRoomScenarios(maxTier);
        }

        private static int GetDefaultMaxTier()
        {
            PrototypeId epicRef = LootFilterHelper.ResolveRarityByName("Epic");
            if (epicRef == PrototypeId.Invalid) return int.MaxValue;
            RarityPrototype epicProto = epicRef.As<RarityPrototype>();
            return epicProto?.Tier ?? int.MaxValue;
        }

        private static string GetValidRarityNames()
        {
            var names = LootFilterHelper.GetRarityMap().Keys.OrderBy(n => n);
            return string.Join(", ", names);
        }
    }
}
