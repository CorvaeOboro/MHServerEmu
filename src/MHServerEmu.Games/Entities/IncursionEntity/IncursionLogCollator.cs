using MHServerEmu.Core.Helpers;
using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Accumulates incursion-related log lines per active invader and saves them to
    /// a dedicated file when the encounter ends (death / cull / timeout).
    ///
    /// an in-memory buffer (StringBuilder per entity) with a single
    /// disk write at session end, aiming for very small overhead during combat.
    /// </summary>
    public static class IncursionLogCollator
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>Master switch for the per-encounter file collator.</summary>
        public static bool Enabled { get; set; }

        // Entity id -> session buffer. Entity IDs are unique across the process.
        private static readonly Dictionary<ulong, Session> _sessions = new();

        private class Session
        {
            public readonly ulong EntityId;
            public readonly string AvatarName;
            public readonly DateTime StartTime;
            public readonly System.Text.StringBuilder Buffer = new();
            public bool HasContent;

            public Session(ulong entityId, string avatarName)
            {
                EntityId = entityId;
                AvatarName = avatarName;
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Opens a new per-encounter log session for the given incursion enemy.
        /// Call once immediately after the entity spawns and its controller is bound.
        /// </summary>
        public static void BeginSession(ulong entityId, string avatarName)
        {
            if (Enabled == false || entityId == 0) return;

            lock (_sessions)
            {
                if (_sessions.ContainsKey(entityId))
                    EndSession(entityId); // orphan flush

                _sessions[entityId] = new Session(entityId, avatarName);
            }
        }

        /// <summary>
        /// Appends a line to the session buffer for the given entity if it is tracked.
        /// </summary>
        public static void WriteLine(ulong entityId, string line)
        {
            if (Enabled == false || entityId == 0 || string.IsNullOrEmpty(line)) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(entityId, out session) == false)
                    return;
            }

            lock (session)
            {
                string timestamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
                foreach (var l in line.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    session.Buffer.AppendLine($"[{timestamp}] {l}");
                session.HasContent = true;
            }
        }

        /// <summary>
        /// Returns true if the given entity currently has an active collator session.
        /// </summary>
        public static bool IsTracked(ulong entityId)
        {
            if (entityId == 0) return false;
            lock (_sessions) return _sessions.ContainsKey(entityId);
        }

        /// <summary>
        /// Closes the session, writes the buffer to disk, and removes the entity from tracking.
        /// Call when the incursion enemy dies, is culled, or times out.
        /// </summary>
        public static void EndSession(ulong entityId)
        {
            if (entityId == 0) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(entityId, out session) == false)
                    return;
                _sessions.Remove(entityId);
            }

            if (session.HasContent == false) return;

            try
            {
                string dir = Path.Combine(FileHelper.ServerRoot, "Logs", "Incursions");
                Directory.CreateDirectory(dir);

                string safeName = string.Join("_", session.AvatarName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"Incursion_{safeName}_{session.StartTime:yyyyMMdd_HHmmss}_{session.EntityId}.log";
                string path = Path.Combine(dir, fileName);

                File.WriteAllText(path, session.Buffer.ToString());
                Logger.Info($"[IncursionLogCollator] Wrote {session.Buffer.Length} chars to '{path}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[IncursionLogCollator] Failed to flush session for {session.AvatarName}#{entityId}: {ex.Message}");
            }
        }
    }
}
