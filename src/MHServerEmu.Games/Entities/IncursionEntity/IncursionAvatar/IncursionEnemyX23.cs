using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// X23 Invader
    /// Powers: 14 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyX23 : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/X23.prototype");

        public IncursionEnemyX23(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "X23 Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/X23/AllNewWolverine.prototype", true),
            new("Entity/Items/Costumes/Prototypes/X23/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/X23/Fang.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/X23/InnocenceLost.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/X23/Skrull.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/X23/TargetX.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/X23/XForce.prototype",          true),
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
            new("Powers/Player/TravelPower/X23Sprint.prototype",                            false, 0.05f),
            new("Powers/Player/X23/BasicBloody.prototype",                                  true,  0.1202f), // 2026-06-18
            new("Powers/Player/X23/BladeSpin.prototype",                                    true,  0.0422f), // 2026-06-20
            new("Powers/Player/X23/CoupDeGrace.prototype",                                  true,  0.05f),
            new("Powers/Player/X23/CoupDeGraceEnableHiddenPassive.prototype",               false, 0.05f),
            new("Powers/Player/X23/CrimsonCircle.prototype",                                true,  0.1200f), // 2026-06-20
            new("Powers/Player/X23/CrimsonLeapStart.prototype",                             true,  0.0445f), // 2026-06-20
            new("Powers/Player/X23/Eviscerate.prototype",                                   true,  0.0928f), // 2026-06-18
            new("Powers/Player/X23/Execute.prototype",                                      true,  0.0513f), // 2026-06-18
            new("Powers/Player/X23/FuriousLunge.prototype",                                 true,  0.1448f), // 2026-06-20
            new("Powers/Player/X23/MoveMechanicHiddenPassive.prototype",                    false, 0.05f),
            new("Powers/Player/X23/MvmtSTSS.prototype",                                     true,  0.1296f), // 2026-06-18
            new("Powers/Player/X23/PassiveStealth.prototype",                               false, 0.05f),
            new("Powers/Player/X23/PassiveStealthCDRHiddenPassive.prototype",               false, 0.05f),
            new("Powers/Player/X23/Pummel.prototype",                                       true,  0.0748f), // 2026-06-20
            new("Powers/Player/X23/SigBladeDance.prototype",                                true,  0.0134f), // 2026-06-20
            new("Powers/Player/X23/Talents/Talent1MaxWrath.prototype",                      false, 0.05f),
            new("Powers/Player/X23/Talents/Talent1WrathGWTicksBleedDmg.prototype",          false, 0.05f),
            new("Powers/Player/X23/Talents/Talent1WrathMvmtDmg.prototype",                  false, 0.05f),
            new("Powers/Player/X23/Talents/Talent2EvisMvmtSTSSDmg.prototype",               false, 0.1296f), // 2026-06-18
            new("Powers/Player/X23/Talents/Talent2GWDurationCritChance.prototype",          false, 0.05f),
            new("Powers/Player/X23/Talents/Talent2PummelExecuteDmgCDR.prototype",           false, 0.0513f), // 2026-06-18
            new("Powers/Player/X23/Talents/Talent3BladeSpinMvmtCrimsonLeapBleed.prototype", false, 0.0422f), // 2026-06-20
            new("Powers/Player/X23/Talents/Talent3CrimsonLeapFuriousLungeCDR.prototype",    false, 0.05f),
            new("Powers/Player/X23/Talents/Talent3EviscerateMvmtSTSSFerocity.prototype",    false, 0.0928f), // 2026-06-18
            new("Powers/Player/X23/Talents/Talent4SigChargeIncrease.prototype",             false, 0.05f),
            new("Powers/Player/X23/Talents/Talent4SigPummelExecuteCharge.prototype",        false, 0.0513f), // 2026-06-18
            new("Powers/Player/X23/Talents/Talent4SigTipleKickCoupDeGraceCharge.prototype", false, 0.05f),
            new("Powers/Player/X23/Talents/Talent5DefenseBuffs.prototype",                  false, 0.05f),
            new("Powers/Player/X23/Talents/Talent5DodgeBuffs.prototype",                    false, 0.05f),
            new("Powers/Player/X23/Talents/Talent5StealthInvisRapidHealing.prototype",      false, 0.05f),
            new("Powers/Player/X23/Traits/DefenseTrait.prototype",                          false, 0.05f),
            new("Powers/Player/X23/Traits/MechanicTrait.prototype",                         false, 0.05f),
            new("Powers/Player/X23/Traits/OffenseTrait.prototype",                          false, 0.05f),
            new("Powers/Player/X23/TripleKick.prototype",                                   true,  0.0541f), // 2026-06-18
            new("Powers/Player/X23/Tumble.prototype",                                       true,  0.05f),
            new("Powers/Player/X23/UltTriggerScent.prototype",                              true,  0.006f),
            new("Powers/Player/X23/UltimateHiddenPassive.prototype",                        false, 0.006f),
            new("Powers/StolenPowers/StealablePowers/X23StolenPower.prototype",             false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
