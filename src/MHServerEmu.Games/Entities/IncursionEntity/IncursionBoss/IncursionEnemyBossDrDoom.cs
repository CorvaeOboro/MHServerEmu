using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// DrDoom - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossDrDoom : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Story/DrDoomPhase1.prototype");

        public IncursionEnemyBossDrDoom(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Dr Doom Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1BallLightning.prototype",           true,  0.8822f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonPhase1OrbSpawn.prototype",    true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonDBotInfernos.prototype",      true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonDoombotsAnim.prototype",      true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonTurretsAnim.prototype",       true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonDBotFlyers.prototype",        true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                 false, 1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1DeathStun.prototype",               false, 1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1SummonDBotBlockades.prototype",     true,  2.0771f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",                false, 1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase1/DrDoomPhase1HomingMissiles.prototype",          true,  1.0f),
        };
    }
}
