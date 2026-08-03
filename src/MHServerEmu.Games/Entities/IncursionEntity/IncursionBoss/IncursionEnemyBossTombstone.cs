using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Tombstone - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossTombstone : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/TombstoneBase.prototype");

        public IncursionEnemyBossTombstone(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Tombstone Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Tombstone/SpinningLariat.prototype",                true,  0.9871f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Tombstone/TombstonePunchFIRST.prototype",           true,  0.9812f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Tombstone/DisengagingBulletSpray.prototype",        false, 1.6295f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Tombstone/DisengagingBulletsprayCombo.prototype",   false, 1.6295f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Tombstone/TombstoneCrushingLeap.prototype",         false, 1.2725f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Tombstone/TombstoneCrushingLeapEnd.prototype",      false, 1.2725f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Tombstone/TombstonePunchSECOND.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Passive/SturdyMobNoKnock.prototype",                     false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",      false, 1.0f),
        };
    }
}
