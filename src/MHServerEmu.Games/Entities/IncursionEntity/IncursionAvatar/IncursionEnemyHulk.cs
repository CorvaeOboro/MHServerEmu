using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Hulk Invader
    /// Powers: 15 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyHulk : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Hulk.prototype");

        public IncursionEnemyHulk(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Hulk Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Hulk/AgeOfUltronMovie.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Hulk/AvengersMovie.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Hulk/Classic.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Hulk/ClassicVU.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Hulk/GrayHulk.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Hulk/HolidayHulk.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Hulk/HorsemanOfApocalypse.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Hulk/Hulk2099.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Hulk/Maestro.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Hulk/MarvelNOW.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Hulk/MrFixIt.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Hulk/MrFixItPinstripe.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Hulk/MrFixItWhite.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Hulk/PlanetHulk.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Hulk/PlanetHulkVU.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Hulk/Revengers.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Hulk/Ultimate.prototype",             true),
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
            new("Powers/Player/Hulk/Rework/BasicMeleeUtil.prototype",             true,  0.0914f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/CarFists.prototype",                   true,  0.05f),
            new("Powers/Player/Hulk/Rework/Clap.prototype",                       true,  0.0207f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/DashCrash.prototype",                  true,  0.1023f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/GammaPunch.prototype",                 true,  0.0767f), // 2026-06-18
            new("Powers/Player/Hulk/Rework/LeapImplode.prototype",                true,  0.0355f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/LeapQuake.prototype",                  true,  0.0434f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/Meteor.prototype",                     true,  0.0137f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/PBAoESlam.prototype",                  true,  0.03f),
            new("Powers/Player/Hulk/Rework/Rawr.prototype",                       true,  0.05f),
            new("Powers/Player/Hulk/Rework/Shockwave.prototype",                  true,  0.1417f), // 2026-06-20
            new("Powers/Player/Hulk/Rework/SmashFace.prototype",                  true,  0.0488f), // 2026-06-18
            new("Powers/Player/Hulk/Rework/Tantrum.prototype",                    true,  0.0572f), // 2026-06-18
            new("Powers/Player/Hulk/Rework/ThrowRock.prototype",                  true,  0.0426f), // 2026-06-18
            new("Powers/Player/Hulk/Talents/Talent1AlwaysAngry.prototype",        false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent1BerserkerHulk.prototype",      false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent1DoubleAngerBonuses.prototype", false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent2DefenseBuff.prototype",        false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent2DeflectBonus.prototype",       false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent2HealthBonus.prototype",        false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent3HulkSmashBonus.prototype",     false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent3TantrumBonus.prototype",       false, 0.0572f), // 2026-06-18
            new("Powers/Player/Hulk/Talents/Talent3ThrowRockBonus.prototype",     false, 0.0426f), // 2026-06-18
            new("Powers/Player/Hulk/Talents/Talent4CarFistsBonus.prototype",      false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent4GammaPunchBonus.prototype",    false, 0.0767f), // 2026-06-18
            new("Powers/Player/Hulk/Talents/Talent4Leaping.prototype",            false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent5ClapBonus.prototype",          false, 0.0207f), // 2026-06-20
            new("Powers/Player/Hulk/Talents/Talent5CooldownReduction.prototype",  false, 0.05f),
            new("Powers/Player/Hulk/Talents/Talent5MeteorBonus.prototype",        false, 0.0137f), // 2026-06-20
            new("Powers/Player/Hulk/Traits/AngerDecayOutOfCombat.prototype",      false, 0.05f),
            new("Powers/Player/Hulk/Traits/DefenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Hulk/Traits/MechanicTraitAnger.prototype",         false, 0.05f),
            new("Powers/Player/Hulk/Traits/OffenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Hulk/Ultimate.prototype",                          true,  0.0094f), // 2026-06-20
            new("Powers/Player/TravelPower/HulkSprint.prototype",                 false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HulkStolenPower.prototype",  false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                   false, 0.05f),
        };
    }
}
