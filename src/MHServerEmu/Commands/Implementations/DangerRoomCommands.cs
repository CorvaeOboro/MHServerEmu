using MHServerEmu.Commands.Attributes;
using MHServerEmu.Core.Network;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Network;

namespace MHServerEmu.Commands.Implementations
{
    [CommandGroup("dangerroom")]
    [CommandGroupDescription("Danger Room scenario management.")]
    public class DangerRoomCommands : CommandGroup
    {
        private static Dictionary<string, PrototypeId> _rarityNameMap;
        private static readonly object _rarityMapLock = new();

        private static void EnsureRarityMapBuilt()
        {
            lock (_rarityMapLock)
            {
                if (_rarityNameMap != null) return;

                _rarityNameMap = new(StringComparer.OrdinalIgnoreCase);
                foreach (PrototypeId rarityRef in DataDirectory.Instance.IteratePrototypesInHierarchy<RarityPrototype>(PrototypeIterateFlags.NoAbstractApprovedOnly))
                {
                    string fullName = rarityRef.GetName();
                    string fileName = System.IO.Path.GetFileName(fullName);

                    if (fileName.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                        fileName = fileName.Substring(0, fileName.Length - ".prototype".Length);

                    _rarityNameMap[fileName] = rarityRef;

                    string suffix = System.Text.RegularExpressions.Regex.Replace(fileName, @"^R\d+", "");
                    if (string.IsNullOrEmpty(suffix) == false)
                        _rarityNameMap[suffix] = rarityRef;
                }
            }
        }

        private static PrototypeId ResolveRarityByName(string name)
        {
            EnsureRarityMapBuilt();
            if (string.IsNullOrWhiteSpace(name))
                return PrototypeId.Invalid;

            name = name.Trim();
            if (name.EndsWith(".prototype", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".prototype".Length);

            if (_rarityNameMap.TryGetValue(name, out PrototypeId rarityRef))
                return rarityRef;
            return PrototypeId.Invalid;
        }

        private static string GetValidRarityNames()
        {
            EnsureRarityMapBuilt();
            var names = _rarityNameMap.Keys.OrderBy(n => n);
            return string.Join(", ", names);
        }

        private static int GetDefaultMaxTier()
        {
            PrototypeId epicRef = ResolveRarityByName("Epic");
            if (epicRef == PrototypeId.Invalid) return int.MaxValue;
            RarityPrototype epicProto = epicRef.As<RarityPrototype>();
            return epicProto?.Tier ?? int.MaxValue;
        }

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
                PrototypeId rarityRef = ResolveRarityByName(@params[0]);
                if (rarityRef == PrototypeId.Invalid)
                    return $"Unknown rarity '{@params[0]}'. Valid names: {GetValidRarityNames()}.";

                RarityPrototype rarityProto = rarityRef.As<RarityPrototype>();
                if (rarityProto == null)
                    return $"Failed to resolve rarity prototype for '{@params[0]}'.";

                maxTier = rarityProto.Tier;
            }

            return player.CombineModDangerRoomScenarios(maxTier);
        }
    }
}
