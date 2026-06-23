using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// KittyPryde Invader
    /// Powers: 16 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyKittyPryde : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/KittyPryde.prototype");

        public IncursionEnemyKittyPryde(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "KittyPryde Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/KittyPryde/AgeOfApocalypse.prototype", true),
            new("Entity/Items/Costumes/Prototypes/KittyPryde/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/KittyPryde/ExcaliburMasked.prototype", true),
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
            new("Powers/Player/KittyPryde/BasicMelee.prototype",                              true,  0.1387f), // 2026-06-20
            new("Powers/Player/KittyPryde/BuddySystem.prototype",                             true,  0.05f),
            new("Powers/Player/KittyPryde/DeathFromBelow.prototype",                          true,  0.0410f), // 2026-06-20
            new("Powers/Player/KittyPryde/Execute.prototype",                                 true,  0.0525f), // 2026-06-18
            new("Powers/Player/KittyPryde/HeartCrush.prototype",                              true,  0.0441f), // 2026-06-18
            new("Powers/Player/KittyPryde/LockheedChannelFire.prototype",                     true,  0.0299f), // 2026-06-20
            new("Powers/Player/KittyPryde/LockheedCharge.prototype",                          true,  0.0450f), // 2026-06-18
            new("Powers/Player/KittyPryde/LockheedFireBreath.prototype",                      true,  0.0701f), // 2026-06-20
            new("Powers/Player/KittyPryde/LockheedFireball.prototype",                        true,  0.0729f), // 2026-06-20
            new("Powers/Player/KittyPryde/LockheedToggleHiddenPassive.prototype",             false, 0.05f),
            new("Powers/Player/KittyPryde/NoCollisionPassive.prototype",                      false, 0.05f),
            new("Powers/Player/KittyPryde/PhaseAoE.prototype",                                true,  0.0151f), // 2026-06-20
            new("Powers/Player/KittyPryde/PhaseDash.prototype",                               true,  0.1331f), // 2026-06-20
            new("Powers/Player/KittyPryde/PhaseOut.prototype",                                true,  0.05f),
            new("Powers/Player/KittyPryde/PullUnderStart.prototype",                          true,  0.0386f), // 2026-06-20
            new("Powers/Player/KittyPryde/Signature.prototype",                               true,  0.0057f), // 2026-06-20
            new("Powers/Player/KittyPryde/TagTeam.prototype",                                 true,  0.0541f), // 2026-06-20
            new("Powers/Player/KittyPryde/Talents/Talent1LockheedActive.prototype",           false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent1LockheedPassive.prototype",          false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent1NoLockheed.prototype",               false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent2LockheedFocus.prototype",            false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent2PhaseDashDoT.prototype",             false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent2SwordRemapping.prototype",           false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent3DFACritIncrease.prototype",          false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent3ExecuteAlwaysBrutal.prototype",      false, 0.0525f), // 2026-06-18
            new("Powers/Player/KittyPryde/Talents/Talent3LockheedAutoAoE.prototype",          false, 0.03f),
            new("Powers/Player/KittyPryde/Talents/Talent4LockheedSuperCharge.prototype",      false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent4MultVsLockheedDoTs.prototype",       false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent4PhaseThroughEnemies.prototype",      false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent5AoeCooldowns.prototype",             false, 0.03f),
            new("Powers/Player/KittyPryde/Talents/Talent5PhaseOutDurationIncrease.prototype", false, 0.05f),
            new("Powers/Player/KittyPryde/Talents/Talent5SignatureMoreHits.prototype",        false, 0.0057f), // 2026-06-20
            new("Powers/Player/KittyPryde/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/KittyPryde/Traits/MechanicTraitLockheedEnergy.prototype",      false, 0.05f),
            new("Powers/Player/KittyPryde/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/KittyPryde/Ultimate.prototype",                                true,  0.0102f), // 2026-06-20
            new("Powers/Player/TravelPower/KittyPrydeFlight.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/KittyPrydeStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                   false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                      false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                         false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                               false, 0.05f),
        };
    }
}
