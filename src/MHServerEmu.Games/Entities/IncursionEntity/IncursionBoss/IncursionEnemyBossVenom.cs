using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Venom - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossVenom : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolBrooklyn/BrooklynEventVenom.prototype");

        public IncursionEnemyBossVenom(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Venom Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Venom/BigPunch.prototype",                       true,  1.3363f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Venom/NewYank.prototype",                        true,  1.0f),
            new("Powers/EnemyPowers/Boss/Venom/VenomMad.prototype",                       true,  1.0f),
            new("Powers/EnemyPowers/Boss/Venom/VenomOMTripleShot.prototype",              true,  1.3470f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Venom/MawFromAbove.prototype",                   false, 1.5212f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Venom/MawFromAboveEnd.prototype",                false, 1.5212f), // 2026-07-28
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
