using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// BlackPanther Invader
    /// Powers: 19 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBlackPanther : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/BlackPanther.prototype");

        public IncursionEnemyBlackPanther(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "BlackPanther Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/BlackPanther/ArmoredPanther.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/CivilWarMovie.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/Classic.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/ClassicVU.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/DoomWar.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/ManWOFearGold.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/ManWOFearGoldCape.prototype", true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/ManWithoutFear.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/Shuri.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/Tribal.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/VibraniumArmor.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackPanther/WakandanTech.prototype",      true),
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
            new("Powers/Player/BlackPanther/AcrobaticAttack.prototype",                              true,  0.0644f), // 2026-06-20
            new("Powers/Player/BlackPanther/BasicDaggerThrow.prototype",                             true,  0.1548f), // 2026-06-20
            new("Powers/Player/BlackPanther/ClawUppercut.prototype",                                 true,  0.0811f), // 2026-06-20
            new("Powers/Player/BlackPanther/DaggerCharge.prototype",                                 true,  0.1249f), // 2026-06-20
            new("Powers/Player/BlackPanther/DisengagingShot.prototype",                              true,  0.1045f), // 2026-06-20
            new("Powers/Player/BlackPanther/DoublePunch.prototype",                                  true,  0.1552f), // 2026-06-17
            new("Powers/Player/BlackPanther/EnergyDaggers.prototype",                                true,  0.1613f), // 2026-06-20
            new("Powers/Player/BlackPanther/EnergyTrap.prototype",                                   true,  0.0644f), // 2026-06-20
            new("Powers/Player/BlackPanther/EnervationDaggers.prototype",                            true,  0.1588f), // 2026-06-20
            new("Powers/Player/BlackPanther/FreezingDaggers.prototype",                              true,  0.2802f), // 2026-06-20
            new("Powers/Player/BlackPanther/MineFieldRanged.prototype",                              true,  0.05f),
            new("Powers/Player/BlackPanther/PantherBomb.prototype",                                  true,  0.0108f), // 2026-06-20
            new("Powers/Player/BlackPanther/QuickSlash.prototype",                                   true,  0.0892f), // 2026-06-20
            new("Powers/Player/BlackPanther/SmokeScreen.prototype",                                  true,  0.05f),
            new("Powers/Player/BlackPanther/Snare.prototype",                                        true,  0.1018f), // 2026-06-20
            new("Powers/Player/BlackPanther/SweepingKick.prototype",                                 true,  0.0476f), // 2026-06-20
            new("Powers/Player/BlackPanther/Talents/AcrobaticAttackBuffTalent.prototype",            false, 0.0644f), // 2026-06-20
            new("Powers/Player/BlackPanther/Talents/DoraDefensiveTalent.prototype",                  false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/DoraOffensiveBonusTalent.prototype",             false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/EnervateStackTalent.prototype",                  false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/FanofKnivesBuffTalent.prototype",                false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/FocEnervatingDaggersSpenderTalent.prototype",    false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/JungleSnareRangedTalent.prototype",              false, 0.1018f), // 2026-06-20
            new("Powers/Player/BlackPanther/Talents/MineFieldMeleeTalent.prototype",                 false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/NoDoraTalent.prototype",                         false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/PredatorsFrenzyBuffTalent.prototype",            false, 0.02f),
            new("Powers/Player/BlackPanther/Talents/SweepBleedDamage.prototype",                     false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/TripleThrowSpenderSpec.prototype",               false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/UnseenPredatorAutomaticTalent.prototype",        false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/UnseenPredatorDamageMitigationTalent.prototype", false, 0.05f),
            new("Powers/Player/BlackPanther/Talents/UnseenPredatorHealRangedTalent.prototype",       false, 0.05f),
            new("Powers/Player/BlackPanther/Traits/DefensePassive.prototype",                        false, 0.05f),
            new("Powers/Player/BlackPanther/Traits/OffenseTrait.prototype",                          false, 0.05f),
            new("Powers/Player/BlackPanther/TripleShot.prototype",                                   true,  0.0956f), // 2026-06-20
            new("Powers/Player/BlackPanther/Tumble.prototype",                                       true,  0.05f),
            new("Powers/Player/BlackPanther/Ultimate.prototype",                                     true,  0.0132f), // 2026-06-20
            new("Powers/Player/TravelPower/BlackPantherSprint.prototype",                            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackPantherStolenPower.prototype",             false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                  false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                          false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                             false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                                false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                      false, 0.05f),
        };
    }
}
