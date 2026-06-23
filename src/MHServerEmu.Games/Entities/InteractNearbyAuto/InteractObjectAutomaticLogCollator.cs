using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Entities.InteractNearbyAuto
{
    /// <summary>
    /// Accumulates InteractNearbyAuto log lines per player session and saves them to
    /// a file when the session ends , logout, disconnect, or deallocate
    ///
    /// an in-memory buffer (StringBuilder per player) with a single
    /// disk write at session end, so it adds negligible overhead during play.
    ///
    /// Captures  AUTO-activation attempts (250ms tick) and MANUAL interactions
    /// ( player clicks) so they can be compared
    /// Config.ini options , default is off , this is for debugging
    /// </summary>
    public static class InteractObjectAutomaticLogCollator
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        // Player id -> session buffer. Player entity IDs are unique across the process.
        private static readonly Dictionary<ulong, Session> _sessions = new();

        private class Session
        {
            public readonly ulong PlayerId;
            public readonly string PlayerName;
            public readonly DateTime StartTime;
            public readonly System.Text.StringBuilder Buffer = new();
            public bool HasContent;

            public Session(ulong playerId, string playerName)
            {
                PlayerId = playerId;
                PlayerName = playerName;
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Opens a new per-session log for the given player.
        /// Called once when the player enters the game.
        /// </summary>
        public static void BeginSession(ulong playerId, string playerName)
        {
            if (playerId == 0) return;

            lock (_sessions)
            {
                if (_sessions.ContainsKey(playerId))
                    EndSession(playerId); 

                _sessions[playerId] = new Session(playerId, playerName);
            }
        }

        /// <summary>
        /// Returns true if the given player currently has an active collator session.
        /// </summary>
        public static bool IsTracked(ulong playerId)
        {
            if (playerId == 0) return false;
            lock (_sessions) return _sessions.ContainsKey(playerId);
        }

        /// <summary>
        /// Appends a line to the session buffer for the given player if tracked.
        /// </summary>
        public static void WriteLine(ulong playerId, string line)
        {
            if (playerId == 0 || string.IsNullOrEmpty(line)) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerId, out session) == false)
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
        /// Closes the session, writes the buffer to disk, and removes the player from tracking.
        /// Call when the player logs out, disconnects, or is deallocated.
        /// </summary>
        public static void EndSession(ulong playerId)
        {
            if (playerId == 0) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerId, out session) == false)
                    return;
                _sessions.Remove(playerId);
            }

            if (session.HasContent == false) return;

            try
            {
                string dir = Path.Combine("Logs", "InteractNearbyAuto");
                Directory.CreateDirectory(dir);

                string safeName = string.Join("_", session.PlayerName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"InteractNearbyAuto_{safeName}_{session.StartTime:yyyyMMdd_HHmmss}_{session.PlayerId}.log";
                string path = Path.Combine(dir, fileName);

                File.WriteAllText(path, session.Buffer.ToString());
                Logger.Info($"[InteractObjectAutomaticLogCollator] Wrote {session.Buffer.Length} chars to '{path}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[InteractObjectAutomaticLogCollator] Failed to flush session for {session.PlayerName}#{playerId}: {ex.Message}");
            }
        }
    }
}
