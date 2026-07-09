using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// BlackCat Invader
    /// Powers: 18 / 47
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBlackCat : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/BlackCat.prototype");

        public IncursionEnemyBlackCat(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "BlackCat Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/BlackCat/ANAD.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/BlackCat/Classic.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackCat/SpiderGwen.prototype", true),
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
            new("Powers/Player/BlackCat/Assassinate.prototype",                                 true,  0.0761f), // 2026-06-18
            new("Powers/Player/BlackCat/BasicClaws.prototype",                                  true,  0.1919f), // 2026-06-08
            new("Powers/Player/BlackCat/BasicWhip.prototype",                                   true,  0.1509f), // 2026-06-11
            new("Powers/Player/BlackCat/ClawPummel.prototype",                                  true,  0.1288f), // 2026-06-11
            new("Powers/Player/BlackCat/ClawSwipes.prototype",                                  true,  0.1310f), // 2026-06-11
            new("Powers/Player/BlackCat/ClawTwirl.prototype",                                   true,  0.1368f), // 2026-06-11
            new("Powers/Player/BlackCat/ConeYank.prototype",                                    true,  0.0943f), // 2026-06-08
            new("Powers/Player/BlackCat/DeathFromAbove.prototype",                              true,  0.0536f), // 2026-06-18
            new("Powers/Player/BlackCat/Garrotte.prototype",                                    true,  0.0662f), // 2026-06-11
            new("Powers/Player/BlackCat/GasTrap.prototype",                                     true,  0.1260f), // 2026-06-11
            new("Powers/Player/BlackCat/GlueTrap.prototype",                                    true,  0.0983f), // 2026-06-11
            new("Powers/Player/BlackCat/MasterThief.prototype",                                 true,  0.05f),
            new("Powers/Player/BlackCat/NineLivesDisableHealthMinHiddenPassive.prototype",      false, 0.05f),
            new("Powers/Player/BlackCat/NineLivesHealthMinHiddenPassive.prototype",             false, 0.05f),
            new("Powers/Player/BlackCat/ShrapnelTrap.prototype",                                true,  0.0994f), // 2026-06-11
            new("Powers/Player/BlackCat/Signature.prototype",                                   true,  0.0134f), // 2026-06-11
            new("Powers/Player/BlackCat/Talents/TalentAssassinateDoesntBreakStealth.prototype", false, 0.0761f), // 2026-06-18
            new("Powers/Player/BlackCat/Talents/TalentClawPummelBonus.prototype",               false, 0.1288f), // 2026-06-11
            new("Powers/Player/BlackCat/Talents/TalentConeYankDamageBonus.prototype",           false, 0.0943f), // 2026-06-08
            new("Powers/Player/BlackCat/Talents/TalentInstantKillPopcorn.prototype",            false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentMasterThiefCrit.prototype",               false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentMasterThiefResetTrap.prototype",          false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentMasterThiefStealChance.prototype",        false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentNineLivesRecharge.prototype",             false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentRandomKnockdown.prototype",               false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentSignatureBleed.prototype",                false, 0.0134f), // 2026-06-11
            new("Powers/Player/BlackCat/Talents/TalentSignatureRemap.prototype",                false, 0.0134f), // 2026-06-11
            new("Powers/Player/BlackCat/Talents/TalentSignatureThrowTraps.prototype",           false, 0.0134f), // 2026-06-11
            new("Powers/Player/BlackCat/Talents/TalentTrapsDontBreakStealth.prototype",         false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentTrapsShareCharges.prototype",             false, 0.05f),
            new("Powers/Player/BlackCat/Talents/TalentTumbleStealthDuration.prototype",         false, 0.05f),
            new("Powers/Player/BlackCat/TaserTrap.prototype",                                   true,  0.05f),
            new("Powers/Player/BlackCat/Traits/DefenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/BlackCat/Traits/MechanicTraitNineLives.prototype",               false, 0.05f),
            new("Powers/Player/BlackCat/Traits/OffenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/BlackCat/Tumble.prototype",                                      true,  0.05f),
            new("Powers/Player/BlackCat/Ultimate.prototype",                                    true,  0.0116f), // 2026-06-18
            new("Powers/Player/BlackCat/UltimateHiddenPassive.prototype",                       false, 0.0116f), // 2026-06-18
            new("Powers/Player/BlackCat/WhipLash.prototype",                                    true,  0.1870f), // 2026-06-11
            new("Powers/Player/TravelPower/BlackCatFlight.prototype",                           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackCatStolenPower.prototype",            false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                             false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                     false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                        false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                           false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                 false, 0.05f),
        };
    }
}
