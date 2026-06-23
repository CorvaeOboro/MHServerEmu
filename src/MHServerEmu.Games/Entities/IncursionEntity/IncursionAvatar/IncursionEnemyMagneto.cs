using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Magneto Invader
    /// Powers: 17 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyMagneto : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Magneto.prototype");

        public IncursionEnemyMagneto(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Magneto Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Magneto/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Magneto/MarvelNOWBlack.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Magneto/MarvelNOWWhite.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Magneto/UncannyAvengers.prototype", true),
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
            new("Powers/Player/Magneto/AllIn.prototype",                                 true,  0.0673f), // 2026-06-17
            new("Powers/Player/Magneto/ChanneledCone.prototype",                         true,  0.0679f), // 2026-06-20
            new("Powers/Player/Magneto/DebrisCrush.prototype",                           true,  0.0690f), // 2026-06-06
            new("Powers/Player/Magneto/DebrisShot.prototype",                            true,  0.3680f), // 2026-06-20
            new("Powers/Player/Magneto/ElectromagneticBeam.prototype",                   true,  0.0827f), // 2026-06-17
            new("Powers/Player/Magneto/ElectromagneticShockwave.prototype",              true,  0.0705f), // 2026-06-17
            new("Powers/Player/Magneto/ForceField.prototype",                            true,  0.05f),
            new("Powers/Player/Magneto/HomingBlast.prototype",                           true,  0.1783f), // 2026-06-20
            new("Powers/Player/Magneto/Implosion.prototype",                             true,  0.1082f), // 2026-06-20
            new("Powers/Player/Magneto/Lunge.prototype",                                 true,  0.1404f), // 2026-06-20
            new("Powers/Player/Magneto/MagneticSphere.prototype",                        true,  0.1444f), // 2026-06-20
            new("Powers/Player/Magneto/MetalCage.prototype",                             true,  0.0880f), // 2026-06-17
            new("Powers/Player/Magneto/MetalObjectSmash.prototype",                      true,  0.0878f), // 2026-06-05
            new("Powers/Player/Magneto/ShrapnelCone.prototype",                          true,  0.1394f), // 2026-06-06
            new("Powers/Player/Magneto/ShrapnelHotspot.prototype",                       true,  0.1342f), // 2026-06-06
            new("Powers/Player/Magneto/SignatureMaelstrom.prototype",                    true,  0.0092f), // 2026-06-20
            new("Powers/Player/Magneto/SpawnMetalOrbHiddenPassive.prototype",            false, 0.05f),
            new("Powers/Player/Magneto/Talents/AllInPickupBuff.prototype",               false, 0.0673f), // 2026-06-17
            new("Powers/Player/Magneto/Talents/AutoDebrisCrush.prototype",               false, 0.0690f), // 2026-06-06
            new("Powers/Player/Magneto/Talents/AutoDebrisFling.prototype",               false, 0.05f),
            new("Powers/Player/Magneto/Talents/AutoDebrisShield.prototype",              false, 0.05f),
            new("Powers/Player/Magneto/Talents/BoomerangScrap.prototype",                false, 0.05f),
            new("Powers/Player/Magneto/Talents/CrushConeBonus.prototype",                false, 0.05f),
            new("Powers/Player/Magneto/Talents/DebrisGeneratorBuff.prototype",           false, 0.05f),
            new("Powers/Player/Magneto/Talents/DebrisSpenderBuff.prototype",             false, 0.05f),
            new("Powers/Player/Magneto/Talents/ElectromagneticCooldownResets.prototype", false, 0.05f),
            new("Powers/Player/Magneto/Talents/HomingBlastBonus.prototype",              false, 0.05f),
            new("Powers/Player/Magneto/Talents/MaelstromCooldown.prototype",             false, 0.02f),
            new("Powers/Player/Magneto/Talents/MaxDebrisBuff.prototype",                 false, 0.05f),
            new("Powers/Player/Magneto/Talents/MetalObjectSmashBonus.prototype",         false, 0.0878f), // 2026-06-05
            new("Powers/Player/Magneto/Talents/NegativePositivePolarity.prototype",      false, 0.05f),
            new("Powers/Player/Magneto/Talents/RapidFire.prototype",                     false, 0.05f),
            new("Powers/Player/Magneto/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/Magneto/Traits/MechanicTraitDebris.prototype",            false, 0.05f),
            new("Powers/Player/Magneto/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/Magneto/UltimateHiddenPassive.prototype",                 false, 0.006f),
            new("Powers/Player/Magneto/UltimateSentinelSmash.prototype",                 true,  0.0227f), // 2026-06-03
            new("Powers/Player/TravelPower/MagnetoFlight.prototype",                     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MagnetoStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",              false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                 false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                    false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                          false, 0.05f),
        };
    }
}
