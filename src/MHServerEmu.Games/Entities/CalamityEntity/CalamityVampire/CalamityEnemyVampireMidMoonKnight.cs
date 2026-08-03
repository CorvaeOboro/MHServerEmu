using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Moon Knight as a vampire, wearing the Secret Avengers costume.
    /// Renders as the Moon Knight avatar so animations play correctly on the pawn.
    /// Power table and damage scaling copied from IncursionEnemyMoonKnight.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidMoonKnight : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/MoonKnight.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/MoonKnight/SecretAvengers.prototype");

        public CalamityEnemyVampireMidMoonKnight(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;
        public override string InvaderDisplayName => "Vampire Moon Knight";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidMoonKnight";

        // Base attributes from IncursionEnemyMoonKnight
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 99999f;       // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageTakenMultiplier => 2.0f;  // vampire-specific

        // Use BossNoOverheadInfo rank: boss-level health, no blue champion glow, no minimap marker.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Powers and damage scaling from IncursionEnemyMoonKnight
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/MoonKnight/BasicCrescentDart.prototype",                             true,  0.2319f), // 2026-07-29
            new("Powers/Player/MoonKnight/BasicGauntletPunch.prototype",                            true,  0.1190f),
            new("Powers/Player/MoonKnight/BasicStaffStrike.prototype",                              true,  0.1284f), // 2026-07-31
            new("Powers/Player/MoonKnight/CestusGauntletPunch.prototype",                           true,  0.0751f),
            new("Powers/Player/MoonKnight/ConeYank.prototype",                                      true,  0.0989f), // 2026-07-31
            new("Powers/Player/MoonKnight/CrescentBola.prototype",                                  true,  0.2607f), // 2026-07-31
            new("Powers/Player/MoonKnight/CrescentDartFan.prototype",                               true,  0.0722f), // 2026-07-31
            new("Powers/Player/MoonKnight/DeathFromAbove.prototype",                                false,  0.0567f),
            new("Powers/Player/MoonKnight/HighlightSteroids.prototype",                             true,  0.05f),
            new("Powers/Player/MoonKnight/KhonshuSteroidHealth.prototype",                          false,  0.05f),
            new("Powers/Player/MoonKnight/NunchuckBulldoze.prototype",                              true,  0.0389f), // 2026-07-31
            new("Powers/Player/MoonKnight/RapidFire.prototype",                                     true,  0.2576f), // 2026-07-29
            new("Powers/Player/MoonKnight/Ricochet.prototype",                                      true,  0.0811f),
            new("Powers/Player/MoonKnight/SignatureFrenzy.prototype",                               true,  0.0633f),
            new("Powers/Player/MoonKnight/StaffPBAoE.prototype",                                    true,  0.0343f), // 2026-07-29
            new("Powers/Player/MoonKnight/Strafe.prototype",                                        true,  0.0679f), // 2026-07-29
            new("Powers/Player/MoonKnight/SummonKhonshuStatue.prototype",                           true,  0.05f),
            new("Powers/Player/MoonKnight/Talents/AngelwingStrafeDFACharges.prototype",             false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/BasicCrescentExplosiveRapidFireBounce.prototype", false, 0.0957f),
            new("Powers/Player/MoonKnight/Talents/BrutalChanceTerrify.prototype",                   false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/CestusPunchLayer.prototype",                      false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/CestusUppercutTribute.prototype",                 false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/ConeYankNunchuckBulldozeBonus.prototype",         false, 0.1255f),
            new("Powers/Player/MoonKnight/Talents/CrescentFanCooldown.prototype",                   false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/HealthDefenseSelfRez.prototype",                  false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuStatueSteroidCombined.prototype",          false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuStatueTerrify.prototype",                  false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuSteroidCastSpeedMult.prototype",           false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/RangedSignature.prototype",                       false, 0.02f),
            new("Powers/Player/MoonKnight/Talents/RicochetCharges.prototype",                       false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/SignatureTributeGain.prototype",                  false, 0.02f),
            new("Powers/Player/MoonKnight/Talents/StaffPBAoEBleed.prototype",                       false, 0.0325f),
            new("Powers/Player/MoonKnight/Traits/DefenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/MoonKnight/Traits/MechanicTrait.prototype",                          false, 0.05f),
            new("Powers/Player/MoonKnight/Traits/OffenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/MoonKnight/Tumble.prototype",                                        true,  0.05f),
            new("Powers/Player/MoonKnight/Ultimate.prototype",                                      true,  0.006f),
            new("Powers/Player/MoonKnight/UltimateHiddenPassive.prototype",                         false, 0.006f),
            new("Powers/Player/TravelPower/MoonKnightFlight.prototype",                             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MoonKnightStolenPower.prototype",              false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                          false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                            false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                               false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                     false, 0.05f),
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
