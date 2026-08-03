using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Blob - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossBlob : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/BlobBase.prototype");

        public IncursionEnemyBossBlob(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Blob Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Blob/BlobDefaultAttack3.prototype",                    true,  1.2063f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Blob/BlobDefaultAttack.prototype",                     true,  1.2063f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Blob/BlobBellyFlopEnd.prototype",                      false, 0.9758f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Blob/BlobSummonToad.prototype",                        true,  1.0f),
            new("Powers/EnemyPowers/Boss/Blob/BlobDirectedShockwave.prototype",                  true,  1.1192f), // 2026-08-01
            new("Powers/EnemyPowers/Passive/SturdyMobNoKnock.prototype",                         false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                           false, 1.0f),
            new("Powers/EnemyPowers/Boss/Blob/BlobDefaultAttack2.prototype",                     true,  1.2063f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Blob/BlobBellyFlop.prototype",                          true,  1.0734f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",          false, 1.0f),
        };
    }
}
