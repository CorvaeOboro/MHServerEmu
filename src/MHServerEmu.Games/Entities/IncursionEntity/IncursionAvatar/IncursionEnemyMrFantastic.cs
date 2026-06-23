using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// MrFantastic Invader
    /// Powers: 18 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyMrFantastic : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/MrFantastic.prototype");

        public IncursionEnemyMrFantastic(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "MrFantastic Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/MrFantastic/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/MrFantastic/FFInverted.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/MrFantastic/FutureFoundation.prototype", true),
            new("Entity/Items/Costumes/Prototypes/MrFantastic/MarvelNOWRed.prototype",     true),
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
            new("Powers/Player/MrFantastic/BasicStretchyPunch.prototype",               true,  0.1175f), // 2026-06-17
            new("Powers/Player/MrFantastic/BigFistedPunch.prototype",                   true,  0.0806f), // 2026-06-17
            new("Powers/Player/MrFantastic/ChargedPBAoE.prototype",                     true,  0.03f),
            new("Powers/Player/MrFantastic/ConePunchSTSS.prototype",                    true,  0.0733f), // 2026-06-17
            new("Powers/Player/MrFantastic/ConeRapidPunch.prototype",                   true,  0.1291f), // 2026-06-17
            new("Powers/Player/MrFantastic/ConeYank.prototype",                         true,  0.1235f), // 2026-06-05
            new("Powers/Player/MrFantastic/DeathFromAbove.prototype",                   true,  0.0367f), // 2026-06-17
            new("Powers/Player/MrFantastic/ElectricAoEGadget.prototype",                true,  0.0676f), // 2026-06-17
            new("Powers/Player/MrFantastic/ExpandingPBAoE.prototype",                   true,  0.03f),
            new("Powers/Player/MrFantastic/GiantGunGadget.prototype",                   true,  0.0765f), // 2026-06-17
            new("Powers/Player/MrFantastic/GiantGunGadgetHiddenPassive.prototype",      false, 0.0765f), // 2026-06-17
            new("Powers/Player/MrFantastic/HammerFist.prototype",                       true,  0.05f),
            new("Powers/Player/MrFantastic/ImplosionGadget.prototype",                  true,  0.0753f), // 2026-06-17
            new("Powers/Player/MrFantastic/SignatureCrushingLeap.prototype",            true,  0.0362f), // 2026-06-17
            new("Powers/Player/MrFantastic/SignatureHiddenPassiveRanks.prototype",      false, 0.02f),
            new("Powers/Player/MrFantastic/SignatureMicroNullifier.prototype",          true,  0.0144f), // 2026-06-17
            new("Powers/Player/MrFantastic/StretchyBrain.prototype",                    true,  0.05f),
            new("Powers/Player/MrFantastic/StretchyDash.prototype",                     true,  0.1604f), // 2026-06-05
            new("Powers/Player/MrFantastic/Talents/BouncyBuild.prototype",              false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/BouncyExpandingPBAOE.prototype",     false, 0.03f),
            new("Powers/Player/MrFantastic/Talents/BouncyWhirlwind.prototype",          false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/ChargedPBAoEConeYank.prototype",     false, 0.1235f), // 2026-06-05
            new("Powers/Player/MrFantastic/Talents/CircularLogic.prototype",            false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/CombatTactics.prototype",            false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/CooldownSynergyBuff.prototype",      false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/DefensiveSpec.prototype",            false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/GadgetsAlwaysCrit.prototype",        false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/GiantGunBonus.prototype",            false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/HERBIE.prototype",                   false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/HammerFistBonus.prototype",          false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/ImplosionPulses.prototype",          false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/MicroNullifierBonus.prototype",      false, 0.05f),
            new("Powers/Player/MrFantastic/Talents/TeslaCoilGadget.prototype",          false, 0.05f),
            new("Powers/Player/MrFantastic/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/MrFantastic/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/MrFantastic/UltimateFantasticFour.prototype",            true,  0.006f),
            new("Powers/Player/MrFantastic/Whirlwind.prototype",                        true,  0.0107f), // 2026-06-05
            new("Powers/Player/TravelPower/MrFantasticSprint.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MrFantasticStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",              false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",             false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                   false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                         false, 0.05f),
        };
    }
}
