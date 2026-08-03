using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// ,_|ЦGGΣЯ₦ΛЦ¯|¯
    /// Powers: 16 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyJuggernaut : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Juggernaut.prototype");

        public IncursionEnemyJuggernaut(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => ",_|ЦGGΣЯ₦ΛЦ¯|¯";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Juggernaut/Classic.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Juggernaut/FearItself.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Juggernaut/Unstoppable.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Juggernaut/Xmen.prototype",        true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 150.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.05f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype",                     true,  0.05f),
            new("Powers/Player/Juggernaut/BigCharge.prototype",                            true,  0.05f),
            new("Powers/Player/Juggernaut/BonusMoveSpeedBasedOnMomentum.prototype",        true,  0.05f),
            new("Powers/Player/Juggernaut/ClotheslinePunch.prototype",                     true,  0.2354f), // 2026-06-10
            new("Powers/Player/Juggernaut/EarthquakeLeap.prototype",                       true,  0.0391f), // 2026-07-25
            new("Powers/Player/Juggernaut/HandClap.prototype",                             true,  0.1001f), // 2026-07-25
            new("Powers/Player/Juggernaut/Headbutt.prototype",                             true,  0.0824f), // 2026-06-10
            new("Powers/Player/Juggernaut/ImInvulnerable.prototype",                       true,  0.05f),
            new("Powers/Player/Juggernaut/MomentumPunch.prototype",                        true,  0.1234f), // 2026-06-07
            new("Powers/Player/Juggernaut/PeoplesElbow.prototype",                         true,  0.0723f), // 2026-07-25
            new("Powers/Player/Juggernaut/Shockwave.prototype",                            true,  0.1313f), // 2026-06-07
            new("Powers/Player/Juggernaut/SundayPunch.prototype",                          true,  0.0651f), // 2026-06-07
            new("Powers/Player/Juggernaut/Talents/AlternatingMomentumFinishers.prototype", false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/AutoAvatarOfCyttorak.prototype",         false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/BigChargeInstagib.prototype",            false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/CrimsonForceFieldTeamBuff.prototype",    false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/CyttorakPowersShareCD.prototype",        false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/DefenseDeflectTalent.prototype",         false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/HandClapFullMomentumSpender.prototype",  false, 0.1151f), // 2026-06-27
            new("Powers/Player/Juggernaut/Talents/HeadbuttBarroomBrawlingBuff.prototype",  false, 0.0804f), // 2026-06-10
            new("Powers/Player/Juggernaut/Talents/MeleePowersSundayPunchBuff.prototype",   false, 0.0671f), // 2026-06-07
            new("Powers/Player/Juggernaut/Talents/MomentumAlwaysGenerates.prototype",      false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/MovementPowersElbowDropBuff.prototype",  false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/RenounceCyttorak.prototype",             false, 0.05f),
            new("Powers/Player/Juggernaut/Talents/UnstoppableChargeCDR.prototype",         false, 0.0124f), // 2026-06-27
            new("Powers/Player/Juggernaut/Talents/UnstoppableChargeInfinite.prototype",    false, 0.0124f), // 2026-06-27
            new("Powers/Player/Juggernaut/Talents/UnstoppableChargeShorter.prototype",     false, 0.0124f), // 2026-06-27
            new("Powers/Player/Juggernaut/Traits/DefenseTrait.prototype",                  false, 0.05f),
            new("Powers/Player/Juggernaut/Traits/MechanicTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Juggernaut/Traits/OffenseTrait.prototype",                  false, 0.05f),
            new("Powers/Player/Juggernaut/TriplePunch.prototype",                          true,  0.1919f), // 2026-06-08
            new("Powers/Player/Juggernaut/Ultimate.prototype",                             true,  0.0098f), // 2026-07-25
            new("Powers/Player/Juggernaut/UltimateHiddenPassive.prototype",                false, 0.0098f), // 2026-07-25
            new("Powers/Player/Juggernaut/UnstoppableCharge.prototype",                    true,  0.0120f), // 2026-07-25
            new("Powers/Player/Juggernaut/WrathOfCyttorak.prototype",                      true,  0.0415f), // 2026-06-08
            new("Powers/Player/TravelPower/JuggernautSprint.prototype",                    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JuggernautStolenPower.prototype",     false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                   false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                      false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                            false, 0.05f),
        };
    }
}
