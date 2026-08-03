using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Black Widow as a vampire, wearing the Original costume.
    /// Renders as the Black Widow avatar so animations play correctly on the pawn.
    /// Power table and damage scaling copied from IncursionEnemyBlackWidow.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidBlackWidow : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/BlackWidow.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/BlackWidow/Original.prototype");

        public CalamityEnemyVampireMidBlackWidow(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;
        public override string InvaderDisplayName => "Vampire Black Widow";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidBlackWidow";

        // Base attributes from IncursionEnemyBlackWidow
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 99999f;       // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageTakenMultiplier => 2.0f;  // vampire-specific

        // Use BossNoOverheadInfo rank: boss-level health, no blue champion glow, no minimap marker.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Powers and damage scaling from IncursionEnemyBlackWidow
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/BlackWidow/CoupDeGrace.prototype",                           true,  0.0653f),
            new("Powers/Player/BlackWidow/ElectricBatons.prototype",                        true,  0.1347f),
            new("Powers/Player/BlackWidow/FlashGrenade.prototype",                          true,  0.1813f), // 2026-07-30
            new("Powers/Player/BlackWidow/FlipKick.prototype",                              true,  0.0367f), // 2026-07-30
            new("Powers/Player/BlackWidow/Microdrones.prototype",                           true,  0.0757f), // 2026-07-30
            new("Powers/Player/BlackWidow/PBAoETaser.prototype",                            true,  0.1136f), // 2026-07-30
            new("Powers/Player/BlackWidow/PistolShot.prototype",                            true,  0.1968f),
            new("Powers/Player/BlackWidow/Plastique.prototype",                             true,  0.1243f), // 2026-07-30
            new("Powers/Player/BlackWidow/Punch.prototype",                                 true,  0.1714f), // 2026-07-30
            new("Powers/Player/BlackWidow/RapidShot.prototype",                             true,  0.8206f), // 2026-07-30
            new("Powers/Player/BlackWidow/RollingGrenades.prototype",                       true,  0.1357f),
            new("Powers/Player/BlackWidow/RoundhouseKick.prototype",                        true,  0.0298f),
            new("Powers/Player/BlackWidow/SniperShot.prototype",                            true,  0.05f),
            new("Powers/Player/BlackWidow/SweepingKick.prototype",                          true,  0.1084f), // 2026-07-30
            new("Powers/Player/BlackWidow/Talents/FightingFocus.prototype",                 false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/FlashGrenadeConductiveGrenade.prototype", false, 0.1958f),
            new("Powers/Player/BlackWidow/Talents/FlipKickExplosives.prototype",            false, 0.0272f),
            new("Powers/Player/BlackWidow/Talents/MicrodronesSecondWave.prototype",         false, 0.0775f),
            new("Powers/Player/BlackWidow/Talents/NeverKnowWhatHitThem.prototype",          false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/PBAoEChargeFullSpend.prototype",          false, 0.03f),
            new("Powers/Player/BlackWidow/Talents/PunchElectricBatons.prototype",           false, 0.1347f),
            new("Powers/Player/BlackWidow/Talents/PunchKnife.prototype",                    false, 0.0983f),
            new("Powers/Player/BlackWidow/Talents/PunchStingProc.prototype",                false, 0.0983f),
            new("Powers/Player/BlackWidow/Talents/RollingGrenadesBonus.prototype",          false, 0.01f),
            new("Powers/Player/BlackWidow/Talents/SniperNest.prototype",                    false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleAcrobaticAttack.prototype",         false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleHaste.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleKineticBattery.prototype",          false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/WidowsBootSpec.prototype",                false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/MechanicTraitElectricCharge.prototype",    false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Tumble.prototype",                                false,  0.05f),
            new("Powers/Player/BlackWidow/TwilightInitiative.prototype",                    false,  0.0301f),
            new("Powers/Player/BlackWidow/Ultimate.prototype",                              false,  0.0243f),
            new("Powers/Player/BlackWidow/WidowsBite.prototype",                            true,  0.0677f),
            new("Powers/Player/BlackWidow/WidowsKiss.prototype",                            true,  0.0564f), // 2026-07-30
            new("Powers/Player/TravelPower/BlackWidowRide.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackWidowStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
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
