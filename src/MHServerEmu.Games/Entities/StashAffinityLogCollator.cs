using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Entities
{
    /// <summary>
    /// Per-player log collator for the StashAffinity mod.
    /// Records the decision tree for every redirected stash move for debugging
    /// </summary>
    public static class StashAffinityLogCollator
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
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

        public static bool Enabled { get; set; }

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

        public static void WriteLine(ulong playerId, string line)
        {
            if (Enabled == false || playerId == 0 || string.IsNullOrEmpty(line)) return;

            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerId, out session) == false) return;
            }

            lock (session)
            {
                string ts = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
                foreach (string l in line.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    session.Buffer.AppendLine($"[{ts}] {l}");
                session.HasContent = true;
            }
        }

        public static void EndSession(ulong playerId)
        {
            if (playerId == 0) return;
            Session session;
            lock (_sessions)
            {
                if (_sessions.TryGetValue(playerId, out session) == false) return;
                _sessions.Remove(playerId);
            }

            if (session.HasContent == false) return;
            try
            {
                string dir = Path.Combine("Logs", "StashAffinity");
                Directory.CreateDirectory(dir);
                string safeName = string.Join("_", session.PlayerName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"StashAffinity_{safeName}_{session.StartTime:yyyyMMdd_HHmmss}_{session.PlayerId}.log";
                string path = Path.Combine(dir, fileName);
                File.WriteAllText(path, session.Buffer.ToString());
                Logger.Info($"[StashAffinityCollator] Wrote {session.Buffer.Length} chars to '{path}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[StashAffinityCollator] Flush failed for {session.PlayerName}#{playerId}: {ex.Message}");
            }
        }
    }
}
