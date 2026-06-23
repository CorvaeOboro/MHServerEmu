using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// SquirrelGirl Invader
    /// Powers: 17 / 44
    /// Damage scale per ability is listed below.
    /// we disabled her Ultimate because it perma roots the player
    /// </summary>
    public class IncursionEnemySquirrelGirl : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/SquirrelGirl.prototype");

        public IncursionEnemySquirrelGirl(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "SquirrelGirl Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/Aviator.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/Christmas.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/GLX.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/Modern.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/TESTONLY.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/Unbeatable.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/SquirrelGirl/UnbeatableVariant.prototype", true),
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
            new("Powers/Player/SquirrelGirl/AcornMeteor.prototype",                      true,  0.1069f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/BafflingDialogue.prototype",                 true,  0.05f),
            new("Powers/Player/SquirrelGirl/BasicMelee.prototype",                       true,  0.1153f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/BasicMeleeSquirrelBonus.prototype",          true,  0.1125f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/BasicRangedSquirrelPiercing.prototype",      true,  0.1576f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/BasicTripleSquirrel.prototype",              true,  0.05f),
            new("Powers/Player/SquirrelGirl/DiveBomb.prototype",                         true,  0.0830f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/DoubleStrike.prototype",                     true,  0.0338f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/GoForTheEyes.prototype",                     true,  0.1221f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/MeleeSquirrelCone.prototype",                true,  0.0818f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/PBAoEKnockdown.prototype",                   true,  0.0775f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/RangedSquirrelAoEVisual.prototype",          true,  0.03f),
            new("Powers/Player/SquirrelGirl/SquirrelAttack.prototype",                   true,  0.05f),
            new("Powers/Player/SquirrelGirl/SquirrelBombs.prototype",                    true,  0.05f),
            new("Powers/Player/SquirrelGirl/SquirrelBuffsHiddenPassive.prototype",       false, 0.05f),
            new("Powers/Player/SquirrelGirl/SquirrelRapidFire.prototype",                true,  0.1843f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/Talents/AcornMeteorBonus.prototype",         false, 0.1069f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/Talents/ClawSpec.prototype",                 false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/DoubleStrikeBonus.prototype",        false, 0.0338f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/Talents/ForMonkeyJoe.prototype",             false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/HulkbusterSquirrels.prototype",      false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/MeleeSquirrelConeBonus.prototype",   false, 0.0818f), // 2026-06-20
            new("Powers/Player/SquirrelGirl/Talents/RangedSpec.prototype",               false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SignatureCooldown.prototype",        false, 0.02f),
            new("Powers/Player/SquirrelGirl/Talents/SpecialForcesSquirrels.prototype",   false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SquirrelArmy.prototype",             false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SquirrelAttackBonus.prototype",      false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SquirrelSaboteursBonus.prototype",   false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SquirrelTwirlBonus.prototype",       false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/SquirrelsFromAbove.prototype",       false, 0.05f),
            new("Powers/Player/SquirrelGirl/Talents/TippyToe.prototype",                 false, 0.05f),
            new("Powers/Player/SquirrelGirl/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/SquirrelGirl/Traits/MechanicTraitSquirrels.prototype",    false, 0.05f),
            new("Powers/Player/SquirrelGirl/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/SquirrelGirl/Tumble.prototype",                           true,  0.05f),
            new("Powers/Player/SquirrelGirl/Ultimate.prototype",                         false,  0.0182f), // 2026-06-20
            new("Powers/Player/TravelPower/SquirrelGirlSprint.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SquirrelGirlStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",              false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                 false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                    false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                          false, 0.05f),
        };
    }
}
