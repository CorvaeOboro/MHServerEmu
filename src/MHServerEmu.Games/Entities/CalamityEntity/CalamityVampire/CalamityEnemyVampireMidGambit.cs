using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Gambit as a vampire, wearing the Death costume.
    /// Renders as the Gambit avatar so animations play correctly on the pawn.
    /// Power table and damage scaling copied from IncursionEnemyGambit.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidGambit : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Gambit.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/Gambit/Death.prototype");

        public CalamityEnemyVampireMidGambit(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;
        public override string InvaderDisplayName => "Vampire Gambit";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidGambit";

        // Base attributes from IncursionEnemyGambit
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 99999f;       // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageTakenMultiplier => 2.0f;  // vampire-specific

        // Use BossNoOverheadInfo rank: boss-level health, no blue champion glow, no minimap marker.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Powers and damage scaling from IncursionEnemyGambit
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Gambit/AceOfSpades.prototype",                       true,  0.0446f),
            new("Powers/Player/Gambit/BasicBoStrike.prototype",                     true,  0.0960f),
            new("Powers/Player/Gambit/BasicKineticCard.prototype",                  true,  0.1909f),
            new("Powers/Player/Gambit/BasicKineticCardHiddenPassive.prototype",     false, 0.05f),
            new("Powers/Player/Gambit/BatterUp.prototype",                          true,  0.0500f), // 2026-07-29
            new("Powers/Player/Gambit/BoBeatdown.prototype",                        true,  0.1378f), // 2026-07-29
            new("Powers/Player/Gambit/BoVault.prototype",                           true,  0.1503f),
            new("Powers/Player/Gambit/BoWhirlwind.prototype",                       true,  0.0937f), // 2026-07-29
            new("Powers/Player/Gambit/CardPickup.prototype",                        true,  0.0261f), // 2026-07-31
            new("Powers/Player/Gambit/ChargeUpCard.prototype",                      true,  0.05f),
            new("Powers/Player/Gambit/FoldEm.prototype",                            true,  0.0934f), // 2026-07-31
            new("Powers/Player/Gambit/GrandSlam.prototype",                         true,  0.0348f),
            new("Powers/Player/Gambit/JacksOrBetter.prototype",                     true,  0.2055f),
            new("Powers/Player/Gambit/RaininPain.prototype",                        true,  0.0903f),
            new("Powers/Player/Gambit/RoyalFlush.prototype",                        true,  0.1329f), // 2026-07-29
            new("Powers/Player/Gambit/StreetSweep.prototype",                       true,  0.1047f), // 2026-07-29
            new("Powers/Player/Gambit/Talents/Talent1LessDowntime.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent1LongerBurn.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent1RaginCajun.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2CheatDeath.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2KingOfHearts.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2SleightOfHand.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3DeucesWild.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3ShuffleUpAndDeal.prototype",   false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3ThreeOfAKind.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4AllOutOfCards.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4JacksOrBetter.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4StackTheDeck.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfClubs.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfDiamonds.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfHearts.prototype",        false, 0.05f),
            new("Powers/Player/Gambit/Traits/DefenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Gambit/Traits/MechanicTraitKineticEnergy.prototype", false, 0.05f),
            new("Powers/Player/Gambit/Traits/OffenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Gambit/Tumble.prototype",                            true,  0.05f),
            new("Powers/Player/Gambit/Ultimate.prototype",                          true,  0.0220f), // 2026-07-31
            new("Powers/Player/TravelPower/GambitSprint.prototype",                 false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GambitStolenPower.prototype",  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",            false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",               false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                     false, 0.05f),
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
