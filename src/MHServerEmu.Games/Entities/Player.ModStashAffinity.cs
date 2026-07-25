#region Stash Affinity
// =============================================================================
// MOD Stash Affinity
// =============================================================================
//   items sort into named stash tabs
//
//   vanilla: affinity for Character-specific stash for bound or avatar-restricted items.
//   MOD: Type-based affinity for other items such as gear and crafting materials.
//
//   Supported stash tab affinity names:
//     Ring                     | ring, rings
//     Artifact                 | artifact, artifacts
//     Rune                     | rune, runes
//     Relic                    | relic, relics
//     Any-Hero Unique          | unique, uniques
//       Gear01/Slot1           | slot1, one, gear1, weapon, unique1
//       Gear02/Slot2           | slot2, two, gear2, body, unique2
//       Gear03/Slot3           | slot3, three, gear3, belt, unique3
//       Gear04/Slot4           | slot4, four, gear4, boots, foot, unique4
//       Gear05/Slot5           | slot5, five, gear5, head, unique5
//     Medal                    | medal, medals, medallion, medallions
//     Insignia                 | insignia
//     Catalyst                 | catalyst, core
//     Team-Up Gear             | teamup, team-up
//     Crafting Ingredients     | crafting, craft
//     Danger Room Scenarios    | maps, danger, dangerroom, scenario
//
//   Applied automatically during inventory moves to stash from non-stash sources.
//   Falls back to the originally requested stash tab when no affinity matches.
//
//  VERSION:: 20260713
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Logging;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.Entities.Items;
using MHServerEmu.Games.Entities.Options;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Loot;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private static readonly Logger ModStashAffinityLogger = LogManager.CreateLogger();

        #region Resolution

        /// <summary>
        /// Resolves the best stash tab for an item based on affinity rules.
        /// Returns <paramref name="requestedStashRef"/> if no better match is found.
        /// </summary>
        private PrototypeId ResolveModStashAffinity(Item item, PrototypeId requestedStashRef)
        {
            bool loggingEnabled = Game.CustomGameOptions.ModStashAffinityLoggingEnable;
            StringBuilder report = loggingEnabled ? new StringBuilder() : null;

            AppendHeader(report, item, requestedStashRef);

            if (Game.CustomGameOptions.ModStashAffinityEnable == false)
            {
                AppendDecision(report, requestedStashRef, "feature disabled");
                FlushReport(report);
                return requestedStashRef;
            }

            // Only apply when the destination is a player stash and the item is coming from a non-stash inventory
            if (Inventory.IsPlayerStashInventory(requestedStashRef) == false)
            {
                AppendDecision(report, requestedStashRef, "requested destination is not a player stash");
                FlushReport(report);
                return requestedStashRef;
            }

            InventoryPrototype sourceInvProto = item.InventoryLocation.InventoryPrototype;
            if (sourceInvProto != null && sourceInvProto.IsPlayerStashInventory)
            {
                AppendDecision(report, requestedStashRef, "source is already a stash");
                FlushReport(report);
                return requestedStashRef;
            }

            AppendAvailableStashes(report);

            // First: character-specific stash for bound / avatar-restricted items
            PrototypeId characterStashRef = ResolveCharacterModStashAffinity(item, requestedStashRef, report, out bool requestedIsCharacterStash);

            // If the player already opened the correct character stash, keep it and do not override with type affinity
            if (requestedIsCharacterStash)
            {
                AppendDecision(report, requestedStashRef, "keeping requested character stash");
                FlushReport(report);
                return requestedStashRef;
            }

            if (characterStashRef != requestedStashRef)
            {
                LogAffinity(item, requestedStashRef, characterStashRef, "character-specific");
                FlushReport(report);
                return characterStashRef;
            }

            // Then: slot-based affinity for any-hero uniques (e.g. slot1-slot5)
            PrototypeId slotStashRef = ResolveSlotModStashAffinity(item, requestedStashRef, report);
            if (slotStashRef != requestedStashRef)
            {
                string slotLabel = TryGetUniqueGearSlotNumber(item, out int slotNum) ? $"unique-slot{slotNum}" : "unique-slot";
                LogAffinity(item, requestedStashRef, slotStashRef, slotLabel);
                FlushReport(report);
                return slotStashRef;
            }

            // Then: type-based affinity (applies to any-hero uniques too, but only if no character stash matched)
            PrototypeId typeStashRef = ResolveTypeModStashAffinity(item, requestedStashRef, report);
            if (typeStashRef != requestedStashRef)
            {
                LogAffinity(item, requestedStashRef, typeStashRef, string.Join("/", GetModStashAffinityKeys(item)));
                FlushReport(report);
                return typeStashRef;
            }

            AppendDecision(report, requestedStashRef, "no affinity match found; falling back to requested stash");
            FlushReport(report);
            return requestedStashRef;
        }

        #endregion

        #region Character Affinity

        /// <summary>
        /// Returns the prototype id of the character this item is intended for, or Invalid if it is any-hero gear.
        /// Checks binding, avatar restriction, and finally the item prototype path/name for avatar names.
        /// </summary>
        private static PrototypeId GetItemCharacterProtoRef(Item item)
        {
            if (item.IsBoundToCharacter)
                return item.BoundAgentProtoRef;

            if (item.ItemPrototype?.IsAvatarRestricted == true)
                return item.ItemSpec?.EquippableBy ?? PrototypeId.Invalid;

            return PrototypeId.Invalid;
        }

        /// <summary>
        /// Returns the character-specific stash for an item bound/restricted to a character, if one exists and has space.
        /// </summary>
        private PrototypeId ResolveCharacterModStashAffinity(Item item, PrototypeId requestedStashRef, StringBuilder report, out bool requestedIsCharacterStash)
        {
            requestedIsCharacterStash = false;

            PrototypeId targetAvatarRef = GetItemCharacterProtoRef(item);
            report?.AppendLine($"  CharacterTarget: {GameDatabase.GetPrototypeName(targetAvatarRef)} (IsBound={item.IsBoundToCharacter}, BoundAgent={GameDatabase.GetPrototypeName(item.BoundAgentProtoRef)}, IsAvatarRestricted={item.ItemPrototype?.IsAvatarRestricted}, EquippableBy={GameDatabase.GetPrototypeName(item.ItemSpec?.EquippableBy ?? PrototypeId.Invalid)})");

            // Fallback: try to infer the character from the item prototype path/name
            if (targetAvatarRef == PrototypeId.Invalid)
            {
                targetAvatarRef = DetectCharacterAvatarFromItemPath(item, report);
                report?.AppendLine($"  PathCharacterTarget: {GameDatabase.GetPrototypeName(targetAvatarRef)}");
            }

            if (targetAvatarRef == PrototypeId.Invalid)
            {
                report?.AppendLine("  CharacterStash: no character target for this item");
                return requestedStashRef;
            }

            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true) == false)
            {
                report?.AppendLine("  CharacterStash: failed to retrieve stash list");
                return requestedStashRef;
            }

            foreach (PrototypeId stashRef in stashRefs)
            {
                PlayerStashInventoryPrototype stashProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(stashRef);
                if (stashProto == null || stashProto.ForAvatar != targetAvatarRef)
                    continue;

                Inventory stashInv = GetInventoryByRef(stashRef);
                string stashName = GetStashDisplayName(stashRef);

                // If the player already opened the correct character stash, mark it and keep it
                if (stashRef == requestedStashRef)
                {
                    requestedIsCharacterStash = true;
                    AppendDecision(report, requestedStashRef, $"requested stash is the character stash for {GameDatabase.GetPrototypeName(targetAvatarRef)}");
                    return requestedStashRef;
                }

                if (stashInv == null)
                {
                    report?.AppendLine($"  CharacterStash: {stashName} is locked or not instantiated");
                    continue;
                }

                if (stashInv.Prototype.AllowEntity(item.Prototype) == false)
                {
                    report?.AppendLine($"  CharacterStash: {stashName} does not allow this item type");
                    continue;
                }

                uint freeSlot = stashInv.GetFreeSlot(item, true, true);
                if (freeSlot != Inventory.InvalidSlot)
                {
                    AppendDecision(report, stashRef, $"found character stash '{stashName}' with space");
                    return stashRef;
                }
                else
                {
                    report?.AppendLine($"  CharacterStash: {stashName} is full");
                }
            }

            AppendDecision(report, requestedStashRef, $"no available character stash for {GameDatabase.GetPrototypeName(targetAvatarRef)}; will consider type affinity");
            return requestedStashRef;
        }

        /// <summary>
        /// Looks at the item prototype path/name and tries to match it against the names of avatars that have unlocked stashes.
        /// This catches unbound character-specific uniques like "Entity/Items/Armor/UniquePrototypes/Avatars/Rogue/Unique335.prototype".
        /// </summary>
        private PrototypeId DetectCharacterAvatarFromItemPath(Item item, StringBuilder report)
        {
            ItemPrototype itemProto = item.ItemPrototype;
            if (itemProto == null) return PrototypeId.Invalid;

            string itemPath = GameDatabase.GetPrototypeName(itemProto.DataRef);
            if (string.IsNullOrEmpty(itemPath)) return PrototypeId.Invalid;

            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true) == false)
                return PrototypeId.Invalid;

            // Prefer longer names first to avoid partial matches (e.g. "ScarletWitch" before "Witch")
            List<(PrototypeId AvatarRef, string Name)> candidates = new();
            foreach (PrototypeId stashRef in stashRefs)
            {
                PlayerStashInventoryPrototype stashProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(stashRef);
                if (stashProto == null || stashProto.ForAvatar == PrototypeId.Invalid)
                    continue;

                string avatarPath = GameDatabase.GetPrototypeName(stashProto.ForAvatar);
                string avatarName = ExtractLastPathSegment(avatarPath);
                if (string.IsNullOrEmpty(avatarName))
                    continue;

                candidates.Add((stashProto.ForAvatar, avatarName));
            }

            candidates.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));

            foreach (var (avatarRef, avatarName) in candidates)
            {
                // Look for the avatar name as a whole path segment
                if (itemPath.Contains($"/{avatarName}/", StringComparison.OrdinalIgnoreCase) ||
                    itemPath.Contains($"\\{avatarName}\\", StringComparison.OrdinalIgnoreCase))
                {
                    report?.AppendLine($"  PathCharacterDetection: item path '{itemPath}' contains avatar segment '{avatarName}'");
                    return avatarRef;
                }
            }

            return PrototypeId.Invalid;
        }

        private static string ExtractLastPathSegment(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            int lastSlash = path.LastIndexOf('/');
            string name = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
            name = name.Replace(".prototype", string.Empty, StringComparison.OrdinalIgnoreCase);
            return name;
        }

        #endregion

        #region Type Affinity

        private static readonly Dictionary<EquipmentInvUISlot, int> _uniqueGearSlotNumberMap = new Dictionary<EquipmentInvUISlot, int>
        {
            { EquipmentInvUISlot.Gear01, 1 },
            { EquipmentInvUISlot.Gear02, 2 },
            { EquipmentInvUISlot.Gear03, 3 },
            { EquipmentInvUISlot.Gear04, 4 },
            { EquipmentInvUISlot.Gear05, 5 },
        };

        private static readonly Dictionary<EquipmentInvUISlot, string[]> _uniqueSlotAliasMap = new Dictionary<EquipmentInvUISlot, string[]>
        {
            { EquipmentInvUISlot.Gear01, new[] { "slot1", "one", "gear1", "weapon", "unique1" } },
            { EquipmentInvUISlot.Gear02, new[] { "slot2", "two", "gear2", "body", "unique2" } },
            { EquipmentInvUISlot.Gear03, new[] { "slot3", "three", "gear3", "belt", "unique3" } },
            { EquipmentInvUISlot.Gear04, new[] { "slot4", "four", "gear4", "boots", "foot", "unique4" } },
            { EquipmentInvUISlot.Gear05, new[] { "slot5", "five", "gear5", "head", "unique5" } },
        };

        /// <summary>
        /// Tries to map an item's default equipment slot to a 1-5 gear slot number for unique-slot affinity.
        /// </summary>
        private static bool TryGetUniqueGearSlotNumber(Item item, out int slotNumber)
        {
            slotNumber = 0;
            return item.ItemPrototype != null &&
                   _uniqueGearSlotNumberMap.TryGetValue(item.ItemPrototype.DefaultEquipmentSlot, out slotNumber);
        }

        /// <summary>
        /// Tries to match a stash tab display name against the unique-slot aliases and returns the slot number.
        /// </summary>
        private static bool TryMatchUniqueSlotTabName(string tabName, out int slotNumber)
        {
            slotNumber = 0;
            if (string.IsNullOrEmpty(tabName))
                return false;

            foreach (var kvp in _uniqueSlotAliasMap)
            {
                foreach (string alias in kvp.Value)
                {
                    if (tabName.Contains(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        slotNumber = _uniqueGearSlotNumberMap[kvp.Key];
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a slot-specific stash tab for any-hero unique gear, if one exists and has space.
        /// Missing middle slots are distributed to the nearest existing slot tab (ties go to the higher slot).
        /// </summary>
        private PrototypeId ResolveSlotModStashAffinity(Item item, PrototypeId requestedStashRef, StringBuilder report)
        {
            // Character-specific uniques are handled by ResolveCharacterModStashAffinity
            if (item.IsBoundToCharacter || item.ItemPrototype?.IsAvatarRestricted == true)
            {
                report?.AppendLine("  SlotStash: item is character-specific, skipping slot affinity");
                return requestedStashRef;
            }

            RarityPrototype rarityProto = item.RarityPrototype;
            if (rarityProto == null || rarityProto.DataRef != GameDatabase.LootGlobalsPrototype.RarityUnique)
            {
                report?.AppendLine("  SlotStash: item is not unique, skipping slot affinity");
                return requestedStashRef;
            }

            if (TryGetUniqueGearSlotNumber(item, out int itemSlotNumber) == false)
            {
                report?.AppendLine($"  SlotStash: unique item has no gear slot mapping (slot={item.ItemPrototype?.DefaultEquipmentSlot})");
                return requestedStashRef;
            }

            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true) == false)
            {
                report?.AppendLine("  SlotStash: failed to retrieve stash list");
                return requestedStashRef;
            }

            // If the player already opened the correct slot tab, keep it
            PlayerStashInventoryPrototype requestedProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(requestedStashRef);
            if (requestedProto != null && requestedProto.ForAvatar == PrototypeId.Invalid)
            {
                Inventory requestedInv = GetInventoryByRef(requestedStashRef);
                if (requestedInv != null && requestedInv.Prototype.AllowEntity(item.Prototype))
                {
                    string requestedName = GetStashDisplayName(requestedStashRef);
                    if (TryMatchUniqueSlotTabName(requestedName, out int requestedSlot) && requestedSlot == itemSlotNumber)
                    {
                        AppendDecision(report, requestedStashRef, $"requested stash matches unique slot {itemSlotNumber}");
                        return requestedStashRef;
                    }
                }
            }

            // Collect available slot-specific tabs that can hold this item
            List<(PrototypeId StashRef, int SlotNumber, string StashName)> existingSlots = new();
            PrototypeId exactMatch = PrototypeId.Invalid;

            foreach (PrototypeId stashRef in stashRefs)
            {
                PlayerStashInventoryPrototype stashProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(stashRef);
                if (stashProto?.ForAvatar != PrototypeId.Invalid)
                    continue;

                Inventory stashInv = GetInventoryByRef(stashRef);
                if (stashInv == null)
                    continue;

                if (stashInv.Prototype.AllowEntity(item.Prototype) == false)
                    continue;

                string stashName = GetStashDisplayName(stashRef);
                if (TryMatchUniqueSlotTabName(stashName, out int slotNumber) == false)
                    continue;

                if (stashInv.GetFreeSlot(item, true, true) == Inventory.InvalidSlot)
                {
                    report?.AppendLine($"  SlotStash: '{stashName}' matches slot {slotNumber} but is full");
                    continue;
                }

                if (slotNumber == itemSlotNumber)
                    exactMatch = stashRef;

                existingSlots.Add((stashRef, slotNumber, stashName));
            }

            if (exactMatch != PrototypeId.Invalid)
            {
                string exactName = GetStashDisplayName(exactMatch);
                AppendDecision(report, exactMatch, $"found exact unique slot {itemSlotNumber} stash '{exactName}' with space");
                return exactMatch;
            }

            if (existingSlots.Count == 0)
            {
                report?.AppendLine($"  SlotStash: no available unique slot tabs for slot {itemSlotNumber}");
                return requestedStashRef;
            }

            // Distribute missing slots to the nearest existing slot tab (ties go up)
            PrototypeId lowerRef = PrototypeId.Invalid;
            int lowerSlot = int.MinValue;
            PrototypeId higherRef = PrototypeId.Invalid;
            int higherSlot = int.MaxValue;

            foreach (var (stashRef, slotNumber, stashName) in existingSlots)
            {
                if (slotNumber < itemSlotNumber && slotNumber > lowerSlot)
                {
                    lowerSlot = slotNumber;
                    lowerRef = stashRef;
                }
                else if (slotNumber > itemSlotNumber && slotNumber < higherSlot)
                {
                    higherSlot = slotNumber;
                    higherRef = stashRef;
                }
            }

            PrototypeId chosenRef;
            int chosenSlot;
            string chosenName;
            string reason;

            if (lowerRef != PrototypeId.Invalid && higherRef != PrototypeId.Invalid)
            {
                int lowerDist = itemSlotNumber - lowerSlot;
                int higherDist = higherSlot - itemSlotNumber;
                if (higherDist <= lowerDist)
                {
                    chosenRef = higherRef;
                    chosenSlot = higherSlot;
                    chosenName = GetStashDisplayName(higherRef);
                    reason = $"nearest unique slot tab (slot {itemSlotNumber} -> {chosenSlot}, tie/upper)";
                }
                else
                {
                    chosenRef = lowerRef;
                    chosenSlot = lowerSlot;
                    chosenName = GetStashDisplayName(lowerRef);
                    reason = $"nearest unique slot tab (slot {itemSlotNumber} -> {chosenSlot}, lower)";
                }
            }
            else if (lowerRef != PrototypeId.Invalid)
            {
                chosenRef = lowerRef;
                chosenSlot = lowerSlot;
                chosenName = GetStashDisplayName(lowerRef);
                reason = $"nearest lower unique slot tab (slot {itemSlotNumber} -> {chosenSlot})";
            }
            else if (higherRef != PrototypeId.Invalid)
            {
                chosenRef = higherRef;
                chosenSlot = higherSlot;
                chosenName = GetStashDisplayName(higherRef);
                reason = $"nearest higher unique slot tab (slot {itemSlotNumber} -> {chosenSlot})";
            }
            else
            {
                report?.AppendLine($"  SlotStash: no distribution target for slot {itemSlotNumber}");
                return requestedStashRef;
            }

            AppendDecision(report, chosenRef, reason);
            return chosenRef;
        }

        /// <summary>
        /// Returns a stash tab whose custom display name matches the item's affinity key, if one exists and has space.
        /// </summary>
        private PrototypeId ResolveTypeModStashAffinity(Item item, PrototypeId requestedStashRef, StringBuilder report)
        {
            string[] itemKeys = GetModStashAffinityKeys(item);
            string keysLabel = itemKeys.Length > 0 ? string.Join("/", itemKeys) : "(none)";
            report?.AppendLine($"  TypeAffinityKeys: {keysLabel}");

            if (itemKeys.Length == 0)
            {
                report?.AppendLine("  TypeStash: no affinity key for this item");
                return requestedStashRef;
            }

            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true) == false)
            {
                report?.AppendLine("  TypeStash: failed to retrieve stash list");
                return requestedStashRef;
            }

            foreach (PrototypeId stashRef in stashRefs)
            {
                if (stashRef == requestedStashRef)
                    continue;

                // Skip character-specific stashes - they are handled by ResolveCharacterModStashAffinity
                PlayerStashInventoryPrototype stashProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(stashRef);
                if (stashProto?.ForAvatar != PrototypeId.Invalid)
                    continue;

                string stashName = GetStashDisplayName(stashRef);
                if (_stashTabOptionsDict.TryGetValue(stashRef, out StashTabOptions options) == false)
                    continue;

                string tabName = options.DisplayName ?? string.Empty;
                string matchedKey = null;
                foreach (string key in itemKeys)
                {
                    if (tabName.Contains(key, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedKey = key;
                        break;
                    }
                }

                if (matchedKey == null)
                    continue;

                Inventory stashInv = GetInventoryByRef(stashRef);
                if (stashInv == null)
                {
                    report?.AppendLine($"  TypeStash: '{stashName}' matches '{matchedKey}' but is locked");
                    continue;
                }

                if (stashInv.Prototype.AllowEntity(item.Prototype) == false)
                {
                    report?.AppendLine($"  TypeStash: '{stashName}' matches '{matchedKey}' but does not allow this item type");
                    continue;
                }

                uint freeSlot = stashInv.GetFreeSlot(item, true, true);
                if (freeSlot != Inventory.InvalidSlot)
                {
                    AppendDecision(report, stashRef, $"found type stash '{stashName}' matching '{matchedKey}' with space");
                    return stashRef;
                }
                else
                {
                    report?.AppendLine($"  TypeStash: '{stashName}' matches '{matchedKey}' but is full");
                }
            }

            AppendDecision(report, requestedStashRef, $"no available type stash matching '{keysLabel}'");
            return requestedStashRef;
        }

        /// <summary>
        /// Returns the affinity keywords for an item (e.g. "ring", "artifact", "unique").
        /// Returns an empty array if the item has no affinity mapping.
        /// </summary>
        private static string[] GetModStashAffinityKeys(Item item)
        {
            ItemPrototype itemProto = item.ItemPrototype;
            if (itemProto == null)
                return Array.Empty<string>();

            // Unique (any-hero) -> unique stash
            // Character-specific uniques are handled by ResolveCharacterModStashAffinity before this
            RarityPrototype rarityProto = item.RarityPrototype;
            if (rarityProto != null && rarityProto.DataRef == GameDatabase.LootGlobalsPrototype.RarityUnique)
                return new[] { "unique" };

            string protoName = GameDatabase.GetPrototypeName(itemProto.DataRef);

            // Danger Room scenario portals -> maps/danger/dangerroom/scenario stash
            // Same detection logic as ModDangerRoomCombine: contains "DangerRoom" + "PortalTo", excludes recipes/crates/relics/etc.
            if (string.IsNullOrEmpty(protoName) == false &&
                protoName.Contains("DangerRoom", StringComparison.OrdinalIgnoreCase) &&
                protoName.Contains("PortalTo", StringComparison.OrdinalIgnoreCase))
            {
                string[] excluded = { "Recipe", "Crate", "Relic", "Tournament", "Gift", "Box", "Daily", "RandomDungeon", "RandomMaxAffixDungeon" };
                bool skip = false;
                foreach (string ex in excluded)
                {
                    if (protoName.Contains(ex, StringComparison.OrdinalIgnoreCase))
                    { skip = true; break; }
                }
                if (skip == false) return new[] { "maps", "danger", "dangerroom", "scenario" };
            }

            if (itemProto is ArtifactPrototype)
                return new[] { "artifact" };

            if (itemProto is MedalPrototype)
                return new[] { "medal" };

            if (itemProto is RelicPrototype)
                return new[] { "relic" };

            if (itemProto is TeamUpGearPrototype)
                return new[] { "teamup" };

            if (itemProto is CostumeCorePrototype)
                return new[] { "catalyst" };

            if (itemProto is CraftingIngredientPrototype)
            {
                if (protoName.Contains("/Runewords/Glyphs/RunewordGlyph") ||
                    protoName.Contains("/Runewords/Glyphs/OnslaughtRune"))
                    return new[] { "rune" };

                return new[] { "crafting" };
            }

            switch (itemProto.DefaultEquipmentSlot)
            {
                case EquipmentInvUISlot.Ring:
                    return new[] { "ring" };
                case EquipmentInvUISlot.Insignia:
                    return new[] { "insignia" };
                case EquipmentInvUISlot.UruForged:
                    return new[] { "uru" };
            }

            return Array.Empty<string>();
        }

        #endregion

        #region Stash Helpers

        private string GetStashDisplayName(PrototypeId stashRef)
        {
            if (_stashTabOptionsDict.TryGetValue(stashRef, out StashTabOptions options) &&
                string.IsNullOrEmpty(options.DisplayName) == false)
                return options.DisplayName;

            return GameDatabase.GetPrototypeName(stashRef);
        }

        #endregion

        #region Logging Helpers

        private void AppendHeader(StringBuilder report, Item item, PrototypeId requestedStashRef)
        {
            if (report == null) return;

            report.AppendLine();
            report.AppendLine($"[ModStashAffinity Decision] Player={this} Item={item}");
            report.AppendLine($"  ItemProto: {GameDatabase.GetPrototypeName(item.ItemPrototype?.DataRef ?? PrototypeId.Invalid)}");
            report.AppendLine($"  Rarity: {GameDatabase.GetPrototypeName(item.RarityPrototype?.DataRef ?? PrototypeId.Invalid)}");
            report.AppendLine($"  IsBoundToCharacter: {item.IsBoundToCharacter}");
            report.AppendLine($"  BoundAgent: {GameDatabase.GetPrototypeName(item.BoundAgentProtoRef)}");
            report.AppendLine($"  IsAvatarRestricted: {item.ItemPrototype?.IsAvatarRestricted}");
            report.AppendLine($"  EquippableBy: {GameDatabase.GetPrototypeName(item.ItemSpec?.EquippableBy ?? PrototypeId.Invalid)}");
            report.AppendLine($"  RequestedStash: {GameDatabase.GetPrototypeName(requestedStashRef)}");
        }

        private void AppendAvailableStashes(StringBuilder report)
        {
            if (report == null) return;

            using var stashRefsHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> stashRefs);
            if (GetStashInventoryProtoRefs(stashRefs, getLocked: false, getUnlocked: true) == false)
            {
                report.AppendLine("  AvailableStashes: failed to retrieve");
                return;
            }

            report.AppendLine("  AvailableStashes:");
            foreach (PrototypeId stashRef in stashRefs)
            {
                PlayerStashInventoryPrototype stashProto = GameDatabase.GetPrototype<PlayerStashInventoryPrototype>(stashRef);
                Inventory stashInv = GetInventoryByRef(stashRef);
                string displayName = GetStashDisplayName(stashRef);
                string forAvatar = GameDatabase.GetPrototypeName(stashProto?.ForAvatar ?? PrototypeId.Invalid);
                string state = stashInv == null ? "locked" : $"unlocked count={stashInv.Count}/{stashInv.GetCapacity()}";
                report.AppendLine($"    - {displayName} (proto={GameDatabase.GetPrototypeName(stashRef)}, avatar={forAvatar}, {state})");
            }
        }

        private void AppendDecision(StringBuilder report, PrototypeId chosenStashRef, string reason)
        {
            if (report == null) return;
            report.AppendLine($"  Decision: {reason} -> {GameDatabase.GetPrototypeName(chosenStashRef)}");
        }

        private void FlushReport(StringBuilder report)
        {
            if (report == null || report.Length == 0) return;
            ModStashAffinityLogCollator.WriteLine(Id, report.ToString());
        }

        private void LogAffinity(Item item, PrototypeId fromRef, PrototypeId toRef, string reason)
        {
            if (Game.CustomGameOptions.ModStashAffinityLoggingEnable == false)
                return;

            ModStashAffinityLogger.Info($"[ModStashAffinity] Player [{this}] moving item [{item}] (reason={reason}) from [{GameDatabase.GetPrototypeName(fromRef)}] to [{GameDatabase.GetPrototypeName(toRef)}]");
        }

        #endregion

        #endregion
    }
}