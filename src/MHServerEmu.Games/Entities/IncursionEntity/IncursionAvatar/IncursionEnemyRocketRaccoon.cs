using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// RocketRaccoon Invader
    /// Powers: 26 / 54
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyRocketRaccoon : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/RocketRaccoon.prototype");

        public IncursionEnemyRocketRaccoon(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "RocketRaccoon Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/CosmicGear.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/DeepSpace.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/GGMarvelNow.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/GotGMovie.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/GotGMovieVol2.prototype", true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/MarvelNOW.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/Modern.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/OfficeAttire.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/OriginalGreen.prototype", true),
            new("Entity/Items/Costumes/Prototypes/RocketRaccoon/Symbiote.prototype",      true),
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
            new("Powers/CharacterSelectOnly/HeroProfilePowerRocketRaccoon.prototype",           true,  0.05f),
            new("Powers/Player/RocketRaccoon/Burrow.prototype",                                 true,  0.05f),
            new("Powers/Player/RocketRaccoon/GravityMine.prototype",                            true,  0.0915f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Groot.prototype",                                  true,  0.0665f), // 2026-06-16
            new("Powers/Player/RocketRaccoon/GrootRide.prototype",                              true,  0.0320f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/ArcTurret.prototype",                       true,  0.1126f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/BFG.prototype",                             true,  0.0316f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/BasicPistols.prototype",                    true,  0.1270f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/BasicRifle.prototype",                      true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/ChargeBeam.prototype",                      true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/DisengagingShot.prototype",                 true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/FlashGrenade.prototype",                    true,  0.0758f), // 2026-06-16
            new("Powers/Player/RocketRaccoon/Rework/GunTurret.prototype",                       true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/JetDash.prototype",                         true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/Minigun.prototype",                         true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/NewSigHadronEnforcer.prototype",            true,  0.0124f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/PassiveShieldHiddenPassive.prototype",      false, 0.05f),
            new("Powers/Player/RocketRaccoon/Rework/PassiveShieldRegenHiddenPassive.prototype", false, 0.05f),
            new("Powers/Player/RocketRaccoon/Rework/PlasmaCannon.prototype",                    true,  0.0951f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/PlasmaCannonLarger.prototype",              true,  0.0951f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/ShieldBoost.prototype",                     true,  0.05f),
            new("Powers/Player/RocketRaccoon/Rework/SignatureNuke.prototype",                   true,  0.02f),
            new("Powers/Player/RocketRaccoon/Rework/UltimateChanneledBeam.prototype",           true,  0.0154f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/UltimateDeathFromAbove.prototype",          true,  0.0197f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Rework/UltimateFuriousLunge.prototype",            true,  0.006f),
            new("Powers/Player/RocketRaccoon/Rework/UltimateMissileLauncher.prototype",         true,  0.006f),
            new("Powers/Player/RocketRaccoon/Rework/WarpTurret.prototype",                      true,  0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent1GrootRide.prototype",               false, 0.0320f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Talents/Talent1HealingSpores.prototype",           false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent1IAmGroot.prototype",                false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent2H7FleetslayerBuffs.prototype",      false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent2HeavyGaussBuffs.prototype",         false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent2PlasmaCannonBuff.prototype",        false, 0.0951f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Talents/Talent3Burrow.prototype",                  false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent3ShieldBoostHealthRegen.prototype",  false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent3WarpTurretBonus.prototype",         false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent4ArcTurretLightning.prototype",      false, 0.1126f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Talents/Talent4HadronEnforcerCharges.prototype",   false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent4MinigunBuff.prototype",             false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent5BFGBuff.prototype",                 false, 0.0316f), // 2026-06-17
            new("Powers/Player/RocketRaccoon/Talents/Talent5BasicRifleBuff.prototype",          false, 0.05f),
            new("Powers/Player/RocketRaccoon/Talents/Talent5Grenades.prototype",                false, 0.01f),
            new("Powers/Player/RocketRaccoon/Traits/DefenseTrait.prototype",                    false, 0.05f),
            new("Powers/Player/RocketRaccoon/Traits/MechanicTraitAmmoShields.prototype",        false, 0.05f),
            new("Powers/Player/RocketRaccoon/Traits/OffenseTrait.prototype",                    false, 0.05f),
            new("Powers/Player/RocketRaccoon/Ultimate.prototype",                               true,  0.0154f), // 2026-06-17
            new("Powers/StolenPowers/StealablePowers/RocketRaccoonStolenPower.prototype",       false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                             false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                     false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                        false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                           false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                 false, 0.05f),
            new("Powers/Player/TravelPower/RocketRacoonFlight.prototype",                       false, 0.05f),
        };
    }
}
