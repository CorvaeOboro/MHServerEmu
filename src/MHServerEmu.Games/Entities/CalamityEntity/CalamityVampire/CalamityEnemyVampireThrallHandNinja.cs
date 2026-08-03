using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Trash Mob
    /// Hand Ninja thrall corrupted by vampire blood. Rendered as a Hand Ninja mob
    /// but tuned down to trash-tier damage and survivability.
    /// Enrages at 50% HP: 2x faster cooldowns.
    /// Gets a random vampire name and a red buff power for visual aura.
    /// </summary>
    public class CalamityEnemyVampireThrallHandNinja : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Mobs/Hand/Redacted/ZZZHandNinjaBase.prototype");
                return _bossRef;
            }
        }

        // Buff power that produces a red visual aura.
        private static readonly PrototypeId[] _buffPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype"),
        };

        // Random vampire-themed names for thralls.
        private static readonly string[] _vampireNames = new[]
        {
            "Vampire Thrall",
        };

        private string _vampireName;

        public CalamityEnemyVampireThrallHandNinja(Game game) : base(game)
        {
            _vampireName = _vampireNames[Game.Random.Next(0, _vampireNames.Length)];
        }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => _vampireName;
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "ThrallHandNinja";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 15f;   // melee range - HandSwordSlash is a melee power
        protected override float ChaseRange => 99999f;
        protected override float GlobalAttackCooldownMs => 150f;
        protected override float PerPowerCooldownMs => 800f;
        protected override float DamageScale => 0.53f;
        protected override float DamageTakenMultiplier => 7.0f;

        // Explicit power table: HandSwordSlash has activation=None (basic mob attack, can't be
        // activated via ActivatePower). Instead we use HandWarriorWhirlwind (melee AoE) and
        // HandFuriousLunge (gap-closer melee) which are proper activated powers.
        // HandNinjaVanish is movement (excluded). LeashReturn powers are non-combat toggles.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/Hand/HandWarriorWhirlwind.prototype", true, 0.53f),
            new("Powers/EnemyPowers/MobPowers/Hand/HandFuriousLunge.prototype", true, 0.53f),
            new("Powers/EnemyPowers/MobPowers/Hand/HandSwordSlash.prototype", false, 0.53f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype", false, 0.53f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype", false, 0.53f),
        };

        // No death VFX (teleport beam, vaporization) for thralls - they just die normally.
        protected override int DeathGracePeriodMs => 0;

        // Trash mobs: disable impatience and stuck recovery - they spawn and wait,
        // then chase + attack when the player approaches. No need for aggressive
        // pressure mechanics that IncursionEnemies use when spawning near the player.
        protected override bool EnableImpatience => false;
        protected override bool EnableStuckRecovery => false;

        // Use BossNoOverheadInfo rank: hides the default white name and removes the
        // champion blur glow. The red vampire name is shown via the nameplate proxy.
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Regular thralls get a nameplate proxy for the red prestige nameplate.
        public override bool NeedsNameplateProxy => true;

        // Thralls drop no loot at all - strip native mob loot tables.
        protected override void ApplyLootPool(Agent agent) { RemoveDeathLootTables(agent); }

        // Enrage at 50% HP
        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;

        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);

            // Apply the buff power as a passive condition for red visual aura.
            ApplyConditionFromPower(agent, _buffPowers[0]);
        }
    }
}
