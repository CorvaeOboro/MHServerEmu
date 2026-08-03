using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Daredevil as a vampire, wearing the EarthX costume (caped devil).
    /// Renders as the Daredevil avatar so animations play correctly on the pawn.
    /// Power table and damage scaling copied from IncursionEnemyDaredevil.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidDaredevil : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Daredevil.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/Daredevil/EarthX.prototype");

        public CalamityEnemyVampireMidDaredevil(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;
        public override string InvaderDisplayName => "Vampire Daredevil";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidDaredevil";

        // Base attributes from IncursionEnemyDaredevil
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 99999f;       // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageTakenMultiplier => 2.0f;  // vampire-specific

        // Use BossNoOverheadInfo rank: boss-level health, no blue champion glow, no minimap marker.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Powers and damage scaling from IncursionEnemyDaredevil
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Daredevil/Talents/BouncingStrikeAdditionalHitsTalents.prototype", false, 0.05f),
            new("Powers/Player/Daredevil/Talents/BrutalStrikeFinisherCritDamage.prototype",      false, 0.05f),
            new("Powers/Player/Daredevil/Talents/ComboHealTalent.prototype",                     false, 0.025f),
            new("Powers/Player/Daredevil/Talents/ComboInvulnTalent.prototype",                   false, 0.025f),
            new("Powers/Player/Daredevil/Talents/DamageCritBrutBuffTalent.prototype",            false, 0.05f),
            new("Powers/Player/Daredevil/Talents/NoComboPointsTalent.prototype",                 false, 0.025f),
            new("Powers/Player/Daredevil/Talents/NormalPointsBuffTalent.prototype",              false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerCaneSlowTalent.prototype",                false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerClubWeakenTalent.prototype",              false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerNunchuckStunTalent.prototype",            false, 0.05f),
            new("Powers/Player/Daredevil/Talents/SigBuffTalent.prototype",                       false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SigCooldownReductionTalent.prototype",          false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SigDoubleDamageCenterTalent.prototype",         false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SlowComboPointTalent.prototype",                false, 0.025f),
            new("Powers/Player/Daredevil/Talents/WhirlingClubStaminaCancelTalent.prototype",     false, 0.1991f),
            new("Powers/Player/Daredevil/Traits/DefenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/Daredevil/Traits/MechanicTraitComboPoints.prototype",             false, 0.025f),
            new("Powers/Player/Daredevil/Traits/OffenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/Daredevil/Ultimate.prototype",                                    true,  0.0199f),
            new("Powers/Player/Daredevil/Update/BillyClubSweep.prototype",                       true,  0.1737f),
            new("Powers/Player/Daredevil/Update/BouncingStrike.prototype",                       true,  0.2171f), // 2026-08-01
            new("Powers/Player/Daredevil/Update/BrutalStrike.prototype",                         true,  0.05f),
            new("Powers/Player/Daredevil/Update/CaneAttack.prototype",                           true,  0.1715f),
            new("Powers/Player/Daredevil/Update/ClubAttack.prototype",                           true,  0.4821f), // 2026-08-01
            new("Powers/Player/Daredevil/Update/ClubRicochet.prototype",                         true,  0.05f),
            new("Powers/Player/Daredevil/Update/ComboPointGainMechanic.prototype",               true,  0.025f),
            new("Powers/Player/Daredevil/Update/ComboPointHiddenPassive.prototype",              false, 0.025f),
            new("Powers/Player/Daredevil/Update/ConeYank.prototype",                             true,  0.0689f),
            new("Powers/Player/Daredevil/Update/NunchuckAttack.prototype",                       true,  0.1504f),
            new("Powers/Player/Daredevil/Update/NunchuckBulldoze.prototype",                     true,  0.0564f), // 2026-08-01
            new("Powers/Player/Daredevil/Update/OpeningLunge.prototype",                         true,  0.0393f), // 2026-08-01
            new("Powers/Player/Daredevil/Update/RoundhouseKick.prototype",                       true,  0.05f),
            new("Powers/Player/Daredevil/Update/ShadowStrike.prototype",                         true,  0.0147f),
            new("Powers/Player/Daredevil/Update/Tumble.prototype",                               true,  0.05f),
            new("Powers/Player/Daredevil/Update/Vault.prototype",                                true,  0.05f),
            new("Powers/Player/Daredevil/Update/WhirlingClub.prototype",                         true,  0.1991f),
            new("Powers/Player/TravelPower/DaredevilFlight.prototype",                           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DaredevilStolenPower.prototype",            false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                              false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                      false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                         false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                            false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                  false, 0.05f),
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
