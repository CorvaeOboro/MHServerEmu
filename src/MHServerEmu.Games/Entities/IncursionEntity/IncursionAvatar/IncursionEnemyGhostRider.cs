using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// GhostRider Invader
    /// Powers: 18 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyGhostRider : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/GhostRider.prototype");

        public IncursionEnemyGhostRider(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "GhostRider Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/GhostRider/AlejandraBlaze.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/AlejandraBlazeVU.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/FantasticFour.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/Modern.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/Original.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/RobbieReyesTESTING.prototype", true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/SecretWars.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/TESTONLYVU.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/GhostRider/TrailOfTears.prototype",       true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.0333f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/GhostRider/BasicChains.prototype",                             true,  0.0942f), // 2026-06-18
            new("Powers/Player/GhostRider/BasicChainsNarrow.prototype",                       true,  0.1025f), // 2026-06-18
            new("Powers/Player/GhostRider/BasicFireball.prototype",                           true,  0.1260f), // 2026-06-18
            new("Powers/Player/GhostRider/BikeLunge.prototype",                               true,  0.1159f), // 2026-06-18
            new("Powers/Player/GhostRider/ChainFlechette.prototype",                          true,  0.0792f), // 2026-06-18
            new("Powers/Player/GhostRider/ChainLineAoE.prototype",                            true,  0.0308f), // 2026-06-18
            new("Powers/Player/GhostRider/ChainRoot.prototype",                               true,  0.0789f), // 2026-06-18
            new("Powers/Player/GhostRider/ChainShockwave.prototype",                          true,  0.0465f), // 2026-06-18
            new("Powers/Player/GhostRider/ChargeUpBike.prototype",                            true,  0.0826f), // 2026-06-16
            new("Powers/Player/GhostRider/ConeYank.prototype",                                true,  0.1225f), // 2026-06-18
            new("Powers/Player/GhostRider/DeathFromAbove.prototype",                          true,  0.0364f), // 2026-06-18
            new("Powers/Player/GhostRider/FireBreath.prototype",                              true,  0.0508f), // 2026-06-18
            new("Powers/Player/GhostRider/FirePillar.prototype",                              true,  0.0991f), // 2026-06-18
            new("Powers/Player/GhostRider/HellfireBeam.prototype",                            true,  0.1134f), // 2026-06-18
            new("Powers/Player/GhostRider/LoopChainWhirlwind.prototype",                      true,  0.0289f), // 2026-06-17
            new("Powers/Player/GhostRider/PenanceStare.prototype",                            true,  0.0200f), // 2026-06-18
            new("Powers/Player/GhostRider/SpiritofVengeance.prototype",                       true,  0.05f),
            new("Powers/Player/GhostRider/Talents/Talent1ChainsAblaze.prototype",             false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent1HellfireCombustion.prototype",       false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent1InfernalContract.prototype",         false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent2LowHealthBuild.prototype",           false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent2MentalBuild.prototype",              false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent2PhysicalBuild.prototype",            false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent3DefensiveHotspot.prototype",         false, 0.008f),
            new("Powers/Player/GhostRider/Talents/Talent3HealthSpendingFireBreath.prototype", false, 0.0508f), // 2026-06-18
            new("Powers/Player/GhostRider/Talents/Talent3ProtectTheInnocent.prototype",       false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent4BikePowerBonuses.prototype",         false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent4LoopChainBike.prototype",            false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent4OneFinalShow.prototype",             false, 0.05f),
            new("Powers/Player/GhostRider/Talents/Talent5ChainFlechette.prototype",           false, 0.0792f), // 2026-06-18
            new("Powers/Player/GhostRider/Talents/Talent5HellfireBeam.prototype",             false, 0.1134f), // 2026-06-18
            new("Powers/Player/GhostRider/Talents/Talent5PenanceStare.prototype",             false, 0.0200f), // 2026-06-18
            new("Powers/Player/GhostRider/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/GhostRider/Traits/MechanicTraitFlameTrail.prototype",          false, 0.05f),
            new("Powers/Player/GhostRider/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/GhostRider/UltimateForRealz.prototype",                        true,  0.0191f), // 2026-06-18
            new("Powers/Player/TravelPower/GhostRiderRide.prototype",                         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GhostRiderStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                   false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                      false, 0.05f),
            new("Powers/Player/LukeCage/Pummel.prototype",                                    false, 0.05f),
            new("Powers/Player/LukeCage/ThrowCar.prototype",                                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                         false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                               false, 0.05f),
        };
    }
}
