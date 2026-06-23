using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// JeanGrey Invader
    /// Powers: 18 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyJeanGrey : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/JeanGrey.prototype");

        public IncursionEnemyJeanGrey(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "JeanGrey Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/JeanGrey/AgeOfApocalypse.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/AllNewXmen.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/BlackQueen.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/DarkPhoenix.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/DarkPhoenixMH2013.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/JeanGreyMH2013.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/MarvelGirl.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/MarvelNow.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/NewXmen.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/NewXmenJacket.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/NewXmenTrenchcoat.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/Phoenix.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/WhitePhoenix.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/WhitePhoenixMH2013.prototype", true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/XMen90s.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/JeanGrey/XMen90sMH2013.prototype",      true),
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
            new("Powers/Player/JeanGrey/DebrisMaelstrom.prototype",                     true,  0.0430f), // 2026-06-17
            new("Powers/Player/JeanGrey/PanicJean.prototype",                           true,  0.05f),
            new("Powers/Player/JeanGrey/PhoenixForceHiddenPassive.prototype",           false, 0.05f),
            new("Powers/Player/JeanGrey/PsiShield.prototype",                           true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/DamageMaelstrom.prototype",              true,  0.0613f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/DarkPhoenixExpelPhoenixForce.prototype", true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/Drain.prototype",                        true,  0.0360f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/ForcePushJean.prototype",                true,  0.0988f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/ImplosionJean.prototype",                true,  0.1316f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/KineticBoltJean.prototype",              true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/LiftAndSlamJean.prototype",              true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/NeuralNetworkJean.prototype",            true,  0.0837f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/PsychicHammerJean.prototype",            true,  0.0872f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/SignatureTKHurlJean.prototype",          true,  0.0171f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/SlowAoE.prototype",                      true,  0.0617f), // 2026-06-20
            new("Powers/Player/JeanGrey/Rework/SpeedRushJean.prototype",                true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/TKTossJean.prototype",                   true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/TelepathicIllusionJean.prototype",       true,  0.05f),
            new("Powers/Player/JeanGrey/Talents/CarThrow.prototype",                    false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/DarkPhoenixBonus.prototype",            false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/DarkPhoenixPassive.prototype",          false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/HybridFormsSpec.prototype",             false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/IllusionBonus.prototype",               false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/ImplosionBonus.prototype",              false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/JeanFormSpec.prototype",                false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/KineticWaveBonus.prototype",            false, 0.03f),
            new("Powers/Player/JeanGrey/Talents/LethargyBonus.prototype",               false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/LiftAndSlamCDR.prototype",              false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/NeuralNetworkBonus.prototype",          false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/PhoenixForceGeneration.prototype",      false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/PhoenixFormSpec.prototype",             false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/SpendersAreFree.prototype",             false, 0.05f),
            new("Powers/Player/JeanGrey/Talents/TKTossTripleThrow.prototype",           false, 0.05f),
            new("Powers/Player/JeanGrey/Traits/DefenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/JeanGrey/Traits/OffenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/JeanGrey/Ultimate.prototype",                            true,  0.0100f), // 2026-06-20
            new("Powers/Player/JeanGrey/UltimateHiddenPassive.prototype",               false, 0.0100f), // 2026-06-20
            new("Powers/Player/TravelPower/JeanGreyFlight.prototype",                   false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/JeanGreyStolenPower.prototype",    false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",              false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",             false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                   false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                         false, 0.05f),
        };
    }
}
