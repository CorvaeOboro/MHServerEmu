using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Kronan - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossKronan : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Limbo/LimboEvent05KronanArcanistBoss.prototype");

        public IncursionEnemyBossKronan(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Kronan Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/Kronan/KronanShockTroopTrapThrow.prototype",              true,  0.7626f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/LimboEvent/KronanBoss/RangedAttack.prototype",                 true,  0.8999f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/LimboEvent/KronanBoss/SummonHotspot.prototype",                true,  1.1296f), // 2026-08-01
            new("Powers/Player/SilverSurfer/BlackHoleSummonLocusCombo.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/LimboEvent/KronanBoss/RangedAttack2.prototype",                true,  0.8999f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",                 false, 1.0f),
        };
    }
}
