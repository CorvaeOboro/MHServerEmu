using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Trash Mob
    /// Purifier Axe Butcher corrupted by vampire blood. Rendered as a DRPurifierAxeSpawn.
    /// Enrages at 50% HP: 2x faster cooldowns.
    /// Gets a random vampire name and a red buff power for visual aura.
    /// </summary>
    public class CalamityEnemyVampireThrallButcher : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Mobs/Purifiers/DangerRoom/DRPurifierAxeSpawn.prototype");
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

        public CalamityEnemyVampireThrallButcher(Game game) : base(game)
        {
            _vampireName = _vampireNames[Game.Random.Next(0, _vampireNames.Length)];
        }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => _vampireName;
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "ThrallButcher";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 250f;  // generous range for lunge gap-closer
        protected override float ChaseRange => 99999f;
        protected override float GlobalAttackCooldownMs => 150f;
        protected override float PerPowerCooldownMs => 2000f;
        protected override float DamageScale => 0.353f;
        protected override float DamageTakenMultiplier => 7.0f;

        // Native powers from DRPurifierAxeSpawn prototype (from server logs):
        //   PurifierAxeSwipe          - basic melee (activation=None, can't be activated)
        //   LeashReturnHeal           - non-combat toggle
        //   LeashReturnNegStatusEffectImmune - non-combat toggle
        // PurifierSpearLunge is an activated melee gap-closer from the Purifier mob power family.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/Purifiers/PurifierSpearLunge.prototype", true, 0.353f),
            new("Powers/EnemyPowers/MobPowers/Purifiers/PurifierAxeSwipe.prototype", false, 0.353f),  // activation=None, can't be activated
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype", false, 0.53f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype", false, 0.53f),
        };

        // No death VFX (teleport beam, vaporization) for thralls - they just die normally.
        protected override int DeathGracePeriodMs => 0;

        // Trash mobs: disable impatience and stuck recovery.
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
