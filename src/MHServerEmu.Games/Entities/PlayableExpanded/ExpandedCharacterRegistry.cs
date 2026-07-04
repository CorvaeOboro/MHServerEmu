using System.Reflection;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;

namespace MHServerEmu.Games.Entities.PlayableExpanded
{
    /// <summary>
    /// Discovers and resolves <see cref="ExpandedCharacter"/> definitions.
    ///
    /// Dedicated per-character subclasses (e.g. ExpandedJubilee) are auto-discovered via
    /// reflection, mirroring how IncursionManager discovers its IncursionEnemy* types.
    /// Team-Up prototypes without a dedicated class resolve to a
    /// <see cref="GenericTeamUpExpandedCharacter"/> with default tuning.
    /// </summary>
    public static class ExpandedCharacterRegistry
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private static readonly List<ExpandedCharacter> _characters = new();
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract || type.IsSubclassOf(typeof(ExpandedCharacter)) == false)
                    continue;

                // Generic wrappers are created on demand, not registered.
                if (type == typeof(GenericTeamUpExpandedCharacter))
                    continue;

                ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    Logger.Warn($"[PlayableExpanded] Skipping character '{type.Name}': no public parameterless constructor.");
                    continue;
                }

                try
                {
                    var character = (ExpandedCharacter)ctor.Invoke(null);
                    if (character.BodyProtoRef == PrototypeId.Invalid)
                    {
                        Logger.Warn($"[PlayableExpanded] Skipping character '{type.Name}': invalid body prototype.");
                        continue;
                    }

                    _characters.Add(character);
                }
                catch (Exception e)
                {
                    Logger.Warn($"[PlayableExpanded] Failed to construct character '{type.Name}': {e.Message}");
                }
            }

            Logger.Info($"[PlayableExpanded] Registered {_characters.Count} dedicated character(s).");
        }

        /// <summary>All dedicated (hand-tuned) characters.</summary>
        public static IReadOnlyList<ExpandedCharacter> DedicatedCharacters
        {
            get { EnsureInitialized(); return _characters; }
        }

        /// <summary>Extracts the short name from a full prototype path (e.g. "SpiderWoman" from
        /// "Entity/Characters/TeamUps/SpiderWoman.prototype").</summary>
        private static string GetShortName(PrototypeId prototypeId)
        {
            string name = GameDatabase.GetPrototypeName(prototypeId);
            int slash = name.LastIndexOf('/');
            string shortName = slash >= 0 ? name[(slash + 1)..] : name;
            return shortName.Replace(".prototype", string.Empty);
        }

        /// <summary>
        /// Resolves a character from a name pattern: dedicated characters first (by display name),
        /// then any Team-Up prototype as a generic fallback. Returns null with a user-facing
        /// message on failure or ambiguity.
        /// </summary>
        public static (ExpandedCharacter, string) Resolve(string pattern)
        {
            EnsureInitialized();

            // 1. Dedicated characters by display name.
            List<ExpandedCharacter> dedicatedMatches = new();
            foreach (ExpandedCharacter character in _characters)
            {
                if (character.DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    dedicatedMatches.Add(character);
            }

            if (dedicatedMatches.Count == 1)
                return (dedicatedMatches[0], null);

            if (dedicatedMatches.Count > 1)
            {
                // Exact name wins.
                foreach (ExpandedCharacter character in dedicatedMatches)
                {
                    if (character.DisplayName.Equals(pattern, StringComparison.OrdinalIgnoreCase))
                        return (character, null);
                }

                var names = dedicatedMatches.Select(c => c.DisplayName);
                return (null, $"Found {dedicatedMatches.Count} matches for '{pattern}': {string.Join(", ", names)}");
            }

            // 2. Generic Team-Up fallback.
            List<PrototypeId> teamUpMatches = new();
            foreach (PrototypeId teamUpRef in GameDatabase.DataDirectory.IteratePrototypesInHierarchy<AgentTeamUpPrototype>(
                PrototypeIterateFlags.NoAbstractApprovedOnly))
            {
                if (GameDatabase.GetPrototypeName(teamUpRef).Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    teamUpMatches.Add(teamUpRef);
            }

            if (teamUpMatches.Count == 0)
                return (null, $"No expanded characters or Team-Up prototypes match '{pattern}'.");

            if (teamUpMatches.Count > 1)
            {
                // Exact short-name match wins (e.g. "spiderwoman" -> SpiderWoman, not SpiderWoman-ShieldVariant).
                foreach (PrototypeId teamUpRef in teamUpMatches)
                {
                    if (GetShortName(teamUpRef).Equals(pattern, StringComparison.OrdinalIgnoreCase))
                        return (new GenericTeamUpExpandedCharacter(teamUpRef), null);
                }

                const int MaxListed = 10;
                var names = teamUpMatches.Take(MaxListed).Select(r => GetShortName(r));
                string header = teamUpMatches.Count <= MaxListed
                    ? $"Found {teamUpMatches.Count} matches for '{pattern}':"
                    : $"Found {teamUpMatches.Count} matches for '{pattern}', first {MaxListed}:";
                return (null, header + "\n" + string.Join("\n", names));
            }

            return (new GenericTeamUpExpandedCharacter(teamUpMatches[0]), null);
        }
    }
}
