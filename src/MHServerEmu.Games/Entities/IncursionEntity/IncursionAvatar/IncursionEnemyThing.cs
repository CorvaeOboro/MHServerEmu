using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Thing Invader
    /// Powers: 18 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyThing : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Thing.prototype");

        public IncursionEnemyThing(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Thing Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Thing/AllNewMarvelNOW.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Thing/ByrneJersey.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Thing/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Thing/FFInverted.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Thing/FantasticPants.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Thing/FearItself.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Thing/FutureFoundation.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Thing/Incognito.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Thing/RedSuit.prototype",          true),
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
            new("Powers/Player/Thing/Rework/Bash.prototype",                                true,  0.0097f), // 2026-06-16
            new("Powers/Player/Thing/Rework/CallHothead.prototype",                         true,  0.0229f), // 2026-06-17
            new("Powers/Player/Thing/Rework/CallStretch.prototype",                         true,  0.0258f), // 2026-06-18
            new("Powers/Player/Thing/Rework/CallSuzie.prototype",                           true,  0.0264f), // 2026-06-18
            new("Powers/Player/Thing/Rework/CrashingLeap.prototype",                        true,  0.0393f), // 2026-06-18
            new("Powers/Player/Thing/Rework/DiscusToss.prototype",                          true,  0.05f),
            new("Powers/Player/Thing/Rework/FoodCart.prototype",                            true,  0.1208f), // 2026-06-18
            new("Powers/Player/Thing/Rework/GroundSmash.prototype",                         true,  0.0412f), // 2026-06-18
            new("Powers/Player/Thing/Rework/GuessWhatTimeItIs.prototype",                   true,  0.0148f), // 2026-06-18
            new("Powers/Player/Thing/Rework/Headbutt.prototype",                            true,  0.0733f), // 2026-06-18
            new("Powers/Player/Thing/Rework/Knockout.prototype",                            true,  0.0337f), // 2026-06-17
            new("Powers/Player/Thing/Rework/LampBatThrow.prototype",                        true,  0.0385f), // 2026-06-18
            new("Powers/Player/Thing/Rework/ParkingMeterSmash.prototype",                   true,  0.0949f), // 2026-06-18
            new("Powers/Player/Thing/Rework/RockslideCharge.prototype",                     true,  0.1023f), // 2026-06-18
            new("Powers/Player/Thing/Rework/RockyPunch.prototype",                          true,  0.0871f), // 2026-06-16
            new("Powers/Player/Thing/Rework/WiseCrack.prototype",                           true,  0.05f),
            new("Powers/Player/Thing/Rework/YancyStreetGang.prototype",                     true,  0.1586f), // 2026-06-18
            new("Powers/Player/Thing/Talents/Talent1CallInNoUseBenefit.prototype",          false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent1CallInSharedCooldown.prototype",        false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent1YancyStreetBuff.prototype",             false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent2ClobberinBoost.prototype",              false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent2HotHeadBuff.prototype",                 false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent2SignatureCooldownReduction.prototype",  false, 0.02f),
            new("Powers/Player/Thing/Talents/Talent3CallSuzieBuff.prototype",               false, 0.0264f), // 2026-06-18
            new("Powers/Player/Thing/Talents/Talent3ClobberinTimeDefensiveBoost.prototype", false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent3TauntBuff.prototype",                   false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent4BrawlingBoost.prototype",               false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent4CallStretchBuff.prototype",             false, 0.0258f), // 2026-06-18
            new("Powers/Player/Thing/Talents/Talent4ThrownObjectBuffs.prototype",           false, 0.05f),
            new("Powers/Player/Thing/Talents/Talent5CrashingLeapBuff.prototype",            false, 0.0393f), // 2026-06-18
            new("Powers/Player/Thing/Talents/Talent5GroundSmashBuff.prototype",             false, 0.0412f), // 2026-06-18
            new("Powers/Player/Thing/Talents/Talent5WeaponsBuff.prototype",                 false, 0.05f),
            new("Powers/Player/Thing/Traits/ClobberinTime.prototype",                       false, 0.05f),
            new("Powers/Player/Thing/Traits/DefenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Thing/Traits/OffenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Thing/Ultimate.prototype",                                   true,  0.0043f), // 2026-06-16
            new("Powers/Player/TravelPower/ThingFlight.prototype",                          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ThingStolenPower.prototype",           false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
