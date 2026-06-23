using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Loki Invader
    /// Powers: 17 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyLoki : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Loki.prototype");

        public IncursionEnemyLoki(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Loki Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Loki/AgentOfAsgard.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Loki/AgentOfAsgardVariant.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/Loki/LadyLoki.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Loki/LokiClassic.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Loki/LokiTravelingFugitive.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Loki/PrisonerMovie.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Loki/Siege.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/Loki/VoteLoki.prototype",              true),
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
            new("Powers/Player/Loki/AsgardianLight.prototype",                    true,  0.0786f), // 2026-06-20
            new("Powers/Player/Loki/ChainBolt.prototype",                         true,  0.1113f), // 2026-06-17
            new("Powers/Player/Loki/ConeOfMagic.prototype",                       true,  0.0879f), // 2026-06-20
            new("Powers/Player/Loki/DecoyIllusion.prototype",                     true,  0.05f),
            new("Powers/Player/Loki/EternalFlame.prototype",                      true,  0.1427f), // 2026-06-17
            new("Powers/Player/Loki/GlacialSpike.prototype",                      true,  0.0807f), // 2026-06-20
            new("Powers/Player/Loki/IllusionCounterHiddenPassive.prototype",      false, 0.05f),
            new("Powers/Player/Loki/IllusionFromAbove.prototype",                 true,  0.05f),
            new("Powers/Player/Loki/IllusionRush.prototype",                      true,  0.1304f), // 2026-06-20
            new("Powers/Player/Loki/MagicChains.prototype",                       true,  0.1737f), // 2026-06-17
            new("Powers/Player/Loki/MagicCrush.prototype",                        true,  0.0808f), // 2026-06-20
            new("Powers/Player/Loki/MeddlingStrike.prototype",                    true,  0.1485f), // 2026-06-05
            new("Powers/Player/Loki/MindControl.prototype",                       true,  0.0912f), // 2026-06-20
            new("Powers/Player/Loki/SorcerousBlast.prototype",                    true,  0.0695f), // 2026-06-20
            new("Powers/Player/Loki/SpiritsOfTheDead.prototype",                  true,  0.0621f), // 2026-06-20
            new("Powers/Player/Loki/SwordSlice.prototype",                        true,  0.1613f), // 2026-06-20
            new("Powers/Player/Loki/Talents/FourRealmSearingEmbers.prototype",    false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsColdFront.prototype",       false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsDarkBolt.prototype",        false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsEternalDarkness.prototype", false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsFrostNova.prototype",       false, 0.01f),
            new("Powers/Player/Loki/Talents/FourRealmsIceShards.prototype",       false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsIncinerate.prototype",      false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsInfernalBinding.prototype", false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsLightBeam.prototype",       false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsLightColumn.prototype",     false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsRefractingBurst.prototype", false, 0.05f),
            new("Powers/Player/Loki/Talents/FourRealmsSoulCrush.prototype",       false, 0.05f),
            new("Powers/Player/Loki/Talents/MainSpecIllusions.prototype",         false, 0.05f),
            new("Powers/Player/Loki/Talents/MainSpecMelee.prototype",             false, 0.05f),
            new("Powers/Player/Loki/Talents/MainSpecRanged.prototype",            false, 0.05f),
            new("Powers/Player/Loki/Traits/DefenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Loki/Traits/MechanicTraitIllusions.prototype",     false, 0.05f),
            new("Powers/Player/Loki/Traits/OffenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Loki/Ultimate.prototype",                          true,  0.0060f), // 2026-06-20
            new("Powers/Player/Loki/UltimateHiddenPassive.prototype",             false, 0.0060f), // 2026-06-20
            new("Powers/Player/Loki/Unveiled.prototype",                          true,  0.0122f), // 2026-06-20
            new("Powers/Player/TravelPower/LokiFlight.prototype",                 false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/LokiStolenPower.prototype",  false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                   false, 0.05f),
        };
    }
}
