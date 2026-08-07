using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace MHServerEmu.Core.Network.Web
{
    /// <summary>
    /// Thread-safe singleton that aggregates per-combatant DPS data from the game service
    /// and exposes it for the WebAPI /webapi/dps endpoint.
    ///
    /// <para>
    /// The game service thread calls <see cref="RecordDamage"/> every time a power result
    /// applies health loss to a target (from <c>WorldEntity.ApplyHealthPowerResults</c>).
    /// The WebFrontend thread calls <see cref="GetSnapshot"/> / <see cref="Reset"/> when
    /// the overlay polls the endpoint. All access is via <see cref="ConcurrentDictionary{TKey, TValue}"/>
    /// so no explicit locking is needed - the trade-off is that a snapshot may include
    /// a damage event that was recorded mid-copy, which is acceptable for a DPS meter
    /// that polls once per second.
    /// </para>
    ///
    /// <para>
    /// Lives in <see cref="MHServerEmu.Core"/> because both <c>MHServerEmu.Games</c>
    /// (writer) and <c>MHServerEmu.WebFrontend</c> (reader) reference Core, but
    /// WebFrontend does NOT reference Games. This shared store avoids adding a
    /// project reference that could create circular dependencies.
    /// </para>
    /// </summary>
    public sealed class ModOverlayDpsDataStore
    {
        public static ModOverlayDpsDataStore Instance { get; } = new();

        private readonly ConcurrentDictionary<string, ModOverlayDpsCombatantEntry> _combatants = new();
        private long _resetTimestampTicks;
        private long _lastDamageTimestampTicks;

        // Periodic eviction timer - removes combatants who haven't dealt damage in a while.
        // Prevents unbounded memory growth on long-running multiplayer servers where players
        // join and leave over days/weeks of uptime. Without this, _combatants grows forever.
        private static readonly TimeSpan EvictionIdleTimeout = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan EvictionCheckInterval = TimeSpan.FromMinutes(2);
        private readonly Timer _evictionTimer;

        private ModOverlayDpsDataStore()
        {
            Reset();
            _evictionTimer = new Timer(_ => EvictIdleOlderThan(EvictionIdleTimeout),
                                       null, EvictionCheckInterval, EvictionCheckInterval);
        }

        /// <summary>
        /// Records a damage event attributed to a combatant.
        /// Called from the game service thread.
        /// </summary>
        /// <param name="combatantKey">Unique key - player name or "playerName:heroName" if per-hero tracking is desired.</param>
        /// <param name="displayName">Name to show in the overlay (player name or hero name).</param>
        /// <param name="heroName">Current hero/avatar name, or null if not a player.</param>
        /// <param name="damage">Total damage dealt in this event (sum of all damage types, &gt; 0).</param>
        /// <param name="isPhantom">True if this combatant is an AI phantom/bot.</param>
        public void RecordDamage(string combatantKey, string displayName, string heroName, long damage, bool isPhantom = false)
        {
            if (damage <= 0 || string.IsNullOrEmpty(combatantKey))
                return;

            var nowTicks = DateTime.UtcNow.Ticks;
            _lastDamageTimestampTicks = nowTicks;

            _combatants.AddOrUpdate(
                combatantKey,
                // Add factory - create a new entry
                _ => new ModOverlayDpsCombatantEntry
                {
                    Key = combatantKey,
                    Name = displayName,
                    HeroName = heroName ?? string.Empty,
                    IsPhantom = isPhantom,
                    TotalDamage = damage,
                    PeakHit = damage,
                    HitCount = 1,
                    FirstHitTimestampTicks = nowTicks,
                    LastHitTimestampTicks = nowTicks,
                },
                // Update factory - accumulate into existing entry
                (_, existing) =>
                {
                    existing.TotalDamage += damage;
                    if (damage > existing.PeakHit) existing.PeakHit = damage;
                    existing.HitCount++;
                    existing.LastHitTimestampTicks = nowTicks;
                    if (string.IsNullOrEmpty(existing.HeroName) && !string.IsNullOrEmpty(heroName))
                        existing.HeroName = heroName;
                    return existing;
                });
        }

        /// <summary>
        /// Returns a point-in-time snapshot of all combatants sorted by total damage descending.
        /// Called from the WebFrontend thread.
        /// </summary>
        public ModOverlayDpsSnapshot GetSnapshot(string playerFilter = null)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            long resetTicks = _resetTimestampTicks;
            long secondsSinceReset = (nowTicks - resetTicks) / TimeSpan.TicksPerSecond;

            var combatants = new List<ModOverlayDpsSnapshotCombatant>();
            foreach (var kvp in _combatants)
            {
                var entry = kvp.Value;

                // Apply player filter if specified (and not "*")
                if (!string.IsNullOrEmpty(playerFilter) && playerFilter != "*")
                {
                    if (!entry.Name.Equals(playerFilter, StringComparison.OrdinalIgnoreCase) &&
                        !entry.Key.Equals(playerFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                long secondsSinceLastHit = (nowTicks - entry.LastHitTimestampTicks) / TimeSpan.TicksPerSecond;
                long combatDuration = Math.Max(1, (entry.LastHitTimestampTicks - entry.FirstHitTimestampTicks) / TimeSpan.TicksPerSecond);

                // DPS over the last 10 seconds
                long tenSecAgoTicks = nowTicks - (10 * TimeSpan.TicksPerSecond);
                double dps10 = entry.LastHitTimestampTicks >= tenSecAgoTicks
                    ? (double)entry.TotalDamage / Math.Max(1, (entry.LastHitTimestampTicks - entry.FirstHitTimestampTicks) / TimeSpan.TicksPerSecond)
                    : 0; // Simplified - full sliding-window would need per-second buckets

                // Overall DPS = total damage / time since first hit (or since reset, whichever is shorter)
                long overallSpan = Math.Max(1, (nowTicks - entry.FirstHitTimestampTicks) / TimeSpan.TicksPerSecond);
                double dpsOverall = (double)entry.TotalDamage / overallSpan;

                combatants.Add(new ModOverlayDpsSnapshotCombatant
                {
                    Name = entry.Name,
                    HeroName = entry.HeroName,
                    IsPhantom = entry.IsPhantom,
                    Total = entry.TotalDamage,
                    PeakHit = entry.PeakHit,
                    Dps10 = dps10,
                    Dps60 = dpsOverall, // Same as overall for now; will refine with sliding window
                    DpsOverall = dpsOverall,
                    SecondsSinceLastHit = secondsSinceLastHit,
                });
            }

            // Sort by total damage descending
            combatants.Sort((a, b) => b.Total.CompareTo(a.Total));

            return new ModOverlayDpsSnapshot
            {
                Ok = true,
                SecondsSinceReset = secondsSinceReset,
                Combatants = combatants,
            };
        }

        /// <summary>
        /// Clears all recorded damage data and resets the timer.
        /// Called from the WebFrontend thread (POST /webapi/dps/reset).
        /// </summary>
        public void Reset()
        {
            _combatants.Clear();
            _resetTimestampTicks = DateTime.UtcNow.Ticks;
            _lastDamageTimestampTicks = _resetTimestampTicks;
        }

        /// <summary>
        /// Removes combatants that haven't dealt damage in the specified time span.
        /// Called periodically to prevent stale entries from accumulating.
        /// </summary>
        public void EvictIdleOlderThan(TimeSpan maxIdle)
        {
            long cutoffTicks = DateTime.UtcNow.Ticks - maxIdle.Ticks;
            foreach (var kvp in _combatants)
            {
                if (kvp.Value.LastHitTimestampTicks < cutoffTicks)
                    _combatants.TryRemove(kvp.Key, out _);
            }
        }
    }

    /// <summary>Internal per-combatant accumulator - not serialized directly.</summary>
    public class ModOverlayDpsCombatantEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string HeroName { get; set; } = string.Empty;
        public bool IsPhantom { get; set; }
        public long TotalDamage { get; set; }
        public long PeakHit { get; set; }
        public long HitCount { get; set; }
        public long FirstHitTimestampTicks { get; set; }
        public long LastHitTimestampTicks { get; set; }
    }

    /// <summary>JSON-serialized DPS response for /webapi/dps.</summary>
    public class ModOverlayDpsSnapshot
    {
        public bool Ok { get; set; } = true;
        public string Error { get; set; }
        public string Player { get; set; }
        public long SecondsSinceReset { get; set; }
        public List<ModOverlayDpsSnapshotCombatant> Combatants { get; set; } = new();
    }

    /// <summary>JSON-serialized per-combatant data in the DPS response.</summary>
    public class ModOverlayDpsSnapshotCombatant
    {
        public string Name { get; set; } = "";
        public string HeroName { get; set; } = "";
        public bool IsPhantom { get; set; }
        public long Total { get; set; }
        public long PeakHit { get; set; }
        public double Dps10 { get; set; }
        public double Dps60 { get; set; }
        public double DpsOverall { get; set; }
        public long SecondsSinceLastHit { get; set; }
    }
}
