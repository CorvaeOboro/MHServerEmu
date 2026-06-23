using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Hawkeye Invader
    /// Powers: 17 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyHawkeye : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Hawkeye.prototype");

        public IncursionEnemyHawkeye(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Hawkeye Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Hawkeye/AgeOfUltronMovie.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/Avengers.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/CivilWar.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/Classic.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/ClassicVU.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/FearItself.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/HawkGuyLongSleeves.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/HawkGuySweatpants.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/HawkGuyTShirt.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/KateBishopYoungAvengers.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/Modern.prototype",                  true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/OldManLogan.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/Ronin.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/Hawkeye/Shield.prototype",                  true),
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
            new("Powers/Player/Hawkeye/AdamantiumArrow.prototype",                     true,  0.05f),
            new("Powers/Player/Hawkeye/BasicArrow.prototype",                          true,  0.1813f), // 2026-06-20
            new("Powers/Player/Hawkeye/DisengagingShot.prototype",                     true,  0.2192f), // 2026-06-18
            new("Powers/Player/Hawkeye/ExplosiveArrow.prototype",                      true,  0.1680f), // 2026-06-18
            new("Powers/Player/Hawkeye/FlashBomb.prototype",                           true,  0.05f),
            new("Powers/Player/Hawkeye/FreezeArrow.prototype",                         true,  0.1656f), // 2026-06-18
            new("Powers/Player/Hawkeye/NullifierArrow.prototype",                      true,  0.05f),
            new("Powers/Player/Hawkeye/PinningShot.prototype",                         true,  0.0856f), // 2026-06-18
            new("Powers/Player/Hawkeye/PoisonGasBomb.prototype",                       true,  0.0892f), // 2026-06-18
            new("Powers/Player/Hawkeye/ShriekingArrow.prototype",                      true,  0.0930f), // 2026-06-18
            new("Powers/Player/Hawkeye/Talents/AutoTrickArrows.prototype",             false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/DisengagingShotTalent.prototype",       false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/FlashBombTalent.prototype",             false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/MeleeHawkeyeTalent.prototype",          false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/PinningShotTalent.prototype",           false, 0.0856f), // 2026-06-18
            new("Powers/Player/Hawkeye/Talents/PymArrowheadsTalent.prototype",         false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/SerratedArrowheadsTalent.prototype",    false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/ShriekingArrowTalent.prototype",        false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/SpeedLoaderPiercingTalent.prototype",   false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/ThreeRoundBurstBonusCharge.prototype",  false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/TrickDmgMultTalent.prototype",          false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/TrickVolleyTalent.prototype",           false, 0.1308f), // 2026-06-18
            new("Powers/Player/Hawkeye/Talents/TridentArrowheadsTalent.prototype",     false, 0.05f),
            new("Powers/Player/Hawkeye/Talents/TurretArrowBonusArrowTalent.prototype", false, 0.0135f), // 2026-06-18
            new("Powers/Player/Hawkeye/Talents/TurretArrowCooldownTalent.prototype",   false, 0.0135f), // 2026-06-18
            new("Powers/Player/Hawkeye/TaserArrow.prototype",                          true,  0.3680f), // 2026-06-17
            new("Powers/Player/Hawkeye/TenArrowSpeedLoader.prototype",                 true,  0.0982f), // 2026-06-18
            new("Powers/Player/Hawkeye/ThreeRoundBurst.prototype",                     true,  0.1694f), // 2026-06-18
            new("Powers/Player/Hawkeye/Traits/DefenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Hawkeye/Traits/OffenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Hawkeye/Traits/TrickQuiverMechanicTrait.prototype",     false, 0.05f),
            new("Powers/Player/Hawkeye/Tumble.prototype",                              true,  0.05f),
            new("Powers/Player/Hawkeye/TurretArrow.prototype",                         true,  0.0135f), // 2026-06-18
            new("Powers/Player/Hawkeye/Ultimate.prototype",                            true,  0.0082f), // 2026-06-18
            new("Powers/Player/Hawkeye/UltimateHiddenPassive.prototype",               false, 0.0082f), // 2026-06-18
            new("Powers/Player/Hawkeye/Volley.prototype",                              true,  0.1308f), // 2026-06-18
            new("Powers/Player/TravelPower/HawkeyeFlight.prototype",                   false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HawkeyeStolenPower.prototype",    false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",             false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",            false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",               false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                  false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                        false, 0.05f),
        };
    }
}
