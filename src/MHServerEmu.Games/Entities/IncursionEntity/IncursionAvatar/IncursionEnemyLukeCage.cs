using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// LukeCage Invader
    /// Powers: 15 / 41
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyLukeCage : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/LukeCage.prototype");

        public IncursionEnemyLukeCage(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "LukeCage Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/LukeCage/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/Classic2013.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/EarthX.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/HeroesForHire.prototype", true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/LukeCage90s.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/Modern.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/Modern2013.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/Noir.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/Skrull.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/StreetClothes.prototype", true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/TV.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/LukeCage/TVVariant.prototype",     true),
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
            new("Powers/Player/LukeCage/BasicCrowbar.prototype",                        true,  0.0960f), // 2026-06-20
            new("Powers/Player/LukeCage/BasicPunch.prototype",                          true,  0.0763f), // 2026-06-17
            new("Powers/Player/LukeCage/Charge.prototype",                              true,  0.1417f), // 2026-06-20
            new("Powers/Player/LukeCage/ChunkOConcrete.prototype",                      true,  0.1001f), // 2026-06-17
            new("Powers/Player/LukeCage/DefensiveLeader.prototype",                     true,  0.05f),
            new("Powers/Player/LukeCage/ElbowDrop.prototype",                           true,  0.0592f), // 2026-06-20
            new("Powers/Player/LukeCage/MeleePunchUppercut.prototype",                  true,  0.1057f), // 2026-06-20
            new("Powers/Player/LukeCage/Pummel.prototype",                              true,  0.05f),
            new("Powers/Player/LukeCage/PunchTheGround.prototype",                      true,  0.05f),
            new("Powers/Player/LukeCage/StreetKick.prototype",                          true,  0.1386f), // 2026-06-20
            new("Powers/Player/LukeCage/SummonIronFist.prototype",                      true,  0.0579f), // 2026-06-20
            new("Powers/Player/LukeCage/SweetChristmas.prototype",                      true,  0.0086f), // 2026-06-20
            new("Powers/Player/LukeCage/Talents/BoxingSpec.prototype",                  false, 0.05f),
            new("Powers/Player/LukeCage/Talents/ChainSpec.prototype",                   false, 0.05f),
            new("Powers/Player/LukeCage/Talents/ColleenWing.prototype",                 false, 0.05f),
            new("Powers/Player/LukeCage/Talents/ComboPointsComboFighter.prototype",     false, 0.025f),
            new("Powers/Player/LukeCage/Talents/ComboPointsIncreaseMax.prototype",      false, 0.025f),
            new("Powers/Player/LukeCage/Talents/ComboPointsNoComboBar.prototype",       false, 0.025f),
            new("Powers/Player/LukeCage/Talents/FightingStyleFistsOfFury.prototype",    false, 0.02f),
            new("Powers/Player/LukeCage/Talents/FightingStyleTheDefender.prototype",    false, 0.05f),
            new("Powers/Player/LukeCage/Talents/HeroesForHireBusinessIsGood.prototype", false, 0.05f),
            new("Powers/Player/LukeCage/Talents/HeroesForHireHeroesCall.prototype",     false, 0.05f),
            new("Powers/Player/LukeCage/Talents/JessicaJones.prototype",                false, 0.05f),
            new("Powers/Player/LukeCage/Talents/MistyKnight.prototype",                 false, 0.05f),
            new("Powers/Player/LukeCage/Talents/MobilityBuffs.prototype",               false, 0.05f),
            new("Powers/Player/LukeCage/Talents/PowerBuffsLongRangeWeapons.prototype",  false, 0.05f),
            new("Powers/Player/LukeCage/Talents/PowerBuffsTauntSteroid.prototype",      false, 0.05f),
            new("Powers/Player/LukeCage/ThrowCar.prototype",                            true,  0.0622f), // 2026-06-20
            new("Powers/Player/LukeCage/Traits/DefenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/LukeCage/Traits/MechanicTraitComboPoints.prototype",     false, 0.025f),
            new("Powers/Player/LukeCage/Traits/OffenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/LukeCage/Ultimate.prototype",                            true,  0.0080f), // 2026-06-20
            new("Powers/Player/LukeCage/Yank.prototype",                                true,  0.0978f), // 2026-06-20
            new("Powers/Player/TravelPower/LukeCageSprint.prototype",                   false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LukeCageStolenPower.prototype",    false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",              false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",             false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                   false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                         false, 0.05f),
        };
    }
}
