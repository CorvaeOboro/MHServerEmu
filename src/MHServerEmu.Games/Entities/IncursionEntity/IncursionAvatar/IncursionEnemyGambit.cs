using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Gambit Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyGambit : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Gambit.prototype");

        public IncursionEnemyGambit(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Gambit Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Gambit/Armored.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Gambit/Death.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Gambit/GambitClassic90.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Gambit/Shirtless.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Gambit/ShirtlessTrench.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Gambit/SoloModernCoat.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Gambit/XFactor.prototype",         true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.05f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/Gambit/AceOfSpades.prototype",                       true,  0.0530f), // 2026-06-18
            new("Powers/Player/Gambit/BasicBoStrike.prototype",                     true,  0.0686f), // 2026-06-18
            new("Powers/Player/Gambit/BasicKineticCard.prototype",                  true,  0.2023f), // 2026-06-18
            new("Powers/Player/Gambit/BasicKineticCardHiddenPassive.prototype",     false, 0.05f),
            new("Powers/Player/Gambit/BatterUp.prototype",                          true,  0.0421f), // 2026-06-18
            new("Powers/Player/Gambit/BoBeatdown.prototype",                        true,  0.0613f), // 2026-06-18
            new("Powers/Player/Gambit/BoVault.prototype",                           true,  0.1593f), // 2026-06-20
            new("Powers/Player/Gambit/BoWhirlwind.prototype",                       true,  0.1593f), // 2026-06-20
            new("Powers/Player/Gambit/CardPickup.prototype",                        true,  0.0741f), // 2026-06-18
            new("Powers/Player/Gambit/ChargeUpCard.prototype",                      true,  0.05f),
            new("Powers/Player/Gambit/FoldEm.prototype",                            true,  0.1014f), // 2026-06-18
            new("Powers/Player/Gambit/GrandSlam.prototype",                         true,  0.0434f), // 2026-06-20
            new("Powers/Player/Gambit/JacksOrBetter.prototype",                     true,  0.2097f), // 2026-06-17
            new("Powers/Player/Gambit/RaininPain.prototype",                        true,  0.0702f), // 2026-06-20
            new("Powers/Player/Gambit/RoyalFlush.prototype",                        true,  0.1658f), // 2026-06-20
            new("Powers/Player/Gambit/StreetSweep.prototype",                       true,  0.1174f), // 2026-06-18
            new("Powers/Player/Gambit/Talents/Talent1LessDowntime.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent1LongerBurn.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent1RaginCajun.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2CheatDeath.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2KingOfHearts.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent2SleightOfHand.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3DeucesWild.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3ShuffleUpAndDeal.prototype",   false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent3ThreeOfAKind.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4AllOutOfCards.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4JacksOrBetter.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent4StackTheDeck.prototype",       false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfClubs.prototype",         false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfDiamonds.prototype",      false, 0.05f),
            new("Powers/Player/Gambit/Talents/Talent5AceOfHearts.prototype",        false, 0.05f),
            new("Powers/Player/Gambit/Traits/DefenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Gambit/Traits/MechanicTraitKineticEnergy.prototype", false, 0.05f),
            new("Powers/Player/Gambit/Traits/OffenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Gambit/Tumble.prototype",                            true,  0.05f),
            new("Powers/Player/Gambit/Ultimate.prototype",                          true,  0.0118f), // 2026-06-18
            new("Powers/Player/TravelPower/GambitSprint.prototype",                 false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GambitStolenPower.prototype",  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",            false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",               false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                     false, 0.05f),
        };
    }
}
