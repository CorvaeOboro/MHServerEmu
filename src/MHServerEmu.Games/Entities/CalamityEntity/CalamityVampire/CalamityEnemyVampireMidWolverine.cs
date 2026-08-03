using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Wolverine as a vampire, wearing the X-Force costume.
    /// Renders as the Wolverine avatar so animations play correctly on the pawn.
    /// Power table and damage scaling copied from IncursionEnemyWolverine.
    /// Fast, aggressive, high chase range.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidWolverine : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Wolverine.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/Wolverine/XForce.prototype");

        public CalamityEnemyVampireMidWolverine(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;
        public override string InvaderDisplayName => "Vampire Wolverine";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidWolverine";

        // Base attributes from IncursionEnemyWolverine
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 99999f;       // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageTakenMultiplier => 2.0f;  // vampire-specific
        protected override float MovementSpeedMult => 1.5f;      // vampire: fast

        // Use BossNoOverheadInfo rank: boss-level health, no blue champion glow, no minimap marker.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Powers and damage scaling from IncursionEnemyWolverine
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/TravelPower/WolverineRide.prototype",                           false, 0.05f),
            new("Powers/Player/Wolverine/BasicRonin.prototype",                                true,  0.1573f),
            new("Powers/Player/Wolverine/BerserkerBarrage.prototype",                          true,  0.0333f), // 2026-08-01
            new("Powers/Player/Wolverine/BloodySteroid.prototype",                             true,  0.05f),
            new("Powers/Player/Wolverine/Dunk.prototype",                                      true,  0.0341f),
            new("Powers/Player/Wolverine/FlyingBleed.prototype",                               false, 0.05f),
            new("Powers/Player/Wolverine/Frenzy.prototype",                                    true,  0.0573f),
            new("Powers/Player/Wolverine/Impale.prototype",                                    true,  0.05f),
            new("Powers/Player/Wolverine/Lunge.prototype",                                     true,  0.1556f), // 2026-08-01
            new("Powers/Player/Wolverine/PBAoE.prototype",                                     true,  0.1081f),
            new("Powers/Player/Wolverine/RapidRegeneration.prototype",                         true,  0.05f),
            new("Powers/Player/Wolverine/Rawr.prototype",                                      true,  0.05f),
            new("Powers/Player/Wolverine/RunThrough.prototype",                                true,  0.0245f), // 2026-07-30
            new("Powers/Player/Wolverine/SignatureDashSlash.prototype",                        true,  0.0632f), // 2026-08-01
            new("Powers/Player/Wolverine/SliceNDice.prototype",                                true,  0.1208f),
            new("Powers/Player/Wolverine/Talents/Talent1FuryGenSpenderDmg.prototype",          false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent1GreivousWoundsFuryBleedDmg.prototype", false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent1PassiveCombatFury.prototype",          false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent2ImpaleBrut.prototype",                 false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent2PBAoEAddBleed.prototype",              false, 0.1081f),
            new("Powers/Player/Wolverine/Talents/Talent2TornadoClawCharges.prototype",         false, 0.0270f),
            new("Powers/Player/Wolverine/Talents/Talent3DunkBleedDmg.prototype",               false, 0.0341f),
            new("Powers/Player/Wolverine/Talents/Talent3PassiveDmg.prototype",                 false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent3RampageBuffs.prototype",               false, 0.02f),
            new("Powers/Player/Wolverine/Talents/Talent4BasicBleedVuln.prototype",             false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent4PBAoEDmgCDCrit.prototype",             false, 0.1081f),
            new("Powers/Player/Wolverine/Talents/Talent4RunThroughFuryDmg.prototype",          false, 0.0293f),
            new("Powers/Player/Wolverine/Talents/Talent5AutoWetwork.prototype",                false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent5CantKeepMeDown.prototype",             false, 0.05f),
            new("Powers/Player/Wolverine/Talents/Talent5FeralRoarRapidRegen.prototype",        false, 0.05f),
            new("Powers/Player/Wolverine/TornadoClaw.prototype",                               true,  0.0642f), // 2026-07-31
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

        // Enrage at 50% HP
        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;

        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;
        protected override float DamageScale => CurrentPhase == 1 ? 0.06f : 0.05f;  // 0.05f base, 1.2x enrage

        // AvatarOfCyttorak gives a red visual aura on spawn.
        private static readonly PrototypeId _buffPower =
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype");

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);

            // Apply red aura buff condition (no animation, just visual VFX).
            ApplyConditionFromPower(agent, _buffPower);

            // Immediately fire a basic power to snap the avatar animation from T-pose.
            if (Powers.Count > 0)
                ActivatePowerAtPosition(agent, Powers[0], agent.RegionLocation.Position, skipActivationCheck: true);
        }
    }
}
