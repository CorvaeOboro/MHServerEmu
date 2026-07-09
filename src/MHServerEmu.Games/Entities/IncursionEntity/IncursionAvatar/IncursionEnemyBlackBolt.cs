using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// BlackBolt Invader
    /// Powers: 16 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBlackBolt : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/BlackBolt.prototype");

        public IncursionEnemyBlackBolt(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "BlackBolt Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/BlackBolt/ANAD.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackBolt/Classic.prototype", true),
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
            new("Powers/Player/BlackBolt/Barrier.prototype",                                  true,  0.05f),
            new("Powers/Player/BlackBolt/BasicPunch.prototype",                               true,  0.1658f), // 2026-06-11
            new("Powers/Player/BlackBolt/Bolt.prototype",                                     true,  0.1753f), // 2026-06-11
            new("Powers/Player/BlackBolt/Burst.prototype",                                    true,  0.1938f), // 2026-06-11
            new("Powers/Player/BlackBolt/ChanneledBeam.prototype",                            true,  0.0491f), // 2026-06-11
            new("Powers/Player/BlackBolt/Dash.prototype",                                     true,  0.1292f), // 2026-06-11
            new("Powers/Player/BlackBolt/DeathFromAboveStart.prototype",                      true,  0.0335f), // 2026-06-27
            new("Powers/Player/BlackBolt/GapClose.prototype",                                 true,  0.0666f), // 2026-06-27
            new("Powers/Player/BlackBolt/Geyser.prototype",                                   true,  0.1027f), // 2026-06-11
            new("Powers/Player/BlackBolt/HypersonicScream.prototype",                         true,  0.0132f), // 2026-06-27
            new("Powers/Player/BlackBolt/Implode.prototype",                                  true,  0.1080f), // 2026-06-11
            new("Powers/Player/BlackBolt/KillingWord.prototype",                              true,  0.0094f), // 2026-06-11
            new("Powers/Player/BlackBolt/MasterBlowStart.prototype",                          true,  0.05f),
            new("Powers/Player/BlackBolt/PBAoE.prototype",                                    true,  0.1555f), // 2026-06-11
            new("Powers/Player/BlackBolt/Pummel.prototype",                                   true,  0.0653f), // 2026-06-11
            new("Powers/Player/BlackBolt/SwoopingStrikes.prototype",                          true,  0.0432f), // 2026-06-11
            new("Powers/Player/BlackBolt/Talents/Talent1DeathFromAboveAuraHotspot.prototype", false, 0.008f),
            new("Powers/Player/BlackBolt/Talents/Talent1EnergyPassive.prototype",             false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent1GapCloseRemap.prototype",             false, 0.0666f), // 2026-06-27
            new("Powers/Player/BlackBolt/Talents/Talent2BeamRemap.prototype",                 false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent2MovementBoost.prototype",             false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent2PummelReset.prototype",               false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent3BarrierRemap.prototype",              false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent3BurstRemap.prototype",                false, 0.1938f), // 2026-06-11
            new("Powers/Player/BlackBolt/Talents/Talent3SwoopingStrikesBoost.prototype",      false, 0.0432f), // 2026-06-11
            new("Powers/Player/BlackBolt/Talents/Talent4SigCooldownReset.prototype",          false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent4SigRangeBoost.prototype",             false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent4SigRemap.prototype",                  false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent5Above30PctEnergyBoost.prototype",     false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent5BarrierAutoRevive.prototype",         false, 0.05f),
            new("Powers/Player/BlackBolt/Talents/Talent5Energy100PctSteroid.prototype",       false, 0.05f),
            new("Powers/Player/BlackBolt/Traits/DefenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/BlackBolt/Traits/MechanicTraitEnergyManipulator.prototype",    false, 0.05f),
            new("Powers/Player/BlackBolt/Traits/OffenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/TravelPower/BlackBoltFlight.prototype",                        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackBoltStolenPower.prototype",         false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                   false, 0.05f),
            new("Powers/Player/Hawkeye/TenArrowSpeedLoader.prototype",                        false, 0.05f),
            new("Powers/Player/Hawkeye/TurretArrow.prototype",                                false, 0.05f),
            new("Powers/Player/Hawkeye/Ultimate.prototype",                                   false, 0.006f),
            new("Powers/Player/Hawkeye/Volley.prototype",                                     false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                      false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                         false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                               false, 0.05f),
        };
    }
}
