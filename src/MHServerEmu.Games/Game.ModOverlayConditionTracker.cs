#region ModOverlayConditionTracker
// =============================================================================
// MOD OVERLAY CONDITION TRACKER - Server-side condition snapshot for MhServerOverlay
// =============================================================================
//   Periodically snapshots all players' avatar conditions (buffs, debuffs, proc
//   triggers) into ModOverlayConditionDataStore (a thread-safe singleton in
//   MHServerEmu.Core) so the WebFrontend's /webapi/conditions endpoint can serve
//   it to the overlay via HTTP polling.
//
//   Unlike the DPS tracker (event-driven - push on every damage event), conditions
//   are stateful and expire on timers. A periodic snapshot model is simpler and
//   matches the overlay's 1-second poll rate.
//
//  Config.ini:
//   ModOverlayConditionTrackerEnable (default: true) - set to false to disable
//
//  Integration:
//   - Game.Update() calls ModOverlaySnapshotConditions() once per tick
//   - Throttled to 1 snapshot per second (matches overlay poll rate)
//   - Iterates all players -> CurrentAvatar -> ConditionCollection -> conditions
//
//  VERSION:: 20260805
// =============================================================================

using MHServerEmu.Core.Network.Web;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Locales;
using MHServerEmu.Games.Powers.Conditions;

namespace MHServerEmu.Games
{
    public partial class Game
    {
        /// <summary>Throttle interval for condition snapshots - matches the overlay's 1s poll rate.</summary>
        private static readonly TimeSpan ModOverlayConditionSnapshotInterval = TimeSpan.FromSeconds(1);

        /// <summary>Last time we snapshotted conditions (game time). Throttle so we don't
        /// iterate all players' condition collections on every game tick (which runs at
        /// ~10 Hz for a 100ms FixedTimeBetweenUpdates).</summary>
        private TimeSpan _modOverlayLastConditionSnapshotTime = TimeSpan.Zero;

