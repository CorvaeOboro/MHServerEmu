#region INCURSION AI
// =============================================================================
// MOD INCURSION 
// =============================================================================
//   INCURSION spawns hostile Avatars, teamup , Bosses , that randomly hunt players 
//   dangerous short encounters 
//
//   IncursionEnemyController 
//
//  VERSION:: 20260728
// =============================================================================
using MHServerEmu.Core.Extensions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.System.Random;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Locomotion;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;
using MHServerEmu.Games.Navi;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Powers.Conditions;
using MHServerEmu.Games.Properties;
using Gazillion;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Enemy Controller
    /// Base class for a mod-driven controller that binds to a spawned hostile
    /// <see cref="Agent"/> and runs a recurring think loop: target, chase, activate powers.
    /// Subclasses supply powers, tuning, and optional health-based phases.
    ///
    /// Visual identity is handled by rendering: a subclass exposes
    /// <see cref="RenderAvatarRef"/> (and optionally <see cref="RenderCostumeRef"/>), which
    /// IncursionManager applies to the spawned body so the client renders/animates that avatar
    /// while the server drives the combat body.
    /// </summary>
    public abstract class IncursionEnemyController
    {
        #region Properties
        protected static readonly Logger Logger = LogManager.CreateLogger();

        // Verbose setup/locomotion/scaling diagnostics. Off by default to keep logs focused on tuning.
        private static volatile bool s_verboseLogging = false;

        /// <summary>Process-wide toggle for setup and locomotion diagnostics.</summary>
        public static bool VerboseLogging
        {
            get => s_verboseLogging;
            set => s_verboseLogging = value;
        }

        protected readonly Game Game;

        /// <summary>Shortcut for the process-wide incursion combat-logging toggle.</summary>
        protected bool IsIncursionLoggingEnabled => Game?.CustomGameOptions?.IncursionLoggingEnable ?? false;

        // Set when the controller is bound to its live agent in Start().
        protected ulong AgentId { get; private set; }

        /// <summary>
        /// Entity ID of the invisible avatar nameplate proxy (TeamUp enemies only).
        /// The proxy provides a red prestige nameplate since the client only applies
        /// prestige colors to AvatarPrototype entities, not AgentTeamUpPrototype.
        /// </summary>
        public ulong ProxyEntityId { get; set; } = Entity.InvalidId;

        /// <summary>
        /// Database ID of the player this invader was spawned for (for hunt tracking).
        /// 0 when spawned from console/trial or without a player context.
        /// </summary>
        public ulong HuntPlayerDbId { get; set; }

        /// <summary>Avatar short name of the player this invader was spawned for (for hunt tracking).</summary>
        public string HuntAvatarName { get; set; }

        /// <summary>True once the hunt kill has been recorded for this controller.</summary>
        private bool _huntKillRecorded;

        // Powers this enemy may use (assigned to the agent during setup).
        protected readonly List<PrototypeId> Powers = new();

        /// <summary>
        /// Optional explicit power table with per-power tuning. Subclasses override this.
        /// </summary>
        protected virtual IncursionPowerEntry[] PowerTable => null;

        private readonly EventGroup _events = new();
        private readonly EventPointer<ThinkEvent> _thinkEvent = new();
        protected readonly Dictionary<PrototypeId, TimeSpan> _cooldownEndTimes = new();

        // Powers whose per-ability damage scale has already been applied (and logged once).
        private readonly HashSet<PrototypeId> _scaledPowers = new();

        // Maps child effect powers (combo hits, triggered powers) back to their parent
        // so damage scaling and logging treat the whole combo as one ability.
        protected readonly Dictionary<PrototypeId, PrototypeId> _effectToParentPower = new();

        // Round-robin priority order: the first ready power is chosen, then moved to the bottom.
        private readonly List<PrototypeId> _powerPriority = new();

        protected TimeSpan _globalAttackCooldownEnd = TimeSpan.Zero;
        private int _phase = -1;

        /// <summary>Current phase index (read-only for subclasses). Set by <see cref="UpdatePhase"/>.</summary>
        protected int CurrentPhase => _phase;

        private bool _disposed;
        private bool _dying;
        private int _deathPhase;          // 0=entered, 1=outro, 2=teleport beam, 3=invisible+hide name, 4=cleanup
        private TimeSpan _deathOutroTime;
        private TimeSpan _deathBeamTime;
        private TimeSpan _deathInvisibleTime;
        private TimeSpan _deathGraceEnd;
        private TimeSpan _deathProxyDestroyTime;
        private bool _proxyDestroyed;

        // Tracks when the nameplate proxy was spawned (set by IncursionManager) so
        // ConfigureSpawnedProxy can log the delay between spawn and deferred config.
        internal TimeSpan _proxySpawnGameTime;

        // Initial think cycles for which locomotion diagnostics are emitted.
        private int _diagThinksRemaining = 12;

        // Last server position sampled by the diagnostic.
        private Vector3? _lastDiagPos;

        // Cached human-readable log label: rendered identity plus entity id suffix.
        private string _label;

        // Lifecycle tracking
        protected TimeSpan _spawnTime;
        private TimeSpan _lastCombatTime;
        private long _maxHealthDeficit;
        private bool _inCombat;
        private bool _permanentAggro = false;  // Once woken, chase forever

        // Additional resilience: temporary damage reduction that decays after the enemy is woken.
        private TimeSpan _resilienceStartTime;
        private bool _resilienceActive = false;

        // Periodic health diagnostic
        private long _lastLoggedHealth = -1;
        private int _healthLogCounter;

        // Impatience: tracks how long we've been near the player without landing a hit.
        private TimeSpan _lastSuccessfulAttackTime;
        private int _impatienceTriggers;

        // Stuck / idle recovery tracking
        private TimeSpan _lastAbilityUseTime;
        private TimeSpan _lastPositionSampleTime;
        private Vector3 _lastSampledPosition;
        private int _stuckCheckCount;
        private int _recoveryAttempts;

        // Channeled power tracking
        private TimeSpan _channelStartTime;
        private PrototypeId _channelPowerRef = PrototypeId.Invalid;
        private int _channelMaxMs;

        // General power execution timeout safety net. Tracks when ANY power started
        // executing so we can force-end it if it runs too long (catches channeled powers
        // that slip through channel tracking, and any other stuck-power scenario).
        private TimeSpan _powerExecStartTime;
        private PrototypeId _powerExecPowerRef = PrototypeId.Invalid;
        private const int DefaultPowerTimeoutMs = 5000;

        // Last power used so we can enforce variety instead of spamming the same ability.
        protected PrototypeId _lastUsedPowerRef = PrototypeId.Invalid;

        // Entrance intro state
        private bool _introActive;
        private TimeSpan _introEndTime;
        private bool _introVfxPlayed;
        private bool _introDialogSaid;
        private bool _pendingIntro;
        private int _introDelayTicks;

        // Deferred proxy configuration: post-spawn operations (loot strip, power strip,
        // AI disable, dormant, attach) all write replicated properties on the proxy.
        // If these run in the same network tick as spec.Spawn(), the client receives
        // property replication updates while still processing the initial entity creation,
        // which can cause it to assign a default SheHulk costume/mesh to the avatar pawn.
        // Deferring to the first think tick gives the client a full tick to process the
        // bare proxy entity before any property-modifying operations occur.
        private bool _pendingProxyConfig;
        private ulong _pendingProxyCombatBodyId;
        private static readonly Logger ProxyLogger = LogManager.CreateLogger("IncursionProxy");

        /// <summary>True once the controlled entity is gone and the controller is finished.</summary>
        public bool IsFinished => _disposed;

        /// <summary>True while the agent is dead but lingering effects are still resolving.</summary>
        public bool IsDying => _dying;

        public ulong EntityId => AgentId;

        #endregion

        #region Tunables

        /// <summary>
        /// How long (ms) the controller stays alive after the agent dies so lingering DoTs / missiles
        /// can resolve with the proper damage scale. 0 = immediate disposal (legacy behaviour).
        /// </summary>
        protected virtual int DeathGracePeriodMs => Game?.CustomGameOptions?.IncursionDeathGracePeriodMs ?? 4000;

        /// <summary>
        /// Per-enemy visual/collision scale override. 0 = use the global config value
        /// (<c>IncursionEnemyVisualScale</c>, default 1.5). Override to shrink or enlarge
        /// a specific enemy (e.g. 0.125 = 8x smaller).
        /// </summary>
        public virtual float VisualScaleOverride => 0f;

        /// <summary>
        /// Movement speed multiplier applied to the agent (1.0 = unchanged, 3.0 = triple speed).
        /// Implemented via <see cref="PropertyEnum.MovementSpeedRate"/>.
        /// </summary>
        protected virtual float MovementSpeedMult => 1.0f;

        /// <summary>How often (ms) the think loop runs.</summary>
        protected virtual int ThinkIntervalMs => 350;

        /// <summary>Max distance at which the enemy will attempt to use powers.</summary>
        protected virtual float AttackRange => 250.0f;

        /// <summary>Beyond this distance the enemy ignores a candidate target.</summary>
        protected virtual float ChaseRange => 5000.0f;

        /// <summary>Initial wake-up radius. Enemy won't chase until a player enters this range.
        /// Once woken, the enemy chases forever (within ChaseRange).</summary>
        protected virtual float WakeRadius => 800.0f;

        /// <summary>Minimum delay (ms) between any two power activations, before phase scaling.</summary>
        protected virtual float GlobalAttackCooldownMs => 1500.0f;

        /// <summary>Per-power cooldown (ms) applied after a successful activation, before phase scaling.</summary>
        protected virtual float PerPowerCooldownMs => 15000.0f;

        /// <summary>
        /// Multiplier applied to the cooldown of ultimate powers.
        /// A value of 4.0 means an ultimate takes 4× as long to recharge as a normal power.
        /// </summary>
        protected virtual float UltimateCooldownMultiplier => 4.0f;

        /// <summary>How long (ms) the entrance intro / excited state lasts after spawn.</summary>
        protected virtual int IntroDurationMs => 8000;

        /// <summary>Multiplier to AttackRange while in the intro excited state.</summary>
        protected virtual float IntroAttackRangeMultiplier => 3.0f;

        /// <summary>Whether to play a warp-in VFX on spawn.</summary>
        protected virtual bool PlayIntroVfx => true;

        /// <summary>Whether to say random intro dialog from <see cref="IntroDialogLines"/>.</summary>
        protected virtual bool SayIntroDialog => true;

        /// <summary>
        /// Delay (ms) after death before the nameplate proxy is destroyed.
        /// 0 = destroy immediately on death. -1 = destroy at the same time as the
        /// body becomes invisible (phase 3, the default for Avatar enemies).
        /// Boss enemies override to 0 for instant nameplate removal.
        /// TeamUp enemies override to half the invisible time for faster removal.
        /// </summary>
        protected virtual int NameplateProxyDestroyDelayMs => -1;

        /// <summary>
        /// Fallback raw-string lines. Unused - the controller now sends locale-based
        /// <see cref="NetMessageShowOverheadText"/> via <see cref="IntroDialogLocaleIds"/>.
        /// Kept here so subclasses can still override if raw text ever becomes reliable.
        /// </summary>
        protected virtual string[] IntroDialogLines => new string[]
        {
            "Crush them!",
            "Just...DIE!",
            "DESTROY!!",
            "Your super hero friends can't save you now.",
            "Suffering awaits...",
            "Vengeance is mine!",
            "You are weakening...",
            "I am your Destroyer!",
            "Resist all you want...",
            "The Doomed have arrived!",
            "Without Fear!",
            "I do not fear death.",
            "Fight me! I fear no being.",
            "Going somewhere, weakling?",
            "Alas, I fear this may be the end... for you!",
            "Those who stand against us shall tremble in fear!",
            "We die fighting!",
            "I shall die fighting!",
            "Fight well. Die well.",
            "Fight, then...and die well!",
            "I will not die without a fight!",
        };

        /// <summary>
        /// Active locale string IDs for intro overhead dialog.
        /// Raw strings proved unreliable; these locale entries are used with <see cref="ShowOverheadText"/>.
        /// Only "Resisting" and "Without Fear" are confirmed to appear in-game.
        /// The rest are preserved but unconfirmed - some may not render.
        /// </summary>
        protected virtual LocaleStringId[] IntroDialogLocaleIds => new LocaleStringId[]
        {
            (LocaleStringId)0x26DD83DB2854053F, // "Crush them!"                    // unconfirmed
            (LocaleStringId)0x9C1C551E287C0542, // "Just...DIE!"                  // unconfirmed
            (LocaleStringId)0x629DADD924E1050B, // "DESTROY!!"                    // unconfirmed
            (LocaleStringId)0x5B7482E72CF2057E, // "Your super hero friends can't save you now." // unconfirmed
            (LocaleStringId)0x8AA5224027F10534, // "Suffering"                    // unconfirmed
            (LocaleStringId)0x8D5AD90F286C0540, // "Vengeance"                    // unconfirmed
            (LocaleStringId)0xA710D33E2867053F, // "Weakening"                    // unconfirmed
            (LocaleStringId)0xD48980A328920543, // "Destroyer"                    // unconfirmed
            (LocaleStringId)0xDC33D1AF24760502, // "Resisting"                    // WORKED (appears)
            (LocaleStringId)0x132100E528E70548, // "The Doomed"                   // unconfirmed
            (LocaleStringId)0x848CF605254D0514, // "Without Fear"                 // WORKED (appears)
            (LocaleStringId)0x0FB436DC2C120573, // "I do not fear death."         // unconfirmed
            (LocaleStringId)0xB7FE16AC28950543, // "Fight me! I fear no being."   // unconfirmed
            (LocaleStringId)0xBC95028F24590500, // "Going somewhere, weakling?"   // unconfirmed
            (LocaleStringId)0x2CBD4A1124980507, // "Alas, I fear this may be the end." // unconfirmed
            (LocaleStringId)0xE622FCC62857053E, // "Those who stand against our case shall tremble in fear!" // unconfirmed
            (LocaleStringId)0x2BBEABE02B9A0568, // "We die fighting!"             // unconfirmed
            (LocaleStringId)0xE20CAD7B288B0543, // "I shall die fighting!"        // unconfirmed
            (LocaleStringId)0xFEBF6D512C080572, // "Fight well. Die well."        // unconfirmed
            (LocaleStringId)0x6C21585A2BD4056A, // "Fight, then...and die well!"  // unconfirmed
            (LocaleStringId)0xC4170818244104FF, // "I will not die without a fight!" // unconfirmed
        };

        #endregion

        #region Combat Scale 

        /// <summary>
        /// Multiplier applied to all outgoing damage (1.0 = unchanged).
        /// Avatar powers deal more damage than mob powers; default scales down.
        /// </summary>
        protected virtual float DamageScale => 0.05f;

        /// <summary>
        /// Base incoming damage multiplier from config (default 2.0 = double damage taken).
        /// Applied as <see cref="PropertyEnum.DamagePctVulnerability"/>.
        /// </summary>
        protected virtual float DamageTakenScale => Game?.CustomGameOptions?.IncursionEnemyDamageTakenMultiplier ?? 2.0f;

        /// <summary>
        /// Additional per-enemy damage taken multiplier (default 1.0 = unchanged).
        /// The final damage taken scale is <see cref="DamageTakenScale"/> * <see cref="DamageTakenMultiplier"/>.
        /// Override in subclasses to make specific enemies tankier (e.g. 0.5 = half damage).
        /// </summary>
        protected virtual float DamageTakenMultiplier => 1.0f;

        /// <summary>
        /// Peak damage-taken multiplier during the wake-up resilience window (0..1 range).
        /// 0.5 = take 50% damage (50% reduction). 1.0 = no extra mitigation.
        /// Applied multiplicatively on top of DamageTakenScale * DamageTakenMultiplier.
        /// </summary>
        protected virtual float AdditionalResilienceMax => 0.5f;

        /// <summary>Seconds of full-strength resilience after waking before decay begins.</summary>
        protected virtual float AdditionalResilienceFullDurationSec => 10f;

        /// <summary>Total seconds after waking for resilience to fully decay to 1.0 (normal).</summary>
        protected virtual float AdditionalResilienceDecayDurationSec => 30f;

        /// <summary>
        /// When false (default), the enemy cannot receive healing from any source.
        /// Subclasses may override to true for self-healing bosses.
        /// </summary>
        protected virtual bool CanRegainHealth => false;

        /// <summary>
        /// When true (default), the enemy is promoted to Boss rank for presence and damage scaling.
        /// Override to false to use Champion rank instead (e.g. to hide minimap markers).
        /// </summary>
        protected virtual bool UseBossRank => true;

        /// <summary>
        /// When set to a valid prototype ref, overrides all rank resolution logic.
        /// Use this to specify an exact rank (e.g. BossNoOverheadInfo for thralls that
        /// need no overhead name and no champion glow).
        /// </summary>
        protected virtual PrototypeId RankOverride => PrototypeId.Invalid;

        /// <summary>
        /// Custom name drawn above this invader. Shown via the avatar nameplate when rendered as an avatar.
        /// <see langword="null"/> or empty => no custom name.
        /// </summary>
        public virtual string InvaderDisplayName => null;

        /// <summary>
        /// Prestige level applied to the agent for the overhead nameplate color.
        /// 0 = default, 1 = green, 2 = blue, 3 = purple, 4 = orange, 5 = red, 6 = yellow (cosmic).
        /// </summary>
        public virtual int NameplatePrestigeLevel => 5;

        /// <summary>
        /// Whether to spawn an invisible nameplate proxy for this enemy.
        /// The proxy provides a prestige-colored nameplate for boss-type and team-up-type
        /// enemies (which don't render as avatars). The proxy's hiding mechanism
        /// (IsClientEntityHidden) can be unreliable during combat, causing visible
        /// SheHulk/SpidermanClone bodies. Override to false for trash-tier enemies
        /// that don't need prestige nameplates.
        /// </summary>
        public virtual bool NeedsNameplateProxy => true;

        /// <summary>
        /// Optional markup prefix for the overhead name. May show literally if the client does not support rich text in nameplates.
        /// </summary>
        public virtual string NameplatePrefix => null;

        /// <summary>
        /// Optional markup suffix for the overhead name. Must match <see cref="NameplatePrefix"/>.
        /// </summary>
        public virtual string NameplateSuffix => null;

        /// <summary>
        /// When true, this enemy is permanently excluded from the random spawn pool and
        /// fuzzy pattern matching, regardless of the IncursionExcludeEnemies config.
        /// Override to true in individual subclass files for broken/WIP/placeholder
        /// characters so they never spawn even if the user wipes their config exclusions.
        /// Explicit exact-shorthand spawns (e.g. "!incursion spawn Foo") still work.
        /// </summary>
        public virtual bool HardcodeExclude => false;

        #endregion

        #region Loot Config

        /// <summary>
        /// Boss loot pools rolled on death. The host body's native loot is stripped first.
        /// One enabled pool is chosen at random. If no pool is enabled, no boss loot drops.
        /// </summary>
        protected virtual IReadOnlyList<IncursionLootPool> LootPools => DefaultLootPools;

        /// <summary>Default boss loot pools. Generic patrol pools are enabled; higher-tier pools are disabled.</summary>
        public static readonly IncursionLootPool[] DefaultLootPools =
        {
            new("Brooklyn Bosses",          "Loot/Tables/Mob/Bosses/PatrolBrooklyn/Subtable/SharedPatrolBrooklynBosses.prototype",          true),
            new("Hightown Bosses",          "Loot/Tables/Mob/Bosses/PatrolHightown/Subtable/SharedPatrolHightownBosses.prototype",          true),
            new("Midtown Bosses",           "Loot/Tables/Mob/Bosses/PatrolMidtown/Subtable/SharedPatrolMidtownBosses.prototype",            true),
            //new("Brooklyn Bosses (Cosmic)", "Loot/Tables/Mob/Bosses/PatrolBrooklyn/Subtable/SharedPatrolBrooklynBossesCosmic.prototype",    false),
            //new("Hightown Bosses (Cosmic)", "Loot/Tables/Mob/Bosses/PatrolHightown/Subtable/SharedPatrolHightownBossesCosmic.prototype",    false),
            //new("Brooklyn Bosses (All)",    "Loot/Tables/Mob/Bosses/PatrolBrooklyn/Subtable/SharedPatrolBrooklynBossesAll.prototype",       false),
        };

        #endregion

        #region Stealable Power 

        /// <summary>
        /// Stealable power info for Rogue. Override per hero to match the rendered avatar.

        /// </summary>
        public virtual PrototypeId StealablePowerInfoRef => PrototypeId.Invalid;

        #endregion

        #region Construct Render 

        protected IncursionEnemyController(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Avatar prototype the client renders this invader as.
        /// <see cref="PrototypeId.Invalid"/> => render the combat body itself, or check <see cref="RenderTeamupRef"/>.
        /// </summary>
        public virtual PrototypeId RenderAvatarRef => PrototypeId.Invalid;

        /// <summary>
        /// Team-Up prototype the client renders this invader as (alternative to <see cref="RenderAvatarRef"/>
        /// for Team-Up-based illusion proxies). <see cref="PrototypeId.Invalid"/> => not a Team-Up render.
        /// </summary>
        public virtual PrototypeId RenderTeamupRef => PrototypeId.Invalid;

        /// <summary>
        /// Boss entity prototype spawned as the combat body itself (no render override).
        /// When non-Invalid, the spawn spec's EntityRef is overridden to this boss prototype,
        /// and no ClientRenderPrototypeRef is set - the boss renders as itself.
        /// <see cref="PrototypeId.Invalid"/> => not a boss spawn (check Avatar/TeamUp refs).
        /// </summary>
        public virtual PrototypeId RenderBossRef => PrototypeId.Invalid;

        /// <summary>
        /// Categorizes this invader for logging and parsing: "Avatar", "TeamUp", or "Boss".
        /// Subclasses override to identify their render/spawn path so the log parser can
        /// group damage by the correct controller class rather than guessing from display names.
        /// </summary>
        public virtual string EnemyType => "Controller";

        /// <summary>
        /// Prefix used for the per-encounter log filename. Default is "Incursion_{EnemyType}".
        /// Calamity entities override this to "Calamity_Vampire" (or similar) so their logs
        /// are grouped separately from standard Incursion logs.
        /// </summary>
        public virtual string LogFilePrefix => $"Incursion_{EnemyType}";

        /// <summary>
        /// When non-null, overrides the display-name portion of the log filename.
        /// Calamity entities set this to a short identifier derived from their class name
        /// (e.g. "BossBloodLord") so logs read "Calamity_Vampire_BossBloodLord_...log".
        /// When null, the collator falls back to the invader label name.
        /// </summary>
        public virtual string LogTrueName => null;

        /// <summary>
        /// Optional costume pool with per-entry enabled toggles. When non-null, one enabled
        /// entry is rolled at random per spawn for <see cref="RenderCostumeRef"/>. Subclasses
        /// override this to keep the full costume reference list while tuning availability.
        /// </summary>
        protected virtual IncursionCostumeEntry[] CostumeTable => null;

        // Costume rolled from CostumeTable for this spawn. Rolled once, then cached so the
        // selection stays stable for repeated reads (spawn spec, logging, labels).
        private PrototypeId _rolledCostumeRef = PrototypeId.Invalid;
        private bool _costumeRolled;

        /// <summary>
        /// Costume for the rendered avatar (its CostumeUnrealClass is the visible model).
        /// Defaults to a random enabled entry from <see cref="CostumeTable"/>.
        /// <see cref="PrototypeId.Invalid"/> => use the avatar's starting costume.
        /// </summary>
        public virtual PrototypeId RenderCostumeRef
        {
            get
            {
                if (_costumeRolled == false)
                {
                    _costumeRolled = true;
                    _rolledCostumeRef = RollCostume();
                }

                return _rolledCostumeRef;
            }
        }

        /// <summary>
        /// Picks a random enabled costume from <see cref="CostumeTable"/>,
        /// or <see cref="PrototypeId.Invalid"/> when no entry is available.
        /// Uses a time-seeded random so each spawn gets a different costume even if Game.Random is deterministic.
        /// </summary>
        private PrototypeId RollCostume()
        {
            IncursionCostumeEntry[] table = CostumeTable;
            if (table == null || table.Length == 0)
                return PrototypeId.Invalid;

            // Build a list of enabled entries and pick one with a fresh random seed.
            PrototypeId picked = PrototypeId.Invalid;
            int enabledCount = 0;

            foreach (IncursionCostumeEntry entry in table)
            {
                if (entry.Enabled == false || entry.Costume == PrototypeId.Invalid)
                    continue;

                enabledCount++;
                if (Game.Random.Next(enabledCount) == 0)
                    picked = entry.Costume;
            }

            if (enabledCount == 0)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: costume table has no enabled entries; using the avatar's starting costume.");
                return PrototypeId.Invalid;
            }

            // Re-roll with an explicit time-based seed to guarantee variety per spawn.
            var costumeRandom = new GRandom((int)(DateTime.UtcNow.Ticks ^ Environment.TickCount ^ enabledCount));
            int pickIndex = costumeRandom.Next(0, enabledCount);
            int index = 0;
            foreach (IncursionCostumeEntry entry in table)
            {
                if (entry.Enabled == false || entry.Costume == PrototypeId.Invalid)
                    continue;
                if (index == pickIndex)
                    return entry.Costume;
                index++;
            }

            return picked; // fallback to the reservoir-sampled pick
        }

        #endregion

        #region Lifecycle

        /// <summary>Resolves the live agent entity (it may have despawned).</summary>
        protected Agent GetAgent() => Game.EntityManager.GetEntity<Agent>(AgentId);

        /// <summary>
        /// Binds the controller to the spawned agent, disables native AI, runs subclass setup,
        /// and starts the think loop.
        /// </summary>
        public void Start(Agent agent)
        {
            if (agent == null)
            {
                Logger.Warn("[IncursionEnemy] Start: agent is null.");
                Dispose();
                return;
            }

            AgentId = agent.Id;

            // Disable native AI so the controller is the sole driver.
            agent.AIController?.SetIsEnabled(false);

            // Some host prototypes start untargetable or bound to a mission/encounter,
            // which prevents mutual damage with players.
            EnableCombat(agent);

            // Scale the boss body to invader-appropriate values.
            ApplyCombatScaling(agent);

            // Prevent incursion enemies from regaining health unless explicitly allowed.
            if (CanRegainHealth == false)
                agent.Properties[PropertyEnum.HealingBlocked] = true;

            // Replace the host body's native death-loot with an incursion boss pool.
            ApplyLootPool(agent);

            try
            {
                OnSetup(agent);
            }
            catch (Exception e)
            {
                Logger.Warn($"[IncursionEnemy] {InvaderLabel} OnSetup threw: {e.Message}");
            }

            // Build child-effect -> parent-power map so combo/multi-hit damage uses the root power's scale.
            BuildEffectToParentMap();

            // Build and shuffle the power priority list so the initial order isn't
            // deterministic (e.g. alphabetical from GetPowersUnlockedAtLevel).
            _powerPriority.Clear();
            foreach (PrototypeId p in Powers)
                _powerPriority.Add(p);
            ShuffleList(_powerPriority, Game.Random);

            // Per-ability outgoing damage scaling (after powers exist).
            ApplyPerPowerDamageScaling(agent);

            ScheduleNextThink();
            LogVerbose($"[IncursionEnemy] {InvaderLabel} started for entity {AgentId} with {Powers.Count} power(s).");
            LogLocomotionStatus(agent, "post-setup");

            int prestigeLevel = NameplatePrestigeLevel;
            if (prestigeLevel > 0)
            {
                agent.Properties[PropertyEnum.AvatarPrestigeLevel] = prestigeLevel;
                LogVerbose($"[IncursionEnemy] {InvaderLabel} nameplate prestige set to {prestigeLevel}.");
            }

            _spawnTime = Game.CurrentTime;
            _lastCombatTime = Game.CurrentTime;
            _lastAbilityUseTime = Game.CurrentTime;
            _lastSuccessfulAttackTime = Game.CurrentTime;
            _lastPositionSampleTime = Game.CurrentTime;
            _lastSampledPosition = agent.RegionLocation.Position;
            _maxHealthDeficit = 0;
            _inCombat = false;
            _stuckCheckCount = 0;
            _recoveryAttempts = 0;
            _impatienceTriggers = 0;
            _channelStartTime = TimeSpan.Zero;
            _channelPowerRef = PrototypeId.Invalid;
            _channelMaxMs = 0;
            _powerExecStartTime = TimeSpan.Zero;
            _powerExecPowerRef = PrototypeId.Invalid;
        }

        /// <summary>Stops the think loop and releases scheduled events.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DestroyNameplateProxy();
            Game?.GameEventScheduler?.CancelAllEvents(_events);
        }

        #endregion

        #region Combat Enable

        // Hostile to Players but not to standard enemy alliances (other NPCs ignore it).
        private const string HostileAllianceName = "Entity/Alliances/EnemiesOmitFriendlies.prototype";

        // Boss rank for presence and damage scaling.
        private const string BossRankName = "Mods/Ranks/Boss.prototype";

        // Boss rank that hides the overhead nameplate. Used for TeamUp enemies
        // whose nameplate is provided by a proxy entity instead.
        private const string BossNoOverheadRankName = "Mods/Ranks/BossNoOverheadInfo.prototype";

        // Champion rank - non-boss rank that doesn't show on the minimap.
        private const string ChampionRankName = "Mods/Ranks/Champion.prototype";

        private static PrototypeId s_hostileAllianceRef = PrototypeId.Invalid;
        private static PrototypeId s_bossRankRef = PrototypeId.Invalid;
        private static PrototypeId s_bossNoOverheadRankRef = PrototypeId.Invalid;
        private static PrototypeId s_championRankRef = PrototypeId.Invalid;

        /// <summary>
        /// Converts the spawned host into a mortal, hostile, mobile combatant regardless of
        /// the host prototype's original role.
        /// </summary>
        protected virtual void EnableCombat(Agent agent)
        {
            agent.Properties[PropertyEnum.Untargetable] = false;
            agent.Properties[PropertyEnum.Unaffectable] = false;
            agent.Properties[PropertyEnum.Invulnerable] = false;

            // Some boss prototypes (e.g. Kaecilius) have a HealthMin passive that sets
            // HealthMin to 1, which prevents health from ever reaching 0 via Math.Clamp.
            // Reset it to 0 so the boss can actually die.
            if (agent.Properties.HasProperty(PropertyEnum.HealthMin))
                agent.Properties[PropertyEnum.HealthMin] = 0L;

            // Hub NPCs can start dormant, leaving the invader inert/invisible.
            agent.Properties[PropertyEnum.Dormant] = false;

            // Prevent the invisible combat body from physically blocking melee players.
            // NoEntityCollide disables entity-entity physical blocking and nav mesh
            // influence while preserving power targeting and damage. Players can walk
            // through the invisible combat body to reach the visible rendered avatar.
            // Only apply when the combat body is invisible (avatar/teamup render type).
            // Boss-type enemies (RenderBossRef) ARE the visible body and need nav mesh
            // for pathfinding, so NoEntityCollide must NOT be set for them.
            if (RenderAvatarRef != PrototypeId.Invalid || RenderTeamupRef != PrototypeId.Invalid)
                agent.Properties[PropertyEnum.NoEntityCollide] = true;

            // Detach from any mission/encounter so cross-encounter hostility checks don't
            // block fighting with players.
            if (agent.Properties.HasProperty(PropertyEnum.MissionPrototype))
                agent.Properties.RemoveProperty(PropertyEnum.MissionPrototype);

            // Force a hostile alliance so damage is mutual.
            PrototypeId hostileAlliance = ResolveHostileAlliance();
            if (hostileAlliance != PrototypeId.Invalid)
                agent.Properties[PropertyEnum.AllianceOverride] = hostileAlliance;

            // Resolve rank: explicit override > UseBossRank logic.
            PrototypeId rankRef = RankOverride;
            if (rankRef == PrototypeId.Invalid)
            {
                if (UseBossRank == false)
                    rankRef = ResolveChampionRank();
                else if (RenderAvatarRef == PrototypeId.Invalid)
                    rankRef = ResolveBossNoOverheadRank();
                else
                    rankRef = ResolveBossRank();
            }
            if (rankRef != PrototypeId.Invalid)
            {
                agent.Properties[PropertyEnum.Rank] = rankRef;
            }

            if (agent.IsHostileToPlayers() == false)
                Logger.Warn($"[IncursionEnemy] {InvaderLabel} ('{agent.PrototypeName}') is NOT hostile to players " +
                            "after override; players may be unable to damage it.");

            // Force-create a locomotor if the host prototype is immobile (e.g. mob base
            // prototypes that set Locomotion.Immobile = true). Without a locomotor the
            // agent cannot chase or path, leaving it stuck at the spawn position.
            if (agent.Locomotor == null)
            {
                var fallbackProto = GameDatabase.GetPrototype<AgentPrototype>(
                    GameDatabase.GetPrototypeRefByName(FallbackLocomotionProtoName));
                if (fallbackProto?.Locomotion != null && fallbackProto.Locomotion.Immobile == false)
                {
                    if (agent.ForceCreateLocomotor(fallbackProto.Locomotion))
                        Logger.Info($"[IncursionEnemy] {InvaderLabel} host prototype is immobile; created fallback locomotor.");
                    else
                        Logger.Warn($"[IncursionEnemy] {InvaderLabel} ForceCreateLocomotor failed.");
                }
                else
                {
                    Logger.Warn($"[IncursionEnemy] {InvaderLabel} host prototype is immobile and fallback locomotion is unavailable.");
                }
            }

            // Boss prototypes have ObjectiveInfo.EdgeEnabled baked into their data, which
            // causes WorldEntityPrototype.DiscoverInRegion to return true. On enter world,
            // the entity is auto-discovered and added to the map. Setting MapTracking = false
            // does not prevent this because DiscoverInRegion is computed from the prototype,
            // not from agent properties. Undiscover the entity to remove it from the map.
            if (agent.IsInWorld && agent.Region != null && agent.Region.IsEntityDiscovered(agent))
                agent.Region.UndiscoverEntity(agent, false);
        }

        private const string FallbackLocomotionProtoName = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";

        private static PrototypeId ResolveHostileAlliance()
        {
            if (s_hostileAllianceRef == PrototypeId.Invalid)
                s_hostileAllianceRef = GameDatabase.GetPrototypeRefByName(HostileAllianceName);
            return s_hostileAllianceRef;
        }

        private static PrototypeId ResolveBossRank()
        {
            if (s_bossRankRef == PrototypeId.Invalid)
                s_bossRankRef = GameDatabase.GetPrototypeRefByName(BossRankName);
            return s_bossRankRef;
        }

        protected static PrototypeId ResolveBossNoOverheadRank()
        {
            if (s_bossNoOverheadRankRef == PrototypeId.Invalid)
                s_bossNoOverheadRankRef = GameDatabase.GetPrototypeRefByName(BossNoOverheadRankName);
            return s_bossNoOverheadRankRef;
        }

        private static PrototypeId ResolveChampionRank()
        {
            if (s_championRankRef == PrototypeId.Invalid)
                s_championRankRef = GameDatabase.GetPrototypeRefByName(ChampionRankName);
            return s_championRankRef;
        }

        #endregion

        #region Loot Pool 

        // Resolved loot table refs are cached per path so repeated spawns don't re-resolve them.
        private static readonly Dictionary<string, PrototypeId> s_lootTableRefCache = new();

        /// <summary>
        /// Strips the host body's native death-loot and assigns one enabled boss loot table at random.
        /// </summary>
        protected virtual void ApplyLootPool(Agent agent)
        {
            RemoveDeathLootTables(agent);

            IReadOnlyList<IncursionLootPool> pools = LootPools;
            if (pools == null || pools.Count == 0)
            {
                LogVerbose($"[IncursionEnemy] {InvaderLabel} has no loot pools defined; invader drops no boss loot.");
                return;
            }

            List<PrototypeId> enabledTables = new();
            foreach (IncursionLootPool pool in pools)
            {
                if (pool.Enabled == false) continue;

                PrototypeId tableRef = ResolveLootTable(pool.LootTablePath);
                if (tableRef != PrototypeId.Invalid)
                    enabledTables.Add(tableRef);
            }

            if (enabledTables.Count == 0)
            {
                LogVerbose($"[IncursionEnemy] {InvaderLabel} has no enabled/valid loot pools; invader drops no boss loot.");
                return;
            }

            PrototypeId chosen = enabledTables[Game.Random.Next(enabledTables.Count)];

            // Key by the drop event the entity's rank uses with a Spawn action.
            LootDropEventType eventType = ResolveLootDropEventType(agent);
            PropertyId lootProp = new(PropertyEnum.LootTablePrototype,
                (PropertyParam)(int)eventType, (PropertyParam)0, (PropertyParam)(int)LootActionType.Spawn);
            agent.Properties[lootProp] = chosen;

            LogVerbose($"[IncursionEnemy] {InvaderLabel} loot pool rolled '{GameDatabase.GetPrototypeName(chosen)}' " +
                       $"from {enabledTables.Count} enabled pool(s) (event {eventType}).");
        }

        /// <summary>Removes all existing death-loot table properties from the agent.</summary>
        public static void RemoveDeathLootTables(Agent agent)
        {
            List<PropertyId> toRemove = new();
            foreach (var kvp in agent.Properties.IteratePropertyRange(PropertyEnum.LootTablePrototype))
                toRemove.Add(kvp.Key);

            foreach (PropertyId propId in toRemove)
                agent.Properties.RemoveProperty(propId);
        }

        /// <summary>The loot drop event the agent's rank uses on death.</summary>
        private static LootDropEventType ResolveLootDropEventType(Agent agent)
        {
            RankPrototype rankProto = agent.GetRankPrototype();
            return rankProto != null && rankProto.LootTableParam != LootDropEventType.None
                ? rankProto.LootTableParam
                : LootDropEventType.OnKilled;
        }

        private static PrototypeId ResolveLootTable(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PrototypeId.Invalid;

            if (s_lootTableRefCache.TryGetValue(path, out PrototypeId cached))
                return cached;

            PrototypeId tableRef = GameDatabase.GetPrototypeRefByName(path);
            if (tableRef == PrototypeId.Invalid)
                Logger.Warn($"[IncursionEnemy] Loot pool path could not be resolved and will be skipped: '{path}'.");

            s_lootTableRefCache[path] = tableRef;
            return tableRef;
        }

        #endregion

        #region Combat Scaling  

        /// <summary>
        /// Applies combat scaling: incoming damage vulnerability and outgoing damage scaling.
        /// Per-ability damage scaling is applied separately after powers are assigned.
        /// </summary>
        protected virtual void ApplyCombatScaling(Agent agent)
        {
            // Incoming damage scaling is now handled by the PowerPayload system via
            // IncursionManager.GetIncomingDamageScale(), which queries the controller's
            // DamageTakenScale * DamageTakenMultiplier directly. This avoids the
            // DamagePctVulnerability property being overridden by conditions (e.g. AvatarOfCyttorak).
            float damageTakenScale = DamageTakenScale * DamageTakenMultiplier;

            // Apply movement speed multiplier.
            float speedMult = MovementSpeedMult;
            if (speedMult != 1.0f && speedMult > 0f)
                agent.Properties[PropertyEnum.MovementSpeedRate] = speedMult;

            LogSpawnDiagnostics(agent, damageTakenScale);
        }

        /// <summary>
        /// Emits a one-time investigative log at spawn covering health, stats, and body properties.
        /// Called by <see cref="ApplyCombatScaling"/> after scaling is applied.
        /// </summary>
        private void LogSpawnDiagnostics(Agent agent, float damageTakenScale)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[IncursionEnemy:SpawnDiag] {InvaderLabel}");
            sb.AppendLine($"  bodyProto='{agent.PrototypeName}'  entityId={agent.Id}  level={agent.CharacterLevel}/{agent.CombatLevel}");

            // Rank
            var rankProto = agent.GetRankPrototype();
            sb.AppendLine($"  rank={(rankProto != null ? rankProto.ToString() : "(none)")}  allegiance={agent.AgentPrototype?.Allegiance}");

            // Health and scaling
            sb.AppendLine($"  health={agent.Properties[PropertyEnum.Health]}/{agent.Properties[PropertyEnum.HealthMax]}  damageTakenScale=x{damageTakenScale:0.###}");

            // Health-related properties
            TryAppendProperty(sb, agent, PropertyEnum.HealthBase, "HealthBase");
            TryAppendProperty(sb, agent, PropertyEnum.HealthMaxMult, "HealthMaxMult");
            TryAppendProperty(sb, agent, PropertyEnum.HealthAddBonus, "HealthAddBonus");
            TryAppendProperty(sb, agent, PropertyEnum.HealthMaxOther, "HealthMaxOther");
            TryAppendProperty(sb, agent, PropertyEnum.HealthPctBonus, "HealthPctBonus");

            // Stats
            TryAppendStat(sb, agent, PropertyEnum.StatFightingSkills, "Fighting");
            TryAppendStat(sb, agent, PropertyEnum.StatDurability, "Durability");
            TryAppendStat(sb, agent, PropertyEnum.StatStrength, "Strength");
            TryAppendStat(sb, agent, PropertyEnum.StatSpeed, "Speed");
            TryAppendStat(sb, agent, PropertyEnum.StatEnergyProjection, "Energy");
            TryAppendStat(sb, agent, PropertyEnum.StatIntelligence, "Intelligence");

            // Defense / damage rating
            TryAppendProperty(sb, agent, PropertyEnum.DamageRating, "DamageRating");
            TryAppendProperty(sb, agent, PropertyEnum.Defense, "Defense");

            // Body prototype passives
            var behaviorProfile = agent.AgentPrototype?.BehaviorProfile;
            if (behaviorProfile?.EquippedPassivePowers != null && behaviorProfile.EquippedPassivePowers.Length > 0)
            {
                var passiveNames = new List<string>();
                foreach (PrototypeId p in behaviorProfile.EquippedPassivePowers)
                    passiveNames.Add(GameDatabase.GetPrototypeName(p));
                sb.AppendLine($"  bodyPassives=[{string.Join(", ", passiveNames)}]");
            }

            // Powers already assigned to the entity (before our controller adds more)
            var powerCollection = agent.PowerCollection;
            if (powerCollection != null)
            {
                var existingPowers = new List<string>();
                foreach (var kvp in powerCollection)
                    existingPowers.Add(GameDatabase.GetPrototypeName(kvp.Key));
                if (existingPowers.Count > 0)
                    sb.AppendLine($"  entityPowers=[{string.Join(", ", existingPowers)}]");
            }

            // Locomotion info
            sb.AppendLine($"  locomotion={(agent.Locomotor != null ? agent.Locomotor.Method.ToString() : "NULL")}  immobileProto={agent.AgentPrototype?.Locomotion?.Immobile}");

            if (IsIncursionLoggingEnabled)
                Logger.Info(sb.ToString());
            IncursionLogCollator.WriteLine(agent.Id, sb.ToString());
        }

        private static void TryAppendProperty(System.Text.StringBuilder sb, Agent agent, PropertyEnum prop, string label)
        {
            try
            {
                if (agent.Properties.HasProperty(prop))
                {
                    long valLong = agent.Properties[prop];
                    sb.AppendLine($"  {label}={valLong}");
                }
            }
            catch { /* property may be indexed */ }
        }

        private static void TryAppendStat(System.Text.StringBuilder sb, Agent agent, PropertyEnum prop, string label)
        {
            try
            {
                long statVal = agent.Properties[prop];
                long statMod = agent.Properties[GetModifierProperty(prop)];
                if (statVal != 0 || statMod != 0)
                    sb.AppendLine($"  {label}={statVal}  {label}Modifier={statMod}");
            }
            catch { /* property not present */ }
        }

        private static PropertyEnum GetModifierProperty(PropertyEnum stat)
        {
            return stat switch
            {
                PropertyEnum.StatFightingSkills => PropertyEnum.StatFightingSkillsModifier,
                PropertyEnum.StatDurability => PropertyEnum.StatDurabilityModifier,
                PropertyEnum.StatStrength => PropertyEnum.StatStrengthModifier,
                PropertyEnum.StatSpeed => PropertyEnum.StatSpeedModifier,
                PropertyEnum.StatEnergyProjection => PropertyEnum.StatEnergyProjectionModifier,
                PropertyEnum.StatIntelligence => PropertyEnum.StatIntelligenceModifier,
                _ => PropertyEnum.StatAllModifier,
            };
        }

        /// <summary>
        /// Logs per-ability damage scales once. Scales are resolved on demand by the damage pipeline;
        /// they are not stored in entity properties.
        /// </summary>
        protected void ApplyPerPowerDamageScaling(Agent agent)
        {
            foreach (PrototypeId powerRef in Powers)
            {
                if (powerRef == PrototypeId.Invalid) continue;
                if (_scaledPowers.Add(powerRef) == false) continue;

                float scale = GetOutgoingDamageScale(powerRef);
                LogVerbose($"[IncursionEnemy] {InvaderLabel} damage scale x{scale:0.###} for '{GameDatabase.GetPrototypeName(powerRef)}'.");
            }
        }

        /// <summary>
        /// Resolves the outgoing damage scale for the given root power.
        /// Queried by the damage pipeline through <see cref="Populations.IncursionManager"/>.
        /// </summary>
        public float GetOutgoingDamageScale(PrototypeId powerRef)
        {
            float scale = GetDamageScaleForPower(powerRef);
            if (scale <= 0f) return 0f;

            float globalMultiplier = Game?.CustomGameOptions?.IncursionEnemyDamageMultiplier ?? 1.0f;
            return scale * globalMultiplier;
        }

        /// <summary>
        /// Returns the full incoming damage scale (DamageTakenScale * DamageTakenMultiplier).
        /// Queried by the damage pipeline through <see cref="Populations.IncursionManager"/>
        /// to apply per-enemy damage vulnerability directly in PowerPayload, bypassing the
        /// DamagePctVulnerability property which can be overridden by conditions.
        /// </summary>
        public float GetIncomingDamageScale()
        {
            return DamageTakenScale * DamageTakenMultiplier * GetAdditionalResilienceMultiplier();
        }

        /// <summary>
        /// Returns the current additional resilience multiplier (0..1, where 1.0 = no mitigation).
        /// During the first <see cref="AdditionalResilienceFullDurationSec"/> seconds after waking,
        /// returns <see cref="AdditionalResilienceMax"/>. Then linearly interpolates to 1.0
        /// over the remaining time until <see cref="AdditionalResilienceDecayDurationSec"/>.
        /// </summary>
        protected virtual float GetAdditionalResilienceMultiplier()
        {
            if (_resilienceActive == false) return 1.0f;

            float elapsedSec = (float)(Game.CurrentTime - _resilienceStartTime).TotalSeconds;

            if (elapsedSec >= AdditionalResilienceDecayDurationSec)
            {
                _resilienceActive = false;
                return 1.0f;
            }

            if (elapsedSec <= AdditionalResilienceFullDurationSec)
                return AdditionalResilienceMax;

            // Linear interpolation from AdditionalResilienceMax to 1.0
            float t = (elapsedSec - AdditionalResilienceFullDurationSec)
                      / (AdditionalResilienceDecayDurationSec - AdditionalResilienceFullDurationSec);
            return AdditionalResilienceMax + (1.0f - AdditionalResilienceMax) * t;
        }

        /// <summary>
        /// Activates (or refreshes) the additional resilience window, resetting the decay timer.
        /// Called automatically when the enemy is first woken by a nearby player.
        /// Subclasses may call this on phase changes to re-trigger resilience.
        /// </summary>
        protected void RefreshAdditionalResilience()
        {
            _resilienceStartTime = Game.CurrentTime;
            _resilienceActive = true;
            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Resilience] {InvaderLabel} additional resilience activated (max={AdditionalResilienceMax}, full={AdditionalResilienceFullDurationSec}s, decay={AdditionalResilienceDecayDurationSec}s).");
        }

        /// <summary>
        /// Builds a map from child effect powers to their parent (root) power by
        /// recursively following <see cref="PowerPrototype.ActionsTriggeredOnPowerEvent"/> chains.
        /// </summary>
        private void BuildEffectToParentMap()
        {
            _effectToParentPower.Clear();
            foreach (PrototypeId powerRef in Powers)
            {
                if (powerRef == PrototypeId.Invalid) continue;
                BuildEffectToParentMapRecursive(powerRef, powerRef, 0);
            }
        }

        private void BuildEffectToParentMapRecursive(PrototypeId parentRef, PrototypeId currentRef, int depth)
        {
            if (depth > 8) return;
            var proto = GameDatabase.GetPrototype<PowerPrototype>(currentRef);
            if (proto?.ActionsTriggeredOnPowerEvent.HasValue() != true) return;

            foreach (var action in proto.ActionsTriggeredOnPowerEvent)
            {
                if (action?.EventAction != PowerEventActionType.UsePower) continue;
                if (action.Power == PrototypeId.Invalid) continue;
                if (action.Power == currentRef) continue; // infinite loop guard

                if (_effectToParentPower.ContainsKey(action.Power) == false)
                    _effectToParentPower[action.Power] = parentRef;

                BuildEffectToParentMapRecursive(parentRef, action.Power, depth + 1);
            }
        }

        /// <summary>
        /// Returns the parent (root) power for a given child effect, or Invalid if none.
        /// </summary>
        public PrototypeId GetParentPowerForEffect(PrototypeId effectRef)
        {
            if (_effectToParentPower.TryGetValue(effectRef, out PrototypeId parentRef))
                return parentRef;
            return PrototypeId.Invalid;
        }

        #endregion

        #region Logging 

        /// <summary>Logs a setup/diagnostic line only when <see cref="VerboseLogging"/> is enabled.</summary>
        protected void LogVerbose(string message)
        {
            if (s_verboseLogging)
                Logger.Info(message);
        }

        /// <summary>
        /// Short log identity for this invader: rendered avatar name plus entity id suffix.
        /// </summary>
        protected string InvaderLabel => _label ??= BuildLabel();

        private string BuildLabel()
        {
            string name = InvaderDisplayName;

            if (string.IsNullOrEmpty(name))
            {
                PrototypeId avatarRef = RenderAvatarRef;
                if (avatarRef != PrototypeId.Invalid)
                    name = ShortPrototypeName(GameDatabase.GetPrototypeName(avatarRef));
                else
                {
                    PrototypeId teamupRef = RenderTeamupRef;
                    if (teamupRef != PrototypeId.Invalid)
                        name = ShortPrototypeName(GameDatabase.GetPrototypeName(teamupRef));
                    else
                    {
                        PrototypeId bossRef = RenderBossRef;
                        name = bossRef != PrototypeId.Invalid
                            ? ShortPrototypeName(GameDatabase.GetPrototypeName(bossRef))
                            : StripControllerPrefix(GetType().Name);
                    }
                }
            }

            return AgentId != 0 ? $"{name}#{AgentId}" : name;
        }

        /// <summary>Last path segment of a prototype name, minus the ".prototype" suffix.</summary>
        private static string ShortPrototypeName(string protoName)
        {
            if (string.IsNullOrEmpty(protoName))
                return "Invader";

            int slash = protoName.LastIndexOf('/');
            string leaf = slash >= 0 ? protoName[(slash + 1)..] : protoName;

            const string suffix = ".prototype";
            if (leaf.EndsWith(suffix, StringComparison.Ordinal))
                leaf = leaf[..^suffix.Length];

            return leaf;
        }

        internal static string StripControllerPrefix(string typeName)
        {
            const string prefix = "IncursionEnemy";
            return typeName.StartsWith(prefix, StringComparison.Ordinal) && typeName.Length > prefix.Length
                ? typeName[prefix.Length..]
                : typeName;
        }

        #endregion

        #region Locomotion 

        /// <summary>
        /// Emits a one-line locomotion diagnostic for the invader.
        /// </summary>
        protected void LogLocomotionStatus(Agent agent, string context)
        {
            if (s_verboseLogging == false) return;

            Vector3 pos = agent.RegionLocation.Position;

            float movedSinceLast = _lastDiagPos.HasValue ? Vector3.Distance2D(_lastDiagPos.Value, pos) : 0f;
            _lastDiagPos = pos;

            int interestedClients = CountInterestedClients(agent);

            Locomotor loco = agent.Locomotor;
            if (loco == null)
            {
                Logger.Info($"[IncursionEnemy:Loco] entity {AgentId} ({context}): Locomotor=NULL, " +
                            $"pos={pos.ToStringNames()}, movedSinceLast={movedSinceLast:F1}, " +
                            $"simulated={agent.IsSimulated}, inWorld={agent.IsAliveInWorld}, canMove={agent.CanMove()}, " +
                            $"moveAuth={agent.IsMovementAuthoritative}, interestedClients={interestedClients}, " +
                            $"immobileProto={agent.AgentPrototype?.Locomotion?.Immobile}.");
                return;
            }

            string goalStr = loco.GetPathGoal(out Vector3 goal) ? goal.ToStringNames() : "<none>";

            Logger.Info($"[IncursionEnemy:Loco] entity {AgentId} ({context}): " +
                        $"pos={pos.ToStringNames()}, movedSinceLast={movedSinceLast:F1}, goal={goalStr}, " +
                        $"moveAuth={agent.IsMovementAuthoritative}, interestedClients={interestedClients}, " +
                        $"simulated={agent.IsSimulated}, inWorld={agent.IsAliveInWorld}, canMove={agent.CanMove()}, " +
                        $"enabled={loco.IsEnabled}, moving={loco.IsMoving}, " +
                        $"method={loco.Method}, pathFlags={loco.PathFlags}, runSpeed={loco.DefaultRunSpeed}, " +
                        $"followId={loco.FollowEntityId}, hasPath={loco.HasPath}, pathResult={loco.LastGeneratedPathResult}, " +
                        $"stuck={loco.IsStuck}.");
        }

        /// <summary>Counts clients receiving proximity AOI updates for this entity (0 => no replication).</summary>
        private int CountInterestedClients(Agent agent)
        {
            var manager = Game.NetworkManager;
            if (manager == null) return -1;

            List<PlayerConnection> connections = new();
            manager.GetInterestedClients(connections, agent, AOINetworkPolicyValues.AOIChannelProximity, false);
            return connections.Count;
        }

        #endregion

        #region Subclass Hooks

        /// <summary>Assign powers, set properties.</summary>
        protected abstract void OnSetup(Agent agent);

        /// <summary>Maps current health fraction (0..1) to a phase index. Default: single phase 0.</summary>
        protected virtual int GetPhaseForHealthPct(float healthPct) => 0;

        /// <summary>Called once when the phase index changes (e.g. enrage). Default: no-op.</summary>
        protected virtual void OnPhaseChanged(Agent agent, int newPhase) { }

        /// <summary>Multiplier applied to all cooldowns for the current phase (smaller = faster). Default 1.</summary>
        protected virtual float PhaseCooldownScale() => 1.0f;

        /// <summary>
        /// Per-ability outgoing damage scale (1.0 = unchanged). Default: <see cref="DamageScale"/>.
        /// </summary>
        protected virtual float GetDamageScaleForPower(PrototypeId powerRef) => DamageScale;

        #endregion

        #region Entrance Intro

        /// <summary>
        /// Kicks off the entrance intro: plays a warp-in VFX, optionally says random overhead dialog,
        /// and puts the enemy into an excited state where it uses powers from much further away.
        /// </summary>
        public void BeginIntro(Agent agent)
        {
            if (_disposed || agent == null || agent.IsAliveInWorld == false) return;

            _introActive = true;
            _introEndTime = Game.CurrentTime + TimeSpan.FromMilliseconds(IntroDurationMs);
            _introVfxPlayed = false;
            _introDialogSaid = false;

            // Face the nearest player so the entrance looks deliberate.
            Avatar target = FindNearestTargetAvatar(agent);
            if (target != null)
                agent.OrientToward(target.RegionLocation.Position);

            if (PlayIntroVfx)
                PlayIntroVfxInternal(agent);

            if (SayIntroDialog)
                SayIntroDialogInternal(agent);

            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Intro] {InvaderLabel} entrance intro started ({IntroDurationMs}ms, attackRange x{IntroAttackRangeMultiplier}).");
        }

        /// <summary>
        /// Defers BeginIntro to the first think tick. This gives the client a full network
        /// tick to process the proxy spawn + attachment before intro VFX triggers AOI
        /// proximity updates that can expose the SheHulk mesh on the nameplate proxy.
        /// </summary>
        public void ScheduleIntro(Agent agent)
        {
            if (_disposed || agent == null || agent.IsAliveInWorld == false) return;
            _pendingIntro = true;
        }

        private void PlayIntroVfxInternal(Agent agent)
        {
            if (_introVfxPlayed) return;
            _introVfxPlayed = true;

            var visualsProto = GameDatabase.PowerVisualsGlobalsPrototype;
            AssetId vfxAsset = visualsProto != null ? visualsProto.AvatarLeashTeleportClass : AssetId.Invalid;
            if (vfxAsset == AssetId.Invalid) return;

            var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                .SetEntityId(agent.Id)
                .SetPowerAssetRef((ulong)vfxAsset)
                .Build();

            Game.NetworkManager?.SendMessageToInterested(msg, agent, AOINetworkPolicyValues.AOIChannelProximity);

            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Intro] {InvaderLabel} warp-in VFX played.");
        }

        private void SayIntroDialogInternal(Agent agent)
        {
            if (_introDialogSaid) return;
            _introDialogSaid = true;

            LocaleStringId[] ids = IntroDialogLocaleIds;
            if (ids == null || ids.Length == 0) return;

            LocaleStringId chosen = ids[Game.Random.Next(ids.Length)];
            if ((ulong)chosen == 0) return;

            agent.ShowOverheadText(chosen, (float)TimeSpan.FromMilliseconds(IntroDurationMs).TotalSeconds);

            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Intro] {InvaderLabel} overhead text: 0x{(ulong)chosen:X16}");
        }

        protected bool IsInIntroState()
        {
            if (_introActive == false) return false;
            if (Game.CurrentTime >= _introEndTime)
            {
                _introActive = false;
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Intro] {InvaderLabel} entrance intro ended.");
                return false;
            }
            return true;
        }

        protected float GetEffectiveAttackRange()
        {
            float range = AttackRange;
            if (IsInIntroState())
                range *= IntroAttackRangeMultiplier;
            return range;
        }

        #endregion

        #region Think 

        private void ScheduleNextThink()
        {
            if (_disposed) return;
            var scheduler = Game.GameEventScheduler;
            if (scheduler == null) return;
            if (_thinkEvent.IsValid) return;

            scheduler.ScheduleEvent(_thinkEvent, TimeSpan.FromMilliseconds(ThinkIntervalMs), _events);
            _thinkEvent.Get().Initialize(this);
        }

        private void Think()
        {
            Agent agent = GetAgent();

            // Fire deferred proxy configuration on the first think tick - this gives the
            // client a full network tick to process the bare proxy entity (with only
            // spec-level properties) before any post-spawn property writes occur.
            // See SpawnNameplateProxyDeferred for details on why this matters.
            if (_pendingProxyConfig)
            {
                _pendingProxyConfig = false;
                ConfigureSpawnedProxy(agent);
                _introDelayTicks = 1;  // delay intro by 1 more tick after proxy config
            }

            // Fire deferred intro one tick AFTER proxy config completes - this gives the
            // client a full network tick to process the proxy config property replication
            // (loot strip, power strip, attach) before the intro VFX sends AOI proximity
            // updates that can cause the client to re-process the avatar pawn.
            if (_pendingIntro)
            {
                if (_introDelayTicks > 0)
                {
                    _introDelayTicks--;
                }
                else
                {
                    _pendingIntro = false;
                    BeginIntro(agent);
                }
            }

            // Safety: if the agent is invisible for any reason (death, stealth, etc.),
            // make sure the spoof nameplate is cleared so it doesn't float without a body.
            // ClearSpoofAvatarPlayerName() is a no-op once the name is already empty.
            if (agent != null && agent.IsInWorld && agent.Properties[PropertyEnum.Visible] == false)
                agent.ClearSpoofAvatarPlayerName();

            // Dying grace period: agent is dead but lingering DoTs / missiles still need the
            // damage-scale lookup ref.  Keep the controller alive until the grace expires.
            if (_dying)
            {
                ThinkDying(agent);
                return;
            }

            // Forced death check: some boss prototypes (e.g. Kaecilius) have a HealthMin
            // passive that clamps health to 1, preventing the normal death pipeline from
            // ever firing (health never reaches 0, so Kill() is never called). If the agent
            // is alive in world but its health has dropped to 1 or below, force-kill it so
            // the dying grace period can proceed.
            if (agent != null && agent.IsAliveInWorld && agent.Properties[PropertyEnum.Health] <= 1L)
            {
                long healthMax = agent.Properties[PropertyEnum.HealthMax];
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} health reached {agent.Properties[PropertyEnum.Health]}/{healthMax} - force-killing to bypass HealthMin passive.");
                IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Death] Force-kill triggered (health={agent.Properties[PropertyEnum.Health]}, HealthMin passive bypass).");
                try { agent.Kill(null, KillFlags.NoLoot | KillFlags.NoExp); }
                catch { /* agent may already be destroyed */ }
            }

            if (agent == null || agent.IsAliveInWorld == false)
            {
                TimeSpan lifetime = Game.CurrentTime - _spawnTime;
                int graceMs = DeathGracePeriodMs;
                string deathMsg = $"[IncursionEnemy:Death] {InvaderLabel} lifetime={lifetime.TotalSeconds:F1}s  maxDeficit={_maxHealthDeficit}  inCombatAtEnd={_inCombat}  graceMs={graceMs}";
                if (IsIncursionLoggingEnabled)
                    Logger.Info(deathMsg);
                IncursionLogCollator.WriteLine(AgentId, deathMsg);

                if (graceMs > 0)
                {
                    _dying = true;
                    _deathPhase = 0;
                    TimeSpan now = Game.CurrentTime;
                    // Short outro: 1.5s dialog hook, 2.5s beam VFX, 3s invisible+exit world.
                    // The entity lingers in the EntityManager for the full grace period.
                    int outroMs = Math.Min(1500, graceMs);
                    int invisibleMs = Math.Min(3000, graceMs);
                    int beamMs = Math.Max(0, invisibleMs - 500);
                    _deathOutroTime = now + TimeSpan.FromMilliseconds(outroMs);
                    _deathBeamTime = now + TimeSpan.FromMilliseconds(beamMs);
                    _deathInvisibleTime = now + TimeSpan.FromMilliseconds(invisibleMs);
                    _deathGraceEnd = now + TimeSpan.FromMilliseconds(graceMs);

                    // Schedule proxy destruction based on the controller's preference.
                    int proxyDelay = NameplateProxyDestroyDelayMs;
                    int proxyDestroyMs = proxyDelay < 0 ? invisibleMs : Math.Min(proxyDelay, graceMs);
                    _deathProxyDestroyTime = now + TimeSpan.FromMilliseconds(proxyDestroyMs);
                    _proxyDestroyed = false;
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} entering grace period for {graceMs}ms so lingering effects can resolve.");
                    IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Death] Entering grace period for {graceMs}ms.");

                    // Keep the agent entity alive in the EntityManager during the grace period
                    // so lingering DoTs / missiles can still walk the ownership chain and resolve
                    // the proper incursion damage scale. Without this, OnRemoveFromWorld may
                    // schedule a Destroy that removes the entity before the grace period ends.
                    if (agent != null)
                    {
                        try
                        {
                            agent.CancelExitWorldEvent();
                            agent.CancelKillEvent();
                            agent.CancelDestroyEvent();
                        }
                        catch { /* entity may already be destroyed */ }

                        // Make it untargetable and invulnerable so players don't keep hitting a dead body.
                        try
                        {
                            agent.Properties[PropertyEnum.Untargetable] = true;
                            agent.Properties[PropertyEnum.Invulnerable] = true;
                        }
                        catch { /* entity may already be destroyed */ }
                    }

                    ScheduleNextThink();
                    return;
                }

                // Grace period disabled - immediate disposal.
                IncursionLogCollator.EndSession(AgentId);
                Dispose();
                return;
            }

            Avatar target = FindNearestTargetAvatar(agent);
            if (target != null)
            {
                UpdatePhase(agent);

                // Freeze movement while executing a non-movement power so the combat body
                // stays in sync with the client's rendered animation.
                // Bosses that cast without matching animations (e.g. BloodLord) opt out.
                // Per-power CanMoveDuringPower overrides the freeze for specific abilities.
                if (FreezeMovementDuringPower == false
                    || IsExecutingNonMovementPower(agent) == false
                    || (agent.ActivePowerRef != PrototypeId.Invalid && CanMoveDuringPower(agent.ActivePowerRef)))
                    ChaseTarget(agent, target);

                CheckAndStopExpiredChannel(agent);

                // Allow subclasses to do custom pattern-based power casting.
                // If the override handles power usage this tick, skip standard logic.
                if (TryCustomPowerCast(agent, target) == false)
                    TryUsePower(agent, target);

                if (EnableImpatience)
                    CheckAndApplyImpatience(agent, target);

                if (_diagThinksRemaining > 0)
                {
                    _diagThinksRemaining--;
                    int dist2 = (int)Vector3.DistanceSquared2D(agent.RegionLocation.Position, target.RegionLocation.Position);
                    LogLocomotionStatus(agent, $"think target={target.Id} dist2={dist2}");
                }
            }

            UpdateCombatState(agent, target);

            // Safety net: force-end any power that has been executing too long.
            // This runs BEFORE stuck recovery so that if a power is stuck, it gets
            // ended here and stuck recovery can then re-engage normally.
            CheckAndStopStuckPower(agent);

            if (EnableStuckRecovery)
                CheckAndRecoverIfStuck(agent, target);
            CheckAndStopExpiredChannel(agent);

            // Sync the nameplate proxy's position to the combat body.
            SyncNameplateProxy(agent);

            ScheduleNextThink();
        }

        #endregion

        #region Nameplate Proxy

        /// <summary>
        /// Schedules deferred post-spawn configuration of the nameplate proxy.
        /// The proxy entity is spawned by IncursionManager with only spec-level properties
        /// (set before spec.Spawn()). All post-spawn operations that write replicated
        /// properties - loot stripping, power stripping, SetDormant, AttachToEntity -
        /// are deferred to the first think tick via this method.
        /// 
        /// WHY: Post-spawn property writes trigger replication to the client. If they
        /// happen in the same network tick as spec.Spawn(), the client receives property
        /// updates while still processing the initial entity creation message. This can
        /// cause the client to re-process the avatar pawn and assign a default SheHulk
        /// costume/mesh - the exact visibility bug we're trying to prevent.
        /// 
        /// KNOWN BEHAVIOR (documented from in-game observation):
        /// - NOT setting CostumeCurrent on the spec -> proxy has no mesh (correct, invisible)
        /// - Setting IsClientEntityHidden + Visible=false on the spec -> client ignores these
        ///   for modded render-as-avatar entities (does NOT hide the mesh)
        /// - Writing ANY property on the proxy after spawn -> triggers replication -> client
        ///   may re-process avatar pawn and assign default SheHulk costume (SHE-HULK VISIBLE)
        /// - Repeatedly writing Visible=false in Think -> makes it WORSE (more replication)
        /// - Deferring all post-spawn property writes to first think tick -> client has
        ///   already processed the bare entity, subsequent replication is less likely to
        ///   trigger costume assignment (CURRENT APPROACH)
        /// </summary>
        public void ScheduleProxyConfig(ulong combatBodyId)
        {
            _pendingProxyCombatBodyId = combatBodyId;
            _pendingProxyConfig = true;
        }

        /// <summary>
        /// Performs all post-spawn proxy configuration: loot stripping, power stripping,
        /// AI disable, dormant, simulate false, and physics attachment. Each operation
        /// writes replicated properties on the proxy, so they are batched here and deferred
        /// to the first think tick to avoid racing with the client's entity creation processing.
        /// </summary>
        private void ConfigureSpawnedProxy(Agent agent)
        {
            if (ProxyEntityId == Entity.InvalidId) return;
            var proxy = Game.EntityManager.GetEntity<WorldEntity>(ProxyEntityId);
            if (proxy == null)
            {
                ProxyLogger.Warn($"[ProxyConfig] {InvaderLabel} proxy entity {ProxyEntityId} not found.");
                return;
            }

            var configDeltaMs = (Game.CurrentTime - _proxySpawnGameTime).TotalMilliseconds;
            string renderProtoName = proxy.ClientPrototypeRefOverride != PrototypeId.Invalid
                ? GameDatabase.GetPrototypeName(proxy.ClientPrototypeRefOverride) : "(none)";
            PrototypeId costumeVal = proxy.Properties[PropertyEnum.CostumeCurrent];
            string costumeName = costumeVal != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeVal) : "Invalid";
            bool visibleVal = proxy.Properties[PropertyEnum.Visible];
            ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} configuring proxy {proxy.Id} (deferred {configDeltaMs:F1}ms after spawn). " +
                             $"Proxy IsInWorld={proxy.IsInWorld}, Visible={visibleVal}, " +
                             $"IsClientEntityHidden={proxy.TestStatus(EntityStatus.ClientOnly) == false}, " +
                             $"IsClientRenderedAsAvatar={proxy.IsClientRenderedAsAvatar}, " +
                             $"ClientRenderProto='{renderProtoName}', SpoofAvatarWorldInstanceId={proxy.SpoofAvatarWorldInstanceId}, " +
                             $"CostumeCurrent={costumeName} ({(ulong)costumeVal}).");

            // 1. Strip loot tables - removes LootTablePrototype properties (replication).
            int lootRemoved = 0;
            if (proxy is Agent proxyAgent0)
            {
                var propsBefore = proxyAgent0.Properties.IteratePropertyRange(PropertyEnum.LootTablePrototype).Count();
                RemoveDeathLootTables(proxyAgent0);
                lootRemoved = propsBefore;
            }
            ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} stripped {lootRemoved} loot table properties.");

            // 2. Strip all powers - UnassignPower modifies the power collection (replication).
            int powersRemoved = 0;
            if (proxy is Agent proxyAgent && proxyAgent.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxyAgent.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                {
                    if (proxyAgent.PowerCollection.ContainsPower(powerRef))
                    {
                        proxyAgent.UnassignPower(powerRef);
                        powersRemoved++;
                    }
                }
            }
            ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} stripped {powersRemoved} powers.");

            // 3. Disable AI - SetDormant is NOT needed here because Dormant=true is set
            // on the spec before spawn. The entity is already dormant. We only disable
            // the AI controller to prevent any future think ticks from firing.
            if (proxy is Agent aiAgent)
            {
                aiAgent.AIController?.SetIsEnabled(false);
                ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} AI disabled (dormant already set on spec).");
            }

            // 4. Prevent simulation - modifies collection membership (likely no replication).
            proxy.SetSimulated(false);
            ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} simulation disabled.");

            // 5. Attach to combat body - writes Properties[AttachedToEntityId] (replication).
            // This is the last operation because attachment triggers the most visible
            // client-side reprocessing (the proxy starts following the combat body).
            if (agent != null && agent.IsInWorld)
            {
                // Diagnostic: log CostumeCurrent and key property state BEFORE attach.
                PrototypeId costumeBefore = proxy.Properties[PropertyEnum.CostumeCurrent];
                string costumeBeforeName = costumeBefore != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeBefore) : "Invalid";
                bool visibleBefore = proxy.Properties[PropertyEnum.Visible];
                var attachedBefore = proxy.Properties.HasProperty(PropertyEnum.AttachedToEntityId);
                ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} BEFORE attach: CostumeCurrent={costumeBeforeName} ({(ulong)costumeBefore}), " +
                                 $"Visible={visibleBefore}, HasAttachedToEntityId={attachedBefore}.");

                proxy.AttachToEntity(agent);

                // Diagnostic: log CostumeCurrent and key property state AFTER attach.
                PrototypeId costumeAfter = proxy.Properties[PropertyEnum.CostumeCurrent];
                string costumeAfterName = costumeAfter != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(costumeAfter) : "Invalid";
                bool visibleAfter = proxy.Properties[PropertyEnum.Visible];
                bool costumeChanged = costumeBefore != costumeAfter;
                ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} AFTER attach: CostumeCurrent={costumeAfterName} ({(ulong)costumeAfter}), " +
                                 $"Visible={visibleAfter}, IsAttached={proxy.IsAttachedToEntity}. " +
                                 $"CostumeChanged={costumeChanged}.");
                ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} attached proxy to combat body {agent.Id}.");
            }
            else
            {
                ProxyLogger.Warn($"[ProxyConfig] {InvaderLabel} combat body not in world - proxy not attached.");
            }

            PrototypeId finalCostume = proxy.Properties[PropertyEnum.CostumeCurrent];
            string finalCostumeName = finalCostume != PrototypeId.Invalid ? GameDatabase.GetPrototypeName(finalCostume) : "Invalid";
            bool finalVisible = proxy.Properties[PropertyEnum.Visible];
            ProxyLogger.Info($"[ProxyConfig] {InvaderLabel} proxy configuration complete. " +
                             $"Final state: Visible={finalVisible}, " +
                             $"IsInWorld={proxy.IsInWorld}, IsAttached={proxy.IsAttachedToEntity}, " +
                             $"CostumeCurrent={finalCostumeName} ({(ulong)finalCostume}).");
        }

        /// <summary>
        /// Syncs the invisible nameplate proxy's position to the combat body.
        /// The proxy is attached via physics, but we also update position explicitly
        /// as a fallback in case the attachment doesn't propagate for invisible entities.
        /// </summary>
        private void SyncNameplateProxy(Agent agent)
        {
            if (ProxyEntityId == Entity.InvalidId) return;
            var proxy = Game.EntityManager.GetEntity<WorldEntity>(ProxyEntityId);
            if (proxy == null || proxy.IsInWorld == false) return;
            if (agent == null || agent.IsInWorld == false) return;

            // Do NOT touch proxy properties here - repeatedly setting Visible=false
            // triggers property replication to the client, which can cause it to
            // re-process the avatar pawn and assign a default SheHulk costume/mesh.
            // The proxy is hidden at spawn time by NOT setting CostumeCurrent.

            Vector3 proxyPos = proxy.RegionLocation.Position;
            Vector3 agentPos = agent.RegionLocation.Position;
            if (Vector3.DistanceSquared2D(proxyPos, agentPos) > 1f)
            {
                proxy.ChangeRegionPosition(agentPos, agent.RegionLocation.Orientation,
                    ChangePositionFlags.DoNotSendToServer | ChangePositionFlags.SkipInterestUpdate);
            }
        }

        /// <summary>
        /// Destroys the invisible nameplate proxy entity. Called during death cleanup
        /// and controller disposal.
        /// </summary>
        private void DestroyNameplateProxy()
        {
            if (ProxyEntityId == Entity.InvalidId) return;
            try
            {
                var proxy = Game.EntityManager.GetEntity<WorldEntity>(ProxyEntityId);
                if (proxy != null)
                {
                    proxy.ClearSpoofAvatarPlayerName();
                    proxy.ExitWorld();
                    proxy.Destroy();
                }
            }
            catch { /* proxy may already be destroyed */ }
            ProxyEntityId = Entity.InvalidId;
        }

        #endregion

        #region Think Dying

        /// <summary>
        /// Runs the 4-phase death sequence during the dying grace period:
        /// 1) outro hook, 2) teleport beam VFX, 3) invisible + hide nameplate + vaporize VFX + exit world,
        /// 4) final cleanup and disposal once the grace period ends.
        /// </summary>
        private void ThinkDying(Agent agent)
        {
            TimeSpan now = Game.CurrentTime;

            // Destroy the nameplate proxy at the configured time (may be before phase 3).
            if (_proxyDestroyed == false && now >= _deathProxyDestroyTime)
            {
                _proxyDestroyed = true;
                DestroyNameplateProxy();
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} nameplate proxy destroyed at {NameplateProxyDestroyDelayMs}ms delay.");
            }

            // Phase 1: Outro (dialog voicebox) at T+1.5s
            if (_deathPhase < 1 && now >= _deathOutroTime)
            {
                _deathPhase = 1;
                if (agent != null)
                {
                    // TODO: Show overhead text when a suitable LocaleStringId is identified.
                    // For now this phase is a hook for future voicebox dialog.
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} outro phase.");
                }
            }

            // Phase 2: Teleport beam VFX a little before the body vanishes.
            if (_deathPhase < 2 && now >= _deathBeamTime)
            {
                _deathPhase = 2;
                if (agent != null)
                {
                    var visualsProto = GameDatabase.PowerVisualsGlobalsPrototype;
                    if (visualsProto != null && visualsProto.AvatarLeashTeleportClass != AssetId.Invalid)
                    {
                        var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                            .SetEntityId(agent.Id)
                            .SetPowerAssetRef((ulong)visualsProto.AvatarLeashTeleportClass)
                            .Build();
                        Game.NetworkManager?.SendMessageToInterested(msg, agent, AOINetworkPolicyValues.AOIChannelProximity);
                    }

                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} teleport beam VFX.");
                }
            }

            // Phase 3: Invisible + hide nameplate + vaporization VFX
            if (_deathPhase < 3 && now >= _deathInvisibleTime)
            {
                _deathPhase = 3;
                if (agent != null)
                {
                    try
                    {
                        // Clear the spoof name BEFORE making invisible so the
                        // replication message reaches clients while the entity
                        // is still in their AOI. Otherwise the nameplate can
                        // persist after the body is hidden.
                        agent.ClearSpoofAvatarPlayerName();
                        agent.Properties[PropertyEnum.Visible] = false;
                    }
                    catch { /* entity may already be destroyed */ }

                    // Play a vaporization VFX at the agent's location
                    var visualsProto = GameDatabase.PowerVisualsGlobalsPrototype;
                    if (visualsProto != null && visualsProto.LootVaporizedClass != AssetId.Invalid)
                    {
                        var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                            .SetEntityId(agent.Id)
                            .SetPowerAssetRef((ulong)visualsProto.LootVaporizedClass)
                            .Build();
                        Game.NetworkManager?.SendMessageToInterested(msg, agent, AOINetworkPolicyValues.AOIChannelProximity);
                    }

                    // Remove the entity from the client's AOI immediately so the nameplate
                    // vanishes with the body. The entity stays in the EntityManager until the
                    // grace period ends so lingering DoTs can still resolve their damage scale.
                    try { agent.ExitWorld(); }
                    catch { /* entity may already be destroyed */ }

                    // Fallback: destroy the proxy if the early check hasn't already done so.
                    if (_proxyDestroyed == false)
                    {
                        _proxyDestroyed = true;
                        DestroyNameplateProxy();
                    }

                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} turned invisible + VFX + exited world.");
                }
            }

            // Phase 4: Actual death cleanup at T=graceMs
            if (now >= _deathGraceEnd)
            {
                if (agent != null)
                {
                    try { agent.Destroy(); } catch { /* entity may already be destroyed */ }
                }

                TimeSpan lifetime = now - _spawnTime;
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Death] {InvaderLabel} cleanup complete. lifetime={lifetime.TotalSeconds:F1}s");
                IncursionLogCollator.EndSession(AgentId);
                Dispose();
            }
            else
            {
                // Still in grace period - keep scheduling thinks so we can check again.
                ScheduleNextThink();
            }
        }

        #endregion

        #region Stuck Recovery

        /// <summary>
        /// Detects when the agent is stuck near a target but not moving, or when it hasn't used
        /// an ability for a long time, and performs a  recovery action.
        /// </summary>
        private void CheckAndRecoverIfStuck(Agent agent, Avatar target)
        {
            if (target == null) return;

            // Don't interrupt a power that's currently animating - let it finish.
            // EXCEPTION: if the power has been running too long, the safety net in
            // CheckAndStopStuckPower will have already force-ended it before we get here.
            if (agent.IsExecutingPower) return;

            TimeSpan now = Game.CurrentTime;

            // Sample position every 2 seconds
            if (now - _lastPositionSampleTime >= TimeSpan.FromMilliseconds(2000))
            {
                _lastPositionSampleTime = now;
                Vector3 currentPos = agent.RegionLocation.Position;
                float moved = Vector3.Distance2D(currentPos, _lastSampledPosition);
                _lastSampledPosition = currentPos;

                float distToTargetSq = Vector3.DistanceSquared2D(currentPos, target.RegionLocation.Position);
                float chaseRangeSq = ChaseRange * ChaseRange;

                // Only count as "potentially stuck" if we are near the target but barely moved
                if (distToTargetSq <= chaseRangeSq && moved < 15.0f)
                    _stuckCheckCount++;
                else
                    _stuckCheckCount = 0;
            }

            // Ability idle check (6 seconds without a successful activation)
            bool idleAbility = now - _lastAbilityUseTime > TimeSpan.FromMilliseconds(6000);

            // Trigger recovery if stuck for ~6s or idle for 6s
            if (_stuckCheckCount >= 3 || idleAbility)
            {
                _stuckCheckCount = 0;
                _recoveryAttempts++;
                string reason = idleAbility ? "idle ability" : "not moving near target";
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Recovery] {InvaderLabel} triggering recovery #{_recoveryAttempts} (reason: {reason}).");
                IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Recovery] attempt #{_recoveryAttempts} (reason: {reason}).");

                // Mimic stun-recovery: hard-reset the combat body first so we don't stay desynced.
                PerformCombatReset(agent, $"recovery #{_recoveryAttempts} ({reason})");

                // 50% chance to try a random power, 25% re-follow, 25% random move.
                //  pushes recovery toward using abilities rather than just repositioning.
                int action = Game.Random.Next(4);
                switch (action)
                {
                    case 0:
                        TryRecoveryReFollow(agent, target);
                        break;
                    case 1:
                    case 2:
                        TryRecoveryRandomPower(agent, target);
                        break;
                    case 3:
                        TryRecoveryRandomMove(agent);
                        break;
                }

                // Reset ability timer so we don't spam recovery
                _lastAbilityUseTime = now;
            }
        }

        private void TryRecoveryReFollow(Agent agent, Avatar target)
        {
            var locomotor = agent.Locomotor;
            if (locomotor == null) return;

            locomotor.Stop();
            locomotor.FollowEntity(target.Id, AttackRange * 0.5f);
            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Recovery] {InvaderLabel} re-follow target at closer range.");
            IncursionLogCollator.WriteLine(AgentId, "[IncursionEnemy:Recovery] Re-follow target at closer range.");
        }

        private void TryRecoveryRandomPower(Agent agent, Avatar target)
        {
            if (Powers.Count == 0) return;

            TimeSpan now = Game.CurrentTime;

            // Gather all ready powers, skipping the last-used one if alternatives exist.
            List<PrototypeId> ready = new();
            PrototypeId lastReady = PrototypeId.Invalid;
            foreach (PrototypeId powerRef in _powerPriority)
            {
                if (_cooldownEndTimes.TryGetValue(powerRef, out TimeSpan end) && now < end)
                    continue;

                lastReady = powerRef;
                if (powerRef != _lastUsedPowerRef)
                    ready.Add(powerRef);
            }

            PrototypeId chosen;
            if (ready.Count > 0)
            {
                chosen = ready[Game.Random.Next(ready.Count)];
            }
            else if (lastReady != PrototypeId.Invalid)
            {
                chosen = lastReady;
            }
            else
            {
                return; // nothing ready
            }

            if (ActivatePowerOnTarget(agent, chosen, target))
            {
                _lastAbilityUseTime = now;
                _cooldownEndTimes[chosen] = now + TimeSpan.FromMilliseconds(GetCooldownMsForPower(chosen));
                _globalAttackCooldownEnd = now + TimeSpan.FromMilliseconds(GlobalAttackCooldownMs * Math.Max(0.05f, PhaseCooldownScale()));

                _lastUsedPowerRef = chosen;

                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Recovery] {InvaderLabel} used '{GameDatabase.GetPrototypeName(chosen)}' as recovery power.");
                IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Recovery] Used recovery power '{GameDatabase.GetPrototypeName(chosen)}'.");
            }
        }

        private void TryRecoveryRandomMove(Agent agent)
        {
            var locomotor = agent.Locomotor;
            if (locomotor == null || agent.CanMove() == false) return;

            // Pick a random nearby offset (200-400 units) to break out of stuck geometry
            float angle = (float)(Game.Random.NextDouble() * Math.PI * 2);
            float dist = 200f + (float)(Game.Random.NextDouble() * 200f);
            Vector3 offset = new Vector3(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
            Vector3 dest = agent.RegionLocation.Position + offset;

            LocomotionOptions options = new();
            options.PathGenerationFlags = PathGenerationFlags.IncompletedPath;

            if (locomotor.PathTo(dest, ref options))
            {
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Recovery] {InvaderLabel} pathing to random nearby offset ({dist:F0} units).");
                IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Recovery] Pathing to random nearby offset ({dist:F0} units).");
            }
            else if (locomotor.MoveTo(dest, ref options))
            {
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Recovery] {InvaderLabel} moving to random nearby offset (simple move).");
                IncursionLogCollator.WriteLine(AgentId, "[IncursionEnemy:Recovery] Moving to random nearby offset (simple move).");
            }
        }

        #endregion

        #region Impatience Reset

        /// <summary>
        /// If the enemy has been near the target for too long without a successful attack,
        /// it gets "impatient": resets combat state, halves remaining cooldowns, and forces
        /// the lowest-cooldown available power so it doesn't just follow the player passively.
        /// </summary>
        private void CheckAndApplyImpatience(Agent agent, Avatar target)
        {
            if (target == null) return;

            // Don't interrupt a power that's currently animating - let it finish.
            // PerformCombatReset would cancel it mid-cast, causing the boss to
            // slide out of its animation prematurely.
            if (agent.IsExecutingPower) return;

            TimeSpan now = Game.CurrentTime;
            float distSq = Vector3.DistanceSquared2D(agent.RegionLocation.Position, target.RegionLocation.Position);

            // Only get impatient when we're actually close enough to fight. ( disabled , enemy should impatiently use powers regardless of range )
            // if (distSq > AttackRange * AttackRange) return;

            // Time since the last successful power activation.
            double idleMs = (now - _lastSuccessfulAttackTime).TotalMilliseconds;
            int thresholdMs = 4000; // first threshold
            if (_impatienceTriggers > 0)
                thresholdMs = Math.Max(3000, thresholdMs - _impatienceTriggers * 1000);

            if (idleMs < thresholdMs) return;

            _impatienceTriggers++;
            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Impatience] {InvaderLabel} trigger #{_impatienceTriggers} (idle {(int)idleMs}ms).");
            IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Impatience] trigger #{_impatienceTriggers} (idle {(int)idleMs}ms).");

            // Hard-reset combat body to clear any desync/stall.
            PerformCombatReset(agent, $"impatience #{_impatienceTriggers}");

            // Halve remaining cooldowns so something is likely ready.
            var keys = new List<PrototypeId>(_cooldownEndTimes.Keys);
            foreach (PrototypeId powerRef in keys)
            {
                TimeSpan end = _cooldownEndTimes[powerRef];
                TimeSpan remaining = end - now;
                if (remaining > TimeSpan.Zero)
                    _cooldownEndTimes[powerRef] = now + TimeSpan.FromMilliseconds(remaining.TotalMilliseconds * 0.5);
            }

            // Reset global cooldown so we can fire immediately.
            _globalAttackCooldownEnd = now;

            // Gather all ready powers, excluding the last-used one if there are other options.
            List<PrototypeId> readyPowers = new();
            PrototypeId bestFallback = PrototypeId.Invalid;
            TimeSpan bestRemaining = TimeSpan.MaxValue;
            foreach (PrototypeId powerRef in Powers)
            {
                if (powerRef == PrototypeId.Invalid) continue;
                TimeSpan remaining = _cooldownEndTimes.TryGetValue(powerRef, out TimeSpan end) ? end - now : TimeSpan.Zero;
                if (remaining > TimeSpan.Zero) continue;

                if (powerRef != _lastUsedPowerRef)
                    readyPowers.Add(powerRef);

                if (remaining < bestRemaining)
                {
                    bestRemaining = remaining;
                    bestFallback = powerRef;
                }
            }

            // Prefer a different power from the last-used one. Pick randomly so the kit feels varied.
            PrototypeId chosen = PrototypeId.Invalid;
            if (readyPowers.Count > 0)
            {
                int idx = Game.Random.Next(readyPowers.Count);
                chosen = readyPowers[idx];
            }
            else if (bestFallback != PrototypeId.Invalid)
            {
                chosen = bestFallback;
            }

            if (chosen != PrototypeId.Invalid && ActivatePowerOnTarget(agent, chosen, target))
            {
                _lastAbilityUseTime = now;
                _lastSuccessfulAttackTime = now;
                _cooldownEndTimes[chosen] = now + TimeSpan.FromMilliseconds(GetCooldownMsForPower(chosen));
                _globalAttackCooldownEnd = now + TimeSpan.FromMilliseconds(GlobalAttackCooldownMs * Math.Max(0.05f, PhaseCooldownScale()));

                _lastUsedPowerRef = chosen;

                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Impatience] {InvaderLabel} forced '{GameDatabase.GetPrototypeName(chosen)}' (ready pool={readyPowers.Count}).");
            }
            else
            {
                // Even if the forced activation failed (e.g. out of melee range), reset the
                // idle timer so impatience doesn't fire every think tick.  The thrall gets
                // a full threshold cycle to close distance before impatience tries again.
                _lastSuccessfulAttackTime = now;
            }
        }

        /// <summary>
        /// Hard-resets the combat body the same way a stun/knockdown recovery does:
        /// ends active powers, stops locomotion, and clears stale state.
        /// </summary>
        private void PerformCombatReset(Agent agent, string reason)
        {
            // End any active power (stun recovery does this).
            Power activePower = agent.GetPower(agent.ActivePowerRef);
            if (activePower != null)
                activePower.EndPower(EndPowerFlags.ExplicitCancel | EndPowerFlags.Interrupting);

            // Stop locomotor so it doesn't stay stuck on a stale path.
            agent.Locomotor?.Stop();

            // Clear our own channeled-power tracking.
            if (_channelPowerRef != PrototypeId.Invalid)
            {
                _channelPowerRef = PrototypeId.Invalid;
                _channelMaxMs = 0;
            }

            // Drop any throwable object (stun recovery unassigns it).
            var throwablePower = agent.GetThrowablePower();
            if (throwablePower != null)
                agent.UnassignPower(throwablePower.PrototypeDataRef);

            if (IsIncursionLoggingEnabled)
                Logger.Info($"[IncursionEnemy:Reset] {InvaderLabel} combat reset ({reason}).");
            IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Reset] {reason}");
        }

        #endregion

        #region Channel Stop

        // Default timeout for channeled powers that don't have an explicit MaxChannelMs
        // in the power table. Prevents enemies from being permanently locked in a
        // channeled power state when FreezeMovementDuringPower stops locomotion.
        private const int DefaultMaxChannelMs = 3000;

        /// <summary>
        /// Returns the MaxChannelMs for a given power from the power table, or a default
        /// value if the power is channeled but not explicitly listed.
        /// </summary>
        private int GetMaxChannelMsForPower(PrototypeId powerRef, Power power = null)
        {
            IncursionPowerEntry[] table = PowerTable;
            if (table != null)
            {
                foreach (var entry in table)
                {
                    if (entry.Power == powerRef)
                        return entry.MaxChannelMs;
                }
            }

            // For channeled powers not in the table, use the default timeout so they
            // don't lock the enemy in a channeling state forever.
            // Use IsChannelingPower() (prototype check) instead of IsChanneling (runtime
            // phase) because the power may still be in the Active phase right after
            // activation and hasn't transitioned to Channeling yet.
            if (power != null && power.IsChannelingPower())
                return DefaultMaxChannelMs;

            // For non-channeled powers not in the table, no timeout needed.
            return 0;
        }

        /// <summary>
        /// Safety net: if the agent has been executing ANY power for longer than
        /// DefaultPowerTimeoutMs, force-end it. This catches channeled powers that
        /// slipped through channel tracking (e.g. phase mismatch at activation time).
        /// </summary>
        private void CheckAndStopStuckPower(Agent agent)
        {
            if (_powerExecPowerRef == PrototypeId.Invalid)
                return;

            if (agent.IsExecutingPower == false || agent.ActivePowerRef != _powerExecPowerRef)
            {
                _powerExecPowerRef = PrototypeId.Invalid;
                return;
            }

            TimeSpan elapsed = Game.CurrentTime - _powerExecStartTime;
            if (elapsed.TotalMilliseconds >= DefaultPowerTimeoutMs)
            {
                Power activePower = agent.GetPower(_powerExecPowerRef);
                if (activePower != null)
                {
                    activePower.EndPower(EndPowerFlags.ExplicitCancel | EndPowerFlags.Interrupting);
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:StuckPower] {InvaderLabel} force-ended '{GameDatabase.GetPrototypeName(_powerExecPowerRef)}' after {(int)elapsed.TotalMilliseconds}ms (safety net).");
                }
                _powerExecPowerRef = PrototypeId.Invalid;
                _channelPowerRef = PrototypeId.Invalid;
                _channelMaxMs = 0;
            }
        }

        /// <summary>
        /// If the agent is currently channeling a power that has exceeded its MaxChannelMs,
        /// forcibly ends it. Also clears tracking if the agent is no longer executing the power.
        /// </summary>
        private void CheckAndStopExpiredChannel(Agent agent)
        {
            if (_channelPowerRef == PrototypeId.Invalid || _channelMaxMs <= 0)
                return;

            if (agent.IsExecutingPower == false || agent.ActivePowerRef != _channelPowerRef)
            {
                // Channel ended naturally
                _channelPowerRef = PrototypeId.Invalid;
                _channelMaxMs = 0;
                return;
            }

            TimeSpan elapsed = Game.CurrentTime - _channelStartTime;
            if (elapsed.TotalMilliseconds >= _channelMaxMs)
            {
                Power activePower = agent.GetPower(_channelPowerRef);
                if (activePower != null && activePower.IsChanneling)
                {
                    activePower.EndPower(EndPowerFlags.ExplicitCancel);
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Channel] {InvaderLabel} stopped '{GameDatabase.GetPrototypeName(_channelPowerRef)}' after {(int)elapsed.TotalMilliseconds}ms (max {_channelMaxMs}ms).");
                    IncursionLogCollator.WriteLine(AgentId, $"[IncursionEnemy:Channel] Stopped '{GameDatabase.GetPrototypeName(_channelPowerRef)}' after {(int)elapsed.TotalMilliseconds}ms.");
                }

                // EndPower clears ActivePowerRef so IsExecutingPower becomes false,
                // allowing the next think tick to resume movement via ChaseTarget.
                _channelPowerRef = PrototypeId.Invalid;
                _channelMaxMs = 0;
            }
        }

        #endregion

        #region Target Move

        private Avatar FindNearestTargetAvatar(Agent agent)
        {
            Region region = agent.Region;
            if (region == null) return null;

            Vector3 selfPos = agent.RegionLocation.Position;
            // Use WakeRadius until first engagement, then ChaseRange forever.
            float effectiveRange = _permanentAggro ? ChaseRange : WakeRadius;
            float bestDistSq = effectiveRange * effectiveRange;
            Avatar nearest = null;

            foreach (Player player in Game.EntityManager.Players)
            {
                Avatar avatar = player?.CurrentAvatar;
                if (avatar == null || avatar.IsAliveInWorld == false) continue;
                if (avatar.Region != region) continue;

                float distSq = Vector3.DistanceSquared2D(selfPos, avatar.RegionLocation.Position);
                if (distSq <= bestDistSq)
                {
                    bestDistSq = distSq;
                    nearest = avatar;
                }
            }

            // Once we find a target within wake radius, permanently aggro so we chase forever.
            if (nearest != null && _permanentAggro == false)
            {
                _permanentAggro = true;
                if (agent != null)
                    agent.Properties[PropertyEnum.AIAlwaysAggroed] = true;
                // Activate the wake-up resilience window (extra damage reduction that decays over time).
                RefreshAdditionalResilience();
                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[IncursionEnemy:Wake] {InvaderLabel} woken by player at {bestDistSq:F0}^2 units - permanent aggro enabled.");
            }

            return nearest;
        }

        /// <summary>
        /// True when the agent is executing a power that should freeze movement
        /// </summary>
        private bool IsExecutingNonMovementPower(Agent agent)
        {
            if (agent == null || agent.IsExecutingPower == false) return false;
            Power activePower = agent.ActivePower;
            return activePower != null && activePower.IsPartOfAMovementPower() == false;
        }

        private void ChaseTarget(Agent agent, Avatar target)
        {
            agent.OrientToward(target.RegionLocation.Position);

            var locomotor = agent.Locomotor;
            if (locomotor == null) return;

            // Safe to call every tick; FollowEntity only resets when the target changes.
            // During the intro the enemy hangs back further to show off ranged powers.
            float maxFollow = IsInIntroState() ? 250f : 120f;
            float followDistance = Math.Min(GetEffectiveAttackRange() * 0.5f, maxFollow);
            locomotor.FollowEntity(target.Id, followDistance);
        }

        #endregion

        #region Power Selection 

        /// <summary>
        /// Looks up a power in the explicit <see cref="PowerTable"/>.
        /// </summary>
        protected IncursionPowerEntry? FindPowerTableEntry(PrototypeId powerRef)
        {
            IncursionPowerEntry[] table = PowerTable;
            if (table == null) return null;
            foreach (var entry in table)
                if (entry.Power == powerRef) return entry;
            return null;
        }

        /// <summary>
        /// Returns true if the power table explicitly allows movement during the given power.
        /// </summary>
        private bool CanMoveDuringPower(PrototypeId powerRef)
        {
            return FindPowerTableEntry(powerRef)?.CanMoveDuringPower == true;
        }

        /// <summary>
        /// Computes the cooldown (in ms) for a given power, accounting for explicit table
        /// overrides, ultimate detection, and phase scaling.
        /// </summary>
        protected float GetCooldownMsForPower(PrototypeId powerRef)
        {
            float phaseScale = Math.Max(0.05f, PhaseCooldownScale());

            // 1. Explicit table override wins everything.
            var entry = FindPowerTableEntry(powerRef);
            if (entry.HasValue && entry.Value.CooldownMs > 0)
                return entry.Value.CooldownMs * phaseScale;

            // 2. Ultimate multiplier if the power prototype is flagged as ultimate.
            var powerProto = powerRef.As<PowerPrototype>();
            if (powerProto != null && powerProto.IsUltimate)
                return PerPowerCooldownMs * UltimateCooldownMultiplier * phaseScale;

            // 3. Default per-power cooldown.
            return PerPowerCooldownMs * phaseScale;
        }

        private void TryUsePower(Agent agent, Avatar target)
        {
            if (Powers.Count == 0) return;

            // Don't try to activate a new power while one is already in progress.
            // CanActivatePower would fail anyway, and this avoids unnecessary work.
            if (agent.IsExecutingPower) return;

            // If we're stuck in a channeled power, stop it before trying anything new.
            if (_channelPowerRef != PrototypeId.Invalid && agent.IsExecutingPower && agent.ActivePowerRef == _channelPowerRef)
            {
                Power activePower = agent.GetPower(_channelPowerRef);
                if (activePower != null && activePower.IsChanneling)
                {
                    activePower.EndPower(EndPowerFlags.ExplicitCancel);
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[IncursionEnemy:Channel] {InvaderLabel} pre-emptively stopped channeled '{GameDatabase.GetPrototypeName(_channelPowerRef)}' to switch powers.");
                    _channelPowerRef = PrototypeId.Invalid;
                    _channelMaxMs = 0;
                }
            }

            TimeSpan now = Game.CurrentTime;
            if (now < _globalAttackCooldownEnd) return;

            // During intro, give the agent 1.5 seconds to settle into the world before attacking.
            // This prevents powers from failing because the entity hasn't fully replicated yet.
            if (IsInIntroState() && (now - _spawnTime).TotalMilliseconds < 1500)
                return;

            float effectiveRange = GetEffectiveAttackRange();
            float distSq = Vector3.DistanceSquared2D(agent.RegionLocation.Position, target.RegionLocation.Position);
            if (distSq > effectiveRange * effectiveRange) return;

            // Gather all ready powers, skipping the last-used one if alternatives exist.
            List<PrototypeId> ready = new();
            PrototypeId lastReady = PrototypeId.Invalid;
            foreach (PrototypeId powerRef in _powerPriority)
            {
                if (_cooldownEndTimes.TryGetValue(powerRef, out TimeSpan end) && now < end)
                    continue;

                lastReady = powerRef;
                if (powerRef != _lastUsedPowerRef)
                    ready.Add(powerRef);
            }

            PrototypeId chosen;
            if (ready.Count > 0)
            {
                // Random pick from ready powers (excluding last-used) so the kit feels varied.
                chosen = ready[Game.Random.Next(ready.Count)];
            }
            else if (lastReady != PrototypeId.Invalid)
            {
                // Only the last-used power is ready - allow it.
                chosen = lastReady;
            }
            else
            {
                return; // nothing ready
            }

            if (ActivatePowerOnTarget(agent, chosen, target) == false)
                return;

            // Apply per-power cooldown ( checks table overrides, ultimate multiplier, and phase scaling).
            _cooldownEndTimes[chosen] = now + TimeSpan.FromMilliseconds(GetCooldownMsForPower(chosen));
            _globalAttackCooldownEnd = now + TimeSpan.FromMilliseconds(GlobalAttackCooldownMs * Math.Max(0.05f, PhaseCooldownScale()));

            _lastUsedPowerRef = chosen;
        }

        /// <summary>
        /// Activates the given power toward the target. Assigns the power if not already present.
        /// </summary>
        protected bool ActivatePowerOnTarget(Agent agent, PrototypeId powerRef, Avatar target)
        {
            if (powerRef == PrototypeId.Invalid) return false;

            Power power = agent.GetPower(powerRef);
            if (power == null)
            {
                PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                if (agent.AssignPower(powerRef, indexProps) == null)
                    return false;
                power = agent.GetPower(powerRef);
                if (power == null) return false;
            }

            // Resolve this power's outgoing damage scale (for the activation log only; the damage
            // pipeline queries it on demand via the IncursionManager registry).
            float damageScale = GetOutgoingDamageScale(powerRef);

            ulong targetId = target.Id;
            Vector3 targetPos = target.RegionLocation.Position;

            PowerUseResult canActivate = agent.CanActivatePower(power, targetId, targetPos);
            if (canActivate != PowerUseResult.Success)
            {
                if (IsIncursionLoggingEnabled)
                {
                    Logger.Info($"[IncursionEnemy:PowerFail] {InvaderLabel} CanActivatePower failed for '{GameDatabase.GetPrototypeName(powerRef)}': {canActivate}");
                }
                return false;
            }

            PowerActivationSettings settings = new(targetId, targetPos, agent.RegionLocation.Position);
            settings.Flags |= PowerActivationSettingsFlags.NotifyOwner;
            bool activated = agent.ActivatePower(powerRef, ref settings) == PowerUseResult.Success;

            if (activated)
            {
                _lastAbilityUseTime = Game.CurrentTime;
                _lastSuccessfulAttackTime = Game.CurrentTime;

                // Stop locomotion for non-movement powers so the combat body doesn't drift
                // away from the rendered animation, which causes invisible-damage desync.
                // Bosses that opt out of FreezeMovementDuringPower (e.g. BloodLord) keep moving.
                // Per-power CanMoveDuringPower overrides the freeze for specific abilities.
                if (FreezeMovementDuringPower && power.IsPartOfAMovementPower() == false && CanMoveDuringPower(powerRef) == false)
                    agent.Locomotor?.Stop();

                // Track channeled powers so we can forcibly stop them after MaxChannelMs.
                // Pass the power instance so GetMaxChannelMsForPower can detect channeled
                // powers not explicitly listed in the table and apply the default timeout.
                int maxChannelMs = GetMaxChannelMsForPower(powerRef, power);
                if (maxChannelMs > 0)
                {
                    _channelStartTime = Game.CurrentTime;
                    _channelPowerRef = powerRef;
                    _channelMaxMs = maxChannelMs;
                    LogVerbose($"[IncursionEnemy] {InvaderLabel} started channeled '{GameDatabase.GetPrototypeName(powerRef)}' (max {maxChannelMs}ms).");
                }

                // Safety net: track ALL power executions so we can force-end any that
                // run too long, even if channel tracking didn't start.
                _powerExecStartTime = Game.CurrentTime;
                _powerExecPowerRef = powerRef;

                LogVerbose($"[IncursionEnemy] {InvaderLabel} used '{GameDatabase.GetPrototypeName(powerRef)}' (damage scale x{damageScale:0.###}) on target {targetId}.");
            }

            return activated;
        }

        /// <summary>
        /// Activates the given power at a specific world position. Used for pattern-based
        /// casting (e.g. cross AOE around the boss). Assigns the power if not already present.
        /// </summary>
        protected bool ActivatePowerAtPosition(Agent agent, PrototypeId powerRef, Vector3 targetPos, bool skipActivationCheck = false)
        {
            if (powerRef == PrototypeId.Invalid) return false;

            Power power = agent.GetPower(powerRef);
            if (power == null)
            {
                PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                if (agent.AssignPower(powerRef, indexProps) == null)
                    return false;
                power = agent.GetPower(powerRef);
                if (power == null) return false;
            }

            // Pattern casts (e.g. cross AOE) activate multiple powers in a single tick.
            // After the first cast, IsExecutingPower is true, which would block subsequent
            // casts via CanActivatePower. skipActivationCheck bypasses this for pattern casts.
            if (skipActivationCheck == false &&
                agent.CanActivatePower(power, Entity.InvalidId, targetPos) != PowerUseResult.Success)
                return false;

            PowerActivationSettings settings = new(Entity.InvalidId, targetPos, agent.RegionLocation.Position);
            settings.Flags |= PowerActivationSettingsFlags.NotifyOwner;
            bool activated = agent.ActivatePower(powerRef, ref settings) == PowerUseResult.Success;

            if (activated)
            {
                _lastAbilityUseTime = Game.CurrentTime;
                _lastSuccessfulAttackTime = Game.CurrentTime;

                // Do NOT stop the locomotor here - pattern-cast powers are expected to
                // play without matching animations (T-pose), and the boss should keep
                // chasing the target between casts.

                // Track channeled powers and general execution for the safety net.
                int maxChannelMs = GetMaxChannelMsForPower(powerRef, power);
                if (maxChannelMs > 0)
                {
                    _channelStartTime = Game.CurrentTime;
                    _channelPowerRef = powerRef;
                    _channelMaxMs = maxChannelMs;
                }
                _powerExecStartTime = Game.CurrentTime;
                _powerExecPowerRef = powerRef;
            }

            return activated;
        }

        /// <summary>
        /// Virtual hook called during the think loop before standard power logic.
        /// Override to implement custom pattern-based power casting (e.g. cross AOE).
        /// Return true to suppress standard TryUsePower this tick, false to fall through.
        /// </summary>
        protected virtual bool TryCustomPowerCast(Agent agent, Avatar target) => false;

        /// <summary>
        /// When true (default), the enemy freezes movement while executing a non-movement power
        /// to stay in sync with the client's rendered animation. Override to false for bosses
        /// that cast powers without matching animations (e.g. BloodLord) so they keep chasing.
        /// </summary>
        protected virtual bool FreezeMovementDuringPower => true;

        /// <summary>
        /// When true (default), the impatience mechanic runs: if the enemy hasn't landed a
        /// successful attack within the threshold, it halves cooldowns and forces a power.
        /// Trash mobs that spawn and wait (e.g. Vampire Thralls) should override to false
        /// since they don't need aggressive pressure - only IncursionEnemies that spawn near
        /// the player and hunt immediately benefit from it.
        /// </summary>
        protected virtual bool EnableImpatience => true;

        /// <summary>
        /// When true (default), the stuck-recovery mechanic runs: if the enemy hasn't moved
        /// or used an ability for ~6s, it performs a combat reset and recovery action.
        /// Trash mobs that spawn far from the player and need to path to them can disable
        /// this to avoid false-positive resets during long approach runs.
        /// </summary>
        protected virtual bool EnableStuckRecovery => true;

        #endregion

        #region Phase Health

        private void UpdatePhase(Agent agent)
        {
            float pct = GetHealthPct(agent);
            int phase = GetPhaseForHealthPct(pct);
            if (phase == _phase) return;

            // Phase only advances forward - healing (e.g. BloodLord phase heal) can push
            // health above the threshold, but we never revert to a lower phase. This prevents
            // the same phase transition from re-triggering and re-healing repeatedly.
            if (phase < _phase)
                return;

            _phase = phase;
            try
            {
                OnPhaseChanged(agent, phase);
            }
            catch (Exception e)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name} OnPhaseChanged threw: {e.Message}");
            }
        }

        protected static float GetHealthPct(Agent agent)
        {
            long health = agent.Properties[PropertyEnum.Health];
            long healthMax = agent.Properties[PropertyEnum.HealthMax];
            return healthMax > 0 ? (float)health / healthMax : 1.0f;
        }

        /// <summary>shuffle for a List</summary>
        private static void ShuffleList<T>(List<T> list, GRandom rng)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region Lifecycle  

        public TimeSpan SpawnTime => _spawnTime;
        public TimeSpan LastCombatTime => _lastCombatTime;

        private void UpdateCombatState(Agent agent, Avatar target)
        {
            if (target != null)
            {
                _lastCombatTime = Game.CurrentTime;
                _inCombat = true;
            }
            else
            {
                _inCombat = false;
            }

            long health = agent.Properties[PropertyEnum.Health];
            long healthMax = agent.Properties[PropertyEnum.HealthMax];
            long deficit = Math.Max(0, healthMax - health);
            if (deficit > _maxHealthDeficit)
                _maxHealthDeficit = deficit;

            // Log health changes or periodic snapshot during combat
            _healthLogCounter++;
            bool healthChanged = _lastLoggedHealth >= 0 && health != _lastLoggedHealth;
            bool periodicLog = _inCombat && _healthLogCounter % 15 == 0; // ~every 5s while in combat
            if (healthChanged || periodicLog)
            {
                string healthMsg = $"[IncursionEnemy:Health] {InvaderLabel} health={health}/{healthMax} ({(healthMax > 0 ? (int)(100f * health / healthMax) : 0)}%)  deficit={deficit}  inCombat={_inCombat}";
                if (IsIncursionLoggingEnabled)
                    Logger.Info(healthMsg);
                IncursionLogCollator.WriteLine(AgentId, healthMsg);
                _lastLoggedHealth = health;
            }
        }

        public bool IsIdle(TimeSpan threshold) => Game.CurrentTime - _lastCombatTime > threshold;

        public bool IsExpired(TimeSpan maxLifetime) => Game.CurrentTime - _spawnTime > maxLifetime;

        /// <summary>
        /// Incursion Max Invader Culling for Optimization
        /// Priority score for culling decisions. Higher = more worthy of preservation.
        /// In-combat invaders get a large bonus; damage taken adds moderate bonus;
        /// age applies a small penalty.
        /// </summary>
        public float GetPriorityScore()
        {
            if (_disposed) return -99999f;
            if (_dying) return 99999f; // never cull a controller that is resolving lingering effects

            Agent agent = GetAgent();
            if (agent == null || agent.IsAliveInWorld == false) return -99999f;

            float score = 0f;

            if (_inCombat) score += 1000f;

            long healthMax = agent.Properties[PropertyEnum.HealthMax];
            if (healthMax > 0)
                score += (float)_maxHealthDeficit / healthMax * 100f;

            TimeSpan age = Game.CurrentTime - _spawnTime;
            score -= (float)age.TotalMinutes;

            return score;
        }

        public string GetLabel() => InvaderLabel;

        /// <summary>Shorthand enemy name for hunt tracking (e.g. "CaptainAmerica", "BossMODOK").</summary>
        public string HuntShorthand => StripControllerPrefix(GetType().Name);

        /// <summary>True if the hunt kill has already been recorded for this controller.</summary>
        public bool HuntKillRecorded => _huntKillRecorded;

        /// <summary>Marks the hunt kill as recorded so it's not double-counted.</summary>
        public void MarkHuntKillRecorded() => _huntKillRecorded = true;

        #endregion

        #region Conditions

        /// <summary>
        /// Applies a condition from a power prototype directly to the agent's ConditionCollection,
        /// bypassing power activation entirely. No animation plays, no T-pose risk.
        /// Uses the same InitializeFromPower pattern as IncursionEnemyBossOnslaught.
        /// </summary>
        protected void ApplyConditionFromPower(Agent agent, PrototypeId powerProtoRef)
        {
            if (agent == null || powerProtoRef == PrototypeId.Invalid) return;

            var conditionCollection = agent.ConditionCollection;
            if (conditionCollection == null) return;

            ConditionPrototype conditionProto = GetConditionPrototypeFromPower(powerProtoRef);
            if (conditionProto == null) return;

            PowerPayload payload = CreateMinimalPayload(powerProtoRef, agent);
            if (payload == null) return;

            Condition condition = ConditionCollection.AllocateCondition();
            condition.InitializeFromPower(
                conditionCollection.NextConditionId, payload, conditionProto, TimeSpan.Zero);
            conditionCollection.AddCondition(condition);
        }

        /// <summary>
        /// Applies conditions from multiple power prototypes in sequence.
        /// </summary>
        protected void ApplyConditionsFromPowers(Agent agent, PrototypeId[] powerProtoRefs)
        {
            if (agent == null || powerProtoRefs == null) return;
            foreach (PrototypeId powerRef in powerProtoRefs)
                ApplyConditionFromPower(agent, powerRef);
        }

        /// <summary>
        /// Extracts the first ConditionPrototype from a power's AppliesConditions mixin list.
        /// </summary>
        protected static ConditionPrototype GetConditionPrototypeFromPower(PrototypeId powerProtoRef)
        {
            if (powerProtoRef == PrototypeId.Invalid) return null;
            var powerProto = powerProtoRef.As<PowerPrototype>();
            if (powerProto?.AppliesConditions == null) return null;
            foreach (var item in powerProto.AppliesConditions)
            {
                if (item.Prototype is ConditionPrototype conditionProto)
                    return conditionProto;
            }
            return null;
        }

        /// <summary>
        /// Creates a minimal PowerPayload with just enough data for InitializeFromPower.
        /// Uses reflection to set private/protected setters on PowerPayload and PowerEffectsPacket.
        /// </summary>
        protected PowerPayload CreateMinimalPayload(PrototypeId powerProtoRef, Agent agent)
        {
            PowerPrototype powerProto = powerProtoRef.As<PowerPrototype>();
            if (powerProto == null) return null;

            var payload = new PowerPayload();
            payload.Init(Game);

            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.PowerPrototype))
                .SetValue(payload, powerProto);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.PowerOwnerId))
                .SetValue(payload, agent.Id);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.UltimateOwnerId))
                .SetValue(payload, agent.Id);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.TargetId))
                .SetValue(payload, agent.Id);

            typeof(PowerPayload).GetProperty(nameof(PowerPayload.PowerProtoRef))
                .SetValue(payload, powerProtoRef);

            return payload;
        }

        #endregion

        #region Scheduled Event

        private class ThinkEvent : CallMethodEvent<IncursionEnemyController>
        {
            protected override CallbackDelegate GetCallback() => (controller) => controller.Think();
        }

        #endregion
    }
}
#endregion
