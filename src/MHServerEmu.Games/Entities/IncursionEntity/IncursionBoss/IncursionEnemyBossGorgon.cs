using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Gorgon - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossGorgon : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/GorgonBase.prototype");

        public IncursionEnemyBossGorgon(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Gorgon Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordSlash1.prototype",                true,  1.6776f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordSlash3.prototype",                true,  1.6776f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordSlash4.prototype",                true,  1.6776f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonStoneGazeSweepBeam.prototype",         true,  0.9768f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordSlash2.prototype",                true,  1.6776f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordSlash5.prototype",                true,  1.6776f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Gorgon/GorgonSwordLunge.prototype",                 true,  0.7579f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",      false, 1.0f),
        };
    }
}
