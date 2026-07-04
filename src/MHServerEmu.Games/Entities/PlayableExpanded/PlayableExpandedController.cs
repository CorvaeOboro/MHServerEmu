using Gazillion;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.Locomotion;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.Events;
using MHServerEmu.Games.Events.Templates;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.PlayableExpanded
{
    /// <summary>
    /// PlayableExpanded controller - lets the player play AS a new character concept
    /// (an <see cref="ExpandedCharacter"/>) that borrows the assets of a non-avatar entity
    /// such as a Team-Up.
    ///
    ///   - The avatar stays as the server-side "driver" (camera, movement, health, aggro).
    ///     It is hidden client-side via <see cref="EntitySettingsOptionFlags.IsClientEntityHidden"/>
    ///     so the player never sees the main character, while the client pawn still exists for
    ///     building occlusion / roof transparency, camera attachment, and movement.
    ///     The visible body is spawned on top.
    ///   - A brand-new body entity is spawned from the character's prototype (SpawnSpec,
    ///     Incursion pattern) - NOT the player's persistent Team-Up agent, so summoning the
    ///     same Team-Up as a pet at the same time works fine.
    ///   - The character's hotbar powers are mapped over the avatar's slotted powers
    ///     (Rogue mapped-power channel). The original powers are unassigned from the avatar's
    ///     PowerCollection and the mapped powers are assigned, so the hotbar AND the Power UI
    ///     (P key) show the expanded character's abilities. After the AOI destroy+recreate
    ///     used to hide the avatar, powers are re-synced to the client via
    ///     <see cref="ResyncPowersToClient"/>.
    ///   - Activations of mapped powers are intercepted in Avatar.ActivatePower and forwarded
    ///     to the body, which plays them on the correct rig.
    ///   - The body is position-synced to the avatar (follow + hard teleport on drift).
    ///
    /// One controller per avatar, created on demand. All access on the owning game's thread.
    /// </summary>
    public class PlayableExpandedController
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private readonly Avatar _avatar;

        private readonly EventGroup _events = new();
        private readonly EventPointer<ThinkEvent> _thinkEvent = new();

        // Original slotted power -> forwarded body power for the current swap.
        private readonly Dictionary<PrototypeId, PrototypeId> _mappedPowers = new();

        // Original slotted power -> the mapped power that occupied it BEFORE the swap
        // (e.g. Rogue stolen powers), so it can be restored on exit.
        private readonly Dictionary<PrototypeId, PrototypeId> _previousMappings = new();

        // Fast lookup for the activation hook.
        private readonly HashSet<PrototypeId> _forwardedPowers = new();

        // Grace-period tracking so we don't teleport while a power animation tail is still playing.
        private TimeSpan _powerEndGraceTime;
        private bool _avatarImmobilized;
        private Vector3 _lastAvatarPosition;
        private Orientation _lastAvatarOrientation;
        private bool _bodyWasMoving;
        private float _lastAvatarMoveSpeed;
        private bool _wasAvatarDead;

        // Snapshot of the hero's original key mapping before we entered playas mode.
        // Used to restore the exact slot assignments on exit, bypassing any incomplete
        // cleanup from UnassignMappedPower.
        private List<(AbilitySlot Slot, PrototypeId PowerRef)> _originalKeyMapping;

        private ExpandedCharacter _character;
        private ulong _bodyId;
        private ulong _spawnGroupId;

        public bool IsActive { get; private set; }
        public ExpandedCharacter Character { get => _character; }

        private Game Game { get => _avatar.Game; }
        private bool IsLoggingEnabled { get => Game?.CustomGameOptions?.PlayableExpandedLoggingEnable ?? false; }
        private float GlobalDamageScale { get => Game?.CustomGameOptions?.PlayableExpandedDamageScale ?? 1f; }

        public PlayableExpandedController(Avatar avatar)
        {
            _avatar = avatar;
        }

        /// <summary>Resolves the live body entity (it may have despawned).</summary>
        private Agent GetBody() => Game.EntityManager.GetEntity<Agent>(_bodyId);

        #region Enter / Exit

        /// <summary>
        /// Enters play-as mode for the given character. Returns a user-facing status message.
        ///
        /// Sequence:
        /// 1. Spawn a fresh body entity from the character's prototype.
        /// 2. Configure the body (alliance, untargetable, intangible, powers, damage scaling).
        /// 3. Cancel any active avatar power to prevent overlap with forwarded body powers.
        /// 4. Map the character's hotbar powers over the avatar's slotted powers via
        ///    <see cref="MapHotbarPowers"/>.
        /// 5. Hide the avatar client-side via an AOI destroy+recreate cycle with
        ///    <see cref="EntitySettingsOptionFlags.IsClientEntityHidden"/>.
        ///    The pawn still exists for occlusion queries but is not rendered.
        /// 6. Re-sync powers to the client after the recreate so the Power UI shows
        ///    the expanded character's abilities.
        /// </summary>
        public string Enter(ExpandedCharacter character)
        {
            if (Game?.CustomGameOptions?.PlayableExpandedEnable == false)
                return "PlayableExpanded is disabled in the server config (PlayableExpandedEnable).";

            if (_avatar.IsAliveInWorld == false)
                return "Your avatar must be alive and in the world to swap.";

            if (character == null || character.BodyProtoRef == PrototypeId.Invalid)
                return "Invalid expanded character.";

            // Re-entering with a different character swaps cleanly.
            if (IsActive)
                Exit("re-entering with a new character");

            // Spawn a fresh, independent body from the character's prototype.
            Agent body = SpawnBody(character, out string spawnError);
            if (body == null)
                return $"Failed to spawn the body: {spawnError}";

            _character = character;
            _bodyId = body.Id;

            SetupBody(character, body);

            character.OnSwapIn(_avatar, body);

            // Defensive: end any currently active power on the avatar so a continuous /
            // toggle / channel from the hero loadout doesn't keep firing in parallel
            // with the forwarded body powers.
            if (_avatar.ActivePower != null)
                _avatar.ActivePower.EndPower(EndPowerFlags.ExplicitCancel);

            // Remember the hero's original loadout so we can restore it exactly on exit.
            _originalKeyMapping = _avatar.CaptureKeyMappingExpanded();

            // Hotbar spoof: map the character's powers over the avatar's slotted powers.
            int mapped = MapHotbarPowers(character);
            if (mapped == 0)
            {
                DespawnBody();
                _character = null;
                return "No powers could be mapped onto your hotbar.";
            }

            IsActive = true;
            _lastAvatarPosition = _avatar.RegionLocation.Position;
            _lastAvatarOrientation = _avatar.RegionLocation.Orientation;
            _wasAvatarDead = _avatar.IsDead;

            // Hide the avatar client-side using IsClientEntityHidden.
            // This performs an AOI destroy+recreate: the client pawn is respawned with the
            // hidden flag so it is not rendered, but it still exists for occlusion queries
            // and camera attachment. Powers are re-synced afterward.
            HideAvatarClientSide();

            ScheduleNextThink();

            // Persist so the setting survives logout and region transfers.
            Player owner = _avatar.GetOwnerOfType<Player>();
            if (owner != null)
            {
                PlayableExpandedStorage.Save(owner.DatabaseUniqueId,
                    new PlayableExpandedData { CharacterRef = character.DisplayName });
            }

            if (IsLoggingEnabled)
                Logger.Info($"[PlayableExpanded] {_avatar.PrototypeName} now playing as '{character.DisplayName}' " +
                            $"(body id {body.Id}, {mapped} power(s) mapped).");

            return $"Now playing as {character.DisplayName} ({mapped} power(s) mapped). Use 'playas off' to swap back.";
        }

        /// <summary>
        /// Exits play-as mode and restores the avatar's loadout and visibility.
        /// Safe to call multiple times and with a despawned body.
        ///
        /// Sequence:
        /// 1. Cancel all scheduled think events.
        /// 2. Remove the Immobilized property if it was set during a power cast.
        /// 3. Unassign each mapped power via <see cref="Avatar.UnassignMappedPower"/>,
        ///    which restores the original power to its hotbar slot and updates its rank.
        /// 4. Remove any forwarded powers that remain in the avatar's PowerCollection.
        /// 5. Restore any pre-existing mappings (e.g. Rogue stolen powers).
        /// 6. Show the avatar client-side via an AOI destroy+recreate WITHOUT the hidden flag.
        /// 7. Despawn the body entity and clean up the spawn group.
        /// </summary>
        public string Exit(string reason)
        {
            if (IsActive == false)
                return "Not currently playing as an expanded character.";

            IsActive = false;
            _powerEndGraceTime = TimeSpan.Zero;

            // Clear persisted setting.
            Player owner = _avatar.GetOwnerOfType<Player>();
            if (owner != null)
                PlayableExpandedStorage.Clear(owner.DatabaseUniqueId);

            if (_avatarImmobilized)
            {
                _avatar.Properties.RemoveProperty(PropertyEnum.Immobilized);
                _avatarImmobilized = false;
            }

            Game?.GameEventScheduler?.CancelAllEvents(_events);

            Agent body = GetBody();
            if (body != null)
                _character?.OnSwapOut(_avatar, body);

            // Defensive: Unassign ALL mapped powers via the avatar's built-in method.
            // This catches any stale mappings that our _mappedPowers tracking might have
            // missed (e.g. when switching characters without !playas off in between).
            _avatar.UnassignAllMappedPowers();

            // Remove any forwarded powers that might still be in the collection.
            foreach (PrototypeId bodyPowerRef in _forwardedPowers)
            {
                if (_avatar.GetPower(bodyPowerRef) != null)
                    _avatar.UnassignPower(bodyPowerRef);
            }

            // Restore the exact original key mapping from before we entered playas mode.
            // This bypasses any incomplete slot restoration from UnassignMappedPower.
            _avatar.RestoreKeyMappingExpanded(_originalKeyMapping);
            _originalKeyMapping = null;

            // Ensure all progression powers are assigned (they may have been removed by
            // previous playas sessions) and fill any empty slots with default powers.
            _avatar.UpdatePowerProgressionPowersExpanded(true);
            _avatar.FillEmptySlotsWithDefaultsExpanded();

            _mappedPowers.Clear();
            _previousMappings.Clear();
            _forwardedPowers.Clear();

            // Restore avatar visibility via AOI destroy+recreate without IsClientEntityHidden.
            ShowAvatarClientSide();

            DespawnBody();

            string characterName = _character?.DisplayName ?? "(unknown)";
            _character = null;

            if (IsLoggingEnabled)
                Logger.Info($"[PlayableExpanded] {_avatar.PrototypeName} swapped back from '{characterName}' ({reason}).");

            return "Swapped back to your hero.";
        }

        /// <summary>
        /// Called when the avatar exits the world for a region transfer (NOT death or logout).
        /// Despawns the body so it doesn't stay behind in the old region, but preserves
        /// active state, mapped powers, and avatar visibility so playas mode resumes on re-entry.
        /// </summary>
        public void OnAvatarExitedWorld()
        {
            if (IsActive == false) return;

            if (IsLoggingEnabled)
                Logger.Info("[PlayableExpanded] Avatar exited world for region transfer - pausing.");

            // Stop the think loop while out of world.
            Game?.GameEventScheduler?.CancelAllEvents(_events);

            // Despawn the body so it doesn't zombie in the old region.
            DespawnBody();
        }

        /// <summary>
        /// Called when the avatar enters a new world after a region transfer.
        /// Respawns the body at the new position and re-hides the avatar client-side.
        /// </summary>
        public void OnAvatarEnteredWorld()
        {
            if (IsActive == false) return;
            if (_character == null) return;

            if (IsLoggingEnabled)
                Logger.Info("[PlayableExpanded] Avatar entered new world - resuming.");

            // Spawn a fresh body at the new location.
            Agent body = SpawnBody(_character, out string spawnError);
            if (body == null)
            {
                Logger.Warn($"[PlayableExpanded] Failed to respawn body after region transfer: {spawnError}");
                Exit("body respawn failed after region transfer");
                return;
            }

            _bodyId = body.Id;
            SetupBody(_character, body);

            // Re-hide the avatar in case the client recreated it without IsClientEntityHidden.
            HideAvatarClientSide();

            ScheduleNextThink();
        }

        #endregion

        #region Body Spawn / Despawn

        /// <summary>
        /// Spawns a fresh body entity from the character's prototype at the avatar's position
        /// (SpawnSpec flow, same as Incursion combat bodies).
        /// </summary>
        private Agent SpawnBody(ExpandedCharacter character, out string error)
        {
            error = null;

            Region region = _avatar.Region;
            if (region == null)
            {
                error = "no region";
                return null;
            }

            Vector3 spawnPosition = RegionLocation.ProjectToFloor(region, _avatar.RegionLocation.Position);

            var manager = region.PopulationManager;
            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(spawnPosition, _avatar.RegionLocation.Orientation);
            group.SpawnCleanup = false;   // keep the body alive outside of normal encounter flow

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = character.BodyProtoRef;
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // The body fights at the avatar's level.
            spec.Properties[PropertyEnum.CharacterLevel] = _avatar.CharacterLevel;
            spec.Properties[PropertyEnum.CombatLevel] = _avatar.CombatLevel;

            spec.Spawn();

            if (spec.ActiveEntity is not Agent body)
            {
                manager.RemoveSpawnGroup(group.Id);
                error = $"spawn failed for {character.BodyProtoRef.GetName()}";
                return null;
            }

            _spawnGroupId = group.Id;

            if (IsLoggingEnabled)
                Logger.Info($"[PlayableExpanded] Spawned body '{body.PrototypeName}' (id {body.Id}) at " +
                            $"{spawnPosition.ToStringNames()} level {_avatar.CharacterLevel}.");

            return body;
        }

        /// <summary>Removes the body entity and its spawn group.</summary>
        private void DespawnBody()
        {
            Agent body = GetBody();
            Region region = body?.Region ?? _avatar.Region;

            body?.SpawnSpec?.Destroy();

            // The spec normally schedules entity destruction; make sure it is gone.
            if (body != null && body.IsDestroyed == false)
                body.Destroy();

            if (_spawnGroupId != 0)
                region?.PopulationManager?.RemoveSpawnGroup(_spawnGroupId);

            _bodyId = 0;
            _spawnGroupId = 0;
        }

        /// <summary>
        /// Turns the freshly spawned entity into a player-driven body.
        ///
        /// Configuration applied:
        /// - Disable AI controller (the controller drives movement directly).
        /// - Set alliance and PowerUserOverrideID so kills/loot/XP credit goes to the player.
        /// - Mark Untargetable so enemy aggro and the reticle stay on the avatar.
        /// - Mark Intangible so the body has no collision and does not block projectiles;
        ///   the avatar takes all damage.
        /// - Assign the character's hotbar powers to the body with per-power damage scaling.
        /// </summary>
        private void SetupBody(ExpandedCharacter character, Agent body)
        {
            // The controller is the sole driver of the body.
            body.AIController?.SetIsEnabled(false);

            // Fight for the player's side and route kill/XP/loot credit to the player
            // (PowerUserOverrideID is the same mechanism real pets use).
            body.SetSummonedAllianceOverride(_avatar.Alliance);
            body.Properties[PropertyEnum.PowerUserOverrideID] = _avatar.Id;

            // Keep enemy aggro and the player's reticle on the (coincident) avatar.
            body.Properties[PropertyEnum.Untargetable] = true;

            // Make the body purely visual - no collision, no projectile blocking,
            // no hit detection. The avatar takes all damage.
            body.Properties[PropertyEnum.Intangible] = true;
            body.Properties[PropertyEnum.Invulnerable] = true;   // prevent death from AoE / DoT / self-damage

            // Assign the hotbar powers up front and apply per-power damage tuning.
            PowerIndexProperties indexProps = new(1, body.CharacterLevel, body.CombatLevel);
            foreach (ExpandedPowerEntry entry in character.GetHotbarPowers())
            {
                if (body.GetPower(entry.PowerRef) == null && body.AssignPower(entry.PowerRef, indexProps) == null)
                {
                    Logger.Warn($"[PlayableExpanded] Failed to assign '{entry.PowerRef.GetName()}' to the body.");
                    continue;
                }

                float scale = entry.DamageScale * GlobalDamageScale;
                if (scale > 0f && scale != 1f)
                    body.Properties[PropertyEnum.DamageMultForPower, entry.PowerRef] = scale - 1f;
            }

            // Position sync is handled by the think loop.
        }

        #endregion

        #region Power Mapping

        /// <summary>
        /// Maps the character's hotbar powers over the avatar's currently slotted progression powers.
        /// Uses <see cref="Avatar.MapPower"/> which handles both hotbar slotting and rank updates.
        /// Returns the number of powers successfully mapped.
        ///
        /// Note: The caller (Enter) performs an AOI destroy+recreate to hide the avatar client-side.
        /// Because EntityCreateMessage does not carry the power collection, mapped powers must be
        /// re-synced to the client afterward via <see cref="ResyncPowersToClient"/>.
        /// </summary>
        private int MapHotbarPowers(ExpandedCharacter character)
        {
            List<ExpandedPowerEntry> hotbarPowers = character.GetHotbarPowers();
            if (hotbarPowers.Count == 0)
                return 0;

            List<PrototypeId> slottedOriginals = _avatar.GetSlottedOriginalPowersExpanded(_previousMappings);

            int count = Math.Min(hotbarPowers.Count, slottedOriginals.Count);
            int mapped = 0;

            for (int i = 0; i < count; i++)
            {
                PrototypeId originalRef = slottedOriginals[i];
                PrototypeId bodyPowerRef = hotbarPowers[i].PowerRef;

                if (_avatar.MapPower(originalRef, bodyPowerRef) == false)
                {
                    Logger.Warn($"[PlayableExpanded] Failed to map '{bodyPowerRef.GetName()}' over '{originalRef.GetName()}'.");
                    continue;
                }

                _mappedPowers[originalRef] = bodyPowerRef;
                _forwardedPowers.Add(bodyPowerRef);
                mapped++;

                if (IsLoggingEnabled)
                    Logger.Info($"[PlayableExpanded] Mapped '{GameDatabase.GetPrototypeName(bodyPowerRef)}' " +
                                $"over '{GameDatabase.GetPrototypeName(originalRef)}'.");
            }

            return mapped;
        }

        #endregion

        #region Power Forwarding

        /// <summary>
        /// Forwards a mapped power activation from the avatar to the body.
        /// Returns true when the activation was handled (the avatar must not execute it).
        /// Called from Avatar.ActivatePower via Avatar.Expanded.cs.
        ///
        /// When successful:
        /// - The power is activated on the body at the body's position.
        /// - The avatar's copy of the power starts cooldown so the client UI shows the correct timer.
        /// - The avatar is immobilized for the cast duration so the body can play its animation
        ///   uninterrupted (the think loop skips teleport during this grace period).
        /// </summary>
        public bool TryForwardPower(PrototypeId powerRef, ref PowerActivationSettings settings, out PowerUseResult result)
        {
            result = PowerUseResult.GenericError;

            if (IsActive == false || _forwardedPowers.Contains(powerRef) == false)
                return false;

            Agent body = GetBody();
            if (body == null || body.IsAliveInWorld == false)
            {
                // The body is gone (death, region transfer edge case) - self-heal by swapping back.
                Exit("body lost");
                return false;
            }

            Power bodyPower = body.GetPower(powerRef);
            if (bodyPower == null)
            {
                result = PowerUseResult.AbilityMissing;
                return true;
            }

            // Activate from the body's actual position, at the avatar's target.
            PowerActivationSettings bodySettings = settings;
            bodySettings.UserPosition = body.RegionLocation.Position;

            // Stop movement during non-movement powers so the cast doesn't slide.
            if (bodyPower.IsPartOfAMovementPower() == false)
                body.Locomotor?.Stop();

            result = body.ActivatePower(powerRef, ref bodySettings);

            if (result == PowerUseResult.Success)
            {
                // The client does local prediction when sending NetMessageTryActivatePower for the
                // avatar. Since we forwarded the activation to the body, the client still has an
                // ongoing predicted power on the hidden avatar. Send an explicit cancel so only
                // the body's NetMessageActivatePower remains visible.
                Player owner = _avatar.GetOwnerOfType<Player>();
                if (owner != null)
                {
                    ulong powerPrototypeEnum = (ulong)DataDirectory.Instance.GetPrototypeEnumValue<PowerPrototype>(powerRef);
                    var cancelPowerMessage = NetMessageCancelPower.CreateBuilder()
                        .SetIdAgent(_avatar.Id)
                        .SetPowerPrototypeId(powerPrototypeEnum)
                        .SetEndPowerFlags((uint)EndPowerFlags.ExplicitCancel)
                        .Build();
                    owner.SendMessage(cancelPowerMessage);
                }

                // Drive the hotbar cooldown on the avatar's copy so the client UI stays accurate.
                Power avatarPower = _avatar.GetPower(powerRef);
                avatarPower?.StartCooldown();

                // Resolve cast time: explicit per-power table entry, or prototype animation time, or 500 ms fallback.
                int castTimeMs = 500;
                if (_character != null)
                {
                    foreach (ExpandedPowerEntry entry in _character.GetHotbarPowers())
                    {
                        if (entry.PowerRef == powerRef)
                        {
                            castTimeMs = entry.CastTimeMs > 0 ? entry.CastTimeMs : 500;
                            break;
                        }
                    }
                }
                else
                {
                    PowerPrototype proto = GameDatabase.GetPrototype<PowerPrototype>(powerRef);
                    if (proto?.AnimationTimeMS > 0)
                        castTimeMs = proto.AnimationTimeMS;
                }

                // Freeze the avatar so the player can't move during the cast.
                // The body then plays its animation uninterrupted without being teleported.
                _avatar.Properties[PropertyEnum.Immobilized] = true;
                _avatarImmobilized = true;
                _powerEndGraceTime = (Game?.CurrentTime ?? TimeSpan.Zero) + TimeSpan.FromMilliseconds(castTimeMs);

                _character?.OnPowerForwarded(_avatar, body, powerRef);
            }
            else if (IsLoggingEnabled)
            {
                Logger.Info($"[PlayableExpanded] Body activation of '{GameDatabase.GetPrototypeName(powerRef)}' failed: {result}.");
            }

            return true;
        }

        #endregion

        #region Think Loop / Position Sync

        private void ScheduleNextThink()
        {
            if (IsActive == false) return;

            var scheduler = Game?.GameEventScheduler;
            if (scheduler == null) return;
            if (_thinkEvent.IsValid) return;

            int intervalMs = Math.Max(8, _character?.ThinkIntervalMs ?? 20);
            scheduler.ScheduleEvent(_thinkEvent, TimeSpan.FromMilliseconds(intervalMs), _events);
            _thinkEvent.Get().Initialize(this);
        }

        private void Think()
        {
            if (IsActive == false)
                return;

            // Avatar out of world - death respawn cycle or region transfer.
            // OnAvatarExitedWorld() handles region-transfer cleanup; we just wait here.
            if (_avatar.IsInWorld == false)
            {
                ScheduleNextThink();
                return;
            }

            // Avatar respawned after death - the client may have recreated the entity
            // without IsClientEntityHidden, so the original avatar becomes visible.
            // Re-apply the hidden flag and snap the body to the new respawn location.
            bool isAvatarDead = _avatar.IsDead;
            if (_wasAvatarDead && isAvatarDead == false)
            {
                if (IsLoggingEnabled)
                    Logger.Info("[PlayableExpanded] Avatar respawned while in playas mode - re-hiding client-side.");
                HideAvatarClientSide();

                Agent respawnedBody = GetBody();
                if (respawnedBody != null && respawnedBody.IsAliveInWorld)
                {
                    respawnedBody.ChangeRegionPosition(_avatar.RegionLocation.Position, _avatar.RegionLocation.Orientation,
                        ChangePositionFlags.Teleport | ChangePositionFlags.PhysicsResolve);
                }
            }
            _wasAvatarDead = isAvatarDead;

            // Skip sync while dead; the body will idle until respawn.
            if (isAvatarDead)
            {
                ScheduleNextThink();
                return;
            }

            Agent body = GetBody();
            if (body == null || body.IsAliveInWorld == false)
            {
                Exit("body despawned");
                return;
            }

            SyncBodyToAvatar(body);
            ScheduleNextThink();
        }

        /// <summary>
        /// Direct-position syncing with animation safety.
        ///
        /// Behavior:
        /// - Never sync while a power is executing (let the full animation play).
        /// - After a power ends, wait for the cast duration before syncing so the animation tail
        ///   can finish and the body transitions back to idle.
        /// - Orientation is always matched to the avatar so the body turns instantly.
        /// - <see cref="Locomotor.MoveForward"/> is called only on movement start, direction change
        ///   (>17 degrees yaw delta), or significant speed change (>20%). Calling it every tick
        ///   causes client-side animation jitter via internal ResetState() calls.
        /// - Small drift (2-50 units) is corrected with <see cref="ChangePositionFlags.Teleport"/>
        ///   + <see cref="ChangePositionFlags.PhysicsResolve"/> for smooth interpolation.
        /// - Large drift (>50 units, e.g. dash / waypoint) uses a hard teleport without stopping
        ///   the locomotor so running can resume immediately.
        /// - Only call <see cref="Locomotor.Stop"/> on the transition from moving to idle.
        /// </summary>
        private void SyncBodyToAvatar(Agent body)
        {
            if (body.IsInWorld == false || _avatar.IsInWorld == false)
                return;

            // Power is currently active -> let animation play uninterrupted.
            if (body.IsExecutingPower)
                return;

            // Grace period after the last power ended -> wait for animation tail.
            TimeSpan now = Game?.CurrentTime ?? TimeSpan.Zero;
            if (now < _powerEndGraceTime)
                return;

            // Grace just expired and we had immobilized the avatar - free it now.
            if (_avatarImmobilized)
            {
                _avatar.Properties.RemoveProperty(PropertyEnum.Immobilized);
                _avatarImmobilized = false;
            }

            Vector3 avatarPos = _avatar.RegionLocation.Position;
            Vector3 bodyPos   = body.RegionLocation.Position;

            // Track avatar movement so we know when to run vs idle.
            float avatarMoveDist = Vector3.Distance2D(avatarPos, _lastAvatarPosition);
            _lastAvatarPosition = avatarPos;

            Locomotor bodyLoco = body.Locomotor;
            if (bodyLoco == null) return;

            float distance = Vector3.Distance2D(avatarPos, bodyPos);
            const float TeleportThreshold = 50.0f;
            if (distance > TeleportThreshold)
            {
                // Hard teleport for extreme drift (dash / waypoint) - don't Stop()
                // so the locomotor can resume running immediately afterwards.
                body.ChangeRegionPosition(avatarPos, _avatar.RegionLocation.Orientation);
            }

            const float MoveEpsilon = 0.25f;
            bool avatarIsMoving = avatarMoveDist > MoveEpsilon;
            float avatarSpeed = avatarIsMoving ? avatarMoveDist / 0.05f : 0f;

            if (avatarIsMoving)
            {
                // Always match orientation so the body turns with the avatar.
                body.RegionLocation.SetOrientation(_avatar.RegionLocation.Orientation);

                // Only call MoveForward on transition, direction change, or when speed
                // changed significantly. Calling it every tick calls ResetState() internally,
                // which causes client-side animation jitter by re-emitting locomotion start events.
                Orientation currOrient = _avatar.RegionLocation.Orientation;
                float yawDelta = Math.Abs(Orientation.WrapAngleRadians(currOrient.Yaw - _lastAvatarOrientation.Yaw));
                bool orientChanged = yawDelta > 0.15f; // ~8.6 degrees
                bool speedChanged = Math.Abs(avatarSpeed - _lastAvatarMoveSpeed) > _lastAvatarMoveSpeed * 0.20f;
                if (_bodyWasMoving == false || orientChanged || speedChanged)
                {
                    LocomotionOptions opts = new() { BaseMoveSpeed = avatarSpeed };
                    bodyLoco.MoveForward(ref opts);
                }

                _bodyWasMoving = true;
                _lastAvatarMoveSpeed = avatarSpeed;
                _lastAvatarOrientation = currOrient;

                // Tiny position correction - more frequent & smaller is less visible.
                const float SnapThreshold = 2.0f;
                if (distance > SnapThreshold)
                    body.ChangeRegionPosition(avatarPos, _avatar.RegionLocation.Orientation,
                        ChangePositionFlags.Teleport | ChangePositionFlags.PhysicsResolve);
            }
            else
            {
                // Only stop on transition - avoids redundant state sync spam.
                if (_bodyWasMoving)
                    bodyLoco.Stop();

                _bodyWasMoving = false;
                _lastAvatarMoveSpeed = 0f;
            }
        }

        #endregion

        #region Avatar Visibility

        /// <summary>
        /// Hides the avatar on the client by forcing an AOI destroy+recreate cycle
        /// with <see cref="EntitySettingsOptionFlags.IsClientEntityHidden"/>.
        ///
        /// Unlike <see cref="PropertyEnum.Visible"/> = false, the client pawn still
        /// exists for occlusion queries, camera attachment, and movement - it is simply
        /// not added to the visible render scene.
        ///
        /// Sequence:
        /// 1. Set <see cref="PropertyEnum.RestrictedToPlayerGuidParty"/> to a non-existent
        ///    party GUID so the owner's AOI drops the avatar (sends NetMessageEntityDestroy).
        /// 2. Clear the restriction so the avatar is eligible again.
        /// 3. Re-add the avatar with <see cref="EntitySettingsOptionFlags.IsClientEntityHidden"/>.
        ///    The client spawns the pawn but skips rendering.
        /// 4. Re-sync powers via <see cref="ResyncPowersToClient"/> because
        ///    EntityCreateMessage does not carry the power collection.
        /// </summary>
        private void HideAvatarClientSide()
        {
            Player owner = _avatar.GetOwnerOfType<Player>();
            if (owner == null) return;

            // Step 1: temporarily restrict the avatar to a non-existent party
            // so the owner's AOI drops it (sends NetMessageEntityDestroy).
            _avatar.Properties[PropertyEnum.RestrictedToPlayerGuidParty] = 1;
            owner.AOI.ConsiderEntity(_avatar);

            // Step 2: clear the restriction so the avatar is eligible again.
            _avatar.Properties.RemoveProperty(PropertyEnum.RestrictedToPlayerGuidParty);

            // Step 3: re-add the avatar with IsClientEntityHidden.
            // The client receives BuildEntityCreateMessage with the hidden flag,
            // spawns the pawn, but skips rendering.
            using EntitySettings settings = ObjectPoolManager.Instance.Get<EntitySettings>();
            settings.OptionFlags = EntitySettingsOptionFlags.IsClientEntityHidden;
            owner.AOI.ConsiderEntity(_avatar, settings);

            // Step 4: re-sync powers. The entity destroy+recreate wiped the client's
            // knowledge of power assignments (EntityCreateMessage doesn't carry them).
            // We must re-send AssignPower messages so Jubilee's powers appear in the Power UI.
            ResyncPowersToClient();
        }

        /// <summary>
        /// Restores the avatar visibility by forcing another AOI destroy+recreate
        /// cycle, this time WITHOUT the hidden flag.
        ///
        /// Sequence:
        /// 1. Set <see cref="PropertyEnum.RestrictedToPlayerGuidParty"/> to drop the avatar.
        /// 2. Clear the restriction.
        /// 3. Re-add the avatar with default settings (no hidden flag).
        ///
        /// Powers do not need re-sync here because <see cref="Exit"/> has already restored
        /// the original powers to the avatar's PowerCollection before calling this method.
        /// </summary>
        private void ShowAvatarClientSide()
        {
            Player owner = _avatar.GetOwnerOfType<Player>();
            if (owner == null) return;

            _avatar.Properties[PropertyEnum.RestrictedToPlayerGuidParty] = 1;
            owner.AOI.ConsiderEntity(_avatar);

            _avatar.Properties.RemoveProperty(PropertyEnum.RestrictedToPlayerGuidParty);

            using EntitySettings settings = ObjectPoolManager.Instance.Get<EntitySettings>();
            owner.AOI.ConsiderEntity(_avatar, settings);

            // After the AOI recreate the client has no powers. Re-sync whatever is
            // currently slotted so the Power UI and hotbar are restored.
            _avatar.ResyncCurrentPowersToClientExpanded();
        }

        /// <summary>
        /// Re-sends power assignments to the client after an AOI destroy+recreate.
        ///
        /// <see cref="ArchiveMessageBuilder.BuildEntityCreateMessage"/> does NOT serialize
        /// the entity's PowerCollection, so the client forgets all assigned powers after a refresh.
        /// This method sends explicit <see cref="NetMessageUnassignPower"/> for each original
        /// power and <see cref="NetMessageAssignPower"/> for each mapped power, ensuring
        /// the Power UI (P key) shows only the expanded character's abilities.
        ///
        /// Called from <see cref="HideAvatarClientSide"/> after the entity recreate.
        /// </summary>
        private void ResyncPowersToClient()
        {
            foreach (var kvp in _mappedPowers)
            {
                PrototypeId originalRef = kvp.Key;
                PrototypeId mappedRef = kvp.Value;

                // Remove original so it doesn't appear in the Power UI.
                _avatar.UnassignPower(originalRef, true);

                // Assign the mapped power so it appears in the Power UI.
                PowerIndexProperties indexProps = new(1, _avatar.CharacterLevel, _avatar.CombatLevel);
                _avatar.AssignPower(mappedRef, indexProps, true);
            }
        }

        #endregion

        #region Scheduled Event

        private class ThinkEvent : CallMethodEvent<PlayableExpandedController>
        {
            protected override CallbackDelegate GetCallback() => (controller) => controller.Think();
        }

        #endregion
    }
}