        /// <summary>
        /// Called from <see cref="Update"/> to snapshot all players' avatar conditions
        /// into <see cref="ModOverlayConditionDataStore"/>. Throttled to 1 snapshot per
        /// second. Does nothing if ModOverlayConditionTrackerEnable is false.
        /// </summary>
        /// <remarks>
        /// The call site in <see cref="Update"/> checks
        /// <see cref="CustomGameOptionsConfig.ModOverlayEnable"/> before invoking
        /// this method. The <see cref="ModOverlayConditionTrackerEnable"/> check here is a
        /// defensive backup for direct calls.
        /// </remarks>
        private void ModOverlaySnapshotConditions()
        {
            // Defensive backup: the call site already checks ModOverlayEnable.
            // Also check the per-feature sub-switch here.
            var options = CustomGameOptions;
            if (options == null || options.ModOverlayConditionTrackerEnable == false)
                return;

            // Throttle: only snapshot once per second
            if (CurrentTime - _modOverlayLastConditionSnapshotTime < ModOverlayConditionSnapshotInterval)
                return;
            _modOverlayLastConditionSnapshotTime = CurrentTime;

            // Iterate all players in this game
            foreach (Player player in new PlayerIterator(this))
            {
                try
                {
                    ModOverlaySnapshotPlayerConditions(player);
                }
                catch (Exception ex)
                {
                    // Don't let one player's error kill the snapshot loop
                    Logger.Warn($"ModOverlaySnapshotConditions: Failed for player [{player}]: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Snapshots a single player's avatar conditions into the data store.
        /// </summary>
        private void ModOverlaySnapshotPlayerConditions(Player player)
        {
            var avatar = player.CurrentAvatar;
            if (avatar == null)
                return;

            var collection = avatar.ConditionCollection;
            if (collection == null)
                return;

            string playerName = player.GetName();
            if (string.IsNullOrEmpty(playerName))
                return;

            var conditions = new List<ModOverlayConditionInfo>();
            var locale = LocaleManager.Instance?.CurrentLocale;

            foreach (Condition condition in collection.IterateConditions(false))
            {
                try
                {
                    var info = ModOverlayMapCondition(condition, locale);
                    conditions.Add(info);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"ModOverlaySnapshotPlayerConditions: Failed for condition [{condition}]: {ex.Message}");
                }
            }

            ModOverlayConditionDataStore.Instance.UpdatePlayerConditions(playerName, conditions);
        }

        /// <summary>
        /// Maps a server-side <see cref="Condition"/> to a <see cref="ModOverlayConditionInfo"/>
        /// DTO that can be serialized to JSON for the overlay.
        /// </summary>
        private static ModOverlayConditionInfo ModOverlayMapCondition(Condition condition, Locale locale)
        {
            var proto = condition.ConditionPrototype;

            // Resolve display name: prefer the prototype's DisplayName locale string,
            // fall back to the condition's ToString() (which returns the full prototype ref name).
            // When falling back to the prototype name, shorten it to the last leaf - e.g.
            // "MarvelHeroes.GameData.Prototypes.CombatProwessConditionPrototype" -> "CombatProwess".
            // This keeps the overlay compact and readable.
            string name = string.Empty;
            if (proto != null && proto.DisplayName != LocaleStringId.Invalid && locale != null)
            {
                name = locale.GetLocaleString(proto.DisplayName);
            }
            if (string.IsNullOrEmpty(name))
                name = ShortenToLastLeaf(condition.ToString() ?? "Unknown");

            // Creator power name - also shortened to last leaf for the same reason
            string creatorPower = string.Empty;
            var creatorPowerProto = condition.CreatorPowerPrototype;
            if (creatorPowerProto != null)
                creatorPower = ShortenToLastLeaf(GameDatabase.GetPrototypeName(condition.CreatorPowerPrototypeRef));

            // Icon path (asset name)
            string iconPath = null;
            if (proto != null)
            {
                AssetId iconAsset = proto.IconPath;
                if (iconAsset != AssetId.Invalid)
                    iconPath = GameDatabase.GetAssetName(iconAsset);
            }

            // Duration / time remaining
            TimeSpan duration = condition.Duration;
            TimeSpan timeRemaining = condition.IsFinite ? condition.TimeRemaining : TimeSpan.Zero;
            TimeSpan elapsed = condition.ElapsedTime;

            // Stack count
            int stacks = 0;
            if (condition.IsInCollection && condition.Collection != null)
                stacks = condition.Collection.GetNumberOfStacks(condition);

            return new ModOverlayConditionInfo
            {
                Name = name,
                CreatorPower = creatorPower,
                ConditionType = proto?.ConditionType.ToString() ?? "Neither",
                UiType = proto?.ConditionTypeUI.ToString() ?? "None",
                DurationMs = (long)duration.TotalMilliseconds,
                TimeRemainingMs = Math.Max(0, (long)timeRemaining.TotalMilliseconds),
                ElapsedMs = Math.Max(0, (long)elapsed.TotalMilliseconds),
                Stacks = stacks,
                IsEnabled = condition.IsEnabled,
                IsPaused = condition.IsPaused,
                CancelOnHit = proto?.CancelOnHit ?? false,
                CancelOnPowerUse = proto?.CancelOnPowerUse ?? false,
                CancelOnKilled = proto?.CancelOnKilled ?? false,
                IconPath = iconPath,
            };
        }

        /// <summary>
        /// Shortens a full prototype path name to its last leaf segment, stripping
        /// common suffixes. Examples:
        /// <list type="bullet">
        ///   <item><c>MarvelHeroes.GameData.Prototypes.CombatProwessConditionPrototype</c> -> <c>CombatProwess</c></item>
        ///   <item><c>SomePowerPrototype</c> -> <c>SomePower</c></item>
        ///   <item><c>AlreadyShort</c> -> <c>AlreadyShort</c></item>
        /// </list>
        /// </summary>
        private static string ShortenToLastLeaf(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return fullName;

            // Take the segment after the last dot
            int lastDot = fullName.LastIndexOf('.');
            string leaf = lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;

            // Strip common suffixes: "Prototype", then "Condition" / "Power" if the result
            // is still long enough to be meaningful (avoids stripping down to empty string)
            leaf = StripSuffix(leaf, "Prototype");
            leaf = StripSuffix(leaf, "Condition");
            // Don't strip "Power" - too aggressive, many power names end in "Power" meaningfully

            return string.IsNullOrEmpty(leaf) ? fullName : leaf;
        }

        /// <summary>Strips a case-sensitive suffix from the end of a string if present
        /// and the remaining text is non-empty.</summary>
        private static string StripSuffix(string s, string suffix)
        {
            if (s.Length > suffix.Length && s.EndsWith(suffix, StringComparison.Ordinal))
                return s[..^suffix.Length];
            return s;
        }
    }
}

#endregion
