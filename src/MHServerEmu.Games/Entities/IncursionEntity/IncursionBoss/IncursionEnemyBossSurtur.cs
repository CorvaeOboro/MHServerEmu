using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Surtur - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// 
    ///  SURTUR is disabled for now as an Incursion Invader ,
    ///  the raid boss really speciifc to their raid not viable for random encounters 
    /// </summary>
    public class IncursionEnemyBossSurtur : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/SurturRaid/SurturBoss.prototype");

        public IncursionEnemyBossSurtur(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Surtur Invader";

        // HardcodeExclude: raid boss - too specific to the Surtur raid, not viable for random encounters.
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/DOTAuraPreTarget.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/FiveMan/MarkedForDeathStart.prototype",       true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/DemonShowerStart.prototype",                   true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/MistressOfMagma/SurturMoMStunCircleCreation.prototype", false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/MarkedForDeathPlayerWarning.prototype",       false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/OnHitProc.prototype",                          false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/DisableAreaDenialPowers.prototype",            false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackCenterMissile.prototype",           false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackRightMissile.prototype",            false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/MarkedForDeath.prototype",                     true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/DetonateIsland.prototype",                     false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackLeftMissile.prototype",             false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/MeteorStrikePlayerWarning.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackCenter.prototype",                  true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/DOTAura.prototype",                             false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/MeteorStrikeStart.prototype",                  true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/StunCircleStart.prototype",                    true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackLeft.prototype",                    true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                    false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/FinalEnrage.prototype",                         false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/SwordAttackRight.prototype",                   true,  1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/FiveMan/MarkedForDeathSummonSafeHotspotVisible.prototype", false, 1.0f),
            new("Powers/EnemyPowers/Boss/SurturRaid/Surtur/MeteorStrikeImpact.prototype",                 false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",                   false, 1.0f),
        };
    }
}
