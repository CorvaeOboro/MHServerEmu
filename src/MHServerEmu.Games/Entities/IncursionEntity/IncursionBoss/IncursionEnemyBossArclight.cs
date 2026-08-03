using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Arclight - uses MarvelAgent_Arclight art. Has HitReactCondition.
    /// Standard boss controller; powers harvested from native power collection.
    /// </summary>
    public class IncursionEnemyBossArclight : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Arclight.prototype");

        public IncursionEnemyBossArclight(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Arclight Invader";

        // HardcodeExclude: broken - has HitReactCondition issues.
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
