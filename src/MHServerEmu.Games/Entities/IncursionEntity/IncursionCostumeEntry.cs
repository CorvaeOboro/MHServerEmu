using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// One row in an incursion enemy's costume table: a costume prototype and whether it is
    /// available for the random render-skin roll. Mirrors <see cref="IncursionPowerEntry"/> so
    /// costumes can be toggled on/off during tuning while keeping the full reference list.
    /// </summary>
    public readonly struct IncursionCostumeEntry
    {
        /// <summary>The costume prototype.</summary>
        public readonly PrototypeId Costume;

        /// <summary>Whether this costume is included in the random selection pool.</summary>
        public readonly bool Enabled;

        public IncursionCostumeEntry(string costumePath, bool enabled)
        {
            Costume = GameDatabase.GetPrototypeRefByName(costumePath);
            Enabled = enabled;
        }

        public IncursionCostumeEntry(PrototypeId costume, bool enabled)
        {
            Costume = costume;
            Enabled = enabled;
        }
    }
}
