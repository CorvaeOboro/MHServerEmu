using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Avengers War Skrull - Hightown patrol boss, uses MarvelAgent_AvengersWarSkrull art.
    /// Parent chain goes through mob WarSkrull, not Boss.defaults, so it has mob-level
    /// health/radius values. The IncursionEnemyBoss controller overrides AI and drives
    /// behavior through the think loop. ModifierSetEnable is Yes.
    /// </summary>
    public class IncursionEnemyBossAvengersWarskrull : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolHightown/HightownEventAvengersWarskrull.prototype");

        public IncursionEnemyBossAvengersWarskrull(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Avengers War Skrull Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/Skrulls/AvengersWarSkrull/HammerMelee.prototype",                 true,  1.1017f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/Skrulls/AvengersWarSkrull/LightningShield.prototype",             true,  1.0428f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/Skrulls/AvengersWarSkrull/LightningLeapAttack.prototype",         false, 0.7412f), // 2026-07-28
            new("Powers/EnemyPowers/MobPowers/Skrulls/AvengersWarSkrull/LightningLeapAttackEnd.prototype",      false, 0.7412f), // 2026-07-28
            new("Powers/EnemyPowers/MobPowers/Skrulls/AvengersWarSkrull/LightningLeapLightningEnd.prototype",   false, 1.0f),
        };
    }
}
