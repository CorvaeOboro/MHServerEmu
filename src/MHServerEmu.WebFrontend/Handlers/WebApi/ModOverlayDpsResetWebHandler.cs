using MHServerEmu.Core.Network.Web;

namespace MHServerEmu.WebFrontend.Handlers.WebApi
{
    /// <summary>
    /// Handles POST /webapi/dps/reset - clears all recorded DPS data.
    ///
    /// <para>
    /// <b>Requires a valid API key</b> with <see cref="WebApiAccessType.ModOverlayDpsReset"/> access,
    /// passed as a Bearer token in the Authorization header. Without a valid key, the
    /// request is rejected with 403 Forbidden.
    /// </para>
    ///
    /// <para>
    /// This is a separate handler from <see cref="ModOverlayDpsWebHandler"/> so that:
    /// (1) GET /webapi/dps remains open (no key) for the overlay to poll,
    /// (2) POST /webapi/dps/reset requires a key to prevent unauthorized griefing,
    /// (3) a stray POST to /webapi/dps does NOT accidentally reset the data.
    /// </para>
    /// </summary>
    public class ModOverlayDpsResetWebHandler : WebHandler
    {
        /// <summary>
        /// Requires an API key with ModOverlayDpsReset access. This prevents any unauthenticated
        /// client from wiping DPS data - important in multiplayer where other players
        /// or processes on the network could reach the endpoint.
        /// </summary>
        public override WebApiAccessType Access { get => WebApiAccessType.ModOverlayDpsReset; }

        protected override async Task Post(WebRequestContext context)
        {
            ModOverlayDpsDataStore.Instance.Reset();
            await context.SendJsonAsync(new { Ok = true, Message = "DPS data reset." });
        }
    }
}
