using MHServerEmu.Core.Extensions;
using MHServerEmu.Core.Logging;
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
        #region Incursion Controller
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

        // Powers this enemy may use (assigned to the agent during setup).
        protected readonly List<PrototypeId> Powers = new();

        /// <summary>
        /// Optional explicit power table with per-power tuning. Subclasses override this.
        /// </summary>
        protected virtual IncursionPowerEntry[] PowerTable => null;

        private readonly EventGroup _events = new();
        private readonly EventPointer<ThinkEvent> _thinkEvent = new();
        private readonly Dictionary<PrototypeId, TimeSpan> _cooldownEndTimes = new();

        // Powers whose per-ability damage scale has already been applied (and logged once).
        private readonly HashSet<PrototypeId> _scaledPowers = new();

        // Maps child effect powers (combo hits, triggered powers) back to their parent
        // so damage scaling and logging treat the whole combo as one ability.
        protected readonly Dictionary<PrototypeId, PrototypeId> _effectToParentPower = new();

        // Round-robin priority order: the first ready power is chosen, then moved to the bottom.
        private readonly List<PrototypeId> _powerPriority = new();

        private TimeSpan _globalAttackCooldownEnd = TimeSpan.Zero;
        private int _phase = -1;
        private bool _disposed;
        private bool _dying;
        private int _deathPhase;          // 0=entered, 1=outro, 2=teleport beam, 3=invisible+hide name, 4=cleanup
        private TimeSpan _deathOutroTime;
        private TimeSpan _deathBeamTime;
        private TimeSpan _deathInvisibleTime;
        private TimeSpan _deathGraceEnd;

        // Initial think cycles for which locomotion diagnostics are emitted.
        private int _diagThinksRemaining = 12;

        // Last server position sampled by the diagnostic.
        private Vector3? _lastDiagPos;

        // Cached human-readable log label: rendered identity plus entity id suffix.
        private string _label;

        // Lifecycle tracking
        private TimeSpan _spawnTime;
        private TimeSpan _lastCombatTime;
        private long _maxHealthDeficit;
        private bool _inCombat;

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

        // Last power used so we can enforce variety instead of spamming the same ability.
        private PrototypeId _lastUsedPowerRef = PrototypeId.Invalid;

        // Entrance intro state
        private bool _introActive;
        private TimeSpan _introEndTime;
        private bool _introVfxPlayed;
        private bool _introDialogSaid;

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

        /// <summary>How often (ms) the think loop runs.</summary>
        protected virtual int ThinkIntervalMs => 350;

        /// <summary>Max distance at which the enemy will attempt to use powers.</summary>
        protected virtual float AttackRange => 250.0f;

        /// <summary>Beyond this distance the enemy ignores a candidate target.</summary>
        protected virtual float ChaseRange => 5000.0f;

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
        /// Fallback raw-string lines. Unused — the controller now sends locale-based
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
        /// The rest are preserved but unconfirmed — some may not render.
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

        #region Combat Scale Config

        /// <summary>
        /// Multiplier applied to all outgoing damage (1.0 = unchanged).
        /// Avatar powers deal more damage than mob powers; default scales down.
        /// </summary>
        protected virtual float DamageScale => 0.05f;

        /// <summary>
        /// Incoming damage multiplier (1.0 = unchanged, 2.0 = double damage taken).
        /// Applied as <see cref="PropertyEnum.DamagePctVulnerability"/>.
        /// Defaults to the <c>IncursionEnemyDamageTakenMultiplier</c> config value.
        /// </summary>
        protected virtual float DamageTakenScale => Game?.CustomGameOptions?.IncursionEnemyDamageTakenMultiplier ?? 2.0f;

        /// <summary>
        /// When false (default), the enemy cannot receive healing from any source.
        /// Subclasses may override to true for self-healing bosses.
        /// </summary>
        protected virtual bool CanRegainHealth => false;

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
        /// Optional markup prefix for the overhead name. May show literally if the client does not support rich text in nameplates.
        /// </summary>
        public virtual string NameplatePrefix => null;

        /// <summary>
        /// Optional markup suffix for the overhead name. Must match <see cref="NameplatePrefix"/>.
        /// </summary>
        public virtual string NameplateSuffix => null;

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
            new("Brooklyn Bosses (Cosmic)", "Loot/Tables/Mob/Bosses/PatrolBrooklyn/Subtable/SharedPatrolBrooklynBossesCosmic.prototype",    false),
            new("Hightown Bosses (Cosmic)", "Loot/Tables/Mob/Bosses/PatrolHightown/Subtable/SharedPatrolHightownBossesCosmic.prototype",    false),
            new("Brooklyn Bosses (All)",    "Loot/Tables/Mob/Bosses/PatrolBrooklyn/Subtable/SharedPatrolBrooklynBossesAll.prototype",       false),
        };

        #endregion

        #region Stealable Power 

        /// <summary>
        /// Stealable power info for Rogue. Override per hero to match the rendered avatar.

        /// </summary>
        public virtual PrototypeId StealablePowerInfoRef => PrototypeId.Invalid;

        #endregion

        #region Construct Render Identity

        protected IncursionEnemyController(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Avatar prototype the client renders this invader as.
        /// <see cref="PrototypeId.Invalid"/> => render the combat body itself.
        /// </summary>
        public virtual PrototypeId RenderAvatarRef => PrototypeId.Invalid;

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
        }

        /// <summary>Stops the think loop and releases scheduled events.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Game?.GameEventScheduler?.CancelAllEvents(_events);
        }

        #endregion

        #region Combat Enablement

        // Hostile to Players but not to standard enemy alliances (other NPCs ignore it).
        private const string HostileAllianceName = "Entity/Alliances/EnemiesOmitFriendlies.prototype";

        // Boss rank for presence and damage scaling.
        private const string BossRankName = "Mods/Ranks/Boss.prototype";

        private static PrototypeId s_hostileAllianceRef = PrototypeId.Invalid;
        private static PrototypeId s_bossRankRef = PrototypeId.Invalid;

        /// <summary>
        /// Converts the spawned host into a mortal, hostile, mobile combatant regardless of
        /// the host prototype's original role.
        /// </summary>
        protected virtual void EnableCombat(Agent agent)
        {
            agent.Properties[PropertyEnum.Untargetable] = false;
            agent.Properties[PropertyEnum.Unaffectable] = false;
            agent.Properties[PropertyEnum.Invulnerable] = false;

            // Hub NPCs can start dormant, leaving the invader inert/invisible.
            agent.Properties[PropertyEnum.Dormant] = false;

            // Detach from any mission/encounter so cross-encounter hostility checks don't
            // block fighting with players.
            if (agent.Properties.HasProperty(PropertyEnum.MissionPrototype))
                agent.Properties.RemoveProperty(PropertyEnum.MissionPrototype);

            // Force a hostile alliance so damage is mutual.
            PrototypeId hostileAlliance = ResolveHostileAlliance();
            if (hostileAlliance != PrototypeId.Invalid)
                agent.Properties[PropertyEnum.AllianceOverride] = hostileAlliance;

            // Promote to Boss rank for presence and damage scaling.
            PrototypeId bossRank = ResolveBossRank();
            if (bossRank != PrototypeId.Invalid)
                agent.Properties[PropertyEnum.Rank] = bossRank;

            if (agent.IsHostileToPlayers() == false)
                Logger.Warn($"[IncursionEnemy] {InvaderLabel} ('{agent.PrototypeName}') is NOT hostile to players " +
                            "after override; players may be unable to damage it.");
        }

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

        /// <summary>Removes all existing death-loot table properties from the host body.</summary>
        private static void RemoveDeathLootTables(Agent agent)
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

        #region Combat Scaling Spawn 

        /// <summary>
        /// Applies combat scaling: incoming damage vulnerability and outgoing damage scaling.
        /// Per-ability damage scaling is applied separately after powers are assigned.
        /// </summary>
        protected virtual void ApplyCombatScaling(Agent agent)
        {
            float damageTakenScale = DamageTakenScale;
            if (damageTakenScale > 1.0f)
            {
                float vulnerability = damageTakenScale - 1.0f;
                agent.Properties[PropertyEnum.DamagePctVulnerability, DamageType.Any] = vulnerability;
            }

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

        #region Logging & Labels

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
                name = avatarRef != PrototypeId.Invalid
                    ? ShortPrototypeName(GameDatabase.GetPrototypeName(avatarRef))
                    : StripControllerPrefix(GetType().Name);
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

        #region Locomotion Diagnostics

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

        private bool IsInIntroState()
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

        private float GetEffectiveAttackRange()
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

                // Grace period disabled — immediate disposal.
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
                if (IsExecutingNonMovementPower(agent) == false)
                    ChaseTarget(agent, target);

                CheckAndStopExpiredChannel(agent);
                TryUsePower(agent, target);
                CheckAndApplyImpatience(agent, target);

                if (_diagThinksRemaining > 0)
                {
                    _diagThinksRemaining--;
                    int dist2 = (int)Vector3.DistanceSquared2D(agent.RegionLocation.Position, target.RegionLocation.Position);
                    LogLocomotionStatus(agent, $"think target={target.Id} dist2={dist2}");
                }
            }

            UpdateCombatState(agent, target);
            CheckAndRecoverIfStuck(agent, target);
            CheckAndStopExpiredChannel(agent);
            ScheduleNextThink();
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
                // Still in grace period — keep scheduling thinks so we can check again.
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

        #region Channel Power Stop

        /// <summary>
        /// Returns the MaxChannelMs for a given power from the power table, or 0 if not listed.
        /// </summary>
        private int GetMaxChannelMsForPower(PrototypeId powerRef)
        {
            IncursionPowerEntry[] table = PowerTable;
            if (table == null) return 0;

            foreach (var entry in table)
            {
                if (entry.Power == powerRef)
                    return entry.MaxChannelMs;
            }
            return 0;
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
            float bestDistSq = ChaseRange * ChaseRange;
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
                // Only the last-used power is ready — allow it.
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

            if (agent.CanActivatePower(power, targetId, targetPos) != PowerUseResult.Success)
                return false;

            PowerActivationSettings settings = new(targetId, targetPos, agent.RegionLocation.Position);
            settings.Flags |= PowerActivationSettingsFlags.NotifyOwner;
            bool activated = agent.ActivatePower(powerRef, ref settings) == PowerUseResult.Success;

            if (activated)
            {
                _lastAbilityUseTime = Game.CurrentTime;
                _lastSuccessfulAttackTime = Game.CurrentTime;

                // Stop locomotion for non-movement powers so the combat body doesn't drift
                // away from the rendered animation, which causes invisible-damage desync.
                if (power.IsPartOfAMovementPower() == false)
                    agent.Locomotor?.Stop();

                // Track channeled powers so we can forcibly stop them after MaxChannelMs
                int maxChannelMs = GetMaxChannelMsForPower(powerRef);
                if (maxChannelMs > 0)
                {
                    _channelStartTime = Game.CurrentTime;
                    _channelPowerRef = powerRef;
                    _channelMaxMs = maxChannelMs;
                    LogVerbose($"[IncursionEnemy] {InvaderLabel} started channeled '{GameDatabase.GetPrototypeName(powerRef)}' (max {maxChannelMs}ms).");
                }

                LogVerbose($"[IncursionEnemy] {InvaderLabel} used '{GameDatabase.GetPrototypeName(powerRef)}' (damage scale x{damageScale:0.###}) on target {targetId}.");
            }

            return activated;
        }

        #endregion

        #region Phase Health

        private void UpdatePhase(Agent agent)
        {
            float pct = GetHealthPct(agent);
            int phase = GetPhaseForHealthPct(pct);
            if (phase == _phase) return;

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

        #region Lifecycle Track Priority

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

        #endregion

        #region Scheduled Event

        private class ThinkEvent : CallMethodEvent<IncursionEnemyController>
        {
            protected override CallbackDelegate GetCallback() => (controller) => controller.Think();
        }

        #endregion
    }
}
