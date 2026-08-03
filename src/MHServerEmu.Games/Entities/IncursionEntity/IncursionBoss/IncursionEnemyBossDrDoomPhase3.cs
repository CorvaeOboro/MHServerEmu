using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Dr. Doom Phase 3 - final phase of the Doom fight, uses Phase3 art
    /// (MarvelAgent_DrDoom_Boss_Phase3). Has HitReactCondition and a longer
    /// RemoveFromWorldTimerMS (26000). Has DrDoomStolenPower.
    /// </summary>
    public class IncursionEnemyBossDrDoomPhase3 : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/DrDoomPhase3Base.prototype");

        public IncursionEnemyBossDrDoomPhase3(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Dr. Doom Phase 3 Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DoomPhase3CosmicBeam.prototype",               true,  0.9664f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3CosmicSummons.prototype",          true,  2.0740f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3CosmicSummonsAnim.prototype",      true,  2.0740f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3RapidFire.prototype",              true,  0.8953f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3SorceryBlasts.prototype",          true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3StarryExpanse.prototype",          true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3SummonPhase3OrbSpawn.prototype",   true,  1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3TeleportComboHeal.prototype",      false, 1.0f),
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3TeleportComboSmash.prototype",     false, 1.0568f), // 2026-07-27
            new("Powers/EnemyPowers/Boss/DrDoom/Phase3/DrDoomPhase3TeleportSmashHeal.prototype",      false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",               false, 1.0f),
        };
    }
}
