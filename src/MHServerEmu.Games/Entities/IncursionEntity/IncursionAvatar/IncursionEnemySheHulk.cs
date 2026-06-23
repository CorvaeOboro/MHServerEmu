using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// SheHulk Invader
    /// Powers: 18 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemySheHulk : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/SheHulk.prototype");

        public IncursionEnemySheHulk(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "SheHulk Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/SheHulk/HeroesForHire.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/SheHulk/LawAndDisorder.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/SheHulk/Lawyer.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/SheHulk/SGFJeans.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/SheHulk/SingleGreenFemale.prototype", true),
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
            new("Powers/Player/SheHulk/Assault.prototype",                                           true,  0.1160f), // 2026-06-18
            new("Powers/Player/SheHulk/BarExam.prototype",                                           true,  0.0401f), // 2026-06-18
            new("Powers/Player/SheHulk/BarristerBeatdown.prototype",                                 true,  0.02f),
            new("Powers/Player/SheHulk/Battery.prototype",                                           true,  0.1023f), // 2026-06-18
            new("Powers/Player/SheHulk/CeaseAndDesist.prototype",                                    true,  0.0579f), // 2026-06-18
            new("Powers/Player/SheHulk/ClosingArguments.prototype",                                  true,  0.0410f), // 2026-06-06
            new("Powers/Player/SheHulk/Conviction.prototype",                                        true,  0.0085f), // 2026-06-18
            new("Powers/Player/SheHulk/CrossExamination.prototype",                                  true,  0.0669f), // 2026-06-18
            new("Powers/Player/SheHulk/DefenseAttorney.prototype",                                   true,  0.05f),
            new("Powers/Player/SheHulk/FinalVerdict.prototype",                                      true,  0.0551f), // 2026-06-18
            new("Powers/Player/SheHulk/FuriousLunge.prototype",                                      true,  0.1580f), // 2026-06-18
            new("Powers/Player/SheHulk/HostileWitness.prototype",                                    true,  0.05f),
            new("Powers/Player/SheHulk/LawyerUp.prototype",                                          true,  0.05f),
            new("Powers/Player/SheHulk/MoveToStrike.prototype",                                      true,  0.0530f), // 2026-06-18
            new("Powers/Player/SheHulk/Objection.prototype",                                         true,  0.2395f), // 2026-06-06
            new("Powers/Player/SheHulk/OpeningStatement.prototype",                                  true,  0.0697f), // 2026-06-18
            new("Powers/Player/SheHulk/SurpriseWitness.prototype",                                   true,  0.2428f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent1IncreaseFinisherDamage.prototype",             false, 0.05f),
            new("Powers/Player/SheHulk/Talents/Talent1IncreaseMaxComboPoints.prototype",             false, 0.025f),
            new("Powers/Player/SheHulk/Talents/Talent1RemoveComboPoints.prototype",                  false, 0.025f),
            new("Powers/Player/SheHulk/Talents/Talent2BarristerBeatdownCooldownReduction.prototype", false, 0.02f),
            new("Powers/Player/SheHulk/Talents/Talent2CeaseAndDesistOnHitObjection.prototype",       false, 0.0579f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent2OpeningStatementBonusDamage.prototype",        false, 0.0697f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent3BarExamToMissile.prototype",                   false, 0.0401f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent3ConvictionCDR.prototype",                      false, 0.0085f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent3HostileWitnessBonus.prototype",                false, 0.05f),
            new("Powers/Player/SheHulk/Talents/Talent4AssaultAndBattery.prototype",                  false, 0.1160f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent4ObjectionMoveToStrikeTalent.prototype",        false, 0.0530f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent4SurpriseWitnessCDR.prototype",                 false, 0.2428f), // 2026-06-18
            new("Powers/Player/SheHulk/Talents/Talent5LawyerUpAsPassive.prototype",                  false, 0.05f),
            new("Powers/Player/SheHulk/Talents/Talent5LawyerUpSuperBuff.prototype",                  false, 0.05f),
            new("Powers/Player/SheHulk/Talents/Talent5LawyerUpUnbreakable.prototype",                false, 0.05f),
            new("Powers/Player/SheHulk/Traits/DefenseTrait.prototype",                               false, 0.05f),
            new("Powers/Player/SheHulk/Traits/MechanicTraitAnger.prototype",                         false, 0.05f),
            new("Powers/Player/SheHulk/Traits/OffenseTrait.prototype",                               false, 0.05f),
            new("Powers/Player/SheHulk/UltimateInitialHit.prototype",                                true,  0.0108f), // 2026-06-18
            new("Powers/Player/TravelPower/SheHulkSprint.prototype",                                 false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SheHulkStolenPower.prototype",                  false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                  false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                          false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                             false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                                false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                      false, 0.05f),
        };
    }
}
