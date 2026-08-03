// =============================================================================
//  MOD Incursion Hunt
// =============================================================================
//  Feature:  Tracks per-player completion of unique incursion enemy encounters.
//            Stores a per-player JSON file with global + per-character kill counts.
//            Influences random spawn selection to cycle through each enemy type
//            uniquely before repeating, so the player "hunts" every incursion type.
//  Storage:  Data/PlayerIncursionHunt/<dbId>.json
//  Commands: !incursion hunt          - show status
//            !incursion hunt reset     - reset all hunt data (global + current char)
//            !incursion hunt reset all - reset everything (all characters)
// =============================================================================

using System.IO;
using System.Text.Json;
using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Per-character hunt tracking: kill counts per enemy shorthand name.
    /// </summary>
    public class IncursionHuntSection
    {
        /// <summary>Enemy shorthand name -> kill count.</summary>
        public Dictionary<string, int> KillCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Per-player incursion hunt data. Contains a global section plus optional
    /// per-character overrides. At spawn time, the effective kill count for an enemy
    /// type is the SUM of global + the active character's count, used to prioritize
    /// least-encountered enemies.
    /// </summary>
    public class IncursionHuntData
    {
        /// <summary>Global kill counts applied to every character.</summary>
        public IncursionHuntSection Global { get; set; } = new();

        /// <summary>Per-character overrides, keyed by avatar short name (e.g. "Rogue").</summary>
        public Dictionary<string, IncursionHuntSection> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the section for the given avatar name, optionally creating it.
        /// </summary>
        public IncursionHuntSection GetCharacterSection(string avatarName, bool create = false)
        {
            if (string.IsNullOrEmpty(avatarName))
                return null;

            if (Characters.TryGetValue(avatarName, out IncursionHuntSection section))
                return section;

            if (create)
            {
                section = new IncursionHuntSection();
                Characters[avatarName] = section;
                return section;
            }

            return null;
        }

        /// <summary>
        /// Returns the effective kill count for an enemy shorthand: global + character.
        /// </summary>
        public int GetEffectiveKillCount(string enemyShorthand, string avatarName)
        {
            int count = 0;
            if (Global.KillCounts.TryGetValue(enemyShorthand, out int globalCount))
                count += globalCount;

            var charSection = GetCharacterSection(avatarName);
            if (charSection != null && charSection.KillCounts.TryGetValue(enemyShorthand, out int charCount))
                count += charCount;

            return count;
        }

        /// <summary>
        /// Records a kill for the given enemy shorthand in the specified scope.
        /// </summary>
        public void RecordKill(string enemyShorthand, string avatarName, bool perCharacter)
        {
            if (perCharacter)
            {
                var section = GetCharacterSection(avatarName, create: true);
                if (section != null)
                {
                    section.KillCounts.TryGetValue(enemyShorthand, out int current);
                    section.KillCounts[enemyShorthand] = current + 1;
                }
            }

            // Always increment global too so the global tally is complete.
            Global.KillCounts.TryGetValue(enemyShorthand, out int globalCurrent);
            Global.KillCounts[enemyShorthand] = globalCurrent + 1;
        }

        /// <summary>
        /// Returns the total unique enemy types encountered (kill count > 0) across
        /// global + the given character.
        /// </summary>
        public int GetUniqueCount(string avatarName, HashSet<string> allEnemyShorthands)
        {
            int unique = 0;
            foreach (string shorthand in allEnemyShorthands)
            {
                if (GetEffectiveKillCount(shorthand, avatarName) > 0)
                    unique++;
            }
            return unique;
        }
    }

    /// <summary>
    /// Handles loading and saving <see cref="IncursionHuntData"/> to disk.
    /// </summary>
    public static class IncursionHuntStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly string BaseDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "PlayerIncursionHunt");
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        public static IncursionHuntData Load(ulong playerDbId)
        {
            string path = GetPath(playerDbId);
            if (File.Exists(path) == false)
                return new IncursionHuntData();

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return new IncursionHuntData();

                return JsonSerializer.Deserialize<IncursionHuntData>(json) ?? new IncursionHuntData();
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to load incursion hunt data for player {playerDbId}: {e.Message}");
                return new IncursionHuntData();
            }
        }

        public static void Save(ulong playerDbId, IncursionHuntData data)
        {
            try
            {
                Directory.CreateDirectory(BaseDir);
                string json = JsonSerializer.Serialize(data, WriteOptions);
                File.WriteAllText(GetPath(playerDbId), json);
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to save incursion hunt data for player {playerDbId}: {e.Message}");
            }
        }

        private static string GetPath(ulong playerDbId) => Path.Combine(BaseDir, $"{playerDbId}.json");
    }
}
