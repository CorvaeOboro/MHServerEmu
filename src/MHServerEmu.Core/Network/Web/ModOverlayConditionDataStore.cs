using System.Collections.Concurrent;

namespace MHServerEmu.Core.Network.Web
{
    /// <summary>
    /// Thread-safe singleton that holds per-player condition (buff/debuff) snapshots
    /// from the game service and exposes them for the WebAPI /webapi/conditions endpoint.
    ///
    /// <para>
    /// The game service thread calls <see cref="UpdatePlayerConditions"/> periodically
    /// (once per game tick, throttled to ~1 second) to push a fresh snapshot of each
    /// player's avatar conditions. The WebFrontend thread calls <see cref="GetSnapshot"/>
    /// when the overlay polls the endpoint. All access is via
    /// <see cref="ConcurrentDictionary{TKey, TValue}"/> so no explicit locking is needed.
    /// </para>
    ///
    /// <para>
    /// Unlike <see cref="ModOverlayDpsDataStore"/> (which is event-driven - push on every
    /// damage event), conditions are stateful and expire on timers. A periodic snapshot
    /// model is simpler and matches the overlay's 1-second poll rate - there's no benefit
    /// to sub-second condition updates because the overlay can't display them faster than
    /// it polls.
    /// </para>
    ///
    /// <para>
    /// Lives in <see cref="MHServerEmu.Core"/> for the same reason as
    /// <see cref="ModOverlayDpsDataStore"/> - both Games (writer) and WebFrontend (reader)
    /// reference Core, but WebFrontend does NOT reference Games.
    /// </para>
    /// </summary>
    public sealed class ModOverlayConditionDataStore
    {
        public static ModOverlayConditionDataStore Instance { get; } = new();

        private readonly ConcurrentDictionary<string, ModOverlayConditionEntry> _playerConditions = new();
        private long _lastSnapshotTimestampTicks;

        // Periodic eviction - removes players who haven't had a condition update in a while.
        // Same rationale as DpsDataStore: prevents unbounded growth on long-running servers.
        private static readonly TimeSpan EvictionIdleTimeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan EvictionCheckInterval = TimeSpan.FromMinutes(1);
        private readonly Timer _evictionTimer;

        private ModOverlayConditionDataStore()
        {
            _lastSnapshotTimestampTicks = DateTime.UtcNow.Ticks;
            _evictionTimer = new Timer(_ => EvictIdleOlderThan(EvictionIdleTimeout),
                                        null, EvictionCheckInterval, EvictionCheckInterval);
        }

        /// <summary>
        /// Replaces the condition list for a single player. Called from the game service
        /// thread by the periodic snapshot hook in
        /// <c>WorldEntity.ModOverlayConditionTracker</c>.
        /// </summary>
        /// <param name="playerKey">Player name (unique key).</param>
        /// <param name="conditions">Full list of active conditions on the player's avatar.
        /// Replaces any previous list for this player. Pass an empty list to clear (e.g.
        /// when the player has no active conditions).</param>
        public void UpdatePlayerConditions(string playerKey, List<ModOverlayConditionInfo> conditions)
        {
            if (string.IsNullOrEmpty(playerKey))
                return;

            _lastSnapshotTimestampTicks = DateTime.UtcNow.Ticks;

            _playerConditions.AddOrUpdate(
                playerKey,
                _ => new ModOverlayConditionEntry
                {
                    PlayerName = playerKey,
                    Conditions = conditions ?? new(),
                    LastUpdateTimestampTicks = DateTime.UtcNow.Ticks,
                },
                (_, existing) =>
                {
                    existing.Conditions = conditions ?? new();
                    existing.LastUpdateTimestampTicks = DateTime.UtcNow.Ticks;
                    return existing;
                });
        }

