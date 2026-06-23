using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// SilverSurfer Invader
    /// Powers: 16 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemySilverSurfer : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/SilverSurfer.prototype");

        public IncursionEnemySilverSurfer(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "SilverSurfer Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/SilverSurfer/Classic.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/SilverSurfer/Exiles.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/SilverSurfer/Keeper.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/SilverSurfer/SilverSavage.prototype", true),
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
            new("Powers/Player/SilverSurfer/BasicBeam.prototype",                        true,  0.1613f), // 2026-06-20
            new("Powers/Player/SilverSurfer/BasicBouncingBeam.prototype",                true,  0.1048f), // 2026-06-17
            new("Powers/Player/SilverSurfer/BigBeam.prototype",                          true,  0.1564f), // 2026-06-17
            new("Powers/Player/SilverSurfer/BlackHole.prototype",                        true,  0.0254f), // 2026-06-20
            new("Powers/Player/SilverSurfer/BoardDash.prototype",                        true,  0.1041f), // 2026-06-20
            new("Powers/Player/SilverSurfer/BoardSweep.prototype",                       true,  0.0483f), // 2026-06-17
            new("Powers/Player/SilverSurfer/ChanneledBeam.prototype",                    true,  0.0902f), // 2026-06-20
            new("Powers/Player/SilverSurfer/CosmicRift.prototype",                       true,  0.0981f), // 2026-06-17
            new("Powers/Player/SilverSurfer/DeathFromBelow.prototype",                   true,  0.0386f), // 2026-06-16
            new("Powers/Player/SilverSurfer/Deconstruction.prototype",                   true,  0.1033f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Disengage.prototype",                        true,  0.0563f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Reconstruction.prototype",                   true,  0.05f),
            new("Powers/Player/SilverSurfer/Singularity.prototype",                      true,  0.0477f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/AutoReconstruct.prototype",          false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/AutoShield.prototype",               false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/AutoSingularity.prototype",          false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/BasicBeamsBuffs.prototype",          false, 0.1613f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/BigBeamLayer.prototype",             false, 0.1564f), // 2026-06-17
            new("Powers/Player/SilverSurfer/Talents/BlackHoleExplosion.prototype",       false, 0.0254f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/BlackHoleInstagib.prototype",        false, 0.0254f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/BlackHoleSurferBuff.prototype",      false, 0.0254f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/ChanneledBeamStack.prototype",       false, 0.0902f), // 2026-06-20
            new("Powers/Player/SilverSurfer/Talents/CosmicGift.prototype",               false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/CosmicWake.prototype",               false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/Microbeams.prototype",               false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/MovementPowerCDR.prototype",         false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/PowerCosmicRegen.prototype",         false, 0.05f),
            new("Powers/Player/SilverSurfer/Talents/ReconstructDeconstruct.prototype",   false, 0.05f),
            new("Powers/Player/SilverSurfer/TeleportDash.prototype",                     true,  0.1045f), // 2026-06-20
            new("Powers/Player/SilverSurfer/TimeWarp.prototype",                         true,  0.05f),
            new("Powers/Player/SilverSurfer/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/SilverSurfer/Traits/MechanicTraitPowerCosmic.prototype",  false, 0.05f),
            new("Powers/Player/SilverSurfer/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/SilverSurfer/Ultimate.prototype",                         true,  0.0052f), // 2026-06-20
            new("Powers/Player/SilverSurfer/UltimateHiddenPassive.prototype",            false, 0.0052f), // 2026-06-20
            new("Powers/Player/TravelPower/SilverSurferFlight.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SilverSurferStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",              false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                 false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                    false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                          false, 0.05f),
        };
    }
}
