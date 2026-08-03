using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Kurse - Thor: The Dark World boss, uses MarvelAgent_Kurse art.
    /// Has KurseStolenPower. Standard boss controller; powers harvested from
    /// native power collection after spawn.
    /// </summary>
    public class IncursionEnemyBossKurse : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/KurseBase.prototype");

        public IncursionEnemyBossKurse(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Kurse Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Kurse/KurseSwordSweep.prototype",      true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kurse/KurseDarkVoidToss.prototype",    true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kurse/KurseSwordLeapEnd.prototype",    true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kurse/KurseSummon.prototype",          false, 1.0f),
        };
    }
}
