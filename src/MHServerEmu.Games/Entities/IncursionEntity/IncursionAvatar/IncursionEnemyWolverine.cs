using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Wolverine Invader
    /// Powers: 15 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyWolverine : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Wolverine.prototype");

        public IncursionEnemyWolverine(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Wolverine Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Wolverine/AllNewMarvelNOW.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Brood.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Brown.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/BrownVU.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/DaysOfFuturePast.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/DaysOfFuturePastVU.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/EOTS.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/FearItself.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Hydra.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/LeatherFang.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/LoganYakuza.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/ModernYellow.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/OldManLogan.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/OldManLoganVU.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Patch.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Ronin.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/Symbiote.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/WeaponX.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/XForce.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/XForceVU.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/XMenUniform.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Wolverine/XMenUniformVU.prototype",      true),
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
            new("Powers/Player/TravelPower/WolverineRide.prototype",                           false, 0.05f),
            new("Powers/Player/Wolverine/BasicRonin.prototype",                                true,  0.1373f), // 2026-06-20
            new("Powers/Player/Wolverine/BerserkerBarrage.prototype",                          true,  0.0392f), // 2026-06-20
            new("Powers/Player/Wolverine/BloodySteroid.prototype",                             true,  0.05f),
            new("Powers/Player/Wolverine/Dunk.prototype",                                      true,  0.0368f), // 2026-06-20
            new("Powers/Player/Wolverine/FlyingBleed.prototype",                               false, 0.05f),
            new("Powers/Player/Wolverine/Frenzy.prototype",                                    true,  0.0547f), // 2026-06-20
            new("Powers/Player/Wolverine/Impale.prototype",                                    true,  0.05f),
            new("Powers/Player/Wolverine/Lunge.prototype",                                     true,  0.1114f), // 2026-06-20
            new("Powers/Player/Wolverine/PBAoE.prototype",                                     true,  0.0867f), // 2026-06-20
            new("Powers/Player/Wolverine/RapidRegeneration.prototype",                         true,  0.05f),
            new("Powers/Player/Wolverine/Rawr.prototype",                                      true,  0.05f),
            new("Powers/Player/Wolverine/RunThrough.prototype",                                true,  0.0313f), // 2026-06-20
            new("Powers/Player/Wolverine/SignatureDashSlash.prototype",                        true,  0.0497f), // 2026-06-20
            new("Powers/Player/Wolverine/SliceNDice.prototype",                                true,  0.1004f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent1FuryGenSpenderDmg.prototype",          false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent1GreivousWoundsFuryBleedDmg.prototype", false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent1PassiveCombatFury.prototype",          false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent2ImpaleBrut.prototype",                 false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent2PBAoEAddBleed.prototype",              false, 0.0867f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent2TornadoClawCharges.prototype",         false, 0.0280f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent3DunkBleedDmg.prototype",               false, 0.0368f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent3PassiveDmg.prototype",                 false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent3RampageBuffs.prototype",               false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent4BasicBleedVuln.prototype",             false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent4PBAoEDmgCDCrit.prototype",             false, 0.0867f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent4RunThroughFuryDmg.prototype",          false, 0.0313f), // 2026-06-20
            new("Powers/Player/Wolverine/Talents/Talent5AutoWetwork.prototype",                false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent5CantKeepMeDown.prototype",             false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent5FeralRoarRapidRegen.prototype",        false, 0.05f),
            new("Powers/Player/Wolverine/TornadoClaw.prototype",                               true,  0.0280f), // 2026-06-20
            new("Powers/Player/Wolverine/Traits/DefenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Wolverine/Traits/MechanicTrait.prototype",                      false, 0.05f),
            new("Powers/Player/Wolverine/Traits/OffenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Wolverine/Ultimate.prototype",                                  true,  0.006f),
            new("Powers/StolenPowers/StealablePowers/WolverineStolenPower.prototype",          false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                            false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                    false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                       false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                          false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                false, 0.05f),
        };
    }
}
