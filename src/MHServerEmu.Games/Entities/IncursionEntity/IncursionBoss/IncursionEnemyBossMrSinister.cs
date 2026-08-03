using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// MrSinister - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMrSinister : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MrSinisterBase.prototype");

        public IncursionEnemyBossMrSinister(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Mr Sinister Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/MrSinister/CloneCylinderSummonFX.prototype",            true,  1.0f),
            new("Powers/EnemyPowers/Boss/MrSinister/TrapSummon.prototype",                       true,  1.6364f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/MrSinister/ConcussiveDartsMain.prototype",               true,  1.0f),
            new("Powers/EnemyPowers/Boss/MrSinister/ConcussiveBlast.prototype",                   true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                            false, 1.0f),
            new("Powers/EnemyPowers/Boss/MrSinister/AstralProjection.prototype",                  true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",           false, 1.0f),
        };
    }
}
