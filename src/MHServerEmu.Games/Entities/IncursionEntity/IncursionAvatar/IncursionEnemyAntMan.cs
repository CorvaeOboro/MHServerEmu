using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// AntMan Invader
    /// Powers: 18 / 46
    /// Damage scale per ability is listed below.
    /// we disabled the Antnado because the render model wouldnt return to the ground after
    /// </summary>
    public class IncursionEnemyAntMan : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/AntMan.prototype");

        public IncursionEnemyAntMan(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "AntMan Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/AntMan/HPClassic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/AntMan/SLAvengersNOW.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/AntMan/SLCivilWarMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/AntMan/SLMovie.prototype",         true),
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
            new("Powers/Player/AntMan/AntAllyBuffsHiddenPassive.prototype",               false, 0.05f),
            new("Powers/Player/AntMan/AntPunch.prototype",                                true,  0.05f),
            new("Powers/Player/AntMan/AntStampede.prototype",                             true,  0.05f),
            new("Powers/Player/AntMan/AntWall.prototype",                                 true,  0.05f),
            new("Powers/Player/AntMan/AnthillActive.prototype",                           true,  0.1116f), // 2026-06-20
            new("Powers/Player/AntMan/AnthillPassive.prototype",                          false, 0.05f),
            new("Powers/Player/AntMan/AntnadoMovementPower.prototype",                    false,  0.0085f), // 2026-06-16
            new("Powers/Player/AntMan/BioElectricBlast.prototype",                        true,  0.1356f), // 2026-06-20
            new("Powers/Player/AntMan/BounceDash.prototype",                              true,  0.1483f), // 2026-06-20
            new("Powers/Player/AntMan/DisruptorBlast.prototype",                          true,  0.0621f), // 2026-06-20
            new("Powers/Player/AntMan/FireAntAttack.prototype",                           true,  0.05f),
            new("Powers/Player/AntMan/FlyingAntSwarm.prototype",                          true,  0.05f),
            new("Powers/Player/AntMan/GiantManFoot.prototype",                            true,  0.0102f), // 2026-06-17
            new("Powers/Player/AntMan/InsectDecoy.prototype",                             true,  0.05f),
            new("Powers/Player/AntMan/MultiStrike.prototype",                             true,  0.0883f), // 2026-06-20
            new("Powers/Player/AntMan/NotSoBigPunch.prototype",                           true,  0.0407f), // 2026-06-20
            new("Powers/Player/AntMan/PymSuit.prototype",                                 true,  0.05f),
            new("Powers/Player/AntMan/RapidShrinkStrike.prototype",                       true,  0.1359f), // 2026-06-17
            new("Powers/Player/AntMan/ShrinkingStrike.prototype",                         true,  0.0939f), // 2026-06-20
            new("Powers/Player/AntMan/Talents/AntDecoyTalent.prototype",                  false, 0.05f),
            new("Powers/Player/AntMan/Talents/AntRespawnTalent.prototype",                false, 0.05f),
            new("Powers/Player/AntMan/Talents/AntUseSpiritTalent.prototype",              false, 0.05f),
            new("Powers/Player/AntMan/Talents/AntVulnerabilityTalent.prototype",          false, 0.05f),
            new("Powers/Player/AntMan/Talents/AntWallTalent.prototype",                   false, 0.05f),
            new("Powers/Player/AntMan/Talents/BouncingBulletTalent.prototype",            false, 0.05f),
            new("Powers/Player/AntMan/Talents/FlyingAntSwarmGrowTalent.prototype",        false, 0.05f),
            new("Powers/Player/AntMan/Talents/HealthOnShrinkHitTalent.prototype",         false, 0.05f),
            new("Powers/Player/AntMan/Talents/OneTwoAntPunchTalent.prototype",            false, 0.05f),
            new("Powers/Player/AntMan/Talents/ParticleOverchargeSteroidTalent.prototype", false, 0.05f),
            new("Powers/Player/AntMan/Talents/RedhotsStunTalent.prototype",               false, 0.05f),
            new("Powers/Player/AntMan/Talents/STSSAntRecharge100Pct.prototype",           false, 0.05f),
            new("Powers/Player/AntMan/Talents/STSSBonusDamageTalent.prototype",           false, 0.05f),
            new("Powers/Player/AntMan/Talents/STSSExtraDoT.prototype",                    false, 0.05f),
            new("Powers/Player/AntMan/Talents/TankerThrowTalent.prototype",               false, 0.05f),
            new("Powers/Player/AntMan/ThrowCar.prototype",                                true,  0.0628f), // 2026-06-20
            new("Powers/Player/AntMan/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/AntMan/Traits/MechanicTraitAnts.prototype",                false, 0.05f),
            new("Powers/Player/AntMan/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/TravelPower/AntmanFlight.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/AntManStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
        };
    }
}
