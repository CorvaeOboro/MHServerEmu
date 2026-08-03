using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Entities.PlayableExpanded;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities.Avatars
{
    /// <summary>
    /// Avatar.Expanded - mod extensions for PlayableExpanded: NEW playable characters that
    /// borrow the assets of non-avatar entities (Team-Ups now; potentially bosses and other
    /// characters later). The real Team-Up companion system is deliberately left untouched.
    ///
    /// Kept as a partial class so mod functionality stays out of the vanilla Avatar.cs.
    /// Avatar.cs contains only two one-line hooks:
    ///   - ActivatePower -> <see cref="TryForwardExpandedPower"/>
    ///   - OnExitedWorld -> <see cref="OnExitedWorldExpanded"/>
    /// </summary>
    public partial class Avatar
    {
        // Created on demand; persists across swaps for the lifetime of the avatar entity.
        private PlayableExpandedController _playableExpandedController;

        /// <summary>The avatar's PlayableExpanded controller, or null if never used.</summary>
        public PlayableExpandedController PlayableExpanded { get => _playableExpandedController; }

        /// <summary>True while this avatar is playing as an expanded character.</summary>
        public bool IsPlayableExpandedActive { get => _playableExpandedController?.IsActive == true; }

        /// <summary>
        /// Starts playing as the given expanded character.
        /// Returns a user-facing status message.
        /// </summary>
        public string EnterPlayableExpanded(ExpandedCharacter character)
        {
            _playableExpandedController ??= new PlayableExpandedController(this);
            return _playableExpandedController.Enter(character);
        }

        /// <summary>
        /// Stops playing as an expanded character if active. Returns a user-facing status message.
        /// </summary>
        public string ExitPlayableExpanded(string reason = "player request")
        {
            if (_playableExpandedController == null)
                return "Not currently playing as an expanded character.";

            return _playableExpandedController.Exit(reason);
        }

        /// <summary>
        /// Power activation hook called from <see cref="ActivatePower(PrototypeId, ref PowerActivationSettings)"/>.
        /// Returns true when the activation was intercepted and handled by an expanded feature
        /// (currently: forwarding mapped PlayableExpanded powers to the body).
        /// </summary>
        private bool TryForwardExpandedPower(PrototypeId powerRef, ref PowerActivationSettings settings, out PowerUseResult result)
        {
            result = PowerUseResult.GenericError;

            if (_playableExpandedController == null || _playableExpandedController.IsActive == false)
                return false;

            return _playableExpandedController.TryForwardPower(powerRef, ref settings, out result);
        }

        /// <summary>
        /// World-exit hook called from <see cref="OnExitedWorld"/>.
        /// Pauses the controller for region transfer (despawns body, keeps state).
        /// Does nothing on death because <see cref="PlayableExpandedController.Think"/> handles respawn.
        /// </summary>
        private void OnExitedWorldExpanded()
        {
            if (_playableExpandedController != null && _playableExpandedController.IsActive)
                _playableExpandedController.OnAvatarExitedWorld();
        }

        /// <summary>
        /// World-enter hook called from <see cref="OnEnteredWorld"/>.
        /// Resumes playas mode after a region transfer, or auto-enters from persisted JSON on login.
        /// </summary>
        private void OnEnteredWorldExpanded()
        {
            if (_playableExpandedController != null && _playableExpandedController.IsActive)
            {
                // Resume after region transfer.
                _playableExpandedController.OnAvatarEnteredWorld();
                return;
            }

            // Auto-enter from persisted JSON (login / new game session).
            Player player = GetOwnerOfType<Player>();
            if (player == null) return;

            PlayableExpandedData data = PlayableExpandedStorage.Load(player.DatabaseUniqueId);
            if (string.IsNullOrWhiteSpace(data.CharacterRef)) return;

            ExpandedCharacter character = TryResolveExpandedCharacter(data.CharacterRef);
            if (character == null)
            {
                Logger.Warn($"[PlayableExpanded] Persisted character '{data.CharacterRef}' not found; clearing.");
                PlayableExpandedStorage.Clear(player.DatabaseUniqueId);
                return;
            }

            string result = EnterPlayableExpanded(character);
            Logger.Info($"[PlayableExpanded] Auto-entered from persisted data: {result}");
        }

        /// <summary>
        /// Resolves an expanded character by exact display name first, then falls back to
        /// <see cref="ExpandedCharacterRegistry.Resolve"/> for partial matches.
        /// </summary>
        private static ExpandedCharacter TryResolveExpandedCharacter(string name)
        {
            foreach (ExpandedCharacter c in ExpandedCharacterRegistry.DedicatedCharacters)
                if (c.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return c;

            var (resolved, error) = ExpandedCharacterRegistry.Resolve(name);
            if (error == null)
                return resolved;

            return null;
        }

        /// <summary>
        /// Collects the original progression powers currently slotted on the active ability bar
        /// (PrimaryAction through ActionKey5). When a slot holds a mapped power (e.g. a Rogue
        /// stolen power), the ORIGINAL power is returned and the existing mapping is recorded in
        /// <paramref name="previousMappings"/> so it can be restored later.
        /// </summary>
        internal List<PrototypeId> GetSlottedOriginalPowersExpanded(Dictionary<PrototypeId, PrototypeId> previousMappings)
        {
            List<PrototypeId> originals = new();

            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping == null)
                return originals;

            for (AbilitySlot slot = AbilitySlot.PrimaryAction; slot < AbilitySlot.NumActions; slot++)
            {
                PrototypeId abilityRef = keyMapping.GetAbilityInAbilitySlot(slot);
                if (abilityRef == PrototypeId.Invalid)
                    continue;

                // Slots can also hold items; only powers are swappable.
                if (GameDatabase.DataDirectory.PrototypeIsA<GameData.Prototypes.PowerPrototype>(abilityRef) == false)
                    continue;

                // Resolve mapped powers back to their original so MapPower() keys stay canonical.
                PrototypeId originalRef = GetOriginalPowerFromMappedPower(abilityRef);
                if (originalRef != PrototypeId.Invalid)
                {
                    if (previousMappings != null)
                        previousMappings[originalRef] = abilityRef;
                }
                else
                {
                    originalRef = abilityRef;
                }

                if (originals.Contains(originalRef) == false)
                    originals.Add(originalRef);
            }

            return originals;
        }

        /// <summary>
        /// Captures the current action-bar slot assignments so they can be restored exactly on exit.
        /// </summary>
        internal List<(AbilitySlot Slot, PrototypeId PowerRef)> CaptureKeyMappingExpanded()
        {
            List<(AbilitySlot, PrototypeId)> mapping = new();
            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping == null) return mapping;

            for (AbilitySlot slot = AbilitySlot.PrimaryAction; slot < AbilitySlot.NumActions; slot++)
                mapping.Add((slot, keyMapping.GetAbilityInAbilitySlot(slot)));

            return mapping;
        }

        /// <summary>
        /// Restores the exact slot assignments from a previously captured key mapping.
        /// </summary>
        internal void RestoreKeyMappingExpanded(List<(AbilitySlot Slot, PrototypeId PowerRef)> savedMapping)
        {
            if (savedMapping == null) return;
            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping == null) return;

            foreach ((AbilitySlot slot, PrototypeId powerRef) in savedMapping)
                keyMapping.SetAbilityInAbilitySlot(powerRef, slot);
        }

        /// <summary>
        /// Re-sends power assignments for whatever is currently in the avatar's key mapping.
        /// Called after an AOI recreate because EntityCreateMessage does not carry powers.
        /// Unassign + re-assign avoids the duplicate-assign check for non-combo powers.
        /// </summary>
        internal void ResyncCurrentPowersToClientExpanded()
        {
            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping == null) return;

            for (AbilitySlot slot = AbilitySlot.PrimaryAction; slot < AbilitySlot.NumActions; slot++)
            {
                PrototypeId abilityRef = keyMapping.GetAbilityInAbilitySlot(slot);
                if (abilityRef == PrototypeId.Invalid)
                    continue;

                if (GameDatabase.DataDirectory.PrototypeIsA<GameData.Prototypes.PowerPrototype>(abilityRef) == false)
                    continue;

                UnassignPower(abilityRef, true);
                PowerIndexProperties indexProps = new(1, CharacterLevel, CombatLevel);
                AssignPower(abilityRef, indexProps, true);
            }
        }

        /// <summary>
        /// Fills any empty slots in the key mapping with the hero's default powers.
        /// Preserves existing custom bindings; only touches slots that are currently Invalid.
        /// </summary>
        internal void FillEmptySlotsWithDefaultsExpanded()
        {
            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping == null) return;

            using var hotkeyDataListHandle = ListPool<HotkeyData>.Instance.Get(out List<HotkeyData> hotkeyDataList);
            if (keyMapping.GetDefaultAbilities(hotkeyDataList, this) == false)
                return;

            foreach (HotkeyData hotkeyData in hotkeyDataList)
            {
                PrototypeId current = keyMapping.GetAbilityInAbilitySlot(hotkeyData.AbilitySlot);
                if (current == PrototypeId.Invalid)
                    keyMapping.SetAbilityInAbilitySlot(hotkeyData.AbilityProtoRef, hotkeyData.AbilitySlot);
            }
        }

        /// <summary>
        /// Wrapper for UpdatePowerProgressionPowers so the controller can refresh progression.
        /// </summary>
        internal void UpdatePowerProgressionPowersExpanded(bool forceUnassign) => UpdatePowerProgressionPowers(forceUnassign);

        /// <summary>
        /// Nuclear option to fix a character whose powers have been permanently corrupted by
        /// previous buggy playas sessions. Unassigns all mappings, removes leftover forwarded
        /// Team-Up powers, refreshes progression, and re-slots the hero's default powers.
        /// </summary>
        public void RestoreMyPowersExpanded()
        {
            // Step 1: Remove all mapped power overrides
            UnassignAllMappedPowers();

            // Step 2: Clean up key mapping slots that hold non-progression powers
            AbilityKeyMapping keyMapping = _currentAbilityKeyMapping;
            if (keyMapping != null)
                keyMapping.CleanUpAfterRespec(this);

            // Step 3: Remove any non-progression powers that look like forwarded Team-Up powers
            // from previous playas sessions. We identify them by prototype path.
            if (PowerCollection != null)
            {
                List<PrototypeId> toRemove = new();
                foreach (var kvp in PowerCollection)
                {
                    PowerCollectionRecord record = kvp.Value;
                    if (record.IsPowerProgressionPower)
                        continue;

                    PrototypeId powerRef = kvp.Key;
                    string protoName = GameDatabase.GetPrototypeName(powerRef);
                    if (string.IsNullOrEmpty(protoName) || protoName.Contains("TeamUps") == false)
                        continue;

                    toRemove.Add(powerRef);
                }

                foreach (PrototypeId powerRef in toRemove)
                    UnassignPower(powerRef, true);
            }

            // Step 4: Refresh progression powers (assign missing, unassign invalid)
            UpdatePowerProgressionPowers(true);

            // Step 5: Fill empty slots with correct default powers for this avatar
            AutoSlotPowers();

            // Step 6: Re-sync to client so the UI reflects the fix
            ResyncCurrentPowersToClientExpanded();
        }
    }
}
