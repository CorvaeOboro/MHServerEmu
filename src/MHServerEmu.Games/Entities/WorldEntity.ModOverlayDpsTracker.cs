#region ModOverlayDpsTracker
// =============================================================================
// MOD DPS Tracker - Server-side damage aggregation for the MhServerOverlay
// =============================================================================
//   Records outgoing damage per combatant into DpsDataStore (a thread-safe
//   singleton in MHServerEmu.Core) so the WebFrontend's /webapi/dps endpoint
//   can serve it to the overlay via HTTP polling.
//
//   This replaces the closed-source overlay's DLL-injection + packet-sniffing
//   approach with a clean server-side data path: since we run our own server,
//   we can broadcast exactly the data the overlay needs without touching the
//   game client process at all.
//
//  Config.ini :
//   ModOverlayDpsTrackerEnable (default: true)
//
//  Integration:
//   - WorldEntity.ApplyHealthPowerResults calls RecordDpsEvent() after
//     calculating the final health delta. Only hostile damage (healthDelta < 0)
//     is recorded; healing is skipped.
//   - The damage is attributed to the "ultimate owner" - the player who
//     ultimately owns the power (resolving through pets/summons).
//
//  VERSION:: 20260804
// =============================================================================

using MHServerEmu.Core.Network.Web;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities
{
    public partial class WorldEntity
    {
        /// <summary>
        /// Called from <see cref="ApplyHealthPowerResults"/> to record outgoing
        /// damage for the DPS tracker. Only records hostile damage (negative
        /// health delta) attributed to a player's avatar via the ultimate owner
        /// chain. Damage to self or same-alliance is skipped.
        /// </summary>
        /// <remarks>
        /// The call site in <see cref="ApplyHealthPowerResults"/> checks
        /// <see cref="CustomGameOptionsConfig.ModOverlayEnable"/> before invoking
        /// this method (hot-path gate). The <see cref="ModOverlayDpsTrackerEnable"/> check
        /// here is a defensive backup for direct calls.
        /// </remarks>
        private void ModOverlayRecordDpsEvent(PowerResults powerResults, WorldEntity ultimateOwner, long healthDelta)
        {
            // Only track damage (negative health delta), not healing
            if (healthDelta >= 0)
                return;

            // Only track hostile power results (damage from an enemy/attacker)
            if (powerResults.TestFlag(PowerResultFlags.Hostile) == false)
                return;

            // Need an ultimate owner to attribute the damage to
            if (ultimateOwner == null)
                return;

            // Resolve the ultimate owner to a player avatar
            var avatar = ultimateOwner.GetMostResponsiblePowerUser<Avatar>();
            if (avatar == null)
                return;

            var player = avatar.GetOwnerOfType<Player>();
            if (player == null)
                return;

            // Defensive backup: the call site already checks ModOverlayEnable.
            // Also check the per-feature sub-switch here.
            var options = Game?.CustomGameOptions;
            if (options == null || options.ModOverlayDpsTrackerEnable == false)
                return;

            // Damage is the absolute value of the negative health delta
            long damage = -healthDelta;
            if (damage <= 0)
                return;

            // Build the combatant key and display name
            string playerName = player.GetName();
            if (string.IsNullOrEmpty(playerName))
                return;

            // Use the avatar's prototype name as the hero name
            string heroName = avatar.PrototypeName ?? string.Empty;

            // Key by player name so all damage from the same player aggregates together
            // regardless of which avatar/pet dealt the hit
            string combatantKey = playerName;

            // Check if this is a phantom (AI bot) - the base MHServerEmu has no phantom
            // support, so this is always false. Forks that add AI heroes can override this.
            bool isPhantom = false;

            ModOverlayDpsDataStore.Instance.RecordDamage(combatantKey, playerName, heroName, damage, isPhantom);
        }
    }
}

#endregion
