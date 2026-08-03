using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Thanos - Black Order boss, uses MindlessOne_Boss art (MarvelAgent_MindlessOne_Boss).
    /// Has SturdyMobNoKnock passive and NoStealablePowerBlank. Very few native offensive powers,
    /// so the controller may rely heavily on the auto-harvested power collection.
    /// 
    /// DISABLED , this is maybe fleshed out in later versions , its just mindless titan currently
    /// </summary>
    public class IncursionEnemyBossThanos : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/BlackOrder/Thanos.prototype");

        public IncursionEnemyBossThanos(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Thanos Invader";

        // HardcodeExclude: unfinished - currently just a Mindless Titan placeholder.
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepLeftStart.prototype",    true,  1.0044f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepRightStart.prototype",   true,  1.0541f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/ChargedLaser.prototype",          true,  1.1288f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/GroundStomp.prototype",           true,  1.0f),
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepLeft.prototype",         false, 1.0044f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/BeamSweepRight.prototype",        false, 1.0541f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/MindlessOneBoss/ChargedLaserHS.prototype",        false, 1.1288f), // 2026-07-28
            new("Powers/EnemyPowers/Passive/SturdyMobNoKnock.prototype",                   false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                     false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",    false, 1.0f),
        };
    }
}
