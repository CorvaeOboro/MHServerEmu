using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// ManApe - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossManApe : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/ManApe.prototype");

        public IncursionEnemyBossManApe(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Man Ape Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/ManApe/BeatChest.prototype",                     true,  1.0f),
            new("Powers/EnemyPowers/Boss/ManApe/LeapStart.prototype",                     true,  1.0149f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/ManApe/SpearJab.prototype",                      true,  1.0f),
            new("Powers/EnemyPowers/Boss/ManApe/SpearThrow.prototype",                    true,  0.7506f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/ManApe/LeapEnd.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Boss/ManApe/LeapMovement.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
            new("Powers/Player/CCImmune2SecondCombo.prototype",                           false, 1.0f),
        };
    }
}
