using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Miniboss
    /// Lady Deathstrike corrupted by vampire blood. Renders as the Ch8 boss entity.
    /// Applies rage/fury/wrath passive conditions on spawn for red aura VFX.
    /// Enrages at 50% HP: 2x faster cooldowns, 1.5x damage.
    /// Summons 2-3 vampire thrall henchmen on spawn.
    /// </summary>
    public class CalamityEnemyVampireMidLadyDeathstrike : IncursionEnemyBoss
    {
        private static PrototypeId _bossRef = PrototypeId.Invalid;
        private static PrototypeId BossRef
        {
            get
            {
                if (_bossRef == PrototypeId.Invalid)
                    _bossRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolMidtown/MidtownEventLadyDeathstrike.prototype");
                return _bossRef;
            }
        }

        public CalamityEnemyVampireMidLadyDeathstrike(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Vampire Lady Deathstrike";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "MidLadyDeathstrike";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 120f;       // melee: run close before attacking
        protected override float ChaseRange => 99999f;      // vampire: infinite chase
        protected override float GlobalAttackCooldownMs => 150f;  // attack frequently
        protected override float PerPowerCooldownMs => 4000f;
        protected override float DamageTakenMultiplier => 1.5f;

        // Explicit power table from MidtownEventLadyDeathstrike.
        // ComboEffect powers (ClawSlash2/3) are children of ClawSlash, disabled.
        // RapidRegenChanneled is self-healing, disabled.
        // Slashthrough is a movement power, disabled.
        // LeashReturn powers are non-combat toggles, disabled.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;
        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash.prototype",             true,  0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/BladedFlurry.prototype",           true,  0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/RapidRegenChanneled.prototype",    false, 0.8f),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash2.prototype",             true, 0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/ClawSlash3.prototype",             true, 0.8f, canMoveDuringPower: true),
            new("Powers/EnemyPowers/Boss/LadyDeathstrike/Slashthrough.prototype",           true, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                      false, 0.8f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",      false, 0.8f),
        };

        // Hide minimap markers and remove champion glow (blue aura).
        protected override bool UseBossRank => false;
        protected override PrototypeId RankOverride => ResolveBossNoOverheadRank();

        // Henchmen: 2-3 vampire Dark Elf thralls
        protected override IncursionHenchmanEntry[] HenchmenPool => _henchmen;
        private static readonly IncursionHenchmanEntry[] _henchmen =
        {
            new("Entity/Characters/Mobs/DarkElves/DarkElfSoldierBase.prototype", 2, 3,
                "Mods/Ranks/BossNoOverheadInfo.prototype"),
        };

        // Enrage at 50% HP
        protected override int GetPhaseForHealthPct(float healthPct) => healthPct < 0.5f ? 1 : 0;

        protected override float PhaseCooldownScale() => CurrentPhase == 1 ? 0.5f : 1.0f;
        protected override float DamageScale => CurrentPhase == 1 ? 1.2f : 0.8f;

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);
            ApplyConditionFromPower(agent, _buffPowers[0]);
        }

        // Buff power that produces a red visual aura (AvatarOfCyttorak ).
        // Other candidates commented out for individual evaluation.
        private static readonly PrototypeId[] _buffPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/ImInvulnerable.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Wolverine/BloodySteroid.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Blade/SerumInjection.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Blade/BloodlustHiddenPassive.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/TeamUps/Drax/RageSteroid.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Wolverine/Frenzy.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Thor/WarriorsWrath.prototype"),
        };

        protected override void OnHenchmanSpawned(Agent boss, WorldEntity henchman)
        {
            // Suppress map markers
            henchman.Properties[PropertyEnum.MapTracking] = false;
            if (henchman.IsInWorld && henchman.Region != null && henchman.Region.IsEntityDiscovered(henchman))
                henchman.Region.UndiscoverEntity(henchman, false);

            // Apply the buff power as a passive condition for red visual aura.
            if (henchman is Agent henchAgent)
            {
                ApplyConditionFromPower(henchAgent, _buffPowers[0]);
            }
        }
    }
}
