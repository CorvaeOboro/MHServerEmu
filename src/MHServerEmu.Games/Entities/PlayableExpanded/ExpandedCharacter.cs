using MHServerEmu.Core.Logging;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities.PlayableExpanded
{
    /// <summary>
    /// One hotbar power of an <see cref="ExpandedCharacter"/>: the power prototype, whether it
    /// goes on the hotbar (in table order), and its outgoing damage scale.
    /// Mirrors Incursion's IncursionPowerEntry per-ability tuning approach.
    /// </summary>
    public class ExpandedPowerEntry
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public PrototypeId PowerRef { get; }
        public bool OnHotbar { get; }
        public float DamageScale { get; }
        /// <summary>How long (ms) the avatar is immobilised while the body plays this power's animation. 0 = 500 ms default.</summary>
        public int CastTimeMs { get; }

        public ExpandedPowerEntry(string powerProtoName, bool onHotbar, float damageScale = 1.0f, int castTimeMs = 0)
        {
            PowerRef = GameDatabase.GetPrototypeRefByName(powerProtoName);
            OnHotbar = onHotbar;
            DamageScale = damageScale;
            CastTimeMs = castTimeMs;

            if (PowerRef == PrototypeId.Invalid)
                Logger.Warn($"[PlayableExpanded] Unknown power prototype '{powerProtoName}'.");
        }

        public ExpandedPowerEntry(PrototypeId powerRef, bool onHotbar, float damageScale = 1.0f, int castTimeMs = 0)
        {
            PowerRef = powerRef;
            OnHotbar = onHotbar;
            DamageScale = damageScale;
            CastTimeMs = castTimeMs;
        }
    }

    /// <summary>
    /// Definition of an "expanded" playable character - a NEW playable character concept that
    /// borrows the assets (model, animations, powers) of an existing non-avatar entity such as a
    /// Team-Up. This deliberately does NOT touch the real Team-Up system: actual Team-Ups remain
    /// untouched NPC pets, and playing as one does not conflict with also summoning one.
    ///
    /// One subclass per character (like IncursionEnemyAntMan for Incursion) holds all
    /// character-specific tuning: the body prototype, the hotbar power table with per-power
    /// damage scales, and optional custom logic hooks. Subclasses are auto-discovered by
    /// <see cref="ExpandedCharacterRegistry"/>.
    /// </summary>
    public abstract class ExpandedCharacter
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>The entity prototype whose assets the body uses (e.g. a Team-Up agent prototype).</summary>
        public abstract PrototypeId BodyProtoRef { get; }

        /// <summary>User-facing name, also matched by the !playas command.</summary>
        public abstract string DisplayName { get; }

        /// <summary>Fallback outgoing damage scale for powers without a table entry.</summary>
        public virtual float DamageScale => 3.0f;

        /// <summary>How often the synced body position is updated, in milliseconds.</summary>
        public virtual int ThinkIntervalMs => 50;

        /// <summary>
        /// Explicit power table (hotbar powers in slot order + per-power damage scales).
        /// Null = derive hotbar powers dynamically from the body's Team-Up power progression.
        /// </summary>
        protected virtual ExpandedPowerEntry[] PowerTable => null;

        #region Custom Logic Hooks (per-character gameplay specifics)

        /// <summary>Called after the body is spawned and synced, before the hotbar is mapped.</summary>
        public virtual void OnSwapIn(Avatar avatar, Agent body) { }

        /// <summary>Called before the body despawns when swapping back.</summary>
        public virtual void OnSwapOut(Avatar avatar, Agent body) { }

        /// <summary>Called after a hotbar power was successfully forwarded to the body.</summary>
        public virtual void OnPowerForwarded(Avatar avatar, Agent body, PrototypeId powerRef) { }

        #endregion

        /// <summary>
        /// Resolves the hotbar powers in slot order: explicit table first, otherwise derived from
        /// the body prototype's Team-Up power progression (active powers by unlock level).
        /// </summary>
        public List<ExpandedPowerEntry> GetHotbarPowers()
        {
            List<ExpandedPowerEntry> powers = new();

            ExpandedPowerEntry[] table = PowerTable;
            if (table != null)
            {
                foreach (ExpandedPowerEntry entry in table)
                {
                    if (entry.OnHotbar && entry.PowerRef != PrototypeId.Invalid)
                        powers.Add(entry);
                }

                return powers;
            }

            // Dynamic fallback for characters without an explicit table yet.
            foreach (PrototypeId powerRef in GetActiveTeamUpPowers(BodyProtoRef))
                powers.Add(new ExpandedPowerEntry(powerRef, true, DamageScale));

            return powers;
        }

        /// <summary>Per-power outgoing damage scale, falling back to <see cref="DamageScale"/>.</summary>
        public float GetDamageScaleForPower(PrototypeId powerRef)
        {
            ExpandedPowerEntry[] table = PowerTable;
            if (table != null)
            {
                foreach (ExpandedPowerEntry entry in table)
                {
                    if (entry.PowerRef == powerRef)
                        return entry.DamageScale;
                }
            }

            return DamageScale;
        }

        /// <summary>
        /// Extracts a Team-Up prototype's active (non-away-passive) progression powers, ordered by
        /// unlock level. Used as the dynamic fallback when no explicit power table is defined.
        /// </summary>
        public static List<PrototypeId> GetActiveTeamUpPowers(PrototypeId bodyProtoRef)
        {
            List<(PrototypeId PowerRef, int Level)> entries = new();

            var teamUpProto = bodyProtoRef.As<AgentTeamUpPrototype>();
            if (teamUpProto?.PowerProgression != null)
            {
                foreach (TeamUpPowerProgressionEntryPrototype entry in teamUpProto.PowerProgression)
                {
                    if (entry.Power == PrototypeId.Invalid)
                        continue;

                    // Away/summoned passives belong to the real Team-Up pipeline, not to us.
                    if (entry.IsPassiveOnAvatarWhileAway || entry.IsPassiveOnAvatarWhileSummoned)
                        continue;

                    var powerProto = entry.Power.As<PowerPrototype>();
                    if (powerProto == null || powerProto.Activation == PowerActivationType.Passive)
                        continue;

                    entries.Add((entry.Power, entry.GetRequiredLevel()));
                }
            }

            entries.Sort((a, b) => a.Level.CompareTo(b.Level));

            List<PrototypeId> powers = new(entries.Count);
            foreach (var entry in entries)
                powers.Add(entry.PowerRef);

            return powers;
        }
    }

    /// <summary>
    /// Generic definition for any Team-Up prototype without a dedicated subclass yet - keeps every
    /// Team-Up playable with default tuning while dedicated ExpandedCharacter classes are written.
    /// </summary>
    public class GenericTeamUpExpandedCharacter : ExpandedCharacter
    {
        private readonly PrototypeId _bodyProtoRef;
        private readonly string _displayName;

        public override PrototypeId BodyProtoRef => _bodyProtoRef;
        public override string DisplayName => _displayName;

        public GenericTeamUpExpandedCharacter(PrototypeId bodyProtoRef)
        {
            _bodyProtoRef = bodyProtoRef;
            _displayName = GameDatabase.GetFormattedPrototypeName(bodyProtoRef);
        }
    }
}
