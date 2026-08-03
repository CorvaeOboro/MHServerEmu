using Gazillion;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Games.Entities.CalamityEntity;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Regions;
using MHServerEmu.Games.UI;

namespace MHServerEmu.Games.Entities.Avatars
{
    public partial class Avatar
    {
        /// <summary>
        /// Handles interaction with the Cloak NPC: shows a dialog and teleports the player
        /// to the Princess Bar region for the Vampire Blood Ritual event.
        /// </summary>
        private void HandleVampireBloodRitualCloakInteraction(Player player, WorldEntity cloakEntity)
        {
            var dialogManager = Game.GameDialogManager;
            if (dialogManager == null)
            {
                Logger.Warn("[VampireBloodRitual] Game has no GameDialogManager, teleporting directly.");
                TeleportToVampireBloodRitual(player);
                return;
            }

            var dialog = dialogManager.CreateInstance(player.DatabaseUniqueId);
            if (VampireBloodRitualEvent.ShowDialogMessage)
                dialog.Message.LocaleString = (LocaleStringId)VampireBloodRitualEvent.DialogMessageStringId;
            dialog.Options = DialogOptionEnum.Modal;
            dialog.TargetId = cloakEntity.Id;
            dialog.InteractorId = Id;

            dialog.AddButton(GameDialogResultEnum.eGDR_Option1,
                (LocaleStringId)VampireBloodRitualEvent.YesButtonStringId, ButtonStyle.Primary, true);
            dialog.AddButton(GameDialogResultEnum.eGDR_Option2,
                (LocaleStringId)VampireBloodRitualEvent.NoButtonStringId, ButtonStyle.SecondaryNegative, true);

            dialog.OnResponse = (responderId, response) =>
            {
                if (response.ButtonIndex == GameDialogResultEnum.eGDR_Option1)
                {
                    var responder = Game.EntityManager.GetEntityByDbGuid<Player>(responderId);
                    if (responder != null)
                    {
                        var avatar = responder.CurrentAvatar;
                        if (avatar != null)
                            avatar.TeleportToVampireBloodRitual(responder);
                    }
                }
            };

            dialogManager.ShowDialog(dialog);
        }

        /// <summary>
        /// Teleports the player to the Game Center (Hulk Busters arcade) for the Vampire Blood Ritual event.
        /// </summary>
        private void TeleportToVampireBloodRitual(Player player)
        {
            RegionPrototype destRegionProto = VampireBloodRitualEvent.EventRegionRef.As<RegionPrototype>();
            if (destRegionProto == null)
            {
                Logger.Warn("[VampireBloodRitual] Game Center region prototype not found.");
                return;
            }

            // Set the request flag so the newly generated region initializes the event.
            Regions.Region.VampireBloodRitualRequested = true;
            PrototypeId eventRegionRef = VampireBloodRitualEvent.EventRegionRef;
            foreach (Region existingRegion in Game.RegionManager)
            {
                if (existingRegion.PrototypeDataRef == eventRegionRef)
                {
                    // Only destroy/reset the region if no other players are inside.
                    // If other players are present, teleport to the existing instance.
                    if (existingRegion.PlayerCount > 0)
                    {
                        Logger.Info($"[VampireBloodRitual] Event region (id={existingRegion.Id}) has {existingRegion.PlayerCount} player(s) - not resetting.");
                        break;
                    }

                    Logger.Info($"[VampireBloodRitual] Destroying existing event region (id={existingRegion.Id}) for reset before teleport (no players inside).");
                    Game.RegionManager.DestroyRegion(existingRegion.Id);
                    break;
                }
            }

            // Find the start target for the destination region
            PrototypeId areaProtoRef = PrototypeId.Invalid;
            PrototypeId cellProtoRef = PrototypeId.Invalid;
            PrototypeId entityProtoRef = PrototypeId.Invalid;

            if (destRegionProto.StartTarget != PrototypeId.Invalid)
            {
                var startTarget = destRegionProto.StartTarget.As<RegionConnectionTargetPrototype>();
                if (startTarget != null)
                {
                    areaProtoRef = startTarget.Area;
                    cellProtoRef = GameDatabase.GetDataRefByAsset(startTarget.Cell);
                    entityProtoRef = startTarget.Entity;
                }
            }

            using Teleporter teleporter = ObjectPoolManager.Instance.Get<Teleporter>();
            teleporter.Initialize(player, TeleportContextEnum.TeleportContext_Portal);

            // Use a unique non-zero seed so the PlayerManager's WorldView.GetMatchingRegion()
            // does NOT reuse the previous event region (which still has defeated enemies).
            // MatchesCreateParams() skips regions whose seed doesn't match when otherParams.Seed != 0.
            // The DestroyRegion loop above only works if the region is in the same Game instance;
            // the seed guarantees a fresh region regardless of which Game owns the old one.
            teleporter.Seed = Game.Random.Next(1, int.MaxValue);

            bool success = teleporter.TeleportToTarget(
                VampireBloodRitualEvent.EventRegionRef,
                areaProtoRef, cellProtoRef, entityProtoRef);

            if (success)
                Logger.Info($"[VampireBloodRitual] Teleported player {player.GetName()} to Game Center (Hulk Busters arcade).");
            else
                Logger.Warn($"[VampireBloodRitual] Failed to teleport player {player.GetName()} to Game Center.");
        }
    }
}
