using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Hood - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossHood : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/HoodBase.prototype");

        public IncursionEnemyBossHood(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Hood Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/TheHood/ChargeShotStart.prototype",              true,  0.8056f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/TheHood/TheHoodDoubleTap.prototype",             true,  1.0f),
            new("Powers/EnemyPowers/Boss/TheHood/TimeBomb.prototype",                     true,  1.0f),
            new("Powers/EnemyPowers/Boss/TheHood/ChargeShot.prototype",                   false, 0.8056f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/TheHood/DeathBlossom.prototype",                 false, 0.9888f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/TheHood/DeathBlossomStart.prototype",            false, 0.9888f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/TheHood/TheHoodBlink.prototype",                 false, 1.0f),
            new("Powers/EnemyPowers/Boss/TheHood/TheHoodDoubleTapLeft.prototype",         false, 1.0f),
            new("Powers/EnemyPowers/Boss/TheHood/TheHoodDoubleTapRight.prototype",        false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
