using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Kraven - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossKraven : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/KravenBase.prototype");

        public IncursionEnemyBossKraven(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Kraven Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Kraven/BolaThrow.prototype",                     true,  1.2898f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Kraven/ConeYank.prototype",                      true,  1.3767f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Kraven/DropTrap.prototype",                      true,  3.6185f), // 2026-07-27
            new("Powers/EnemyPowers/Boss/Kraven/KravenDefaultAttack.prototype",           true,  0.7872f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Kraven/KravenDefaultAttack2.prototype",          true,  0.7872f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Kraven/AcrobaticAttack.prototype",               false, 1.1721f), // 2026-07-27
            new("Powers/EnemyPowers/Boss/Kraven/AcrobaticAttackCombo.prototype",          false, 1.1721f), // 2026-07-27
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
