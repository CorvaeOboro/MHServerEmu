using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Blade Invader
    /// Powers: 17 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBlade : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Blade.prototype");

        public IncursionEnemyBlade(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Blade Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Blade/Modern.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Blade/Original.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Blade/SF.prototype",       true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.05f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Blade/AllOutAssault.prototype",                            true,  0.0957f), // 2026-06-20
            new("Powers/Player/Blade/BloodlustHiddenPassive.prototype",                   false, 0.05f),
            new("Powers/Player/Blade/DeathFromAbove.prototype",                           true,  0.0438f), // 2026-06-20
            new("Powers/Player/Blade/HandCannon.prototype",                               true,  0.1294f), // 2026-06-20
            new("Powers/Player/Blade/Helichopter.prototype",                              true,  0.0427f), // 2026-06-20
            new("Powers/Player/Blade/HemoglycerinGauntlet.prototype",                     true,  0.0862f), // 2026-06-20
            new("Powers/Player/Blade/HemoglycerinGrenade.prototype",                      true,  0.0910f), // 2026-06-18
            new("Powers/Player/Blade/JustStayDown.prototype",                             true,  0.0129f), // 2026-06-20
            new("Powers/Player/Blade/KnifeBarrage.prototype",                             true,  0.0146f), // 2026-06-10
            new("Powers/Player/Blade/PBAoEGlaive.prototype",                              true,  0.0952f), // 2026-06-20
            new("Powers/Player/Blade/RapidFire.prototype",                                true,  0.0278f), // 2026-06-20
            new("Powers/Player/Blade/SerumInjection.prototype",                           true,  0.05f),
            new("Powers/Player/Blade/Shotgun.prototype",                                  true,  0.0739f), // 2026-06-18
            new("Powers/Player/Blade/StakeThroughTheHeart.prototype",                     true,  0.0212f), // 2026-06-16
            new("Powers/Player/Blade/StakeThrower.prototype",                             true,  0.0644f), // 2026-06-20
            new("Powers/Player/Blade/SwordDash.prototype",                                true,  0.1475f), // 2026-06-20
            new("Powers/Player/Blade/Talents/ArsenalTalent.prototype",                    false, 0.05f),
            new("Powers/Player/Blade/Talents/BasicCritChanceTalent.prototype",            false, 0.05f),
            new("Powers/Player/Blade/Talents/BerserkerTalent.prototype",                  false, 0.05f),
            new("Powers/Player/Blade/Talents/BleedSlowTalent.prototype",                  false, 0.05f),
            new("Powers/Player/Blade/Talents/DFAInnerHitTalent.prototype",                false, 0.05f),
            new("Powers/Player/Blade/Talents/GlaiveCooldownTalent.prototype",             false, 0.05f),
            new("Powers/Player/Blade/Talents/PulsingUVGrenadeTalent.prototype",           false, 0.0370f), // 2026-06-20
            new("Powers/Player/Blade/Talents/SigDamageCooldownReductionTalent.prototype", false, 0.02f),
            new("Powers/Player/Blade/Talents/SigSpiritRestoreTalent.prototype",           false, 0.02f),
            new("Powers/Player/Blade/Talents/SignatureMapTalent.prototype",               false, 0.02f),
            new("Powers/Player/Blade/Talents/SpecHighRisk.prototype",                     false, 0.05f),
            new("Powers/Player/Blade/Talents/SpecLowRisk.prototype",                      false, 0.05f),
            new("Powers/Player/Blade/Talents/SpecRotational.prototype",                   false, 0.05f),
            new("Powers/Player/Blade/Talents/StakeTalent.prototype",                      false, 0.05f),
            new("Powers/Player/Blade/Talents/ToxinPowerFearTalent.prototype",             false, 0.05f),
            new("Powers/Player/Blade/Traits/BloodLustMechanicTrait.prototype",            false, 0.05f),
            new("Powers/Player/Blade/Traits/DefenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/Blade/Traits/OffenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/Blade/UVGrenade.prototype",                                true,  0.0370f), // 2026-06-20
            new("Powers/Player/Blade/UnleashGlaive.prototype",                            true,  0.1110f), // 2026-06-20
            new("Powers/Player/TravelPower/BladeRide.prototype",                          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BladeStolenPower.prototype",         false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
        };
    }
}
