using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Dr. Doom Phase 2 - second phase of the Doom fight, uses Phase2 art
    /// (MarvelAgent_DrDoom_Boss_Phase2). Faster movement (Speed 350) with a
    /// 6-field behavior profile. Has DrDoomStolenPower.
    /// </summary>
    public class IncursionEnemyBossDrDoomPhase2 : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/DrDoomPhase2Base.prototype");

        public IncursionEnemyBossDrDoomPhase2(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Dr. Doom Phase 2 Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
