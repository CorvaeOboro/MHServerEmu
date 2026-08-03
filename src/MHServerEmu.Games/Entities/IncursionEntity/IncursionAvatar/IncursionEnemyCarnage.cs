using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Carnage Invader
    /// Powers: 16 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyCarnage : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Carnage.prototype");

        public IncursionEnemyCarnage(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Carnage Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Carnage/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Carnage/SpiderCarnage.prototype", true),
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
            new("Powers/Player/Carnage/AxeBleedHiddenPassive.prototype",                   false, 0.0509f), // 2026-07-30
            new("Powers/Player/Carnage/AxeDFA.prototype",                                  true,  0.0362f), // 2026-06-20
            new("Powers/Player/Carnage/AxeSweep.prototype",                                true,  0.0691f), // 2026-06-10
            new("Powers/Player/Carnage/AxeThrow.prototype",                                true,  0.0364f), // 2026-06-10
            new("Powers/Player/Carnage/BasicClaws.prototype",                              true,  0.1081f), // 2026-07-08
            new("Powers/Player/Carnage/ClawPummel.prototype",                              true,  0.1217f), // 2026-06-10
            new("Powers/Player/Carnage/GroundSmash.prototype",                             true,  0.0110f), // 2026-06-10
            new("Powers/Player/Carnage/KnifeBarrage.prototype",                            true,  0.0908f), // 2026-06-10
            new("Powers/Player/Carnage/Lunge.prototype",                                   true,  0.2184f), // 2026-06-05
            new("Powers/Player/Carnage/MaceHand.prototype",                                true,  0.0359f), // 2026-07-26
            new("Powers/Player/Carnage/MegaClaw.prototype",                                true,  0.0628f), // 2026-06-05
            new("Powers/Player/Carnage/MegaClawHiddenPassive.prototype",                   false, 0.05f),
            new("Powers/Player/Carnage/MeleeSwordSpin.prototype",                          true,  0.0405f), // 2026-07-26
            new("Powers/Player/Carnage/OrganicWebbing.prototype",                          true,  0.0615f), // 2026-06-10
            new("Powers/Player/Carnage/ReapingTime.prototype",                             true,  0.0145f), // 2026-06-10
            new("Powers/Player/Carnage/Talents/AxeWeaponsAxeSweep.prototype",              false, 0.0720f), // 2026-06-10
            new("Powers/Player/Carnage/Talents/AxeWeaponsDFA.prototype",                   false, 0.05f),
            new("Powers/Player/Carnage/Talents/BladeWeaponsRanged.prototype",              false, 0.05f),
            new("Powers/Player/Carnage/Talents/BladeWeaponsSwordSpin.prototype",           false, 0.05f),
            new("Powers/Player/Carnage/Talents/CarnageRules.prototype",                    false, 0.05f),
            new("Powers/Player/Carnage/Talents/ClawWeaponsExcessHealingStorage.prototype", false, 0.05f),
            new("Powers/Player/Carnage/Talents/ClawWeaponsHealing.prototype",              false, 0.05f),
            new("Powers/Player/Carnage/Talents/ExcessProtectionStorage.prototype",         false, 0.05f),
            new("Powers/Player/Carnage/Talents/FullTransfusion.prototype",                 false, 0.05f),
            new("Powers/Player/Carnage/Talents/HyperMobile.prototype",                     false, 0.05f),
            new("Powers/Player/Carnage/Talents/MaceWeaponsCharges.prototype",              false, 0.05f),
            new("Powers/Player/Carnage/Talents/MaceWeaponsMacePummel.prototype",           false, 0.05f),
            new("Powers/Player/Carnage/Talents/SaferPlay.prototype",                       false, 0.05f),
            new("Powers/Player/Carnage/Talents/SavageRebirth.prototype",                   false, 0.05f),
            new("Powers/Player/Carnage/Talents/SymbioticControl.prototype",                false, 0.05f),
            new("Powers/Player/Carnage/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Carnage/Traits/MechanicTraitSymbioteArmor.prototype",       false, 0.05f),
            new("Powers/Player/Carnage/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Carnage/Traits/SymbioteArmorHiddenPassive.prototype",       false, 0.05f),
            new("Powers/Player/Carnage/TransfusionPBAoE.prototype",                        true,  0.0863f), // 2026-06-10
            new("Powers/Player/Carnage/Ultimate.prototype",                                true,  0.0029f), // 2026-06-10
            new("Powers/Player/Carnage/UltimateHiddenPassive.prototype",                   false, 0.0029f), // 2026-06-10
            new("Powers/Player/Carnage/YankImpale.prototype",                              true,  0.0375f), // 2026-06-10
            new("Powers/Player/TravelPower/CarnageFlight.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CarnageStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                   false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                      false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                            false, 0.05f),
        };
    }
}
