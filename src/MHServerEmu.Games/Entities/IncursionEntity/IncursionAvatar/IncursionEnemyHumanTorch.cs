using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// HumanTorch Invader
    /// Powers: 15 / 41
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyHumanTorch : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/HumanTorch.prototype");

        public IncursionEnemyHumanTorch(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "HumanTorch Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/HumanTorch/ClassicBlue.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/Inhumans.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/LightBrigade.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/Modern.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/Original.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/Red.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/HumanTorch/TwoThousandNinetyNine.prototype", true),
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
            new("Powers/Player/HumanTorch/BasicFireWedge.prototype",                   true,  0.0424f), // 2026-06-17
            new("Powers/Player/HumanTorch/BasicFireball.prototype",                    true,  0.1562f), // 2026-06-05
            new("Powers/Player/HumanTorch/BowlingBall.prototype",                      true,  0.1231f), // 2026-06-17
            new("Powers/Player/HumanTorch/ChanneledEnergyBeam.prototype",              true,  0.0308f), // 2026-06-17
            new("Powers/Player/HumanTorch/ChargeUpBlowup.prototype",                   true,  0.05f),
            new("Powers/Player/HumanTorch/Consume.prototype",                          true,  0.05f),
            new("Powers/Player/HumanTorch/FallbackBlast.prototype",                    true,  0.0841f), // 2026-06-17
            new("Powers/Player/HumanTorch/FlameOn.prototype",                          true,  0.0227f), // 2026-06-06
            new("Powers/Player/HumanTorch/FlameTornado.prototype",                     true,  0.0493f), // 2026-06-06
            new("Powers/Player/HumanTorch/HomingShot.prototype",                       true,  0.1613f), // 2026-06-05
            new("Powers/Player/HumanTorch/NovaBurst.prototype",                        true,  0.2435f), // 2026-06-06
            new("Powers/Player/HumanTorch/NovaCharge.prototype",                       true,  0.2056f), // 2026-06-05
            new("Powers/Player/HumanTorch/SummonFireHotspot.prototype",                true,  0.0706f), // 2026-06-06
            new("Powers/Player/HumanTorch/SummonFireWall.prototype",                   true,  0.0929f), // 2026-06-17
            new("Powers/Player/HumanTorch/Talents/BouncingFireballs.prototype",        false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/BurningProc.prototype",              false, 0.025f),
            new("Powers/Player/HumanTorch/Talents/Cauterize.prototype",                false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/ChargeUpBlowupBonus.prototype",      false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/ConsumeHeatBuilder.prototype",       false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/ConsumeProtectiveFlames.prototype",  false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/FireWedgeConsumer.prototype",        false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/FlameCyclone.prototype",             false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/FlameTornado.prototype",             false, 0.0493f), // 2026-06-06
            new("Powers/Player/HumanTorch/Talents/FlameWave.prototype",                false, 0.03f),
            new("Powers/Player/HumanTorch/Talents/HeatGeneration.prototype",           false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/NovaBurstBonus.prototype",           false, 0.2435f), // 2026-06-06
            new("Powers/Player/HumanTorch/Talents/OverheatMorePotent.prototype",       false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/OverheatedSingleTargets.prototype",  false, 0.05f),
            new("Powers/Player/HumanTorch/Talents/TooHotToHit.prototype",              false, 0.05f),
            new("Powers/Player/HumanTorch/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/HumanTorch/Traits/MechanicTraitHeat.prototype",         false, 0.05f),
            new("Powers/Player/HumanTorch/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/HumanTorch/UltimateStart.prototype",                    true,  0.0187f), // 2026-06-17
            new("Powers/Player/TravelPower/HumanTorchFlight.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/HumanTorchStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",             false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",            false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",               false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                  false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                        false, 0.05f),
        };
    }
}
