using System.IO;
using System.Text.Json;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;

namespace MHServerEmu.Games.Entities.PlayableExpanded
{
    /// <summary>
    /// Simple per-player DTO stored as a human-editable JSON file.
    /// </summary>
    public class PlayableExpandedData
    {
        /// <summary>
        /// Expanded character prototype reference (e.g. "Jubilee").
        /// Empty string means no expanded character is active.
        /// </summary>
        public string CharacterRef { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles loading and saving PlayableExpanded settings to disk,
    /// mirroring the pattern used by <see cref="PlayerLootFilterStorage"/>.
    /// </summary>
    public static class PlayableExpandedStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly string BaseDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "PlayableExpanded");
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        public static PlayableExpandedData Load(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            if (File.Exists(path) == false)
                return new PlayableExpandedData();

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<PlayableExpandedData>(json) ?? new PlayableExpandedData();
            }
            catch (Exception e)
            {
                Logger.Warn($"[PlayableExpanded] Failed to load persisted data for player {playerDbId}: {e.Message}");
                return new PlayableExpandedData();
            }
        }

        public static void Save(ulong playerDbId, PlayableExpandedData data)
        {
            try
            {
                Directory.CreateDirectory(BaseDir);
                string json = JsonSerializer.Serialize(data, WriteOptions);
                File.WriteAllText(GetPath(playerDbId), json);
            }
            catch (Exception e)
            {
                Logger.Warn($"[PlayableExpanded] Failed to save persisted data for player {playerDbId}: {e.Message}");
            }
        }

        public static void Clear(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Logger.Warn($"[PlayableExpanded] Failed to clear persisted data for player {playerDbId}: {e.Message}");
            }
        }

        private static string GetPath(ulong playerDbId) => Path.Combine(BaseDir, $"{playerDbId}.json");
    }
}
