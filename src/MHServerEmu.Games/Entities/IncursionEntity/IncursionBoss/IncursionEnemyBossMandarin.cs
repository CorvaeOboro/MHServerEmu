using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Mandarin - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMandarin : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MandarinBase.prototype");

        public IncursionEnemyBossMandarin(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Mandarin Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinPoisonCloud.prototype",                true,  1.1709f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinIceRing.prototype",                    true,  1.9184f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinFlameRing.prototype",                  true,  1.3732f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinConcussiveBlast.prototype",            true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                           false, 1.0f),
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinElectricStorm.prototype",              true,  1.2057f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Mandarin/MandarinIceRingStun.prototype",                false, 1.9184f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",          false, 1.0f),
        };
    }
}
