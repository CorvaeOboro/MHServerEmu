using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Malekith corrupted by vampire blood. Renders as the Ch9 boss entity.
    /// Applies size-increase and AvatarOfCyttorak conditions on spawn for red aura VFX.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// </summary>
    public class CalamityEnemyVampireMidMalekith : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MalekithCh9.prototype");
                return _bossRef;
            }
        }

        public CalamityEnemyVampireMidMalekith(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Vampire Malekith";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidMalekith";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 120f;       // melee: run close before attacking
        protected override float ChaseRange => 99999f;      // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 150f;  // attack frequently
        protected override float PerPowerCooldownMs => 4000f;
        protected override float DamageTakenMultiplier => 2.0f;

        // Explicit power table from MalekithCh9.
        // ComboEffect powers (ElektraShadowStrikeReappear, DeathFromAboveDrop) are children, disabled.
        // MalekithSummonDarkElves is a summon power, disabled.
        // LeashReturn powers are non-combat toggles, disabled.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Malekith/MalekithChargeStart.prototype",        true,  0.8f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDarkBeam.prototype",           true,  1.1922f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDarkShroudStart.prototype",    true,  1.3045f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Malekith/MalekithSummonVoidHotspot.prototype",   true,  0.8896f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDeathFromAboveStart.prototype", false, 0.8f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithSummonDarkElves.prototype",     false, 0.8f),
            new("Powers/EnemyPowers/Boss/Elektra/ElektraShadowStrikeReappear.prototype",  false, 0.8f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDeathFromAboveDrop.prototype",  false, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",    false, 0.8f),
        };

        // Buff power that produces a red visual aura.
        private static readonly PrototypeId[] _buffPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype"),
        };

        // Hide minimap markers and remove champion glow (blue aura).
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Enrage at 50% HP
        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;

        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;
        protected override float DamageScale => CurrentPhase == 1 ? 1.0f : 0.8f;

        // MalekithCh9 has a story-mode on-death summon proc passive that prevents
        // proper death cleanup outside its chapter context. Strip it so the Incursion
        // death sequence (teleport beam + vaporization VFX + destroy) handles cleanup.
        private static readonly PrototypeId _onDeathSummonProc =
            GameDatabase.GetPrototypeRefByName("Powers/EnemyPowers/Boss/Malekith/MalekithOnDeathSummonProc.prototype");

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);
            ApplyConditionFromPower(agent, _buffPowers[0]);

            // Remove the story-mode on-death summon passive so it doesn't interfere
            // with the Incursion death sequence and leave the model lingering.
            if (agent.PowerCollection != null && agent.GetPower(_onDeathSummonProc) != null)
                agent.UnassignPower(_onDeathSummonProc);
        }
    }
}