        /// <summary>
        /// Returns a point-in-time snapshot of conditions for the specified player
        /// (or all players if <paramref name="playerFilter"/> is null or "*").
        /// Called from the WebFrontend thread.
        /// </summary>
        public ModOverlayConditionSnapshot GetSnapshot(string playerFilter = null)
        {
            var players = new List<ModOverlayConditionPlayerData>();

            foreach (var kvp in _playerConditions)
            {
                var entry = kvp.Value;

                // Apply player filter if specified (and not "*")
                if (!string.IsNullOrEmpty(playerFilter) && playerFilter != "*")
                {
                    if (!entry.PlayerName.Equals(playerFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                players.Add(new ModOverlayConditionPlayerData
                {
                    PlayerName = entry.PlayerName,
                    Conditions = entry.Conditions,
                    SecondsSinceUpdate = (DateTime.UtcNow.Ticks - entry.LastUpdateTimestampTicks) / TimeSpan.TicksPerSecond,
                });
            }

            return new ModOverlayConditionSnapshot
            {
                Ok = true,
                Player = playerFilter ?? "*",
                ServerTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Players = players,
            };
        }

        /// <summary>Clears all condition data.</summary>
        public void Clear()
        {
            _playerConditions.Clear();
            _lastSnapshotTimestampTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Removes players that haven't had a condition update in the specified time span.
        /// Called periodically to prevent stale entries from accumulating (e.g. after a
        /// player disconnects, their last snapshot would otherwise persist forever).
        /// </summary>
        public void EvictIdleOlderThan(TimeSpan maxIdle)
        {
            long cutoffTicks = DateTime.UtcNow.Ticks - maxIdle.Ticks;
            foreach (var kvp in _playerConditions)
            {
                if (kvp.Value.LastUpdateTimestampTicks < cutoffTicks)
                    _playerConditions.TryRemove(kvp.Key, out _);
            }
        }
    }

    /// <summary>Internal per-player condition storage - not serialized directly.</summary>
    public class ModOverlayConditionEntry
    {
        public string PlayerName { get; set; } = string.Empty;
        public List<ModOverlayConditionInfo> Conditions { get; set; } = new();
        public long LastUpdateTimestampTicks { get; set; }
    }

    /// <summary>JSON-serialized condition snapshot response for /webapi/conditions.</summary>
    public class ModOverlayConditionSnapshot
    {
        public bool Ok { get; set; } = true;
        public string Error { get; set; }
        public string Player { get; set; }
        public long ServerTimeMs { get; set; }
        public List<ModOverlayConditionPlayerData> Players { get; set; } = new();
    }

    /// <summary>JSON-serialized per-player data in the condition snapshot.</summary>
    public class ModOverlayConditionPlayerData
    {
        public string PlayerName { get; set; } = "";
        public List<ModOverlayConditionInfo> Conditions { get; set; } = new();
        public long SecondsSinceUpdate { get; set; }
    }

    /// <summary>
    /// JSON-serialized info for a single active condition (buff/debuff/proc trigger).
    /// Matches the data the overlay needs to render the condition panel.
    /// </summary>
    public class ModOverlayConditionInfo
    {
        /// <summary>Condition display name (from ConditionPrototype.DisplayName or ToString()).</summary>
        public string Name { get; set; } = "";

        /// <summary>Name of the power that created/applied this condition.</summary>
        public string CreatorPower { get; set; } = "";

        /// <summary>Condition type: "Buff", "Debuff", "Boost", or "Neither".</summary>
        public string ConditionType { get; set; } = "Neither";

        /// <summary>UI condition type: "Buff", "Debuff", "Boost", "Raid", etc.</summary>
        public string UiType { get; set; } = "None";

        /// <summary>Total duration in milliseconds (0 = permanent / until removed).</summary>
        public long DurationMs { get; set; }

        /// <summary>Remaining time in milliseconds (0 = expired or permanent).</summary>
        public long TimeRemainingMs { get; set; }

        /// <summary>Elapsed time in milliseconds since the condition was applied.</summary>
        public long ElapsedMs { get; set; }

        /// <summary>Stack count (0 = non-stacking condition, 1+ = stacked).</summary>
        public int Stacks { get; set; }

        /// <summary>Whether the condition is currently enabled (active).</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Whether the condition's duration countdown is paused.</summary>
        public bool IsPaused { get; set; }

        /// <summary>True if this condition cancels on hit - identifies "on hit" proc triggers.</summary>
        public bool CancelOnHit { get; set; }

        /// <summary>True if this condition cancels on power use - identifies "on power use" triggers.</summary>
        public bool CancelOnPowerUse { get; set; }

        /// <summary>True if this condition cancels on killed.</summary>
        public bool CancelOnKilled { get; set; }

        /// <summary>Icon asset path (for future UI icon rendering). May be null.</summary>
        public string IconPath { get; set; }
    }
}
