// =============================================================================
// MOD INCURSION 
// =============================================================================
//   INCURSION spawns hostile Avatars, teamup , Bosses , that randomly hunt players 
//   dangerous short encounters 
//
//   Decoupled Rendering is the primary workaround being used for the Incursion Avatar Enemies
//   and also used to spoof custom color nameplates for Incursion NPC Enemies like Teamups and Bosses 
//
//  VERSION:: 20260728
// =============================================================================
using Gazillion;
using MHServerEmu.Core.Serialization;
using MHServerEmu.Games.Common;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Social.Guilds;

namespace MHServerEmu.Games.Entities
{
    public partial class WorldEntity
    {
        // Prototype the client renders this entity as. The server continues driving the real prototype as the combat body.
        public PrototypeId ClientPrototypeRefOverride { get; set; } = PrototypeId.Invalid;

        // True when the render override is an AvatarPrototype. Extra replication fields are emitted
        // so the client builds an avatar actor.
        public bool IsClientRenderedAsAvatar { get; private set; }
        public uint SpoofAvatarWorldInstanceId { get; private set; }

        // Bound only when rendering as an avatar.
        private RepVar_string _spoofAvatarPlayerName;
        private List<AbilityKeyMapping> _spoofAvatarAbilityKeyMappings;

        // Counter for unique SpoofAvatarWorldInstanceId values. Starts high to avoid collisions
        // with real player avatar world instance IDs (which are typically small sequential numbers).
        // Each render-as-avatar entity must get a unique ID so the client creates a separate pawn
        // instead of reusing cached pawn data (which can assign a default costume to nameplate proxies).
        private static uint s_nextSpoofId = 1000000;

        /// <summary>
        /// Clears the replicated overhead name drawn above this entity when it is rendered as an avatar.
        /// </summary>
        public void ClearSpoofAvatarPlayerName()
        {
            _spoofAvatarPlayerName?.Set(string.Empty);
        }

        /// <summary>
        /// Sets the replicated overhead name drawn above this entity when it is rendered as an avatar.
        /// </summary>
        public void SetSpoofAvatarPlayerName(string name)
        {
            _spoofAvatarPlayerName?.Set(name);
        }

        /// <summary>
        /// The prototype the client should render this entity as. Returns the real
        /// <see cref="Entity.PrototypeDataRef"/> unless a render override is active.
        /// </summary>
        public PrototypeId GetClientPrototypeDataRef()
        {
            return ClientPrototypeRefOverride != PrototypeId.Invalid ? ClientPrototypeRefOverride : PrototypeDataRef;
        }

        /// <summary>
        /// Sets up decoupled rendering state when a SpawnSpec provides a ClientRenderPrototypeRef.
        /// Called from Initialize().
        /// </summary>
        private void InitializeDecoupledRendering(EntitySettings settings)
        {
            if (settings.SpawnSpec == null || settings.SpawnSpec.ClientRenderPrototypeRef == PrototypeId.Invalid)
                return;

            ClientPrototypeRefOverride = settings.SpawnSpec.ClientRenderPrototypeRef;

            // Prepare avatar-compatible replication state for non-avatar entities rendered
            // as an avatar (e.g. Incursion avatar enemies that use a playable character model).
            if (this is not Avatar && ClientPrototypeRefOverride.As<AvatarPrototype>() != null)
            {
                IsClientRenderedAsAvatar = true;
                SpoofAvatarWorldInstanceId = s_nextSpoofId++;  // unique per entity to prevent client pawn cache collisions

                _spoofAvatarPlayerName = new();
                _spoofAvatarPlayerName.Bind(this, AOINetworkPolicyValues.AOIChannelProximity | AOINetworkPolicyValues.AOIChannelParty | AOINetworkPolicyValues.AOIChannelOwner);

                // Custom name drawn above the entity when rendered as an avatar.
                if (string.IsNullOrEmpty(settings.SpawnSpec.ClientRenderPlayerName) == false)
                    _spoofAvatarPlayerName.Set(settings.SpawnSpec.ClientRenderPlayerName);

                _spoofAvatarAbilityKeyMappings = new();
            }
        }

        /// <summary>
        /// Unbinds decoupled rendering replicated fields. Called from UnbindReplicatedFields().
        /// </summary>
        private void UnbindDecoupledRendering()
        {
            _spoofAvatarPlayerName?.Unbind();
        }

        /// <summary>
        /// Appends avatar transient replication tail for non-avatar entities rendered as an avatar.
        /// Called from Serialize().
        /// </summary>
        private void SerializeDecoupledRendering(Archive archive, ref bool success)
        {
            if (IsClientRenderedAsAvatar == false || this is Avatar)
                return;

            if (archive.IsTransient)
            {
                success &= Serializer.Transfer(archive, ref _spoofAvatarPlayerName);

                ulong ownerPlayerDbId = 0;
                success &= Serializer.Transfer(archive, ref ownerPlayerDbId);

                string emptyString = string.Empty;
                success &= Serializer.Transfer(archive, ref emptyString);

                if (archive.IsReplication)
                {
                    ulong guildId = GuildManager.InvalidGuildId;
                    string guildName = string.Empty;
                    GuildMembership guildMembership = GuildMembership.eGMNone;
                    success &= GuildMember.SerializeReplicationRuntimeInfo(archive, ref guildId, ref guildName, ref guildMembership);
                }
            }

            success &= Serializer.Transfer(archive, ref _spoofAvatarAbilityKeyMappings);
        }

        /// <summary>
        /// Logs health changes caused by incursion enemies for tuning visibility.
        /// Called from ApplyPowerResultsInternal().
        /// </summary>
        private void LogIncursionHealthChange(PowerResults powerResults, WorldEntity ultimateOwner, long startHealth, long health)
        {
            bool ultimateOwnerIsIncursion = ultimateOwner != null
                && (ultimateOwner.IsClientRenderedAsAvatar
                    || (Game?.IncursionManager != null && Game.IncursionManager.IsIncursionEntity(ultimateOwner.Id)));
            if (ultimateOwnerIsIncursion && health != startHealth)
            {
                PrototypeId hitPowerRef = powerResults.PowerPrototype != null ? powerResults.PowerPrototype.DataRef : PrototypeId.Invalid;
                string hpMsg = $"[IncursionEnemy] HP: target '{PrototypeName}' (id {Id}) {startHealth} -> {health} " +
                               $"(delta {health - startHealth}) from '{ultimateOwner.PrototypeName}' power '{GameDatabase.GetPrototypeName(hitPowerRef)}'.";
                if (Game?.CustomGameOptions?.IncursionLoggingEnable == true)
                    Logger.Info(hpMsg);
                IncursionLogCollator.WriteLine(Id, hpMsg);
                IncursionLogCollator.WriteLine(ultimateOwner.Id, hpMsg);
            }
        }
    }
}
