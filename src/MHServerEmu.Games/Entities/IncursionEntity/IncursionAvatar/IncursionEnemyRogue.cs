using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Rogue Invader
    /// Powers: 123 / 338
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyRogue : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Rogue.prototype");

        public IncursionEnemyRogue(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Rogue Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Rogue/AgeOfX.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Rogue/Classic90s.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Rogue/Marvel1602.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Rogue/Modern.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Rogue/SavageLand.prototype", true),
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
            new("Powers/Player/Rogue/Charge.prototype",                                        true,  0.05f),
            new("Powers/Player/Rogue/DiveBomb.prototype",                                      true,  0.0480f), // 2026-06-20
            new("Powers/Player/Rogue/DrainLife.prototype",                                     true,  0.0291f), // 2026-06-17
            new("Powers/Player/Rogue/DrainPunch.prototype",                                    true,  0.1095f), // 2026-06-09
            new("Powers/Player/Rogue/ExtremeDrain.prototype",                                  true,  0.0417f), // 2026-06-10
            new("Powers/Player/Rogue/GlovesOff.prototype",                                     true,  0.05f),
            new("Powers/Player/Rogue/Haymaker.prototype",                                      true,  0.0858f), // 2026-06-17
            new("Powers/Player/Rogue/RapidPunchDash.prototype",                                true,  0.0328f), // 2026-06-18
            new("Powers/Player/Rogue/RecallOverload.prototype",                                true,  0.0209f), // 2026-06-17
            new("Powers/Player/Rogue/RecallOverloadMental.prototype",                          true,  0.0249f), // 2026-06-20
            new("Powers/Player/Rogue/RecallOverloadPhysical.prototype",                        true,  0.0426f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowerLibrarySlot1.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot2.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot3.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot4.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot5.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot6.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot7.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowerLibrarySlot8.prototype",                       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/AngelDeathFromAbove.prototype",              true,  0.0424f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/ArachneBouncingWeb.prototype",               true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/BetaRayBillLightningBarrage.prototype",      true,  0.0372f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowers/BigLimboDemonShockwave.prototype",           true,  0.0545f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowers/BlackBoltWhisper.prototype",                 true,  0.0393f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowers/BlackPantherSweepingKick.prototype",         true,  0.0427f), // 2026-06-10
            new("Powers/Player/Rogue/StolenPowers/BlackWidowTumble.prototype",                 true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/BladeBloodlust.prototype",                   true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/BlobBellyFlop.prototype",                    true,  0.0420f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowers/BrevikCowbell.prototype",                    true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/BrimstoneMeteorStrike.prototype",            true,  0.0437f), // 2026-06-17
            new("Powers/Player/Rogue/StolenPowers/CableKineticBarrier.prototype",              true,  0.1023f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/CleaSummonFlames.prototype",                 true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/ColossusMetalSkin.prototype",                true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/CyclopsBouncingBeam.prototype",              true,  0.0811f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/DoctorStrangeFangNuke.prototype",            true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/DoctorStrangeFangNukeHiddenPassi.prototype", true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/DrDoomBallLightning.prototype",              true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/ElectroElementalStorm.prototype",            true,  0.03f),
            new("Powers/Player/Rogue/StolenPowers/ElektraShadowStrike.prototype",              true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/ElektraShadowStrikeHiddenPassi.prototype",   true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/EmmaFrostControlMob.prototype",              true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/FireGiantBossExplosion.prototype",           true,  0.01f),
            new("Powers/Player/Rogue/StolenPowers/FirestarEnergyRainStart.prototype",          true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/FlametosserBossFireBreath.prototype",        true,  0.1362f), // 2026-06-09
            new("Powers/Player/Rogue/StolenPowers/FrostGiantFrostNova.prototype",              true,  0.01f),
            new("Powers/Player/Rogue/StolenPowers/GambitRaginCajun.prototype",                 true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/GambitRaginCajunHiddenPassive.prototype",    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/GhostRiderFireBreath.prototype",             true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/GorgonStoneGaze.prototype",                  true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/GrimReaperEnergyBlast.prototype",            true,  0.137f), // 2026-06-09
            new("Powers/Player/Rogue/StolenPowers/GrootOut.prototype",                         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/HavokConeShot.prototype",                    true,  0.0658f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/HellfireDoTAura.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/HowardTheDuckDeathPunch.prototype",          true,  0.0434f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/HulkSmash.prototype",                        true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/HumanTorchNovaBurst.prototype",              true,  0.0727f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/HydeDirectedShockwave.prototype",            true,  0.03f),
            new("Powers/Player/Rogue/StolenPowers/IcemanIceGolem.prototype",                   true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/InvisibleWomanInvisibility.prototype",       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/IronFistStanceSwappingSteroid.prototype",    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/JeanGreyPullTowardsPoint.prototype",         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/JessicaJonesThrowConcrete.prototype",        true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/JubileeBoom.prototype",                      true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/JuggernautImInvulnerable.prototype",         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/KaeciliusHealChannel.prototype",             true,  0.025f),
            new("Powers/Player/Rogue/StolenPowers/KirigiVanish.prototype",                     true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/KittyPrydeDeathFromBelow.prototype",         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/KronanArcanistSummonHotspot.prototype",      true,  0.008f),
            new("Powers/Player/Rogue/StolenPowers/LimboDemonBossTeleport.prototype",           true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/LivingLaserLaserBlast.prototype",            true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/LokiIllusionRush.prototype",                 true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/LukeCagePummel.prototype",                   true,  0.0597f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/MODOKPsychicShockwave.prototype",            true,  0.03f),
            new("Powers/Player/Rogue/StolenPowers/MagikSummonDemons.prototype",                true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MagnetoAllIn.prototype",                     true,  0.0456f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/MalekithDarkBeam.prototype",                 true,  0.0254f), // 2026-06-18
            new("Powers/Player/Rogue/StolenPowers/ManApeBeatChest.prototype",                  true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MandarinElectricStorm.prototype",            true,  0.03f),
            new("Powers/Player/Rogue/StolenPowers/MedusaAutoSlap.prototype",                   true,  0.0850f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/MilesMoralesInvisSteroid.prototype",         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MindlessOneBeam.prototype",                  true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MistressOfMagmaMentalBlast.prototype",       true,  0.0746f), // 2026-06-09
            new("Powers/Player/Rogue/StolenPowers/MoleManSummonMoloids.prototype",             true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MordoMistsDoT.prototype",                    true,  0.0631f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/MrFantasticConeRapidPunch.prototype",        true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MrSinisterAstralProjection.prototype",       true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MsMarvelConeRapidPunch.prototype",           true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/MsMarvelPhotonicWave.prototype",             true,  0.1120f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/NastirhHealingShield.prototype",             true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/NickFuryPetSteroid.prototype",               true,  0.02f),
            new("Powers/Player/Rogue/StolenPowers/NightcrawlerValiantLeap.prototype",          true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/NovaBlastoff.prototype",                     true,  0.01f),
            new("Powers/Player/Rogue/StolenPowers/OnslaughtSummonMentalOrb.prototype",         true,  0.02f),
            new("Powers/Player/Rogue/StolenPowers/PassiveAngela.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveAntMan.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveBatroc.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveBeast.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveBlackCat.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveBullseye.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveCaptainAmerica.prototype",            false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveCarnage.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveCosmicDoop.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveCoulson.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveCrossbones.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveDaredevil.prototype",                 false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveDeadpool.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveDocOc.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveDomino.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveDrax.prototype",                      false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveFalcon.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveGamora.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveGreenGoblin.prototype",               false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveHawkeye.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveHood.prototype",                      false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveHydraAgent.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveIronMan.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveKingpin.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveKraven.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveKurse.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveLadyDeathstrike.prototype",           false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveLizard.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveMadameHydra.prototype",               false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveMoonKnight.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassivePunisher.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassivePyro.prototype",                      false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveRedSkull.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveRescue.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveRockTrollBerserker.prototype",        false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveRocketRaccoon.prototype",             false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveSabretooth.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveShieldAgent.prototype",               false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveShocker.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveSpiderGwen.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveSpiderman.prototype",                 false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveStarLord.prototype",                  false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveTaskmaster.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveThing.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveTombstone.prototype",                 false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveVenom.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveWarMachine.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveWinterSoldier.prototype",             false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PassiveWizard.prototype",                    false, 0.05f),
            new("Powers/Player/Rogue/StolenPowers/PsylockeLunge.prototype",                    true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/QuakeBeam.prototype",                        true,  0.03f),
            new("Powers/Player/Rogue/StolenPowers/QuicksilverSuperSonicCyclone.prototype",     true,  0.1063f), // 2026-06-09
            new("Powers/Player/Rogue/StolenPowers/RhinoBigCharge.prototype",                   true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/RobbieReyesDriveBy.prototype",               true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SauronSwoopingFlames.prototype",             true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/ScarletWitchShadowBolt.prototype",           true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SheHulkLawyerUp.prototype",                  true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SilverSurferChanneledBeam.prototype",        true,  0.025f),
            new("Powers/Player/Rogue/StolenPowers/SlagFireMeteor.prototype",                   true,  0.01f),
            new("Powers/Player/Rogue/StolenPowers/SpiderwomanVenomBlast.prototype",            true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SquirrelGirlSquirrelPets.prototype",         true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/StormHailstorm.prototype",                   true,  0.0569f), // 2026-06-18
            new("Powers/Player/Rogue/StolenPowers/SunspotPunch.prototype",                     true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SuperSkrullWhirlwind.prototype",             true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/SurturSwordAttack.prototype",                true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/ThorGodlyValor.prototype",                   true,  0.0467f), // 2026-06-20
            new("Powers/Player/Rogue/StolenPowers/ToadTongueYank.prototype",                   true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/WaspBiospray.prototype",                     true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/WolverineBasicRonin.prototype",              true,  0.05f),
            new("Powers/Player/Rogue/StolenPowers/X23CrimsonCircle.prototype",                 true,  0.05f),
            new("Powers/Player/Rogue/Talents/GlovesOffAuto.prototype",                         false, 0.05f),
            new("Powers/Player/Rogue/Talents/HealthDefenseBuff.prototype",                     false, 0.05f),
            new("Powers/Player/Rogue/Talents/NonStolenPowersBuff.prototype",                   false, 0.05f),
            new("Powers/Player/Rogue/Talents/RapidPunchDashCharges.prototype",                 false, 0.0328f), // 2026-06-18
            new("Powers/Player/Rogue/Talents/RecallOverloadCooldown.prototype",                false, 0.0209f), // 2026-06-17
            new("Powers/Player/Rogue/Talents/RecallOverloadMental.prototype",                  false, 0.0209f), // 2026-06-17
            new("Powers/Player/Rogue/Talents/RecallOverloadPhysical.prototype",                false, 0.0209f), // 2026-06-17
            new("Powers/Player/Rogue/Talents/StolenPowersBuff.prototype",                      false, 0.05f),
            new("Powers/Player/Rogue/Talents/SuperDrains.prototype",                           false, 0.05f),
            new("Powers/Player/Rogue/Taunt.prototype",                                         true,  0.05f),
            new("Powers/Player/Rogue/Traits/DefenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/Rogue/Traits/OffenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/Rogue/Traits/StolenPassivePowerSlot1.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/Traits/StolenPassivePowerSlot2.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/Traits/StolenPassivePowerSlot3.prototype",                false, 0.05f),
            new("Powers/Player/Rogue/UltimateBamf.prototype",                                  true,  0.006f),
            new("Powers/Player/Rogue/UltimateBasicSlash.prototype",                            true,  0.006f),
            new("Powers/Player/Rogue/UltimateDashSlash.prototype",                             true,  0.006f),
            new("Powers/Player/Rogue/UltimateMetalRegeneration.prototype",                     true,  0.006f),
            new("Powers/Player/Rogue/UltimateRaginCajunTooltip.prototype",                     true,  0.006f),
            new("Powers/Player/Rogue/UltimateSeekerButterflies.prototype",                     true,  0.0158f), // 2026-06-18
            new("Powers/Player/Rogue/UltimateSignatureBamf.prototype",                         true,  0.0217f), // 2026-06-20
            new("Powers/Player/Rogue/UltimateSwordFlurryStart.prototype",                      true,  0.006f),
            new("Powers/Player/Rogue/UltimateTransform.prototype",                             true,  0.006f),
            new("Powers/Player/Rogue/Uppercut.prototype",                                      true,  0.1023f), // 2026-06-20
            new("Powers/Player/TravelPower/RogueFlight.prototype",                             false, 0.05f),
            new("Powers/SynergyPowers/SynergyRogueSpiritOnHit.prototype",                      true,  0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                            false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                    false, 0.05f),
            new("Powers/Player/EmmaFrost/Update/ControlledMobHiddenPassive.prototype",         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                       false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                          false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/AngelStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/AngelaStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/AntManStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ArachneStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BatrocStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BeastStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BetaRayBillStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BigLimboDemonStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackBoltStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackCatStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackPantherStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackWidowStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BladeStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlobStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BrevikStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BrimstoneStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BullseyeStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CableStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CaptainAmericaStolenPower.prototype",     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CarnageStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CleaStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ColossusStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CosmicDoopStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CoulsonStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CrossbonesStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CyclopsStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DaredevilStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DeadpoolStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DocOcStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DoctorStrangeStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DominoStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DrDoomStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DraxStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ElectroStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ElektraStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/EmmaFrostStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/FalconStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/FireGiantBossStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/FirestarStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/FlametosserBossStolenPower.prototype",    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/FrostGiantBossStolenPower.prototype",     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GambitStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GamoraStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GhostRiderStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GorgonStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GreenGoblinStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GrimReaperStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GrootStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HavokStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HawkeyeStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HellfireStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HoodStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HowardTheDuckStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HulkStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HumanTorchStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HydeStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HydraAgentStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IcemanStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/InvisibleWomanStolenPower.prototype",     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IronFistStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IronManStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JeanGreyStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JessicaJonesStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JubileeStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JuggernautStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KaeciliusStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KamalaKhanStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KingpinStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KirigiStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KittyPrydeStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KravenStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KronanBossStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KurseStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LadyDeathstrikeStolenPower.prototype",    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LimboDemonBossStolenPower.prototype",     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LivingLaserStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LizardStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LokiStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LukeCageStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MODOKStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MadameHydraStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MagikStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MagnetoStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MalekithStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ManApeStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MandarinStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MedusaStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MilesMoralesStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MindlessOneStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MistressofMagmaStolenPower.prototype",    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MoleManStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MoonKnightStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MordoStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MrFantasticStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MrSinisterStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MsMarvelStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NastirhStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NickFuryStolenPower.prototype",           false, 0.02f),
            new("Powers/StolenPowers/StealablePowers/NightcrawlerStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NovaStolenPower.prototype",               false, 0.01f),
            new("Powers/StolenPowers/StealablePowers/OnslaughtStolenPower.prototype",          false, 0.02f),
            new("Powers/StolenPowers/StealablePowers/PsylockeStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/PunisherStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/PyroStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/QuakeStolenPower.prototype",              false, 0.03f),
            new("Powers/StolenPowers/StealablePowers/QuicksilverStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RedSkullStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RescueStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RhinoStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RobbieReyesStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RockTrollBossStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/RocketRaccoonStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SabertoothStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SauronStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ScarletWitchStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SheHulkStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ShieldAgentStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ShockerStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SilverSurferStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SlagStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SpiderGwenStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SpidermanStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SpiderwomanStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SquirrelGirlStolenPower.prototype",       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/StarLordStolenPower.prototype",           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/StormStolenPower.prototype",              false, 0.03f),
            new("Powers/StolenPowers/StealablePowers/SunspotStolenPower.prototype",            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SuperSkrullStolenPower.prototype",        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SurturStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/TaskmasterStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ThingStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ThorStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ToadStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/TombstoneStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/VenomStolenPower.prototype",              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/WarMachineStolenPower.prototype",         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/WaspStolenPower.prototype",               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/WinterSoldierStolenPower.prototype",      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/WizardStolenPower.prototype",             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/WolverineStolenPower.prototype",          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/X23StolenPower.prototype",                false, 0.05f),
        };
    }
}
