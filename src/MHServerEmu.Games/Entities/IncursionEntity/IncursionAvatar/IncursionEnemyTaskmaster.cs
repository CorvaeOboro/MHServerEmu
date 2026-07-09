using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Taskmaster Invader
    /// Powers: 20 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTaskmaster : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Taskmaster.prototype");

        public IncursionEnemyTaskmaster(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Taskmaster Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Taskmaster/AgeOfUltron.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Taskmaster/AvengersWorld.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Taskmaster/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Taskmaster/Udon.prototype",          true),
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
            new("Powers/Player/Taskmaster/BasicShot.prototype",                                true,  0.1854f), // 2026-06-11
            new("Powers/Player/Taskmaster/BasicShotTwo.prototype",                             true,  0.1688f), // 2026-06-11
            new("Powers/Player/Taskmaster/Cocoon.prototype",                                   true,  0.0847f), // 2026-06-11
            new("Powers/Player/Taskmaster/ConeYank.prototype",                                 true,  0.0338f), // 2026-06-11
            new("Powers/Player/Taskmaster/DisengagingShot.prototype",                          true,  0.0804f), // 2026-06-11
            new("Powers/Player/Taskmaster/FuriousLunge.prototype",                             true,  0.1881f), // 2026-06-11
            new("Powers/Player/Taskmaster/PoisonGasBomb.prototype",                            true,  0.1329f), // 2026-06-11
            new("Powers/Player/Taskmaster/ShieldBash.prototype",                               true,  0.0549f), // 2026-07-02
            new("Powers/Player/Taskmaster/ShieldBounce.prototype",                             true,  0.0777f), // 2026-06-11
            new("Powers/Player/Taskmaster/SteroidHotspot.prototype",                           true,  0.008f),
            new("Powers/Player/Taskmaster/SwingingAssault.prototype",                          true,  0.0446f), // 2026-06-11
            new("Powers/Player/Taskmaster/SwordStrike.prototype",                              true,  0.1344f), // 2026-06-11
            new("Powers/Player/Taskmaster/SwordStrikeTwo.prototype",                           true,  0.1108f), // 2026-06-11
            new("Powers/Player/Taskmaster/Talents/BlackKnightsGuardTalent.prototype",          false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/CaptainsStrengthTalent.prototype",           false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/CaptainsVigorTalent.prototype",              false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/DaredevilsMastery.prototype",                false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/DeadpoolHybridTalent.prototype",             false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/DoubleTimeTalent.prototype",                 false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/ElektraMarkForDeath.prototype",              false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/HawkeyesPrecisionTalent.prototype",          false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/IronFistTechnique.prototype",                false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/LukeCageComboTalent.prototype",              false, 0.025f),
            new("Powers/Player/Taskmaster/Talents/PunisherTenacityTalent.prototype",           false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/SignatureCooldownReductionTalent.prototype", false, 0.02f),
            new("Powers/Player/Taskmaster/Talents/SignatureStudentsTalent.prototype",          false, 0.02f),
            new("Powers/Player/Taskmaster/Talents/SpideysDexterityTalent.prototype",           false, 0.05f),
            new("Powers/Player/Taskmaster/Talents/WidowsGraceTalent.prototype",                false, 0.05f),
            new("Powers/Player/Taskmaster/ThreeRoundBurst.prototype",                          true,  0.1510f), // 2026-06-11
            new("Powers/Player/Taskmaster/Traits/DefensiveTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Taskmaster/Traits/OffensiveTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Taskmaster/TripleStrike.prototype",                             true,  0.1337f), // 2026-06-11
            new("Powers/Player/Taskmaster/Ultimate.prototype",                                 true,  0.006f),
            new("Powers/Player/Taskmaster/Volley.prototype",                                   true,  0.1244f), // 2026-06-11
            new("Powers/Player/Taskmaster/WebSplat.prototype",                                 true,  0.1263f), // 2026-06-11
            new("Powers/Player/Taskmaster/WhirlingClub.prototype",                             true,  0.0221f), // 2026-07-02
            new("Powers/Player/Taskmaster/WidowsBite.prototype",                               true,  0.0735f), // 2026-06-11
            new("Powers/Player/TravelPower/TaskmasterFlight.prototype",                        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/TaskmasterStolenPower.prototype",         false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                            false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                    false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                       false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                          false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                false, 0.05f),
        };
    }
}
