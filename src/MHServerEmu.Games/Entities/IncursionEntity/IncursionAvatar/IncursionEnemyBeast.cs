using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Beast Invader
    /// Powers: 16 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBeast : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Beast.prototype");

        public IncursionEnemyBeast(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Beast Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Beast/Astonishing.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Beast/UncannyInhumans.prototype", true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.0333f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Beast/BeastBamf.prototype",                           true,  0.0257f), // 2026-06-20
            new("Powers/Player/Beast/BeastDash.prototype",                           true,  0.0641f), // 2026-06-17
            new("Powers/Player/Beast/CloseGap.prototype",                            true,  0.0530f), // 2026-06-20
            new("Powers/Player/Beast/DeathFromAbove.prototype",                      true,  0.0940f), // 2026-06-20
            new("Powers/Player/Beast/ElectroGadget.prototype",                       true,  0.0452f), // 2026-06-17
            new("Powers/Player/Beast/GlueBomb.prototype",                            true,  0.0760f), // 2026-06-14
            new("Powers/Player/Beast/HulkingSlam.prototype",                         true,  0.0147f), // 2026-06-16
            new("Powers/Player/Beast/MeleeBasic.prototype",                          true,  0.1025f), // 2026-06-17
            new("Powers/Player/Beast/MeleePBAoE.prototype",                          true,  0.0312f), // 2026-06-20
            new("Powers/Player/Beast/Pummel.prototype",                              true,  0.0437f), // 2026-06-17
            new("Powers/Player/Beast/ShieldGadget.prototype",                        true,  0.0352f), // 2026-06-16
            new("Powers/Player/Beast/SleepGasGadget.prototype",                      true,  0.0825f), // 2026-06-16
            new("Powers/Player/Beast/Talents/Talent1BrainsCDR.prototype",            false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent1DFARemap.prototype",             false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent1TumbleCharge.prototype",         false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent2FlyingBeatdownRemap.prototype",  false, 0.02f),
            new("Powers/Player/Beast/Talents/Talent2PummelBrutChance.prototype",     false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent2PummelCooldownReset.prototype",  false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent3CloseGapCharge.prototype",       false, 0.0530f), // 2026-06-20
            new("Powers/Player/Beast/Talents/Talent3PummelCDR.prototype",            false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent3ShieldGadgetRemap.prototype",    false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent4AutoShield.prototype",           false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent4BrainsHealthBuffProc.prototype", false, 0.025f),
            new("Powers/Player/Beast/Talents/Talent4BrainsSpiritBuffProc.prototype", false, 0.025f),
            new("Powers/Player/Beast/Talents/Talent5BeastModeBuff.prototype",        false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent5SigJubilee.prototype",           false, 0.05f),
            new("Powers/Player/Beast/Talents/Talent5SigMomentumGen.prototype",       false, 0.05f),
            new("Powers/Player/Beast/TeslaTowerGadget.prototype",                    true,  0.0995f), // 2026-06-16
            new("Powers/Player/Beast/TetherballPBAoE.prototype",                     true,  0.0257f), // 2026-06-20
            new("Powers/Player/Beast/Traits/DefenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Beast/Traits/MechanicTrait.prototype",                false, 0.05f),
            new("Powers/Player/Beast/Traits/OffenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/Beast/Tumble.prototype",                              true,  0.0926f), // 2026-06-16
            new("Powers/Player/Beast/Ultimate.prototype",                            true,  0.0016f), // 2026-06-20
            new("Powers/Player/TravelPower/BeastSprint.prototype",                   false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BeastStolenPower.prototype",    false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",           false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",          false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",             false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                      false, 0.05f),
        };
    }
}
