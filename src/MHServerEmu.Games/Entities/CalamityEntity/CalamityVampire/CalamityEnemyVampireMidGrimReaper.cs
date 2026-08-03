using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Grim Reaper as a death-themed vampire. Renders as GrimReaperBase.
    /// Applies rage/fury/wrath passive conditions on spawn for red aura VFX.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidGrimReaper : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/GrimReaperBase.prototype");
                return _bossRef;
            }
        }

        public CalamityEnemyVampireMidGrimReaper(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Vampire Grim Reaper";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidGrimReaper";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 120f;       // melee: run close before attacking
        protected override float ChaseRange => 99999f;      // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 150f;  // attack frequently
        protected override float PerPowerCooldownMs => 4000f;
        protected override float DamageTakenMultiplier => 1f;

        // Explicit power table from GrimReaperBase.
        // ComboEffect powers (GrimEnergyBlast2/3) are children of GrimEnergyBlast, disabled.
        // GrimReaperResurrection is a self-revive, disabled.
        // GrimReaperTeleport is a movement power, enabled.
        // LeashReturn powers are non-combat toggles, disabled.
        // CanMoveDuringPower=true for combat attacks so he can chase while channeling.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimEnergyBlast.prototype",       true,  0.8f, canMoveDuringPower: false),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimReaperBladedFlurry.prototype", true,  0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimScytheRoundhouse.prototype",   true,  0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimEnergyBlast2.prototype",       false, 0.8f),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimEnergyBlast3.prototype",       false, 0.8f),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimReaperResurrection.prototype", false, 0.8f),
            new("Powers/EnemyPowers/Boss/GrimReaper/GrimReaperTeleport.prototype",     true, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                 false, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype", false, 0.8f),
        };

        // Hide minimap markers and remove champion glow (blue aura).
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Enrage at 50% HP
        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;

        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;
        protected override float DamageScale => CurrentPhase == 1 ? 1.2f : 0.8f;

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);
        }
    }
}
