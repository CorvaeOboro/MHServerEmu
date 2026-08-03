using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Summoned Trash Mob
    /// A basic vampire thrall summoned by the BloodLord during phase 2.
    /// Same combat tuning as regular thralls but without a nameplate proxy -
    /// these are disposable adds that don't need individual nameplates.
    /// </summary>
    public class CalamityEnemyVampireThrallSummoned : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Mobs/DarkElves/DarkElfSoldierBase.prototype");
                return _bossRef;
            }
        }

        // Buff power that produces a red visual aura.
        private static readonly PrototypeId[] _buffPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype"),
        };

        public CalamityEnemyVampireThrallSummoned(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Vampire Thrall";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "ThrallSummoned";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 400f;
        protected override float ChaseRange => 99999f;
        protected override float GlobalAttackCooldownMs => 500f;
        protected override float PerPowerCooldownMs => 1500f;
        protected override float DamageScale => 0.353f;
        protected override float DamageTakenMultiplier => 7.0f;

        // Explicit power table: same single ranged power as DarkElf thrall.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/DarkElves/DESoldierRangedSkillshot.prototype", true, 0.353f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype", false, 0.53f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype", false, 0.53f),
        };

        protected override int DeathGracePeriodMs => 0;

        // Trash mobs: disable impatience and stuck recovery.
        protected override bool EnableImpatience => false;
        protected override bool EnableStuckRecovery => false;

        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Summoned thralls are disposable adds - no nameplate proxy needed.
        public override bool NeedsNameplateProxy => false;

        // Summoned thralls drop no loot.
        protected override void ApplyLootPool(Agent agent) { RemoveDeathLootTables(agent); }

        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;
        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);
            ApplyConditionFromPower(agent, _buffPowers[0]);
        }
    }
}
