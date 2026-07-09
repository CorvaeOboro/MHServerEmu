using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Starlord Invader
    /// Powers: 16 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyStarlord : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Starlord.prototype");

        public IncursionEnemyStarlord(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Starlord Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Starlord/Conquest.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Starlord/ConquestRed.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Starlord/CosmicAvenger.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Starlord/CosmicAvengerHelmet.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Starlord/GotGMovie.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Starlord/GotGMovie2.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Starlord/Legendary.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Starlord/MarvelNOW.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Starlord/MovieHeadphones.prototype",     true),
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
            new("Powers/Player/Starlord/Air.prototype",                                                true,  0.1428f), // 2026-06-10
            new("Powers/Player/Starlord/BasicEGun.prototype",                                          true,  0.1054f), // 2026-06-16
            new("Powers/Player/Starlord/Bulletspray.prototype",                                        true,  0.0385f), // 2026-06-10
            new("Powers/Player/Starlord/ChargedEGun.prototype",                                        true,  0.0557f), // 2026-06-07
            new("Powers/Player/Starlord/ClusterBomb.prototype",                                        true,  0.1641f), // 2026-06-10
            new("Powers/Player/Starlord/DisengagingShot.prototype",                                    true,  0.0961f), // 2026-06-07
            new("Powers/Player/Starlord/Earth.prototype",                                              true,  0.0652f), // 2026-06-10
            new("Powers/Player/Starlord/Fire.prototype",                                               true,  0.1370f), // 2026-06-10
            new("Powers/Player/Starlord/FreezeGrenade.prototype",                                      true,  0.0967f), // 2026-06-10
            new("Powers/Player/Starlord/GravityGrenade.prototype",                                     true,  0.0972f), // 2026-06-05
            new("Powers/Player/Starlord/Lunge.prototype",                                              true,  0.1964f), // 2026-06-10
            new("Powers/Player/Starlord/RapidEGun.prototype",                                          true,  0.1876f), // 2026-06-07
            new("Powers/Player/Starlord/SigOrbitalStrikeSummon.prototype",                             true,  0.02f),
            new("Powers/Player/Starlord/Strafe.prototype",                                             true,  0.1202f), // 2026-06-07
            new("Powers/Player/Starlord/Talents/Talent1ChargedEGunDamageMult.prototype",               false, 0.0557f), // 2026-06-07
            new("Powers/Player/Starlord/Talents/Talent1EElementalAmmoRemoval.prototype",               false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent1ElementalAmmoUseBuff.prototype",                false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent2ClusterBombRemap.prototype",                    false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent2GrenadeDamageMultWithAmmoWaterEarth.prototype", false, 0.0652f), // 2026-06-10
            new("Powers/Player/Starlord/Talents/Talent2GunDamageCritBonus.prototype",                  false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent3StrafeCDR.prototype",                           false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent3StrafeProc.prototype",                          false, 0.025f),
            new("Powers/Player/Starlord/Talents/Talent3StrafeRemap.prototype",                         false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent4GrenadeDamageAndCDIncrease.prototype",          false, 0.01f),
            new("Powers/Player/Starlord/Talents/Talent4GrenadeDamageMultWithAmmoAirFire.prototype",    false, 0.01f),
            new("Powers/Player/Starlord/Talents/Talent4OrbitalStrikeCDR.prototype",                    false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent5AmmoSharedCooldown.prototype",                  false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent5DisengagingShotRemap.prototype",                false, 0.05f),
            new("Powers/Player/Starlord/Talents/Talent5GrenadeCDROnLunge.prototype",                   false, 0.01f),
            new("Powers/Player/Starlord/Traits/DefenseTrait.prototype",                                false, 0.05f),
            new("Powers/Player/Starlord/Traits/MechanicTraitAmmo.prototype",                           false, 0.05f),
            new("Powers/Player/Starlord/Traits/OffenseTrait.prototype",                                false, 0.05f),
            new("Powers/Player/Starlord/UltCube.prototype",                                            true,  0.0119f), // 2026-06-18
            new("Powers/Player/Starlord/Water.prototype",                                              true,  0.0837f), // 2026-06-10
            new("Powers/Player/TravelPower/StarlordFlight.prototype",                                  false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/StarLordStolenPower.prototype",                   false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                             false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                    false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                            false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                               false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                                  false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                        false, 0.05f),
        };
    }
}
