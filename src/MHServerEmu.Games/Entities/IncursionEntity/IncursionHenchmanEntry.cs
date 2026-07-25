using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// One entry in a boss's henchmen pool: an NPC prototype to spawn alongside the boss,
    /// with a count range and rank. The controller rolls a random count between Min and Max
    /// for each entry and spawns that many NPCs near the boss on setup.
    /// </summary>
    public readonly struct IncursionHenchmanEntry
    {
        /// <summary>Agent prototype path for the henchman NPC.</summary>
        public readonly PrototypeId Entity;

        /// <summary>Minimum number to spawn (inclusive).</summary>
        public readonly int Min;

        /// <summary>Maximum number to spawn (inclusive).</summary>
        public readonly int Max;

        /// <summary>Rank override for the spawned NPC. PrototypeId.Invalid = use NPC's native rank.</summary>
        public readonly PrototypeId RankOverride;

        public IncursionHenchmanEntry(string entityPath, int min, int max, string rankPath = null)
        {
            Entity = GameDatabase.GetPrototypeRefByName(entityPath);
            Min = min;
            Max = max;
            RankOverride = rankPath != null ? GameDatabase.GetPrototypeRefByName(rankPath) : PrototypeId.Invalid;
        }

        public IncursionHenchmanEntry(PrototypeId entity, int min, int max, PrototypeId rankOverride = default)
        {
            Entity = entity;
            Min = min;
            Max = max;
            RankOverride = rankOverride;
        }
    }
}
