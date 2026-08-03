using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// LadyDeathstrike - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossLadyDeathstrike : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/LadyDeathstrikeCH8.prototype");

        public IncursionEnemyBossLadyDeathstrike(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Lady Deathstrike Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/BladedFlurry.prototype",          true,  1.2647f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash.prototype",             true,  1.0375f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/RapidRegenChanneled.prototype",   true,  1.0f),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash2.prototype",            false, 1.0375f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash3.prototype",            false, 1.0375f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/Slashthrough.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                     false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",    false, 1.0f),
        };
    }
}
