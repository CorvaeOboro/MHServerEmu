using MHServerEmu.Core.Config;

namespace MHServerEmu.Games
{
    public class CustomGameOptionsConfig : ConfigContainer
    {
        public int AutosaveIntervalMinutes { get; private set; } = 15;
        public float ESCooldownOverrideMinutes { get; private set; } = -1f;
        public bool CombineESStacks { get; private set; } = false;
        public bool AutoUnlockAvatars { get; private set; } = false;
        public bool AutoUnlockTeamUps { get; private set; } = false;
        public bool DisableMovementPowerChargeCost { get; private set; } = false;
        public bool AllowSameGroupTalents { get; private set; } = false;
        public bool EnableCreditChestConversion { get; private set; } = false;
        public float CreditChestConversionMultiplier { get; private set; } = 2f;
        public bool DisableInstancedLoot { get; private set; } = false;
        public float LootSpawnGridCellRadius { get; private set; } = 20f;
        public float TrashedItemExpirationTimeMultiplier { get; private set; } = 1f;
        public bool DisableAccountBinding { get; private set; } = false;
        public bool DisableCharacterBinding { get; private set; } = false;
        public bool DisableMissionXPBonuses { get; private set; } = false;
        public bool UsePrestigeLootTable { get; private set; } = false;
        public bool EnableUltimatePrestige { get; private set; } = false;
        public bool ApplyHiddenPvPDamageModifiers { get; private set; } = false;
        public bool EnableItemAutoPickup { get; private set; } = true;
        public float ItemAutoPickupRadius { get; private set; } = 1400f;
        public int ItemAutoPickupIntervalMs { get; private set; } = 1500;
        public bool EnableLootFilters { get; private set; } = true;
        public bool EnableCharacterSpecificLootFilter { get; private set; } = true;
        public bool EnableCraftingIngredientAutoPickup { get; private set; } = true;
        public bool CraftingIngredientAutoPickupToStash { get; private set; } = true;
        public bool EnableCraftingIngredientAutoPickupVerboseLogging { get; private set; } = false;
        public bool DisableInteractiveThrowables { get; private set; } = true;
        public bool AutoCancelThrowableOnPowerUse { get; private set; } = true;
        public bool AutoThrowOnMovementPower { get; private set; } = true;
        public bool EnableItemChestAutoOpen { get; private set; } = true;
        public int ItemChestAutoOpenCooldownMs { get; private set; } = 1000;
        public string ItemChestAutoOpenWhitelist { get; private set; } = "Chest,Crate,LootBox,Giftbox,GiftBox";
        public bool EnableItemChestAutoOpenVerboseLogging { get; private set; } = false;
    }
}
