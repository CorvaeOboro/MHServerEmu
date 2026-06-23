using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Missions
{
    /// <summary>
    /// Per-player log collator for the TerminalCubeShard mod.
    ///
    /// Captures all decisions, state transitions, and property changes related to
    /// daily terminal mission auto-completion so they can be reviewed in isolation
    /// instead of being buried in the global server log.
    ///
    /// Session key: <see cref="Player.DbGuid"/> (persists across logins / region transfers).
    /// </summary>
    public static class TerminalCubeShardLogCollator
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        // player DbGuid -> session buffer
        private static readonly Dictionary<ulong, Session> _sessions = new();

        private class Session
        {
            public readonly ulong PlayerDbGuid;
            public readonly string PlayerName;
            public readonly DateTime StartTime;
            public readonly System.Text.StringBuilder Buffer = new();
            public bool HasContent;

            public Session(ulong dbGuid, string playerName)
            {
                PlayerDbGuid = dbGuid;
                PlayerName = playerName;
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Opens a new collator session for the given player.
        /// Call once when the player logs in or when the first TerminalCubeShard event fires.
        /// </summary>
        public static void BeginSession(ulong playerDbGuid, string playerName)
        {
            if (playerDbGuid == 0) return;

            lock (_sessions)
            {
                if (_sessions.ContainsKey(playerDbGuid))
                    EndSession(playerDbGuid); // orphan flush

                _sessions[playerDbGuid] = new Session(playerDbGuid, playerName);
            }
        }

        /// <summary>
        /// Appends a line to the session buffer for the given player if tracked.
        /// Safe to call from any log site; no-op if the player is not tracked.
        /// </summary>
        public static void WriteLine(ulong playerDbGuid, string line)
        {
            if (playerDbGuid == 0 || string.IsNullOrEmpty(line)) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerDbGuid, out session) == false)
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
        /// Returns true if the given player currently has an active collator session.
        /// </summary>
        public static bool IsTracked(ulong playerDbGuid)
        {
            if (playerDbGuid == 0) return false;
            lock (_sessions) return _sessions.ContainsKey(playerDbGuid);
        }

        /// <summary>
        /// Closes the session, writes the buffer to disk, and removes the player from tracking.
        /// Call on logout, disconnect, or when explicitly ending a diagnostic run.
        /// </summary>
        public static void EndSession(ulong playerDbGuid)
        {
            if (playerDbGuid == 0) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerDbGuid, out session) == false)
                    return;
                _sessions.Remove(playerDbGuid);
            }

            if (session.HasContent == false) return;

            try
            {
                string dir = Path.Combine("Logs", "TerminalCubeShard");
                Directory.CreateDirectory(dir);

                string safeName = string.Join("_", session.PlayerName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"TerminalCubeShard_{safeName}_{session.StartTime:yyyyMMdd_HHmmss}_{session.PlayerDbGuid}.log";
                string path = Path.Combine(dir, fileName);

                File.WriteAllText(path, session.Buffer.ToString());
                Logger.Info($"[TerminalCubeShardLogCollator] Wrote {session.Buffer.Length} chars to '{path}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[TerminalCubeShardLogCollator] Failed to flush session for {session.PlayerName}#{playerDbGuid}: {ex.Message}");
            }
        }
    }
}
