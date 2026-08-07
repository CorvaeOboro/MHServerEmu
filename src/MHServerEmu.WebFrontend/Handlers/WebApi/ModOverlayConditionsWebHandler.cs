using MHServerEmu.Core.Network.Web;

namespace MHServerEmu.WebFrontend.Handlers.WebApi
{
    /// <summary>
    /// Serves condition (buff/debuff) data from <see cref="ModOverlayConditionDataStore"/>
    /// at /webapi/conditions.
    ///
    /// <para>
    /// <b>GET /webapi/conditions?player=*</b> - returns a
    /// <see cref="ModOverlayConditionSnapshot"/> with active conditions for all players.
    /// The <c>player</c> query parameter filters by player name (default: * = all).
    /// This endpoint is <b>open (no API key required)</b> - same security model as
    /// /webapi/dps (read-only, localhost-only by default).
    /// </para>
    ///
    /// <para>
    /// The overlay polls this endpoint once per second to display active buffs,
    /// debuffs, and proc triggers (on-hit / on-attacked effects) on the player's
    /// avatar. Condition data includes name, type, remaining duration, stack count,
    /// and cancel-on-hit/power-use flags for identifying proc triggers.
    /// </para>
    /// </summary>
    public class ModOverlayConditionsWebHandler : WebHandler
    {
        /// <summary>
        /// GET is open (Access = None) - no API key required.
        /// Same rationale as /webapi/dps: read-only, localhost-only by default.
        /// </summary>

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

            var snapshot = ModOverlayConditionDataStore.Instance.GetSnapshot(playerFilter);

            await context.SendJsonAsync(snapshot);
        }
    }
}
