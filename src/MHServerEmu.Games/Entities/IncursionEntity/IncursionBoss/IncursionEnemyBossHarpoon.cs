using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Harpoon - uses MarvelAgent_Harpoon_Boss art. Has HitReactCondition.
    /// Standard boss controller; powers harvested from native power collection.
    /// </summary>
    public class IncursionEnemyBossHarpoon : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Harpoon.prototype");

        public IncursionEnemyBossHarpoon(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Harpoon Invader";

                // HardcodeExclude: broken invis
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
