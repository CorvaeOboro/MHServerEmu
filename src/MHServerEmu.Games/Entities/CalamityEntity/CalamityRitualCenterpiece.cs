#region CENTERPIECE
// =============================================================================
// MOD CALAMITY 
// =============================================================================
//   CALAMITY is a collection of custom encounters that are short, small 
//   and play like the games existing "terminals". 
//
//   Ritual Centerpiece is a prop of the boss fight to interact with , elude to lore , or be decorative 
//   for Example the "Amulet of Quiox" item is being showcased to elude to its lore 
//   this vampire coven is attempting to corrupt it so that it may not be used to slow vampirism
//   the player standing near the amulet long enough will purify it ( red to blue vfx ) , 
//   but entering into the ritual circle slows ( kacilius mirror dimension cirle )
//
//  VERSION:: 20260713
// =============================================================================

using Gazillion;
using MHServerEmu.Core.Collisions;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;
using System.Collections.Generic;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Ritual centerpiece: a floating item model with looping VFX and a blue prestige nameplate.
    /// Designed for the Vampire Blood Ritual event but extensible for future centerpieces
    /// that may have health, protection mechanics, or interactive features.
    ///
    /// This is a multi-entity visual:
    /// - Item body: an ItemPrototype (e.g. relic/artifact) rendered as a floating 3D model.
    /// - Nameplate proxy: an invisible avatar-rendered entity with prestige level 2 (blue)
    ///   and a custom display name, positioned at the same location as the item body.
    /// - Looping VFX: plays a PowerVisuals asset (e.g. InfinityPowerPointEarnedClass)
    ///   on the item body on a recurring timer via the game event scheduler.
    /// - Purification: when a player stands within 300 units for ~5 cumulative seconds,
    ///   the VFX swaps to InfinityMindPointEarnedClass and a white "PURIFIED" nameplate
    ///   proxy appears above the existing blue nameplate.
    /// </summary>
    public class RitualCenterpiece
    {
        #region Configuration

        private static readonly Logger Logger = LogManager.CreateLogger();

        // Toggle: enable or disable the item 3D model rendering (nameplate + VFX still work).
        private static readonly bool RenderItemBody = false;

        private readonly Game _game;
        private readonly Region _region;

        // Tracked entity IDs for cleanup
        private ulong _itemBodyEntityId;
        private ulong _nameplateProxyEntityId;
        private ulong _purifiedNameplateProxyEntityId;
        private ulong _hotspotEntityId;

        // Event scheduling for VFX loop and purification tick
        private readonly EventGroup _events = new();
        private readonly EventPointer<VfxLoopEvent> _vfxLoopEvent = new();
        private readonly EventPointer<PurificationTickEvent> _purificationTickEvent = new();

        // Configuration
        private readonly string _displayName;
        private readonly int _prestigeLevel;       // 2 = blue
        private readonly string _itemProtoPath;    // prototype path for the visual item model
        private readonly float _hoverHeight;       // Y offset above the spawn position
        private readonly float _boundsScale;       // visual scale override
        private readonly AssetId _vfxAssetId;      // VFX asset to loop
        private readonly TimeSpan _vfxInterval;    // time between VFX replays

        // Purification state
        private Vector3 _spawnPosition;
        private float _purifyRange = 300f;
        private TimeSpan _purifyRequiredTime = TimeSpan.FromSeconds(5);
        private TimeSpan _purifyAccumulated;
        private bool _isPurified;
        private AssetId _purifiedVfxAssetId;
        private const int PurificationTickMs = 500;

        #endregion

        #region Properties

        /// <summary>
        /// The entity ID of the item body (the visible floating model), or 0 if not spawned.
        /// </summary>
        public ulong ItemBodyEntityId => _itemBodyEntityId;

        /// <summary>
        /// The entity ID of the nameplate proxy, or 0 if not spawned.
        /// </summary>
        public ulong NameplateProxyEntityId => _nameplateProxyEntityId;

        /// <summary>
        /// Whether the centerpiece has been purified.
        /// </summary>
        public bool IsPurified => _isPurified;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a RitualCenterpiece configuration.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="region">Region to spawn in.</param>
        /// <param name="displayName">Nameplate text (e.g. "Amulet of Quiox").</param>
        /// <param name="prestigeLevel">Prestige color: 0=default, 1=green, 2=blue, 3=purple, 4=orange, 5=red, 6=yellow.</param>
        /// <param name="itemProtoPath">Prototype path for the item model (e.g. a relic or artifact).</param>
        /// <param name="hoverHeight">Y offset above the spawn position for floating effect.</param>
        /// <param name="boundsScale">Visual scale multiplier for the item model.</param>
        /// <param name="vfxAssetId">VFX asset to play on loop (AssetId.Invalid = no VFX).</param>
        /// <param name="vfxIntervalMs">Time between VFX replays in milliseconds.</param>
        public RitualCenterpiece(Game game, Region region,
            string displayName = " ", //Amulet of Quiox is handled by the item 3d model renderer
            int prestigeLevel = 2,
            string itemProtoPath = "Entity/Items/Artifacts/Prototypes/Tier1Artifacts/Art050.prototype",
            float hoverHeight = 100f,
            float boundsScale = 1.5f,
            AssetId vfxAssetId = default,
            int vfxIntervalMs = 1000)
        {
            _game = game;
            _region = region;
            _displayName = displayName;
            _prestigeLevel = prestigeLevel;
            _itemProtoPath = itemProtoPath;
            _hoverHeight = hoverHeight;
            _boundsScale = boundsScale;
            _vfxAssetId = vfxAssetId;
            _vfxInterval = TimeSpan.FromMilliseconds(vfxIntervalMs);
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Spawns the centerpiece at the given position. This creates:
        /// 1. An item body entity (floating 3D model).
        /// 2. An invisible avatar nameplate proxy with the configured display name and prestige color.
        /// 3. Starts the looping VFX on the item body.
        /// </summary>
        public bool Spawn(Vector3 position)
        {
            if (_region == null) return false;

            Vector3 spawnPos = position + new Vector3(0f, 0f, _hoverHeight);
            _spawnPosition = spawnPos;

            // --- Spawn the item body (visible floating model) ---
            if (RenderItemBody)
                SpawnItemBody(spawnPos);

            // --- Spawn the nameplate proxy (invisible avatar with prestige nameplate) ---
            SpawnNameplateProxy(spawnPos);

            // --- Start looping VFX on the nameplate proxy (Agent with a pawn) ---
            // VFX must play on an Agent/Avatar entity, not an Item - the client only
            // processes NetMessagePlayPowerVisuals for entities with Unreal pawns.
            if (_vfxAssetId != AssetId.Invalid && _nameplateProxyEntityId != 0)
                StartVfxLoop();

            // --- Resolve purified VFX asset for later use ---
            var globalsProto = GameDatabase.PowerVisualsGlobalsPrototype;
            if (globalsProto != null)
                _purifiedVfxAssetId = globalsProto.InfinityMindPointEarnedClass;

            // --- Spawn the Kaecilius MorphEnvironment hotspot (ritual circle AOE) ---
            // This is a sorcerer circle that slows players who enter it, NOT a region-wide
            // darkness effect. It spawns at the centerpiece location.
            SpawnRitualCircleHotspot(position);

            // --- Start purification proximity check ---
            StartPurificationTick();

            return _nameplateProxyEntityId != 0;
        }

        /// <summary>
        /// Cleans up all entities and cancels scheduled events.
        /// Called when the region shuts down or the centerpiece is destroyed.
        /// </summary>
        public void Destroy()
        {
            // Cancel all scheduled events (VFX loop + purification tick)
            _game?.GameEventScheduler?.CancelAllEvents(_events);

            // Destroy purified nameplate proxy
            if (_purifiedNameplateProxyEntityId != 0)
            {
                var purifiedProxy = _game?.EntityManager?.GetEntity<WorldEntity>(_purifiedNameplateProxyEntityId);
                if (purifiedProxy != null)
                {
                    try { purifiedProxy.Destroy(); } catch { /* may already be destroyed */ }
                }
                _purifiedNameplateProxyEntityId = 0;
            }

            // Destroy nameplate proxy
            if (_nameplateProxyEntityId != 0)
            {
                var proxy = _game?.EntityManager?.GetEntity<WorldEntity>(_nameplateProxyEntityId);
                if (proxy != null)
                {
                    try { proxy.Destroy(); } catch { /* may already be destroyed */ }
                }
                _nameplateProxyEntityId = 0;
            }

            // Destroy item body
            if (_itemBodyEntityId != 0)
            {
                var body = _game?.EntityManager?.GetEntity<WorldEntity>(_itemBodyEntityId);
                if (body != null)
                {
                    try { body.Destroy(); } catch { /* may already be destroyed */ }
                }
                _itemBodyEntityId = 0;
            }

            // Destroy ritual circle hotspot
            if (_hotspotEntityId != 0)
            {
                var hotspot = _game?.EntityManager?.GetEntity<WorldEntity>(_hotspotEntityId);
                if (hotspot != null)
                {
                    try { hotspot.Destroy(); } catch { /* may already be destroyed */ }
                }
                _hotspotEntityId = 0;
            }

            Logger.Info($"[RitualCenterpiece] Destroyed '{_displayName}'.");
        }

        #endregion

        // ------------------------------------------------------------------
        // Item body spawning
        // ------------------------------------------------------------------

        #region Item Spawn

        private void SpawnItemBody(Vector3 spawnPos)
        {
            PrototypeId itemRef = GameDatabase.GetPrototypeRefByName(_itemProtoPath);
            if (itemRef == PrototypeId.Invalid)
            {
                Logger.Warn($"[RitualCenterpiece] Item prototype '{_itemProtoPath}' not found.");
                return;
            }

            // Use the SpawnSpec pipeline (same as the nameplate proxy) for reliable world entry.
            // SpawnSpec handles cell validation, snap-to-floor, and automatic ItemSpec creation
            // for ItemPrototype entities. The old EntityManager.CreateEntity approach failed
            // silently when the hover-height position wasn't on the navmesh.
            var manager = _region.PopulationManager;
            if (manager == null)
            {
                Logger.Warn("[RitualCenterpiece] PopulationManager is null - cannot spawn item body.");
                return;
            }

            var group = manager.CreateSpawnGroup();
            // Position only - SpawnSpec.Spawn() only extracts Yaw from the transform,
            // so we set full 3D orientation after spawn via ChangeRegionPosition.
            group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = itemRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = false;   // Don't snap to floor - we want it floating at hover height
            spec.BoundsScaleOverride = _boundsScale;

            // Set properties via the spec's PropertyCollection (copied to EntitySettings during Spawn).
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.Unaffectable] = true;
            spec.Properties[PropertyEnum.Invulnerable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;
            spec.Properties[PropertyEnum.MapTracking] = false;
            spec.Properties[PropertyEnum.Visible] = true;
            spec.Properties[PropertyEnum.LootTablePrototype] = PrototypeId.Invalid;

            // Set rank to BossNoOverheadInfo to hide the item's own nameplate -
            // we have a separate avatar nameplate proxy for the display name.
            PrototypeId bossNoOverheadRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
            if (bossNoOverheadRef != PrototypeId.Invalid)
                spec.Properties[PropertyEnum.Rank] = bossNoOverheadRef;

            if (spec.Spawn() == false)
            {
                Logger.Warn($"[RitualCenterpiece] SpawnSpec.Spawn() failed for item body '{_itemProtoPath}'.");
                manager.RemoveSpawnGroup(group.Id);
                return;
            }

            var entity = spec.ActiveEntity;
            if (entity == null)
            {
                Logger.Warn($"[RitualCenterpiece] SpawnSpec.ActiveEntity is null for item body '{_itemProtoPath}'.");
                manager.RemoveSpawnGroup(group.Id);
                return;
            }

            _itemBodyEntityId = entity.Id;

            // Set full 3D orientation after spawn. SpawnSpec.Spawn() only extracts Yaw from
            // the transform (Orientation.FromTransform3 discards Pitch/Roll), so we use
            // ChangeRegionPosition with ForceUpdate to send the complete orientation.
            // Default (0,0,0): face=+Z (up), base=+X (east).
            // Yaw=0 (face south), Pitch=-PI/2 (tip face from up to south/-Y), Roll=-PI/2 (swing base from east to down/-Z).
            if (entity is WorldEntity worldEntity)
            {
                worldEntity.ChangeRegionPosition(null,
                    new Orientation(0f, -MathF.PI / 2f, -MathF.PI / 2f),
                    ChangePositionFlags.ForceUpdate);
            }

            // Reassert properties after spawn (some may be overwritten during initialization) .
            entity.Properties[PropertyEnum.Untargetable] = true;
            entity.Properties[PropertyEnum.Unaffectable] = true;
            entity.Properties[PropertyEnum.Invulnerable] = true;
            entity.Properties[PropertyEnum.NoEntityCollide] = true;
            entity.Properties[PropertyEnum.MapTracking] = false;
            entity.Properties[PropertyEnum.Visible] = true;
            entity.Properties[PropertyEnum.LootTablePrototype] = PrototypeId.Invalid;
            // Reassert BossNoOverheadInfo rank to keep the item's nameplate hidden.
            PrototypeId bossNoOverheadRef2 = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
            if (bossNoOverheadRef2 != PrototypeId.Invalid)
                entity.Properties[PropertyEnum.Rank] = bossNoOverheadRef2;

            // The spawn position already includes the hover height offset (set in Spawn()).
            // Since SnapToFloor is false, the entity should spawn at the correct height.
            // But if the engine adjusted it, force it back to the intended hover position.
            if (_hoverHeight != 0f && entity.IsInWorld)
            {
                Vector3 hoverPos = new(spawnPos.X, spawnPos.Y, spawnPos.Z);
                entity.ChangeRegionPosition(hoverPos, null, ChangePositionFlags.Force);
            }

            Logger.Info($"[RitualCenterpiece] Spawned item body '{_itemProtoPath}' at {spawnPos} (id={entity.Id}, isInWorld={entity.IsInWorld}).");
        }

        #endregion

        // ------------------------------------------------------------------
        // Ritual circle hotspot (Kaecilius MorphEnvironment AOE)
        // ------------------------------------------------------------------

        #region Ritual Circle

        /// <summary>
        /// Spawns the Kaecilius MorphEnvironment hotspot at the centerpiece position.
        /// This creates a sorcerer circle AOE on the ground that slows players who enter it.
        /// It is NOT a region-wide darkness effect - it is a localized ground hazard.
        /// </summary>
        private void SpawnRitualCircleHotspot(Vector3 position)
        {
            if (_region == null) return;

            PrototypeId hotspotRef = GameDatabase.GetPrototypeRefByName(
                "Entity/PowerEntities/PrototypesHotspot/KaeciliusMorphZoneHotspotArea.prototype");
            if (hotspotRef == PrototypeId.Invalid)
            {
                Logger.Warn("[RitualCenterpiece] KaeciliusMorphZoneHotspotArea prototype not found - ritual circle AOE not spawned.");
                return;
            }

            // Project to floor to get a valid ground position
            Vector3 groundPos = RegionLocation.ProjectToFloor(_region, position);
            if (groundPos == Vector3.Zero)
            {
                Logger.Warn("[RitualCenterpiece] Failed to project ritual circle hotspot position to floor.");
                return;
            }

            using EntitySettings hotspotSettings = ObjectPoolManager.Instance.Get<EntitySettings>();
            hotspotSettings.EntityRef = hotspotRef;
            hotspotSettings.RegionId = _region.Id;
            hotspotSettings.Position = groundPos;
            hotspotSettings.HotspotSkipCollide = true;

            var hotspot = _game.EntityManager.CreateEntity(hotspotSettings);
            if (hotspot != null)
            {
                _hotspotEntityId = hotspot.Id;
                Logger.Info($"[RitualCenterpiece] Spawned Kaecilius MorphEnvironment ritual circle hotspot (id={hotspot.Id}) at {groundPos}.");
            }
            else
                Logger.Warn("[RitualCenterpiece] Failed to create Kaecilius MorphEnvironment ritual circle hotspot.");
        }

        #endregion

        // ------------------------------------------------------------------
        // Nameplate proxy spawning
        // ------------------------------------------------------------------

        #region Name Spawn

        private void SpawnNameplateProxy(Vector3 spawnPos)
        {
            // Use the same avatar prototype as IncursionManager for nameplate rendering.
            // The client only applies prestige-based name coloring for AvatarPrototype entities.
            PrototypeId avatarRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/SheHulk.prototype");
            var avatarProto = avatarRef.As<AvatarPrototype>();
            if (avatarProto == null)
            {
                Logger.Warn("[RitualCenterpiece] SheHulk avatar prototype not found for nameplate proxy.");
                return;
            }

            // Use the SpidermanClone combat body (same as IncursionManager nameplate proxies)
            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(
                "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype");
            if (combatBodyRef == PrototypeId.Invalid)
            {
                Logger.Warn("[RitualCenterpiece] Combat body prototype not found for nameplate proxy.");
                return;
            }

            // BossNoOverheadInfo rank: hides all default overhead info (level, rank name, etc.)
            PrototypeId bossNoOverheadRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
            if (bossNoOverheadRef == PrototypeId.Invalid)
            {
                Logger.Warn("[RitualCenterpiece] BossNoOverheadInfo rank prototype not found.");
                return;
            }

            var manager = _region.PopulationManager;
            if (manager == null) return;

            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = combatBodyRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Render as avatar so the client applies prestige name colors
            spec.ClientRenderPrototypeRef = avatarRef;
            spec.ClientRenderPlayerName = _displayName;

            // Prestige level: 2 = blue nameplate
            spec.Properties[PropertyEnum.AvatarPrestigeLevel] = _prestigeLevel;

            // Level -1: client skips displaying a level number for avatar-rendered entities
            spec.Properties[PropertyEnum.CharacterLevel] = -1;
            spec.Properties[PropertyEnum.CombatLevel] = -1;

            // BossNoOverheadInfo rank: hides all overhead info including level number
            spec.Properties[PropertyEnum.Rank] = bossNoOverheadRef;

            // Non-hostile, untargetable, invulnerable, no collision - purely visual, survives the fight.
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.Unaffectable] = true;
            spec.Properties[PropertyEnum.Invulnerable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;

            // Hide the proxy's 3D model - we only want the nameplate
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;

            spec.Spawn();

            var proxy = spec.ActiveEntity;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                Logger.Warn("[RitualCenterpiece] Failed to spawn nameplate proxy.");
                return;
            }

            _nameplateProxyEntityId = proxy.Id;

            // Reassert level and invulnerability after spawn
            proxy.Properties[PropertyEnum.CharacterLevel] = -1;
            proxy.Properties[PropertyEnum.CombatLevel] = -1;
            proxy.Properties[PropertyEnum.Untargetable] = true;
            proxy.Properties[PropertyEnum.Unaffectable] = true;
            proxy.Properties[PropertyEnum.Invulnerable] = true;
            proxy.Properties[PropertyEnum.NoEntityCollide] = true;

            // Strip all powers from the proxy so it doesn't play animations or attack.
            // Check ContainsPower before each UnassignPower because unassigning a combo
            // power can condemn related powers, removing them from the collection.
            if (proxy is Agent proxyAgent && proxyAgent.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxyAgent.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                {
                    if (proxyAgent.PowerCollection.ContainsPower(powerRef))
                        proxyAgent.UnassignPower(powerRef);
                }
            }

            // Disable AI and set dormant
            if (proxy is Agent aiAgent)
            {
                aiAgent.AIController?.SetIsEnabled(false);
                aiAgent.SetDormant(true);
            }

            Logger.Info($"[RitualCenterpiece] Spawned nameplate proxy '{_displayName}' (prestige={_prestigeLevel}) at {spawnPos} (id={proxy.Id}).");
        }

        #endregion

        // ------------------------------------------------------------------
        // VFX loop
        // ------------------------------------------------------------------

        #region VFX Loop

        private void StartVfxLoop()
        {
            var scheduler = _game?.GameEventScheduler;
            if (scheduler == null) return;

            // Play VFX immediately
            PlayVfx();

            // Schedule recurring playback
            scheduler.ScheduleEvent(_vfxLoopEvent, _vfxInterval, _events);
            _vfxLoopEvent.Get().Initialize(this);
        }

        private void PlayVfx()
        {
            // Use purified VFX if purified, otherwise the original
            AssetId assetId = _isPurified && _purifiedVfxAssetId != AssetId.Invalid
                ? _purifiedVfxAssetId
                : _vfxAssetId;
            PlayVfxInternal(assetId);
        }

        private void PlayVfxInternal(AssetId assetId)
        {
            if (assetId == AssetId.Invalid) return;

            // Play VFX on the nameplate proxy (an Agent with a pawn) rather than the
            // item body (an Item with no pawn). The client only processes power visuals
            // for entities that have Unreal pawns (Avatar/Agent). The proxy is at the
            // same position as the item body so the VFX appears in the right place.
            var entity = _game?.EntityManager?.GetEntity<WorldEntity>(_nameplateProxyEntityId);
            if (entity == null) return;

            var msg = NetMessagePlayPowerVisuals.CreateBuilder()
                .SetEntityId(entity.Id)
                .SetPowerAssetRef((ulong)assetId)
                .Build();

            _game?.NetworkManager?.SendMessageToInterested(msg, entity, AOINetworkPolicyValues.AOIChannelProximity);
        }

        /// <summary>
        /// Called by the scheduled VFX loop event. Plays the VFX and reschedules.
        /// </summary>
        private void OnVfxLoopTick()
        {
            PlayVfx();

            var scheduler = _game?.GameEventScheduler;
            if (scheduler == null) return;

            scheduler.ScheduleEvent(_vfxLoopEvent, _vfxInterval, _events);
            _vfxLoopEvent.Get().Initialize(this);
        }

        #endregion

        // ------------------------------------------------------------------
        // Purification system
        // ------------------------------------------------------------------

        #region Purification

        private void StartPurificationTick()
        {
            var scheduler = _game?.GameEventScheduler;
            if (scheduler == null) return;

            scheduler.ScheduleEvent(_purificationTickEvent, TimeSpan.FromMilliseconds(PurificationTickMs), _events);
            _purificationTickEvent.Get().Initialize(this);
        }

        /// <summary>
        /// Called every PurificationTickMs. Checks if any player avatar is within
        /// _purifyRange of the centerpiece. If so, accumulates time toward purification.
        /// After _purifyRequiredTime of cumulative in-range time, triggers purification.
        /// </summary>
        private void OnPurificationTick()
        {
            if (_isPurified) return;
            if (_region == null) return;

            // Get the item body position (or fallback to spawn position)
            Vector3 centerPos = _spawnPosition;
            var itemBody = _game?.EntityManager?.GetEntity<WorldEntity>(_itemBodyEntityId);
            if (itemBody != null && itemBody.IsInWorld)
                centerPos = itemBody.RegionLocation.Position;

            // Check for nearby avatars
            bool playerNearby = false;
            Sphere volume = new(centerPos, _purifyRange);
            foreach (Avatar avatar in _region.IterateAvatarsInVolume(volume))
            {
                if (avatar != null && avatar.IsAliveInWorld)
                {
                    playerNearby = true;
                    break;
                }
            }

            if (playerNearby)
            {
                _purifyAccumulated += TimeSpan.FromMilliseconds(PurificationTickMs);
                Logger.Info($"[RitualCenterpiece] Purifying '{_displayName}': {_purifyAccumulated.TotalSeconds:F1}s / {_purifyRequiredTime.TotalSeconds:F1}s");

                if (_purifyAccumulated >= _purifyRequiredTime)
                {
                    TriggerPurification();
                    return;  // No more ticks needed
                }
            }

            // Reschedule
            var scheduler = _game?.GameEventScheduler;
            if (scheduler == null) return;
            scheduler.ScheduleEvent(_purificationTickEvent, TimeSpan.FromMilliseconds(PurificationTickMs), _events);
            _purificationTickEvent.Get().Initialize(this);
        }

        /// <summary>
        /// Triggers the purification state: swaps VFX to InfinityMindPointEarnedClass
        /// and spawns a white "PURIFIED" nameplate proxy above the existing nameplate.
        /// </summary>
        private void TriggerPurification()
        {
            _isPurified = true;
            Logger.Info($"[RitualCenterpiece] '{_displayName}' has been PURIFIED!");

            // Swap VFX asset for subsequent playback ( red to blue ) 
            if (_purifiedVfxAssetId != AssetId.Invalid)
            {
                // Play the new VFX immediately
                PlayVfxInternal(_purifiedVfxAssetId);
            }

            // Spawn the "PURIFIED" nameplate proxy above the existing one
            SpawnPurifiedNameplateProxy();
        }

        /// <summary>
        /// Spawns the PURIFIED nameplate proxy above the existing blue nameplate.
        /// Uses the same hiding flags as the base proxy (IsClientEntityHidden + Visible=false
        /// + no costume) to ensure the SheHulk mesh stays hidden.
        /// </summary>
        private void SpawnPurifiedNameplateProxy()
        {
            if (_region == null) return;

            // Get the position of the existing nameplate proxy
            var existingProxy = _game?.EntityManager?.GetEntity<WorldEntity>(_nameplateProxyEntityId);
            if (existingProxy == null) return;

            Vector3 pos = existingProxy.RegionLocation.Position;
            // Small diagonal offset in X/Y so "PURIFIED" appears just above the base
            // nameplate in isometric view (Z is up, diagonal back = up on screen).
            Vector3 purifiedPos = new(pos.X + 20f, pos.Y + 20f, pos.Z + 0f);

            PrototypeId avatarRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/SheHulk.prototype");
            var avatarProto = avatarRef.As<AvatarPrototype>();
            if (avatarProto == null) return;

            PrototypeId combatBodyRef = GameDatabase.GetPrototypeRefByName(
                "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype");
            if (combatBodyRef == PrototypeId.Invalid) return;

            PrototypeId bossNoOverheadRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
            if (bossNoOverheadRef == PrototypeId.Invalid) return;

            var manager = _region.PopulationManager;
            if (manager == null) return;

            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(purifiedPos, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = combatBodyRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Render as avatar for prestige name coloring
            spec.ClientRenderPrototypeRef = avatarRef;
            spec.ClientRenderPlayerName = "PURIFIED";

            // Prestige level 0 = white/default nameplate
            spec.Properties[PropertyEnum.AvatarPrestigeLevel] = 0;
            spec.Properties[PropertyEnum.CharacterLevel] = -1;
            spec.Properties[PropertyEnum.CombatLevel] = -1;
            spec.Properties[PropertyEnum.Rank] = bossNoOverheadRef;

            // Non-hostile, untargetable, invulnerable, no collision
            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.Unaffectable] = true;
            spec.Properties[PropertyEnum.Invulnerable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;

            // Hide the proxy's 3D model - we only want the nameplate
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;

            // Prevent simulation so the proxy doesn't activate powers or locomotion
            spec.Properties[PropertyEnum.Dormant] = true;

            spec.Spawn();

            var proxy = spec.ActiveEntity;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                Logger.Warn("[RitualCenterpiece] Failed to spawn PURIFIED nameplate proxy.");
                return;
            }

            _purifiedNameplateProxyEntityId = proxy.Id;

            // Reassert hiding flags after spawn - attachment and spawn initialization
            // can reset these, causing the SheHulk mesh to become visible.
            proxy.Properties[PropertyEnum.CharacterLevel] = -1;
            proxy.Properties[PropertyEnum.CombatLevel] = -1;
            proxy.Properties[PropertyEnum.Untargetable] = true;
            proxy.Properties[PropertyEnum.Unaffectable] = true;
            proxy.Properties[PropertyEnum.Invulnerable] = true;
            proxy.Properties[PropertyEnum.NoEntityCollide] = true;
            proxy.Properties[PropertyEnum.Visible] = false;

            // Strip powers and disable AI.
            // Check ContainsPower before each UnassignPower because unassigning a combo
            // power can condemn related powers, removing them from the collection.
            if (proxy is Agent proxyAgent && proxyAgent.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxyAgent.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                {
                    if (proxyAgent.PowerCollection.ContainsPower(powerRef))
                        proxyAgent.UnassignPower(powerRef);
                }
            }

            if (proxy is Agent aiAgent)
            {
                aiAgent.AIController?.SetIsEnabled(false);
                aiAgent.SetDormant(true);
            }

            // Do NOT attach to the existing proxy - AttachToEntity triggers a client
            // hierarchy update that overrides IsClientEntityHidden, making the SheHulk
            // mesh visible. The proxy is stationary (like the base proxy) so attachment
            // is not needed for position tracking.

            Logger.Info($"[RitualCenterpiece] Spawned PURIFIED nameplate proxy at {purifiedPos} (id={proxy.Id}).");
        }

        #endregion

        // ------------------------------------------------------------------
        // Scheduled events
        // ------------------------------------------------------------------

        #region Scheduled Events

        private class VfxLoopEvent : CallMethodEvent<RitualCenterpiece>
        {
            protected override CallbackDelegate GetCallback() => target => target.OnVfxLoopTick();
        }

        private class PurificationTickEvent : CallMethodEvent<RitualCenterpiece>
        {
            protected override CallbackDelegate GetCallback() => target => target.OnPurificationTick();
        }

        #endregion

        #endregion
    }
}
