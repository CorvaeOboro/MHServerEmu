namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Enemies drop form random pool of patrol and related boss drops  
    /// Boss loot table with an enabled toggle. One entry is rolled at random from the enabled set
    /// and assigned as the invader's death-loot, replacing the host body's native loot.
    /// </summary>
    public readonly struct IncursionLootPool
    {
        /// <summary>Human-readable label.</summary>
        public string Name { get; }

        /// <summary>Loot table prototype path.</summary>
        public string LootTablePath { get; }

        /// <summary>Whether this pool is included in the random roll.</summary>
        public bool Enabled { get; }

        public IncursionLootPool(string name, string lootTablePath, bool enabled)
        {
            Name = name;
            LootTablePath = lootTablePath;
            Enabled = enabled;
        }

        /// <summary>Returns a copy of this pool with a different <see cref="Enabled"/> flag.</summary>
        public IncursionLootPool With(bool enabled) => new(Name, LootTablePath, enabled);
    }
}
