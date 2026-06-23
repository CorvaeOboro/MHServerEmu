using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Thor Invader
    /// Powers: 18 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyThor : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Thor.prototype");

        public IncursionEnemyThor(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Thor Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Thor/AgeOfUltronMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Thor/AvengersMovie.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Thor/BetaRayBill.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Thor/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Thor/DestroyerArmor.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Thor/EarthX.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Thor/GoldenArmor.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Thor/JaneFoster.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Thor/MarvelNow.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Thor/Modern.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Thor/MovieVariant.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Thor/OldThor.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Thor/Skrull.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Thor/Ultimate.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Thor/VisualUpdate.prototype",     true),
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
            new("Powers/Player/Thor/Rework/BasicMelee.prototype",                     true,  0.0955f), // 2026-06-17
            new("Powers/Player/Thor/Rework/BoltSpray.prototype",                      true,  0.0416f), // 2026-06-20
            new("Powers/Player/Thor/Rework/Charge.prototype",                         true,  0.05f),
            new("Powers/Player/Thor/Rework/DeathFromAbove.prototype",                 true,  0.0590f), // 2026-06-20
            new("Powers/Player/Thor/Rework/GroundSmash.prototype",                    true,  0.0365f), // 2026-06-16
            new("Powers/Player/Thor/Rework/HammerDash.prototype",                     true,  0.0603f), // 2026-06-20
            new("Powers/Player/Thor/Rework/HammerThrow.prototype",                    true,  0.0418f), // 2026-06-20
            new("Powers/Player/Thor/Rework/ImmortalCombatRestore.prototype",          true,  0.0349f), // 2026-06-20
            new("Powers/Player/Thor/Rework/KnockOut.prototype",                       true,  0.05f),
            new("Powers/Player/Thor/Rework/LightningStrike.prototype",                true,  0.1155f), // 2026-06-20
            new("Powers/Player/Thor/Rework/Ragnarok.prototype",                       true,  0.05f),
            new("Powers/Player/Thor/Rework/Shockwave.prototype",                      true,  0.0562f), // 2026-06-20
            new("Powers/Player/Thor/Rework/SignatureAntiforce.prototype",             true,  0.0047f), // 2026-06-20
            new("Powers/Player/Thor/Rework/SteroidStrong.prototype",                  true,  0.05f),
            new("Powers/Player/Thor/Rework/Taunt.prototype",                          true,  0.05f),
            new("Powers/Player/Thor/Rework/ThunderSpot.prototype",                    true,  0.0905f), // 2026-06-20
            new("Powers/Player/Thor/StormHammerSummon.prototype",                     true,  0.0467f), // 2026-06-20
            new("Powers/Player/Thor/Talents/BasicMeleeLightningBoltTalent.prototype", false, 0.0955f), // 2026-06-17
            new("Powers/Player/Thor/Talents/BasicMeleeThunderclapTalent.prototype",   false, 0.0955f), // 2026-06-17
            new("Powers/Player/Thor/Talents/BruiserTalent.prototype",                 false, 0.05f),
            new("Powers/Player/Thor/Talents/DeathFromAboveTalent.prototype",          false, 0.0590f), // 2026-06-20
            new("Powers/Player/Thor/Talents/ForAsgardBeserkerTalent.prototype",       false, 0.05f),
            new("Powers/Player/Thor/Talents/ForAsgardEmpoweredTalent.prototype",      false, 0.05f),
            new("Powers/Player/Thor/Talents/ForAsgardWrathOfGod.prototype",           false, 0.05f),
            new("Powers/Player/Thor/Talents/HybridManTalent.prototype",               false, 0.05f),
            new("Powers/Player/Thor/Talents/KnockoutSuperStrike.prototype",           false, 0.05f),
            new("Powers/Player/Thor/Talents/MjolnirThrowTalent.prototype",            false, 0.05f),
            new("Powers/Player/Thor/Talents/OdinforceRemovedTalent.prototype",        false, 0.05f),
            new("Powers/Player/Thor/Talents/OdinforceSpendingTalent.prototype",       false, 0.05f),
            new("Powers/Player/Thor/Talents/SmashySmashyTalent.prototype",            false, 0.05f),
            new("Powers/Player/Thor/Talents/StackingOdinforceTalent.prototype",       false, 0.05f),
            new("Powers/Player/Thor/Talents/StormcallerTalent.prototype",             false, 0.03f),
            new("Powers/Player/Thor/Traits/DefensiveTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Thor/Traits/MechanicTraitOdinforce.prototype",         false, 0.05f),
            new("Powers/Player/Thor/Traits/OffensiveTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Thor/Ultimate.prototype",                              true,  0.0165f), // 2026-06-20
            new("Powers/Player/TravelPower/ThorFlight.prototype",                     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ThorStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",            false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                   false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",           false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",              false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                 false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                       false, 0.05f),
        };
    }
}
