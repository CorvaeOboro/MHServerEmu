using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.System.Time;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.CalamityEntity;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Games.Regions
{
    public partial class Region
    {
        #region Prototypes 

        private static readonly PrototypeId NPEAvengersTowerHUBRegionRef = (PrototypeId)9142075282174842340;
        private static readonly PrototypeId EGPVEManhattanRef = (PrototypeId)6460789910415087113;
        private static readonly PrototypeId VampireBloodRitualRegionRef = (PrototypeId)16693804270797857925;  // TRGameCenterRegion (Hulk Busters arcade)

        // Resolved lazily - NOT in a static field initializer to avoid GameDatabase timing issues.
        private static PrototypeId? _cloakNPCRef;
        public static PrototypeId CloakNPCRef
        {
            get
            {
                if (_cloakNPCRef == null)
                    _cloakNPCRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/NPCs/Cloak.prototype");
                return _cloakNPCRef.Value;
            }
        }

        // Kaecilius MorphEnvironment power - applies a visual environment morph/darkening effect.
        // Resolved lazily to avoid GameDatabase timing issues.
        private static PrototypeId? _kaeciliusMorphZonePowerRef;
        public static PrototypeId KaeciliusMorphZonePowerRef
        {
            get
            {
                if (_kaeciliusMorphZonePowerRef == null)
                    _kaeciliusMorphZonePowerRef = GameDatabase.GetPrototypeRefByName(
                        "Powers/EnemyPowers/Boss/Kaecilius/SummonMorphZone.prototype");
                return _kaeciliusMorphZonePowerRef.Value;
            }
        }

        #endregion

        #region  Properties

        private VampireBloodRitualEvent _vampireBloodRitualEvent;
        private Event<EntityDeadGameEvent>.Action _vampireBloodRitualEntityDeadAction;

        // Entity ID of the Cloak NPC spawned by SpawnVampireBloodRitualCloakNPC.
        // Used to distinguish our "Herald of Darkness" from the native Cloak NPC already in Avengers Tower.
        private ulong _vampireBloodRitualCloakEntityId;
        public ulong VampireBloodRitualCloakEntityId => _vampireBloodRitualCloakEntityId;

        // Set to true when the player clicks "Yes" on the Herald of Darkness dialog.
        // Checked during region generation so the event only initializes when the
        // player explicitly travels via the Cloak NPC, not on normal region visits.
        private static bool _vampireBloodRitualRequested;
        public static bool VampireBloodRitualRequested
        {
            get => _vampireBloodRitualRequested;
            set => _vampireBloodRitualRequested = value;
        }

        // Scheduled event for delayed Skrull cleanup after region generation.
        private readonly EventGroup _vampireEvents = new();
        private readonly EventPointer<SkrullCleanupEvent> _skrullCleanupEvent = new();

        #endregion

        #region Spawn Quest NPC

        /// <summary>
        /// Spawns the Cloak NPC ("Herald of Darkness") in Avengers Tower as the entry point
        /// for the Vampire Blood Ritual event. Positioned downstairs to the left of the event
        /// machine, facing south so the player can see his face. A red nameplate proxy provides
        /// the custom display name with prestige-level 5 (red) coloring.
        /// </summary>
        private void SpawnVampireBloodRitualCloakNPC()
        {
            if (Game?.CustomGameOptions?.VampireBloodRitualEventEnable != true) return;
            if (PrototypeDataRef != NPEAvengersTowerHUBRegionRef) return;

            // Resolve Cloak prototype lazily - not in a static initializer
            PrototypeId cloakRef = CloakNPCRef;
            if (cloakRef == PrototypeId.Invalid)
            {
                Logger.Warn("[VampireBloodRitual] CloakNPCRef is Invalid - prototype lookup failed.");
                return;
            }

            // Use the EGPVEManhattan transition marker as a reference point
            Vector3 landingPosition = Vector3.Zero;
            Orientation landingOrientation = Orientation.Zero;
            if (FindTargetLocation(ref landingPosition, ref landingOrientation, PrototypeId.Invalid, PrototypeId.Invalid, EGPVEManhattanRef) == false)
            {
                Logger.Warn("[VampireBloodRitual] Failed to find the EGPVEManhattan marker position for Cloak NPC spawn.");
                return;
            }

            #region locaiton

            // Toggle: true = use absolute coordinates from the exported note (NOTE_NPC_01).
            //         false = use relative offset from the EGPVEManhattan landing marker.
            bool useAbsolutePosition = true;

            Vector3 cloakPosition;
            Orientation cloakOrientation;

            if (useAbsolutePosition)
            {
                // Absolute coordinates exported from the client note tool.
                cloakPosition = new(-98.75f, 268.75f, 177f); // next to event vendor machine downstairs to the left
                cloakOrientation = new(-MathF.PI / 2f, 0f, 0f);
            }
            else
            {
                // Relative: offset from the near PORTAL ( EGPVEManhattan ) landing marker.
                cloakPosition = new(
                    landingPosition.X - 800f,   // move screenright+down diagonal
                    landingPosition.Y - 300f,    // move screenright+up diagonal 
                    landingPosition.Z - 400f);   // downstairs (lower elevation)
                cloakOrientation = new(-MathF.PI / 2f, 0f, 0f);
            }

            #endregion

            var entityManager = Game.EntityManager;
            using EntitySettings entitySettings = ObjectPoolManager.Instance.Get<EntitySettings>();
            entitySettings.EntityRef = cloakRef;
            entitySettings.Position = cloakPosition;
            entitySettings.Orientation = cloakOrientation;
            entitySettings.RegionId = Id;

            var cloakEntity = entityManager.CreateEntity(entitySettings);
            if (cloakEntity == null)
            {
                Logger.Warn("[VampireBloodRitual] Failed to create the Cloak NPC entity.");
                return;
            }

            _vampireBloodRitualCloakEntityId = cloakEntity.Id;
            Logger.Info($"[VampireBloodRitual] Spawned Cloak NPC 'Herald of Darkness' (id={cloakEntity.Id}) at {cloakPosition} in Avengers Tower.");

            // Hide the Cloak NPC's default overhead nameplate so only the red proxy nameplate shows.
            PrototypeId bossNoOverheadRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
            if (bossNoOverheadRef != PrototypeId.Invalid)
                cloakEntity.Properties[PropertyEnum.Rank] = bossNoOverheadRef;

            // Attach an invisible avatar-rendered nameplate proxy with a red (prestige 5)
            // "Herald of Darkness" nameplate above the Cloak NPC.
            if (cloakEntity is Agent cloakAgent)
                Game.IncursionManager?.SpawnNameplateProxy(this, cloakAgent, "Herald of Darkness", 5);
            else
                Logger.Warn("[VampireBloodRitual] Cloak NPC entity is not an Agent - cannot attach nameplate proxy.");
        }

        /// <summary>
        /// Initializes the Vampire Blood Ritual event when the Game Center (Hulk Busters arcade) region is generated.
        /// Clears default Skrull enemies first, then spawns custom event mobs.
        /// </summary>
        public void InitializeVampireBloodRitualEvent()
        {
            if (Game?.CustomGameOptions?.VampireBloodRitualEventEnable != true) return;
            if (PrototypeDataRef != VampireBloodRitualRegionRef) return;
            if (_vampireBloodRitualEvent != null) return;

            // Only initialize if the player explicitly requested the event via the
            // Herald of Darkness dialog. The flag is NOT reset after initialization
            // so that re-entry after death (region shuts down and regenerates) still
            // initializes the event. Without this, skipDefaultPopulation would leave
            // the player in an empty region.
            if (_vampireBloodRitualRequested == false) return;

            _vampireBloodRitualEvent = new VampireBloodRitualEvent(Game, this);
            _vampireBloodRitualEvent.Initialize();

            // Hook entity death event for win condition
            _vampireBloodRitualEntityDeadAction = OnVampireBloodRitualEntityDead;
            EntityDeadEvent.AddActionBack(_vampireBloodRitualEntityDeadAction);

            // Schedule a delayed cleanup to kill any native NPCs (e.g. Skrulls) that
            // the region's metagame or mission system spawns after Initialize().
            // The metagame schedules spawns via game events that fire after we return,
            // so we can't catch them synchronously. A 3-second delay gives them time
            // to spawn before we purge them.
            var scheduler = Game?.GameEventScheduler;
            if (scheduler != null)
            {
                scheduler.ScheduleEvent(_skrullCleanupEvent, TimeSpan.FromMilliseconds(3000), _vampireEvents);
                _skrullCleanupEvent.Get().Initialize(this);
            }
        }

        #endregion

        #region NPC Cleanup

        /// <summary>
        /// Kills all hostile NPC agents in the region that are not tracked by the
        /// Vampire Blood Ritual event. This removes native Skrulls and any other
        /// default population that slipped through despite skipDefaultPopulation.
        /// </summary>
        private void PurgeNativeNpcs()
        {
            if (_vampireBloodRitualEvent == null) return;

            var trackedIds = _vampireBloodRitualEvent.GetTrackedEntityIds();
            int killed = 0;

            foreach (Entity entity in Entities)
            {
                if (entity is not Agent agent) continue;
                if (entity is Avatar) continue;          // don't kill players
                if (agent.IsAliveInWorld == false) continue;
                if (agent.IsHostileToPlayers() == false) continue;  // only hostile NPCs
                if (trackedIds.Contains(entity.Id)) continue;       // don't kill vampire event entities

                // Skip visual props - nameplate proxies and ritual centerpiece entities
                // are Untargetable. They're hostile (mob body prototype) but not actual enemies.
                // Nameplate proxies have Untargetable=true but NOT Invulnerable=true.
                if (agent.Properties[PropertyEnum.Untargetable])
                    continue;

                agent.Kill(null, KillFlags.NoLoot | KillFlags.NoExp | KillFlags.NoDeadEvent | KillFlags.DestroyImmediate);
                killed++;
            }

            if (killed > 0)
                Logger.Info($"[VampireBloodRitual] Purged {killed} native NPC(s) from region.");
        }

        #endregion

        #region Event Lifecycle

        private void OnVampireBloodRitualEntityDead(in EntityDeadGameEvent evt)
        {
            _vampireBloodRitualEvent?.OnEntityDied(evt.Defender);
        }

        /// <summary>
        /// Shuts down the Vampire Blood Ritual event if active.
        /// </summary>
        public void ShutdownVampireBloodRitualEvent()
        {
            if (_vampireBloodRitualEvent == null) return;
            Game?.GameEventScheduler?.CancelAllEvents(_vampireEvents);
            if (_vampireBloodRitualEntityDeadAction != null)
                EntityDeadEvent.RemoveAction(_vampireBloodRitualEntityDeadAction);
            _vampireBloodRitualEvent.Shutdown();
            _vampireBloodRitualEvent = null;
        }

        /// <summary>
        /// Spawns a Dark Elf thrall at the given position via the Vampire Blood Ritual event's
        /// standard spawn pipeline. Used by the BloodLord's summon ability to ensure thralls
        /// are spawned identically to region-spawned ones.
        /// </summary>
        public Agent SpawnVampireBloodRitualThrall(Vector3 position)
        {
            return _vampireBloodRitualEvent?.SpawnThrall(position);
        }

        #endregion

        #region Skrull Cleanup Event

        /// <summary>
        /// Scheduled event that calls <see cref="PurgeNativeNpcs"/> after a delay.
        /// </summary>
        private class SkrullCleanupEvent : CallMethodEvent<Region>
        {
            protected override CallbackDelegate GetCallback() => (region) => region.PurgeNativeNpcs();
        }

        #endregion
    }
}
