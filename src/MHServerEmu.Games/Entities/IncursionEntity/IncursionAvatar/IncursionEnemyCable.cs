using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Cable Invader
    /// Powers: 21 / 48
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyCable : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Cable.prototype");

        public IncursionEnemyCable(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Cable Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Cable/ArmoredCyborg.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Cable/Bjorn.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Cable/BlackOps.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Cable/ClassicXForce.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Cable/MarvelNOW.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Cable/Modern.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Cable/Original.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Cable/Technovirus.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Cable/XForceLegendary.prototype", true),
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
            new("Powers/Player/Cable/ConcussionBlast.prototype",                  true,  0.0803f), // 2026-06-17
            new("Powers/Player/Cable/EnergyPulse.prototype",                      true,  0.2037f), // 2026-06-17
            new("Powers/Player/Cable/EyeForWeakness.prototype",                   true,  0.05f),
            new("Powers/Player/Cable/FutureBomb.prototype",                       true,  0.0397f), // 2026-06-10
            new("Powers/Player/Cable/KineticBarrier.prototype",                   true,  0.05f),
            new("Powers/Player/Cable/ParticleAccelerator.prototype",              true,  0.1017f), // 2026-06-17
            new("Powers/Player/Cable/PlasmaBarrage.prototype",                    true,  0.0620f), // 2026-06-17
            new("Powers/Player/Cable/PsimitarCyclone.prototype",                  true,  0.0861f), // 2026-06-17
            new("Powers/Player/Cable/PsimitarImpale.prototype",                   true,  0.0493f), // 2026-06-17
            new("Powers/Player/Cable/PsimitarLunge.prototype",                    true,  0.1409f), // 2026-06-17
            new("Powers/Player/Cable/PsimitarLungeHiddenPassive.prototype",       false, 0.1409f), // 2026-06-17
            new("Powers/Player/Cable/PsimitarWaves.prototype",                    true,  0.0739f), // 2026-06-16
            new("Powers/Player/Cable/PsychicBullets.prototype",                   true,  0.1689f), // 2026-06-16
            new("Powers/Player/Cable/PsychicHaze.prototype",                      true,  0.1009f), // 2026-06-17
            new("Powers/Player/Cable/PulseBolt.prototype",                        true,  0.1313f), // 2026-06-17
            new("Powers/Player/Cable/TKOverload.prototype",                       true,  0.0724f), // 2026-06-17
            new("Powers/Player/Cable/TKSpearSlam.prototype",                      true,  0.0345f), // 2026-06-17
            new("Powers/Player/Cable/Talents/ConcussionBlastLayer.prototype",     false, 0.0803f), // 2026-06-17
            new("Powers/Player/Cable/Talents/IllusionLayer.prototype",            false, 0.05f),
            new("Powers/Player/Cable/Talents/ImpaleLayer.prototype",              false, 0.05f),
            new("Powers/Player/Cable/Talents/KineticRepulsion.prototype",         false, 0.05f),
            new("Powers/Player/Cable/Talents/MindBarrierLayer.prototype",         false, 0.05f),
            new("Powers/Player/Cable/Talents/ParticleAcceleratorBuff.prototype",  false, 0.1017f), // 2026-06-17
            new("Powers/Player/Cable/Talents/PsychicHazeLayer.prototype",         false, 0.1009f), // 2026-06-17
            new("Powers/Player/Cable/Talents/SweepLayer.prototype",               false, 0.05f),
            new("Powers/Player/Cable/Talents/SwiftLungeLayer.prototype",          false, 0.05f),
            new("Powers/Player/Cable/Talents/TKOverloadBuff.prototype",           false, 0.0724f), // 2026-06-17
            new("Powers/Player/Cable/Talents/TKSpearSlamBuff.prototype",          false, 0.0345f), // 2026-06-17
            new("Powers/Player/Cable/Talents/TechnoOrganicInterface.prototype",   false, 0.05f),
            new("Powers/Player/Cable/Talents/TechnoOrganicSoldier.prototype",     false, 0.05f),
            new("Powers/Player/Cable/Talents/ViperBeamLayer.prototype",           false, 0.0293f), // 2026-06-16
            new("Powers/Player/Cable/Talents/VortexGrenadeLayer.prototype",       false, 0.0698f), // 2026-06-16
            new("Powers/Player/Cable/TelepathicIllusion.prototype",               true,  0.05f),
            new("Powers/Player/Cable/Teleport.prototype",                         true,  0.05f),
            new("Powers/Player/Cable/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Cable/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Cable/Ultimate.prototype",                         true,  0.006f),
            new("Powers/Player/Cable/UltimateHiddenPassive.prototype",            false, 0.006f),
            new("Powers/Player/Cable/ViperBeam.prototype",                        true,  0.0293f), // 2026-06-16
            new("Powers/Player/Cable/VortexGrenade.prototype",                    true,  0.0698f), // 2026-06-16
            new("Powers/Player/TravelPower/CableSprint.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CableStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                   false, 0.05f),
        };
    }
}
