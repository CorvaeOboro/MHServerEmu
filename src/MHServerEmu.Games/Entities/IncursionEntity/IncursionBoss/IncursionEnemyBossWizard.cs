using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Wizard - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossWizard : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/WizardBase.prototype");

        public IncursionEnemyBossWizard(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Wizard Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Wizard/BallDashStart.prototype",                 true,  2.6727f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Wizard/DiscPBAoE.prototype",                     true,  0.7854f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Wizard/WizardTripleDisk.prototype",              true,  1.0f),
            new("Powers/EnemyPowers/Boss/Wizard/BallDash.prototype",                      false, 2.6727f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Wizard/BallDashSummonCombo.prototype",           false, 1.0f),
            new("Powers/EnemyPowers/Boss/Wizard/DiscPBAoEHit2.prototype",                 false, 0.7854f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Wizard/WizardFlyHere.prototype",                 false, 1.0f),
            new("Powers/EnemyPowers/Boss/Wizard/WizardFlyHereEnd.prototype",              false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
