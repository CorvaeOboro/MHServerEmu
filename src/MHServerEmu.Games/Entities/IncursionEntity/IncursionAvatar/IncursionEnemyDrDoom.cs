using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// DrDoom Invader
    /// Powers: 21 / 64
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyDrDoom : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/DrDoom.prototype");

        public IncursionEnemyDrDoom(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "DrDoom Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/DrDoom/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/DrDoom/Doom2099.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/DrDoom/FutureFoundation.prototype", true),
            new("Entity/Items/Costumes/Prototypes/DrDoom/GodEmperor.prototype",       true),
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
            new("Powers/Player/DrDoom/AirStrike.prototype",                                 true,  0.1222f), // 2026-06-05
            new("Powers/Player/DrDoom/AoEDebuff.prototype",                                 false, 0.03f),
            new("Powers/Player/DrDoom/BallLightning.prototype",                             true,  0.0574f), // 2026-06-13
            new("Powers/Player/DrDoom/BasicPunch.prototype",                                true,  0.0685f), // 2026-06-18
            new("Powers/Player/DrDoom/ChanneledBeam.prototype",                             true,  0.0506f), // 2026-06-18
            new("Powers/Player/DrDoom/ConcussiveBlasts.prototype",                          true,  0.0967f), // 2026-06-05
            new("Powers/Player/DrDoom/DiplomaticImmunity.prototype",                        true,  0.05f),
            new("Powers/Player/DrDoom/DoomBots.prototype",                                  true,  0.1598f), // 2026-06-17
            new("Powers/Player/DrDoom/DoombotBlockadeCallIn.prototype",                     true,  0.1018f), // 2026-06-18
            new("Powers/Player/DrDoom/DoombotInfernoCallIn.prototype",                      true,  0.05f),
            new("Powers/Player/DrDoom/DoombotThumperCallIn.prototype",                      true,  0.1633f), // 2026-06-17
            new("Powers/Player/DrDoom/DoomsDay.prototype",                                  true,  0.2025f), // 2026-06-17
            new("Powers/Player/DrDoom/FingerLasers.prototype",                              true,  0.0137f), // 2026-06-17
            new("Powers/Player/DrDoom/FootDive.prototype",                                  true,  0.0450f), // 2026-06-18
            new("Powers/Player/DrDoom/GroundSmash.prototype",                               true,  0.0887f), // 2026-06-18
            new("Powers/Player/DrDoom/MagicLance.prototype",                                true,  0.1559f), // 2026-06-05
            new("Powers/Player/DrDoom/MagicOrbSummon.prototype",                            true,  0.0552f), // 2026-06-18
            new("Powers/Player/DrDoom/Missiles.prototype",                                  true,  0.1576f), // 2026-06-17
            new("Powers/Player/DrDoom/RapidFire.prototype",                                 true,  0.4710f), // 2026-06-05
            new("Powers/Player/DrDoom/Repulsors.prototype",                                 true,  0.1578f), // 2026-06-18
            new("Powers/Player/DrDoom/Talents/Talent1DoombotFlyers.prototype",              false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent1MagicLanceBasicPunch.prototype",       false, 0.0685f), // 2026-06-18
            new("Powers/Player/DrDoom/Talents/Talent1MagicOrbSmart.prototype",              false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent2CrumbleFools.prototype",               false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent2DoomBotsBonus.prototype",              false, 0.1598f), // 2026-06-17
            new("Powers/Player/DrDoom/Talents/Talent2EldritchReckoning.prototype",          false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent3ChanneledTormentBonus.prototype",      false, 0.025f),
            new("Powers/Player/DrDoom/Talents/Talent3DoomBurstBonus.prototype",             false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent3RobotSummonBonus.prototype",           false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent4LordSignatureRemap.prototype",         false, 0.02f),
            new("Powers/Player/DrDoom/Talents/Talent4MagicSignature.prototype",             false, 0.02f),
            new("Powers/Player/DrDoom/Talents/Talent4TechnicalTyrantCooldownRed.prototype", false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent5AllMagic.prototype",                   false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent5AllPower.prototype",                   false, 0.05f),
            new("Powers/Player/DrDoom/Talents/Talent5Hybrid.prototype",                     false, 0.05f),
            new("Powers/Player/DrDoom/Teleport.prototype",                                  true,  0.05f),
            new("Powers/Player/DrDoom/Traits/DefenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/DrDoom/Traits/MechanicTraitPowerMagic.prototype",            false, 0.05f),
            new("Powers/Player/DrDoom/Traits/OffenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/DrDoom/UltimateHiddenPassive.prototype",                     false, 0.006f),
            new("Powers/Player/DrDoom/UnworthyPistol.prototype",                            true,  0.0407f), // 2026-06-17
            new("Powers/Player/TravelPower/DrDoomFlight.prototype",                         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DrDoomStolenPower.prototype",          false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/AstralFormBonus.prototype",            false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/AstralLegion.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/AstralProjectionTwincast.prototype",   false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/FireAndIce.prototype",                 false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/LightAndDarkness.prototype",           false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismDamagePulse.prototype",       false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismNoReset.prototype",           false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismRestoration.prototype",       false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/RitualCircle.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SealOfHoggoth.prototype",              false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SealOfOshtur.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SevenRings.prototype",                 false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/ShieldOfSeraphimAutoshield.prototype", false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/ShieldOfSeraphimTeamBuff.prototype",   false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/WindAndLightning.prototype",           false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
