using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// MadameHydra - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMadameHydra : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MadameHydraBase.prototype");

        public IncursionEnemyBossMadameHydra(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Madame Hydra Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        // Henchmen: 2-3 elite HYDRA Hulkbusters, 3-5 champion HYDRA jetpacks, 4-6 normal HYDRA gunners.
        protected override IncursionHenchmanEntry[] HenchmenPool => _henchmen;
        private static readonly IncursionHenchmanEntry[] _henchmen =
        {
            new("Entity/Characters/Mobs/Hydra/HydraHulkbusterBase", 2, 3, "Mods/Ranks/Elite.prototype"),
            new("Entity/Characters/Mobs/Hydra/HydraJetpackBase", 3, 5, "Mods/Ranks/Champion.prototype"),
            new("Entity/Characters/Mobs/Hydra/HydraPlasmaCasterBase", 4, 6),
        };
    }
}
