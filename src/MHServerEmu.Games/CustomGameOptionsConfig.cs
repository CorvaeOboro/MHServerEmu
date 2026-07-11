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
        // MODDED FEATURES = LootFilter, ModItemPickupAuto, ThrowableDisable, ModChestOpenAuto,
        //                   Incursion, TerminalDailyCompleteAnyDifficulty, ModInteractNearbyAuto
        // ==============================================================================

        // LOOT FILTER
        public bool LootFilterEnable { get; private set; } = true;
        public bool LootFilterCharacterSpecificEnable { get; private set; } = true;
        public bool LootFilterLoggingEnable { get; private set; } = false;

        // MOD ITEM PICKUP AUTO to STASH = currency, crafting, relics, runes
        public bool ModItemPickupAutoEnable { get; private set; } = true;
        public float ModItemPickupAutoRadius { get; private set; } = 1400f;
        public int ModItemPickupAutoIntervalMs { get; private set; } = 2100;
        // - crafting
        public bool ModItemPickupAutoCraftingIngredientEnable { get; private set; } = true;
        public bool ModItemPickupAutoCraftingIngredientToStash { get; private set; } = true;
        public bool ModItemPickupAutoCraftingIngredientLoggingEnable { get; private set; } = false;
        // - relics
        public bool ModItemPickupAutoRelicEnable { get; private set; } = true;
        public bool ModItemPickupAutoRelicToStash { get; private set; } = true;
        public bool ModItemPickupAutoRelicEquipIfSameTypeEquippedEnable { get; private set; } = true;
        public bool ModItemPickupAutoRelicLoggingEnable { get; private set; } = false;
        // - runes
        public bool ModItemPickupAutoRuneEnable { get; private set; } = true;
        public bool ModItemPickupAutoRuneToStash { get; private set; } = true;
        public bool ModItemPickupAutoRuneLoggingEnable { get; private set; } = false;

        // THROWABLE DISABLE = preference to not throw, don't get animlocked by throwing
        public bool ThrowableDisableInteractive { get; private set; } = true;
        public bool ThrowableAutoCancelOnPowerUse { get; private set; } = true;
        public bool ThrowableAutoThrowOnMovementPower { get; private set; } = true;

        // MOD CHEST OPEN AUTO = opens chests and giftboxes in player inventory
        public bool ModChestOpenAutoEnable { get; private set; } = true;
        public int ModChestOpenAutoCooldownMs { get; private set; } = 1100;
        public string ModChestOpenAutoWhitelist { get; private set; } = "Chest,Crate,LootBox,Giftbox,GiftBox";
        public bool ModChestOpenAutoLoggingEnable { get; private set; } = false;

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

        // PLAYABLE EXPANDED = play as NEW characters built from Team-Up (and other) assets - EXPERIMENTAL
        public bool PlayableExpandedEnable { get; private set; } = true;
        public float PlayableExpandedDamageScale { get; private set; } = 1.0f; // global multiplier on top of per-character/per-power scales
        public bool PlayableExpandedCommandsRequireAdmin { get; private set; } = false;
        public bool PlayableExpandedLoggingEnable { get; private set; } = false;

        // TERMINAL DAILY COMPLETE any DIFFICULTY = any difficulty clears available - NOT WORKING
        public bool TerminalDailyCompleteAnyDifficultyEnable { get; private set; } = false;
        public bool TerminalDailyCompleteAnyDifficultyLoggingEnable { get; private set; } = false;

        // MISSION TRACKER HIDE COMPLETED SHARED QUESTS = hides shared quest objectives after daily bonus is consumed
        public bool MissionTrackerHideCompletedSharedQuestsEnable { get; private set; } = false;
        public bool MissionTrackerHideCompletedSharedQuestsLoggingEnable { get; private set; } = false;

        // MOD INTERACT NEARBY AUTO = mission objectives, civilians, chests
        public bool ModInteractNearbyAutoEnable { get; private set; } = true;
        public int ModInteractNearbyAutoIntervalMs { get; private set; } = 250;
        public bool ModInteractNearbyAutoLoggingEnable { get; private set; } = false;
        public string ModInteractNearbyAutoWhitelist { get; private set; } = "DoombotFactoryCommandConsole,HeroCommendationReward,BoxcarMutantDesirae,";
        public string ModInteractNearbyAutoBlacklist { get; private set; } = "StanLee,Stash,Vendor,Waypoint,GLFSupplyOfficer,Trans,Transition,EGPVEManhattan,EGPVESubterranea,Elevator,Door,Floor,Portal,DefaultEND,ReturnToLastBase,XMansionToHeli,";

        // MOD DANGER ROOM COMBINE COMMAND = combines lower-rarity scenarios into higher-rarity ones
        public bool ModDangerRoomCombineCommandEnable { get; private set; } = true;

        // MOD STASH AFFINITY = redirects items to stash tabs whose custom names match item type
        public bool ModStashAffinityEnable { get; private set; } = true;
        public bool ModStashAffinityLoggingEnable { get; private set; } = false;
    }
}
