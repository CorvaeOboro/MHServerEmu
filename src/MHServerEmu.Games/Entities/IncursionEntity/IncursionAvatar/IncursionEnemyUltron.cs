using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Ultron Invader
    /// Powers: 18 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyUltron : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Ultron.prototype");

        public IncursionEnemyUltron(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Ultron Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Ultron/AoUComicsGold.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Ultron/AoUMovie.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Ultron/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Ultron/Ultron11.prototype",      true),
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
            new("Powers/Player/TravelPower/UltronFlight.prototype",                       false, 0.006f),
            new("Powers/Player/Ultron/BigBigBlast.prototype",                             true,  0.0583f), // 2026-06-17
            new("Powers/Player/Ultron/CommandingShot.prototype",                          true,  0.0418f), // 2026-06-17
            new("Powers/Player/Ultron/ConcussionBlast.prototype",                         true,  0.0388f), // 2026-06-17
            new("Powers/Player/Ultron/Dash.prototype",                                    true,  0.1061f), // 2026-06-17
            new("Powers/Player/Ultron/DroneStrafe.prototype",                             true,  0.1324f), // 2026-06-18
            new("Powers/Player/Ultron/EncephaloBeam.prototype",                           true,  0.0621f), // 2026-06-18
            new("Powers/Player/Ultron/FingerLaserBlasts.prototype",                       true,  0.05f),
            new("Powers/Player/Ultron/FingerLasers.prototype",                            true,  0.1099f), // 2026-06-18
            new("Powers/Player/Ultron/FocusedConcussion.prototype",                       true,  0.1081f), // 2026-06-17
            new("Powers/Player/Ultron/GroundThrow.prototype",                             true,  0.0380f), // 2026-06-18
            new("Powers/Player/Ultron/LeapStrike.prototype",                              true,  0.0275f), // 2026-06-17
            new("Powers/Player/Ultron/MeleeStrike.prototype",                             true,  0.0819f), // 2026-06-16
            new("Powers/Player/Ultron/RadiationBlast.prototype",                          true,  0.0744f), // 2026-06-18
            new("Powers/Player/Ultron/Signature.prototype",                               true,  0.0407f), // 2026-06-18
            new("Powers/Player/Ultron/Slam.prototype",                                    true,  0.0482f), // 2026-06-18
            new("Powers/Player/Ultron/SpinAttack.prototype",                              true,  0.0482f), // 2026-06-17
            new("Powers/Player/Ultron/SummonSuicideDrone.prototype",                      true,  0.0579f), // 2026-06-18
            new("Powers/Player/Ultron/Talents/Talent1EncephaloBeamBuff.prototype",        false, 0.0621f), // 2026-06-18
            new("Powers/Player/Ultron/Talents/Talent1MoreFlyingExplodeyDrones.prototype", false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent1SpinAttackCooldownReset.prototype",  false, 0.0482f), // 2026-06-17
            new("Powers/Player/Ultron/Talents/Talent2LeapstrikeCharges.prototype",        false, 0.0275f), // 2026-06-17
            new("Powers/Player/Ultron/Talents/Talent2RangeRadiationBlast.prototype",      false, 0.0744f), // 2026-06-18
            new("Powers/Player/Ultron/Talents/Talent2RangedDrones.prototype",             false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent3CrushBuff.prototype",                false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent3MeleeDrones.prototype",              false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent3SelfRez.prototype",                  false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent4BigBlastBuff.prototype",             false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent4CommandingShotBuff.prototype",       false, 0.0418f), // 2026-06-17
            new("Powers/Player/Ultron/Talents/Talent4PullTowards.prototype",              false, 0.05f),
            new("Powers/Player/Ultron/Talents/Talent5ConcussionBlastBuff.prototype",      false, 0.0388f), // 2026-06-17
            new("Powers/Player/Ultron/Talents/Talent5RadiationBlastDefensive.prototype",  false, 0.0744f), // 2026-06-18
            new("Powers/Player/Ultron/Talents/Talent5SignatureExtraSwarm.prototype",      false, 0.02f),
            new("Powers/Player/Ultron/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Ultron/Traits/MechanicTraitBandwidth.prototype",           false, 0.05f),
            new("Powers/Player/Ultron/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Ultron/Ultimate.prototype",                                true,  0.0182f), // 2026-06-17
            new("Powers/Player/Ultron/UltimateHiddenPassive.prototype",                   false, 0.0182f), // 2026-06-17
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NoStealablePowerBlank.prototype",    false, 0.05f),
        };
    }
}
