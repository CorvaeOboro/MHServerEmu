using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Vision Invader
    /// Powers: 14 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyVision : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Vision.prototype");

        public IncursionEnemyVision(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Vision Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Vision/AgeOfUltronMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Vision/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Vision/Spectral.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Vision/SuburbanDad.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Vision/UncannyAvengers.prototype",  true),
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
            new("Powers/Player/TravelPower/VisionFlight.prototype",                       false, 0.05f),
            new("Powers/Player/Vision/AtomicFootDive.prototype",                          true,  0.0146f), // 2026-06-19
            new("Powers/Player/Vision/ControlRobot.prototype",                            true,  0.0530f), // 2026-06-20
            new("Powers/Player/Vision/ControlRobotHPassiveBuff.prototype",                false, 0.0530f), // 2026-06-20
            new("Powers/Player/Vision/DeathfromBelow.prototype",                          true,  0.0521f), // 2026-06-19
            new("Powers/Player/Vision/DensityHiddenPassiveController.prototype",          false, 0.05f),
            new("Powers/Player/Vision/EnhanceRobot.prototype",                            true,  0.05f),
            new("Powers/Player/Vision/FocusBeam.prototype",                               true,  0.0343f), // 2026-06-20
            new("Powers/Player/Vision/HealingNanites.prototype",                          true,  0.05f),
            new("Powers/Player/Vision/ModeToggle.prototype",                              false, 0.05f),
            new("Powers/Player/Vision/Phase.prototype",                                   true,  0.0464f), // 2026-06-19
            new("Powers/Player/Vision/PhaseHand.prototype",                               true,  0.0464f), // 2026-06-19
            new("Powers/Player/Vision/PhasePunch.prototype",                              true,  0.1337f), // 2026-06-10
            new("Powers/Player/Vision/ScorchedEarth.prototype",                           true,  0.0592f), // 2026-06-20
            new("Powers/Player/Vision/SolarBolt.prototype",                               true,  0.1600f), // 2026-06-20
            new("Powers/Player/Vision/SolarChanneledEnergyBeam.prototype",                true,  0.0682f), // 2026-06-20
            new("Powers/Player/Vision/SolarCone.prototype",                               true,  0.0850f), // 2026-06-20
            new("Powers/Player/Vision/SolarOvercharge.prototype",                         true,  0.0275f), // 2026-06-19
            new("Powers/Player/Vision/SolarOverchargeHiddenPassive.prototype",            false, 0.0275f), // 2026-06-19
            new("Powers/Player/Vision/StealthToggle.prototype",                           false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent1PhasePunchDefBuff.prototype",        false, 0.1337f), // 2026-06-10
            new("Powers/Player/Vision/Talents/Talent1SolarConeBuffMeleeDmg.prototype",    false, 0.0850f), // 2026-06-20
            new("Powers/Player/Vision/Talents/Talent1SolarRateRegenMax.prototype",        false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent2DenseModeBuff.prototype",            false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent2DensityShiftCDRDefBuff.prototype",   false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent2PhaseModeBuff.prototype",            false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent3AugmentedBeams.prototype",           false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent3SelfRepair.prototype",               false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent3SolarFists.prototype",               false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent4EnhanceRobotNoDetonation.prototype", false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent4NoPetBuff.prototype",                false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent4RobotPetSolarBuff.prototype",        false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent5SigCDR.prototype",                   false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent5SigDFABuff.prototype",               false, 0.05f),
            new("Powers/Player/Vision/Talents/Talent5SigSolarEnergyRegen.prototype",      false, 0.05f),
            new("Powers/Player/Vision/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Vision/Traits/MechanicTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Vision/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NoStealablePowerBlank.prototype",    false, 0.05f),
        };
    }
}
