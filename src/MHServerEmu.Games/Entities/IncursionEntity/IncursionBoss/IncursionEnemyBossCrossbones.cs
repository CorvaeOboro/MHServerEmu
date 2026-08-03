using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Crossbones - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossCrossbones : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/CivilWar/Crossbones.prototype");

        public IncursionEnemyBossCrossbones(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Crossbones Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/BombToss.prototype",                      true,  1.1019f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/ConeShockwave.prototype",                 true,  1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/DefaultAttack1.prototype",                true,  0.6898f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/DefaultAttack3.prototype",                true,  0.6898f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/Earthquake.prototype",                    true,  1.2733f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/Flashbang.prototype",                     true,  0.9723f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SMGBurst.prototype",                      true,  1.6091f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SummonReinforcements.prototype",          true,  1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SuperPunch.prototype",                    true,  1.2503f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/TargetedBomb.prototype",                  true,  1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/BackflipExplosive.prototype",             false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/BackflipExplosiveEnd.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/BombTossThrowBombs.prototype",            false, 1.1019f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/DefaultAttack1B.prototype",               false, 0.6898f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/DefaultAttack3B.prototype",               false, 0.6898f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/EarthQuakeSlowEffect.prototype",          false, 1.2733f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/EarthquakeKnockdownEffect.prototype",     false, 3.2259f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/Lunge.prototype",                         false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/ReEngage.prototype",                      false, 1.2114f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/ReEngageEnd.prototype",                   false, 1.2114f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SideDash.prototype",                      false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SideDashLeft.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SideDashPowerLock.prototype",             false, 1.0f),
            new("Powers/EnemyPowers/Boss/CivilWar/Crossbones/SummonStickyBombAtSelfCombo.prototype",   false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                 false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",                false, 1.0f),
        };
    }
}
