using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// IronMan Invader
    /// Powers: 18 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyIronMan : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/IronMan.prototype");

        public IncursionEnemyIronMan(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "IronMan Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/IronMan/AgeOfUltronMovieHulkBuster.prototype", true),
            new("Entity/Items/Costumes/Prototypes/IronMan/AgeOfUltronMovieMark43.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Avengers.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/IronMan/CivilWar.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Classic.prototype",                    true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Extremis.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/IronMan/GoldAvenger.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/IronMan/GotG.prototype",                       true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Heartbreaker.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/IronMan/HomecomingMark47.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Hulkbuster.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/IronMan/IronMan3Movie.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/IronMan/MacPac.prototype",                     true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark1.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark2.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark3.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark4.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark5.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Mark6.prototype",                      true),
            new("Entity/Items/Costumes/Prototypes/IronMan/MarvelNOW.prototype",                  true),
            new("Entity/Items/Costumes/Prototypes/IronMan/NightClubRed.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Shotgun.prototype",                    true),
            new("Entity/Items/Costumes/Prototypes/IronMan/SilverCenturion.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Starboost.prototype",                  true),
            new("Entity/Items/Costumes/Prototypes/IronMan/Stealth.prototype",                    true),
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
            new("Powers/Player/IronMan/BasicDentingPunch.prototype",                true,  0.1041f), // 2026-06-18
            new("Powers/Player/IronMan/BasicRepulsorBeam.prototype",                true,  0.0980f), // 2026-06-18
            new("Powers/Player/IronMan/BrutalStrike.prototype",                     true,  0.0683f), // 2026-06-18
            new("Powers/Player/IronMan/ChanneledEnergyBeam.prototype",              true,  0.0391f), // 2026-06-18
            new("Powers/Player/IronMan/DeathFromAbove.prototype",                   true,  0.0681f), // 2026-06-18
            new("Powers/Player/IronMan/FreonRay.prototype",                         true,  0.0829f), // 2026-06-18
            new("Powers/Player/IronMan/JetThrustPunch.prototype",                   true,  0.0713f), // 2026-06-18
            new("Powers/Player/IronMan/Micromissiles.prototype",                    true,  0.0860f), // 2026-06-18
            new("Powers/Player/IronMan/RainOfMissiles.prototype",                   true,  0.0759f), // 2026-06-18
            new("Powers/Player/IronMan/RapidFire.prototype",                        true,  0.1621f), // 2026-06-18
            new("Powers/Player/IronMan/RepulsorBurst.prototype",                    true,  0.1109f), // 2026-06-18
            new("Powers/Player/IronMan/RepulsorSpray.prototype",                    true,  0.0441f), // 2026-06-17
            new("Powers/Player/IronMan/ShieldOverload.prototype",                   true,  0.0950f), // 2026-06-13
            new("Powers/Player/IronMan/Signature.prototype",                        true,  0.0106f), // 2026-06-18
            new("Powers/Player/IronMan/SpeedRush.prototype",                        true,  0.1774f), // 2026-06-18
            new("Powers/Player/IronMan/Talents/ExtremisHealingNanites.prototype",   false, 0.05f),
            new("Powers/Player/IronMan/Talents/LifeSupport.prototype",              false, 0.05f),
            new("Powers/Player/IronMan/Talents/MissileTargeting.prototype",         false, 0.01f),
            new("Powers/Player/IronMan/Talents/Overclocked.prototype",              false, 0.05f),
            new("Powers/Player/IronMan/Talents/ReactiveArmor.prototype",            false, 0.05f),
            new("Powers/Player/IronMan/Talents/UnstableCore.prototype",             false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeArcReactor.prototype",        false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeArmorHydraulics.prototype",   false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeCoolingSystem.prototype",     false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeJetThrusters.prototype",      false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeMicrolaser.prototype",        false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeMissilePayloads.prototype",   false, 0.01f),
            new("Powers/Player/IronMan/Talents/UpgradeOrbitalInterface.prototype",  false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeRepulsors.prototype",         false, 0.05f),
            new("Powers/Player/IronMan/Talents/UpgradeTargetingComputer.prototype", false, 0.05f),
            new("Powers/Player/IronMan/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/IronMan/Traits/MechanicTraitSuitPower.prototype",    false, 0.05f),
            new("Powers/Player/IronMan/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/IronMan/UltimateSummonSuits.prototype",              true,  0.0264f), // 2026-06-18
            new("Powers/Player/IronMan/UniBeam.prototype",                          true,  0.05f),
            new("Powers/Player/IronMan/WristRocket.prototype",                      true,  0.1121f), // 2026-06-18
            new("Powers/Player/TravelPower/IronManFlight.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IronManStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",          false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",            false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",               false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                     false, 0.05f),
        };
    }
}
