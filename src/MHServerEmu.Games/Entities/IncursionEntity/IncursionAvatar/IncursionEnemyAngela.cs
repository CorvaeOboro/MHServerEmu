using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Angela Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyAngela : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Angela.prototype");

        public IncursionEnemyAngela(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Angela Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Angela/AsgardAssassin.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Angela/Marvel1602.prototype",     true),
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
            new("Powers/Player/Angela/BowPBAoE.prototype",                               true,  0.0948f), // 2026-06-11
            new("Powers/Player/Angela/Constrict.prototype",                              true,  0.0562f), // 2026-06-10
            new("Powers/Player/Angela/DeathFromAbove.prototype",                         true,  0.0318f), // 2026-07-30
            new("Powers/Player/Angela/DisablingRibbons.prototype",                       true,  0.0704f), // 2026-06-10
            new("Powers/Player/Angela/DoubleAxeThrow.prototype",                         true,  0.0494f), // 2026-07-30
            new("Powers/Player/Angela/ExecuteChop.prototype",                            true,  0.0203f), // 2026-07-05
            new("Powers/Player/Angela/HackSlash.prototype",                              true,  0.2137f), // 2026-06-10
            new("Powers/Player/Angela/IchorBasic.prototype",                             true,  0.1504f), // 2026-06-11
            new("Powers/Player/Angela/MiraculousAssaultStart.prototype",                 true,  0.0406f), // 2026-07-30
            new("Powers/Player/Angela/RibbonChannel.prototype",                          true,  0.0578f), // 2026-06-11
            new("Powers/Player/Angela/RibbonDancer.prototype",                           true,  0.05f),
            new("Powers/Player/Angela/SigNoMatchStart.prototype",                        true,  0.0158f), // 2026-07-30
            new("Powers/Player/Angela/SpartaKick.prototype",                             true,  0.0352f), // 2026-06-20
            new("Powers/Player/Angela/SwordLunge.prototype",                             true,  0.3052f), // 2026-07-30
            new("Powers/Player/Angela/SwordPummel.prototype",                            true,  0.0416f), // 2026-07-30
            new("Powers/Player/Angela/Talents/AutoDisablingRibbons.prototype",           false, 0.0762f), // 2026-06-10
            new("Powers/Player/Angela/Talents/AxeBuffs.prototype",                       false, 0.05f),
            new("Powers/Player/Angela/Talents/DFAExtraHitTalent.prototype",              false, 0.05f),
            new("Powers/Player/Angela/Talents/HevensWrathDamageBoost.prototype",         false, 0.05f),
            new("Powers/Player/Angela/Talents/HybridTreeModTalent.prototype",            false, 0.05f),
            new("Powers/Player/Angela/Talents/RibbonBuffs.prototype",                    false, 0.05f),
            new("Powers/Player/Angela/Talents/RibbonsCooldownReductionTalent.prototype", false, 0.05f),
            new("Powers/Player/Angela/Talents/SignatureAllRibbons.prototype",            false, 0.02f),
            new("Powers/Player/Angela/Talents/SignatureAllSword.prototype",              false, 0.02f),
            new("Powers/Player/Angela/Talents/SwordBuffs.prototype",                     false, 0.05f),
            new("Powers/Player/Angela/Talents/SwordLungeSlowTalent.prototype",           false, 0.1496f), // 2026-07-08
            new("Powers/Player/Angela/Talents/WeaponsCooldownReductionTalent.prototype", false, 0.05f),
            new("Powers/Player/Angela/Talents/WhippingRibbonsReflectTalent.prototype",   false, 0.1138f), // 2026-07-08
            new("Powers/Player/Angela/Talents/WhippingRibbonsSpeedTalent.prototype",     false, 0.1138f), // 2026-07-08
            new("Powers/Player/Angela/Talents/WhippingRibbonsYankTalent.prototype",      false, 0.1138f), // 2026-07-08
            new("Powers/Player/Angela/Traits/DefenseTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Angela/Traits/MechanicTraitWhippingRibbons.prototype",    false, 0.1138f), // 2026-07-08
            new("Powers/Player/Angela/Traits/OffensiveTrait.prototype",                  false, 0.05f),
            new("Powers/Player/Angela/Ultimate.prototype",                               true,  0.0053f), // 2026-06-10
            new("Powers/Player/Angela/WhippingRibbons.prototype",                        true,  0.1328f), // 2026-07-30
            new("Powers/Player/TravelPower/AngelaFlight.prototype",                      false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/AngelaStolenPower.prototype",       false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",              false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                 false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                    false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                          false, 0.05f),
        };
    }
}
