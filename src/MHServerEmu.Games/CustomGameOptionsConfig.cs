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
        public bool DisableCharacterBinding { get; private set; } = true; // can use unbinding currency anyways , this mostly applies to rings and insignia
        public bool DisableMissionXPBonuses { get; private set; } = false;
        public bool UsePrestigeLootTable { get; private set; } = false;
        public bool EnableUltimatePrestige { get; private set; } = false;
        public bool ApplyHiddenPvPDamageModifiers { get; private set; } = false;


        // ==============================================================================
        // MODDED FEATURES = LootFilter, AutoPickup, ThrowableDisable, ItemChestAutoOpen,
        //                   Incursion, TerminalDailyCompleteAnyDifficulty, InteractNearbyAuto
        // ==============================================================================

        // LOOT FILTER
        public bool LootFilterEnable { get; private set; } = true;
        public bool LootFilterCharacterSpecificEnable { get; private set; } = true;
        public bool LootFilterLoggingEnable { get; private set; } = false;

        // ITEM AUTO PICKUP to STASH = currency, crafting, relics, runes
        public bool ItemAutoPickupEnable { get; private set; } = true;
        public float ItemAutoPickupRadius { get; private set; } = 1400f;
        public int ItemAutoPickupIntervalMs { get; private set; } = 2100;
        // - crafting
        public bool ItemAutoPickupCraftingIngredientEnable { get; private set; } = true;
        public bool ItemAutoPickupCraftingIngredientToStash { get; private set; } = true;
        public bool ItemAutoPickupCraftingIngredientLoggingEnable { get; private set; } = false;
        // - relics
        public bool ItemAutoPickupRelicEnable { get; private set; } = true;
        public bool ItemAutoPickupRelicToStash { get; private set; } = true;
        public bool ItemAutoPickupRelicEquipIfSameTypeEquippedEnable { get; private set; } = true;
        public bool ItemAutoPickupRelicLoggingEnable { get; private set; } = false;
        // - runes
        public bool ItemAutoPickupRuneEnable { get; private set; } = true;
        public bool ItemAutoPickupRuneToStash { get; private set; } = true;
        public bool ItemAutoPickupRuneLoggingEnable { get; private set; } = false;

        // THROWABLE DISABLE = preference to not throw, don't get animlocked by throwing
        public bool ThrowableDisableInteractive { get; private set; } = true;
        public bool ThrowableAutoCancelOnPowerUse { get; private set; } = true;
        public bool ThrowableAutoThrowOnMovementPower { get; private set; } = true;

        // ITEM CHEST AUTO OPEN = opens chests and giftboxes in player inventory
        public bool ItemChestAutoOpenEnable { get; private set; } = true;
        public int ItemChestAutoOpenCooldownMs { get; private set; } = 1100;
        public string ItemChestAutoOpenWhitelist { get; private set; } = "Chest,Crate,LootBox,Giftbox,GiftBox";
        public bool ItemChestAutoOpenLoggingEnable { get; private set; } = false;

        // INCURSION = spawns random Hero Variant minibosses at random intervals - EXPERIMENTAL
        public bool IncursionEnable { get; private set; } = true;
        public int IncursionIntervalMs { get; private set; } = 180000;
        public int IncursionRandomIntervalMaxMs { get; private set; } = 360000;
        public int IncursionMaxActiveInvaders { get; private set; } = 10;
        public int IncursionMaxLifetimeMs { get; private set; } = 1200000; // 20 minutes
        public int IncursionIdleTimeoutMs { get; private set; } = 120000; // 2 minutes
        public int IncursionDeathGracePeriodMs { get; private set; } = 20000;
        public float IncursionEnemyDamageTakenMultiplier { get; private set; } = 2.0f;
        public float IncursionEnemyVisualScale { get; private set; } = 1.5f;
        public float IncursionEnemyDamageMultiplier { get; private set; } = 1.0f;
        public string IncursionExcludeEnemies { get; private set; } = "";
        public string IncursionEnemyPrototype { get; private set; } = "";
        public bool IncursionCommandsRequireAdmin { get; private set; } = false;
        public bool IncursionLoggingEnable { get; private set; } = false;
        public bool IncursionLogVerboseEnable { get; private set; } = false;
        public bool IncursionLogAllDamageTargetsEnable { get; private set; } = false;
        public bool IncursionLogCollatorEnable { get; private set; } = false;

        // TERMINAL DAILY COMPLETE any DIFFICULTY = any difficulty clears available - NOT WORKING
        public bool TerminalDailyCompleteAnyDifficultyEnable { get; private set; } = false;
        public bool TerminalDailyCompleteAnyDifficultyLoggingEnable { get; private set; } = false;

        // INTERACT NEARBY AUTO = mission objectives, civilians, chests
        public bool InteractNearbyAutoEnable { get; private set; } = true;
        public int InteractNearbyAutoIntervalMs { get; private set; } = 250;
        public bool InteractNearbyAutoLoggingEnable { get; private set; } = false;
        public string InteractNearbyAutoWhitelist { get; private set; } = "DoombotFactoryCommandConsole,HeroCommendationReward,BoxcarMutantDesirae,";
        public string InteractNearbyAutoBlacklist { get; private set; } = "StanLee,Stash,Vendor,Waypoint,GLFSupplyOfficer,Trans,Transition,EGPVEManhattan,EGPVESubterranea,Elevator,Door,Floor,Portal,DefaultEND,ReturnToLastBase,XMansionToHeli,";
    }
}
