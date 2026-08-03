using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// MindlessTitan - uses MindlessOne_Boss art (MarvelAgent_MindlessOne_Boss).
    /// Has SturdyMobNoKnock passive and MindlessOneStolenPower.
    /// Standard boss controller; powers harvested from native power collection after spawn.
    /// </summary>
    public class IncursionEnemyBossMindlessTitan : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MindlessTitanBase.prototype");

        public IncursionEnemyBossMindlessTitan(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Mindless Titan Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepLeftStart.prototype",    true,  1.1665f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepRightStart.prototype",   true,  0.8376f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/ChargedLaser.prototype",          true,  1.0136f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/GroundStomp.prototype",           true,  1.0f),
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepLeft.prototype",         false, 1.1665f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepRight.prototype",        false, 0.8376f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/ChargedLaserHS.prototype",        false, 1.0136f), // 2026-08-01
            new("Powers/EnemyPowers/Passive/SturdyMobNoKnock.prototype",                   false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                     false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",    false, 1.0f),
        };
    }
}
