using System.Text.Json;
using MHServerEmu.Core.Helpers;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Logging
{
    /// <summary>
    /// Stores area-design notes (labelled map pins) placed by devs via the !note command.
    /// Each note captures a world position, category, and optional comment, and can spawn
    /// an invisible nameplate proxy in-world so the label is visible while editing.
    /// Notes can be flushed to JSON for use by other tools (enemy placement, event design).
    /// </summary>
    public static class AreaNoteCollator
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        // Combat body prototype for the invisible nameplate proxy (same as IncursionManager).
        private const string CombatBodyProtoName = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";
        private static PrototypeId s_combatBodyRef = PrototypeId.Invalid;

        // Avatar prototype used for the render-as-avatar nameplate proxy.
        private static readonly PrototypeId AvatarRenderRef = (PrototypeId)12394659164528645362; // SheHulk

        // All notes across all regions.
        private static readonly List<AreaNote> _notes = new();
        private static readonly object _lock = new();

        // Per-region category counters for auto-incrementing labels (NOTE_BOSS_01, NOTE_MOB_02, ...).
        private static readonly Dictionary<ulong, Dictionary<string, int>> _counters = new();

        /// <summary>
        /// A single area-design note placed at a world position.
        /// </summary>
        public class AreaNote
        {
            public string Label { get; set; }
            public string Category { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public string RegionName { get; set; }
            public ulong RegionId { get; set; }
            public DateTime Timestamp { get; set; }
            public ulong MarkerEntityId { get; set; }
            public string Comment { get; set; }

            public Vector3 Position => new(X, Y, Z);
        }

        /// <summary>
        /// Places a new note at the given avatar's current position and spawns an invisible
        /// nameplate marker in-world. Returns the generated label.
        /// </summary>
        public static string PlaceNote(Avatar avatar, string category, string comment = null)
        {
            if (avatar == null || avatar.IsAliveInWorld == false)
                return null;

            Region region = avatar.Region;
            if (region == null)
                return null;

            string cat = SanitizeCategory(category);
            ulong regionId = region.Id;

            string label;
            lock (_lock)
            {
                if (_counters.TryGetValue(regionId, out var catCounters) == false)
                {
                    catCounters = new();
                    _counters[regionId] = catCounters;
                }

                catCounters.TryGetValue(cat, out int count);
                count++;
                catCounters[cat] = count;

                label = $"NOTE_{cat.ToUpperInvariant()}_{count:D2}";
            }

            Vector3 pos = avatar.RegionLocation.Position;

            ulong markerId = SpawnNoteMarker(avatar.Game, region, pos, label);

            var note = new AreaNote
            {
                Label = label,
                Category = cat,
                X = pos.X,
                Y = pos.Y,
                Z = pos.Z,
                RegionName = region.PrototypeName,
                RegionId = regionId,
                Timestamp = DateTime.Now,
                MarkerEntityId = markerId,
                Comment = comment,
            };

            lock (_lock)
            {
                _notes.Add(note);
            }

            Logger.Info($"[AreaNote] Placed '{label}' at {pos.ToStringNames()} in '{region.PrototypeName}' (marker={markerId}).");
            return label;
        }

        /// <summary>
        /// Removes a note by label. Despawns the marker entity if it still exists.
        /// </summary>
        public static bool RemoveNote(Game game, string label)
        {
            AreaNote note;
            lock (_lock)
            {
                note = _notes.FirstOrDefault(n => string.Equals(n.Label, label, StringComparison.OrdinalIgnoreCase));
                if (note == null)
                    return false;
                _notes.Remove(note);
            }

            DespawnMarker(game, note.MarkerEntityId);
            Logger.Info($"[AreaNote] Removed '{note.Label}'.");
            return true;
        }

        /// <summary>
        /// Clears all notes for the given region and despawns their markers.
        /// Returns the number of notes cleared.
        /// </summary>
        public static int ClearNotesForRegion(Game game, ulong regionId)
        {
            List<AreaNote> toRemove;
            lock (_lock)
            {
                toRemove = _notes.Where(n => n.RegionId == regionId).ToList();
                foreach (var note in toRemove)
                    _notes.Remove(note);

                if (_counters.TryGetValue(regionId, out var catCounters))
                    catCounters.Clear();
            }

            foreach (var note in toRemove)
                DespawnMarker(game, note.MarkerEntityId);

            Logger.Info($"[AreaNote] Cleared {toRemove.Count} note(s) for region {regionId}.");
            return toRemove.Count;
        }

        /// <summary>
        /// Clears all notes across all regions and despawns their markers.
        /// </summary>
        public static int ClearAll(Game game)
        {
            List<AreaNote> all;
            lock (_lock)
            {
                all = _notes.ToList();
                _notes.Clear();
                _counters.Clear();
            }

            foreach (var note in all)
                DespawnMarker(game, note.MarkerEntityId);

            Logger.Info($"[AreaNote] Cleared all {all.Count} note(s).");
            return all.Count;
        }

        /// <summary>
        /// Returns a list of all notes for the given region.
        /// </summary>
        public static List<AreaNote> GetNotesForRegion(ulong regionId)
        {
            lock (_lock)
                return _notes.Where(n => n.RegionId == regionId).ToList();
        }

        /// <summary>
        /// Returns a summary string of all notes grouped by region and category.
        /// </summary>
        public static string GetSummary()
        {
            lock (_lock)
            {
                if (_notes.Count == 0)
                    return "No area notes placed.";

                var byRegion = _notes.GroupBy(n => n.RegionName);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Area notes: {_notes.Count} total across {byRegion.Count()} region(s).");

                foreach (var regionGroup in byRegion)
                {
                    sb.AppendLine($"  {regionGroup.Key}:");
                    foreach (var catGroup in regionGroup.GroupBy(n => n.Category))
                        sb.AppendLine($"    {catGroup.Key}: {catGroup.Count()} note(s)");
                }

                return sb.ToString().TrimEnd();
            }
        }

        /// <summary>
        /// Flushes all notes to a JSON file in Logs/AreaNotes/. Returns the file path.
        /// </summary>
        public static string FlushToJson()
        {
            List<AreaNote> snapshot;
            lock (_lock)
                snapshot = _notes.ToList();

            if (snapshot.Count == 0)
                return "No notes to save.";

            try
            {
                string dir = Path.Combine(FileHelper.ServerRoot, "Logs", "AreaNotes");
                Directory.CreateDirectory(dir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"AreaNotes_{timestamp}.json";
                string path = Path.Combine(dir, fileName);

                var byRegion = snapshot.GroupBy(n => n.RegionName);
                var regions = new List<object>();

                foreach (var regionGroup in byRegion)
                {
                    var notes = regionGroup.Select(n => new
                    {
                        label = n.Label,
                        category = n.Category,
                        position = new { x = n.X, y = n.Y, z = n.Z },
                        comment = n.Comment ?? "",
                        timestamp = n.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                    }).ToList();

                    regions.Add(new
                    {
                        region = regionGroup.Key,
                        regionId = regionGroup.First().RegionId,
                        notes = notes,
                    });
                }

                var export = new
                {
                    exportedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    totalNotes = snapshot.Count,
                    regions = regions,
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(export, options);
                File.WriteAllText(path, json);

                Logger.Info($"[AreaNote] Flushed {snapshot.Count} note(s) to '{path}'.");
                return path;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[AreaNote] Failed to flush notes: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        // ------------------------------------------------------------------
        // Marker spawning (reuses the IncursionManager nameplate-proxy pattern)
        // ------------------------------------------------------------------

        /// <summary>
        /// Spawns an invisible avatar-rendered proxy entity at the given position that
        /// displays the note label as an overhead nameplate. No 3D model is drawn.
        /// Returns the entity ID, or 0 on failure.
        /// </summary>
        private static ulong SpawnNoteMarker(Game game, Region region, Vector3 position, string label)
        {
            if (game == null || region == null)
                return 0;

            PrototypeId combatBodyRef = ResolveCombatBodyRef();
            if (combatBodyRef == PrototypeId.Invalid)
            {
                Logger.Warn($"[AreaNote] Could not resolve combat body prototype '{CombatBodyProtoName}'.");
                return 0;
            }

            var manager = region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(position, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = combatBodyRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Render as avatar so the client shows a prestige nameplate.
            spec.ClientRenderPrototypeRef = AvatarRenderRef;
            spec.ClientRenderPlayerName = label;

            // No costume => no visible mesh, just the nameplate pawn.
            // Prestige level 5 = red nameplate for visibility.
            spec.Properties[PropertyEnum.AvatarPrestigeLevel] = 5;

            // Non-hostile, untargetable, hidden model.
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;

            spec.Spawn();

            var proxy = spec.ActiveEntity;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                Logger.Warn($"[AreaNote] Failed to spawn marker proxy for '{label}'.");
                return 0;
            }

            // Zero out level so the nameplate doesn't show a level number.
            proxy.Properties[PropertyEnum.CharacterLevel] = 0;
            proxy.Properties[PropertyEnum.CombatLevel] = 0;

            // Strip powers so the proxy doesn't play any power animations.
            if (proxy is Agent proxyAgent && proxyAgent.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxyAgent.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                    proxyAgent.UnassignPower(powerRef);
            }

            // Disable AI, set dormant, stop simulation.
            if (proxy is Agent aiAgent)
            {
                aiAgent.AIController?.SetIsEnabled(false);
                aiAgent.SetDormant(true);
            }

            proxy.SetSimulated(false);

            return proxy.Id;
        }

        /// <summary>
        /// Despawns a marker entity by ID if it still exists in the world.
        /// </summary>
        private static void DespawnMarker(Game game, ulong entityId)
        {
            if (game == null || entityId == 0)
                return;

            var entity = game.EntityManager.GetEntity<WorldEntity>(entityId);
            if (entity != null && entity.IsInWorld)
                entity.ScheduleDestroyEvent(TimeSpan.Zero);
        }

        private static PrototypeId ResolveCombatBodyRef()
        {
            if (s_combatBodyRef != PrototypeId.Invalid)
                return s_combatBodyRef;

            s_combatBodyRef = GameDatabase.GetPrototypeRefByName(CombatBodyProtoName);
            if (s_combatBodyRef == PrototypeId.Invalid)
                Logger.Warn($"[AreaNote] Combat body prototype '{CombatBodyProtoName}' not found.");

            return s_combatBodyRef;
        }

        private static string SanitizeCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "misc";

            string sanitized = category.Trim().Replace(' ', '_');
            return sanitized;
        }
    }
}
