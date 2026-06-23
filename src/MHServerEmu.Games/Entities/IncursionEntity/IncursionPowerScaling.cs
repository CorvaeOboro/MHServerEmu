using System;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// this is a last resort , the IncursionAvatars hold per-power damage scales , this is used as a fallback
    /// Keyword-targeted outgoing damage scaling for incursion invaders.
    /// Powers not in the explicit power table fall back to keyword-based tiers
    /// instead of the flat enemy-wide scale.
    /// </summary>
    public static class IncursionPowerScaling
    {
        // Most aggressive first; first match wins. '^' prefix means leaf starts with the token;
        // all other tokens match as a substring of the leaf.
        private static readonly (float Scale, string[] Tokens)[] Tiers =
        {
            (0.006f, new[] { "^ult", "ultimate", "nornstones" }),                                   // ultimate / channeled finisher
            (0.008f, new[] { "hotspot", "groundeffect", "lingering", "persistent" }),               // persistent ground / hotspot tick
            (0.010f, new[] { "missile", "nova", "explosion", "bazooka", "grenade", "meteor",
                             "supernova", "apocalypse", "armageddon", "annihil" }),                 // missile / nova / explosion burst
            (0.020f, new[] { "^sig", "signature", "bladedance", "beatdown", "barrage",
                             "onslaught", "frenzy", "fury", "maelstrom", "rampage" }),              // signature / multi-hit burst
            (0.025f, new[] { "eruption", "combo", "dotapplied", "_dot", "proc", "channel" }),       // combo / eruption / DoT proc secondary
            (0.030f, new[] { "aoe", "area", "storm", "blizzard", "quake", "shockwave",
                             "tornado", "wave" }),                                                  // area / crowd-clearing
        };

        /// <summary>
        /// Resolves the keyword-tier damage scale for the given power.
        /// </summary>
        public static bool TryGetKeywordScale(PrototypeId powerRef, out float scale)
        {
            scale = 0f;
            if (powerRef == PrototypeId.Invalid)
                return false;

            string leaf = LeafName(GameDatabase.GetPrototypeName(powerRef));
            if (string.IsNullOrEmpty(leaf))
                return false;

            foreach ((float tierScale, string[] tokens) in Tiers)
            {
                if (LeafMatches(leaf, tokens))
                {
                    scale = tierScale;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Last path segment of a prototype name, minus the ".prototype" suffix, lower-case.</summary>
        private static string LeafName(string protoName)
        {
            if (string.IsNullOrEmpty(protoName))
                return string.Empty;

            int slash = protoName.LastIndexOf('/');
            string leaf = slash >= 0 ? protoName[(slash + 1)..] : protoName;

            const string suffix = ".prototype";
            if (leaf.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                leaf = leaf[..^suffix.Length];

            return leaf.ToLowerInvariant();
        }

        private static bool LeafMatches(string leaf, string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (token.Length > 0 && token[0] == '^')
                {
                    if (leaf.StartsWith(token[1..], StringComparison.Ordinal))
                        return true;
                }
                else if (leaf.Contains(token, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
