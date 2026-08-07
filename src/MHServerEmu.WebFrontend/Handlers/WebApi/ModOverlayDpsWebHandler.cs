using System.Collections.Specialized;
using MHServerEmu.Core.Network.Web;

namespace MHServerEmu.WebFrontend.Handlers.WebApi
{
    /// <summary>
    /// Serves DPS data from <see cref="ModOverlayDpsDataStore"/> at /webapi/dps.
    ///
    /// <para>
    /// <b>GET /webapi/dps?player=*</b> - returns a <see cref="ModOverlayDpsSnapshot"/> with all combatants.
    /// The <c>player</c> query parameter filters by player name (default: * = all).
    /// This endpoint is <b>open (no API key required)</b> because it is read-only and the
    /// WebFrontend binds to localhost by default. If the server is configured to listen on
    /// a public interface, any reachable client can read player names and damage data -
    /// see the SECURITY note in the README.
    /// </para>
    /// <para>
    /// <b>POST /webapi/dps/reset</b> - clears all recorded damage data.
    /// This endpoint <b>requires a valid API key</b> with <see cref="WebApiAccessType.ModOverlayDpsReset"/>
    /// access. Without a key, the POST is rejected with 403 Forbidden. This prevents
    /// unauthorized griefing (repeated resets) in multiplayer settings.
    /// </para>
    ///
    /// <para>
    /// This endpoint is the server-side replacement for the closed-source overlay's
    /// DLL-injection + packet-sniffing approach. The overlay polls this endpoint once
    /// per second via HTTP - no Npcap, no injection, no hooks in the game client.
    /// </para>
    /// </summary>
    public class ModOverlayDpsWebHandler : WebHandler
    {
        /// <summary>
        /// GET is open (Access = None) - no API key required.
        /// The WebFrontend binds to localhost by default, so only local processes
        /// can reach this endpoint unless the admin explicitly changes the bind address.
        /// </summary>
        // Access defaults to WebApiAccessType.None (open) - no override needed for GET.

        protected override async Task Get(WebRequestContext context)
        {
            // Parse query string for optional player filter
            string playerFilter = "*";
            var query = await context.ReadQueryStringAsync();
            if (query != null)
            {
                string player = query["player"];
                if (string.IsNullOrWhiteSpace(player) == false)
                    playerFilter = player;
            }

            var snapshot = ModOverlayDpsDataStore.Instance.GetSnapshot(playerFilter);
            snapshot.Player = playerFilter;

            await context.SendJsonAsync(snapshot);
        }
    }
}
