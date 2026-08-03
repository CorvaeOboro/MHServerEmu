#region DeathRecap
// =============================================================================
// MOD Death Recap
// =============================================================================
//   Records incoming damage/healing events on the avatar in a circular buffer.
//   On death, the buffer is flushed and the top damage sources are sent to chat.
//
//  Config.ini :
//   DeathRecapEnable, DeathRecapMaxEvents, DeathRecapTopN,
//   DeathRecapNameLength, DeathRecapDamageTypeLength,
//   DeathRecapLoggingEnable
//
//  Integration:
//   - WorldEntity.ApplyPowerResultsInternal calls RecordDamageEvent() on every
//     hostile hit where the target is an Avatar.
//   - WorldEntity.ApplyPowerResultsInternal calls FlushOnDeath() when the avatar
//     dies (health <= 0, no cheat-death proc).
//   - Player.ModDeathRecap.cs handles chat output and /recap command storage.
//
//  VERSION:: 20260721
// =============================================================================

using System.Text;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities.Avatars
{
    public partial class Avatar
    {
        private DeathRecapBuffer _deathRecapBuffer;

        /// <summary>
        /// Lazily creates the death recap buffer. Returns null if the feature is disabled.
        /// </summary>
        internal DeathRecapBuffer GetOrInitDeathRecapBuffer()
        {
            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.DeathRecapEnable == false)
                return null;

            _deathRecapBuffer ??= new DeathRecapBuffer(customOptions.DeathRecapMaxEvents);
            return _deathRecapBuffer;
        }

        /// <summary>
        /// Called from WorldEntity.ApplyPowerResultsInternal for every hostile power result
        /// where the target is an Avatar. Records per-type damage, flags, source, and power.
        /// </summary>
        internal void RecordDamageEvent(PowerResults powerResults, WorldEntity ultimateOwner, long startHealth, long endHealth)
        {
            var buffer = GetOrInitDeathRecapBuffer();
            if (buffer == null) return;

            long healthDelta = endHealth - startHealth;
            bool isHeal = healthDelta > 0 && powerResults.TestFlag(PowerResultFlags.Hostile) == false;

            // Skip zero-change events (invulnerable, etc.)
            if (healthDelta == 0) return;

            // For hostile hits, only record if there was actual damage
            if (powerResults.TestFlag(PowerResultFlags.Hostile) && healthDelta >= 0) return;

            // Heals are not recorded - death recap focuses on damage sources only
            if (isHeal) return;

            string sourceName = "Unknown";
            if (ultimateOwner != null)
            {
                if (Game?.IncursionManager?.TryGetInvaderDisplayName(ultimateOwner.Id, out string invaderName) == true)
                    sourceName = invaderName;
                else
                    sourceName = ultimateOwner.PrototypeName;
            }
            string powerName = powerResults.PowerPrototype != null
                ? GameDatabase.GetPrototypeName(powerResults.PowerPrototype.DataRef)
                : "Unknown";

            float physical = powerResults.GetDamageForClient(DamageType.Physical);
            float energy = powerResults.GetDamageForClient(DamageType.Energy);
            float mental = powerResults.GetDamageForClient(DamageType.Mental);
            float healing = isHeal ? powerResults.HealingForClient : 0f;

            var entry = new DeathRecapEntry
            {
                Timestamp = Game.CurrentTime,
                SourceEntityId = ultimateOwner?.Id ?? 0,
                SourceName = sourceName,
                PowerName = powerName,
                PhysicalDamage = physical,
                EnergyDamage = energy,
                MentalDamage = mental,
                Healing = healing,
                Flags = powerResults.Flags,
                HealthBefore = startHealth,
                HealthAfter = endHealth
            };

            buffer.Add(entry);

            if (Game.CustomGameOptions.DeathRecapLoggingEnable)
                Logger.Trace($"[DeathRecap] Recorded: {sourceName} -> {PrototypeName} phys={physical} ene={energy} men={mental} heal={healing} flags={powerResults.Flags}");
        }

        /// <summary>
        /// Called from WorldEntity.ApplyPowerResultsInternal when the avatar dies
        /// (health <= 0 and no cheat-death proc saved them).
        /// Returns the buffer for the caller to pass to Player for chat output.
        /// </summary>
        internal DeathRecapBuffer FlushOnDeath()
        {
            var buffer = _deathRecapBuffer;
            if (buffer == null || buffer.Count == 0) return null;

            if (Game?.CustomGameOptions?.DeathRecapLoggingEnable == true)
                Logger.Info($"[DeathRecap] Flushing {buffer.Count} events on death of {PrototypeName}.");

            return buffer;
        }

        /// <summary>
        /// Clears the death recap buffer (e.g. on avatar exit/world change).
        /// </summary>
        internal void ClearDeathRecap()
        {
            _deathRecapBuffer?.Clear();
        }
    }

    /// <summary>
    /// Lightweight circular buffer of damage/healing events for death recap.
    /// </summary>
    internal sealed class DeathRecapBuffer
    {
        private readonly DeathRecapEntry[] _entries;
        private readonly int _capacity;
        private int _head;  // next write position
        private int _count;

        public DeathRecapBuffer(int capacity)
        {
            _capacity = Math.Max(4, capacity);
            _entries = new DeathRecapEntry[_capacity];
        }

        public int Count => _count;

        public void Add(DeathRecapEntry entry)
        {
            _entries[_head] = entry;
            _head = (_head + 1) % _capacity;
            if (_count < _capacity) _count++;
        }

        public void Clear()
        {
            _head = 0;
            _count = 0;
            Array.Clear(_entries, 0, _capacity);
        }

        /// <summary>
        /// Returns all entries in chronological order (oldest first).
        /// </summary>
        public DeathRecapEntry[] ToChronologicalArray()
        {
            if (_count == 0) return Array.Empty<DeathRecapEntry>();
            var result = new DeathRecapEntry[_count];
            int start = (_head - _count + _capacity) % _capacity;
            for (int i = 0; i < _count; i++)
                result[i] = _entries[(start + i) % _capacity];
            return result;
        }

        /// <summary>
        /// Returns the top N damage sources by total damage dealt, aggregated by source entity.
        /// </summary>
        public DeathRecapSummary[] GetTopDamageSources(int topN)
        {
            if (_count == 0) return Array.Empty<DeathRecapSummary>();

            var bySource = new Dictionary<ulong, DeathRecapSummary>();

            int start = (_head - _count + _capacity) % _capacity;
            for (int i = 0; i < _count; i++)
            {
                ref readonly var e = ref _entries[(start + i) % _capacity];
                if (e.Healing > 0) continue;  // skip heal entries for damage aggregation

                if (bySource.TryGetValue(e.SourceEntityId, out var existing) == false)
                {
                    existing = new DeathRecapSummary
                    {
                        SourceEntityId = e.SourceEntityId,
                        SourceName = e.SourceName,
                        PhysicalDamage = 0,
                        EnergyDamage = 0,
                        MentalDamage = 0,
                        HitCount = 0,
                        IsCrit = false,
                        IsDoT = false
                    };
                    bySource[e.SourceEntityId] = existing;
                }

                existing.PhysicalDamage += e.PhysicalDamage;
                existing.EnergyDamage += e.EnergyDamage;
                existing.MentalDamage += e.MentalDamage;
                existing.HitCount++;
                if (e.Flags.HasFlag(PowerResultFlags.Critical) || e.Flags.HasFlag(PowerResultFlags.SuperCritical))
                    existing.IsCrit = true;
                if (e.Flags.HasFlag(PowerResultFlags.OverTime))
                    existing.IsDoT = true;
            }

            return bySource.Values
                .OrderByDescending(s => s.TotalDamage)
                .Take(topN)
                .ToArray();
        }

        /// <summary>
        /// Returns heal entries in chronological order (if any).
        /// </summary>
        public DeathRecapEntry[] GetHealEntries()
        {
            if (_count == 0) return Array.Empty<DeathRecapEntry>();
            var heals = new List<DeathRecapEntry>();
            var all = ToChronologicalArray();
            foreach (var e in all)
            {
                if (e.Healing > 0) heals.Add(e);
            }
            return heals.ToArray();
        }

        /// <summary>
        /// Returns the time span between the first and last entry in the buffer.
        /// </summary>
        public TimeSpan GetTimeSpan()
        {
            if (_count < 2) return TimeSpan.Zero;
            var all = ToChronologicalArray();
            return all[^1].Timestamp - all[0].Timestamp;
        }
    }

    internal struct DeathRecapEntry
    {
        public TimeSpan Timestamp;
        public ulong SourceEntityId;
        public string SourceName;
        public string PowerName;
        public float PhysicalDamage;
        public float EnergyDamage;
        public float MentalDamage;
        public float Healing;
        public PowerResultFlags Flags;
        public long HealthBefore;
        public long HealthAfter;

        public readonly float TotalDamage => PhysicalDamage + EnergyDamage + MentalDamage;
    }

    internal sealed class DeathRecapSummary
    {
        public ulong SourceEntityId;
        public string SourceName;
        public float PhysicalDamage;
        public float EnergyDamage;
        public float MentalDamage;
        public int HitCount;
        public bool IsCrit;
        public bool IsDoT;

        public float TotalDamage => PhysicalDamage + EnergyDamage + MentalDamage;
    }
}
#endregion
