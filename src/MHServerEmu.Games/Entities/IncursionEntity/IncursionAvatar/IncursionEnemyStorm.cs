using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Storm Invader
    /// Powers: 17 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyStorm : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Storm.prototype");

        public IncursionEnemyStorm(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Storm Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Storm/AfricanGoddess.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Storm/Asgard.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Storm/Astonishing.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Storm/AvX.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Storm/Classic90sXmen.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Storm/ClassicBlack.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Storm/MarvelNOW.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Storm/Modern.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Storm/ModernVU.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Storm/Mohawk.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Storm/Ultimate.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Storm/XMenUniform.prototype",    true),
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
            new("Powers/Player/Storm/BallLightning.prototype",                    true,  0.1403f), // 2026-06-18
            new("Powers/Player/Storm/ChainLightning.prototype",                   true,  0.1042f), // 2026-06-18
            new("Powers/Player/Storm/ChanneledLightning.prototype",               true,  0.0336f), // 2026-06-18
            new("Powers/Player/Storm/ChargedStrike.prototype",                    true,  0.0919f), // 2026-06-17
            new("Powers/Player/Storm/ChargedStrikeHiddenPassive.prototype",       false, 0.05f),
            new("Powers/Player/Storm/DisengagingShot.prototype",                  true,  0.0616f), // 2026-06-18
            new("Powers/Player/Storm/ElementalStorm.prototype",                   true,  0.0774f), // 2026-06-18
            new("Powers/Player/Storm/Fog.prototype",                              true,  0.05f),
            new("Powers/Player/Storm/Hailstorm.prototype",                        true,  0.0976f), // 2026-06-18
            new("Powers/Player/Storm/LightningBolt.prototype",                    true,  0.1569f), // 2026-06-18
            new("Powers/Player/Storm/Maelstrom.prototype",                        true,  0.02f),
            new("Powers/Player/Storm/Microburst.prototype",                       true,  0.1027f), // 2026-06-18
            new("Powers/Player/Storm/SiroccoLunge.prototype",                     true,  0.1023f), // 2026-06-18
            new("Powers/Player/Storm/StormSurge.prototype",                       true,  0.03f),
            new("Powers/Player/Storm/Talents/AllTempests.prototype",              false, 0.05f),
            new("Powers/Player/Storm/Talents/ColdDamageSpec.prototype",           false, 0.05f),
            new("Powers/Player/Storm/Talents/FreezingTempest.prototype",          false, 0.05f),
            new("Powers/Player/Storm/Talents/HealingRain.prototype",              false, 0.05f),
            new("Powers/Player/Storm/Talents/HurricaneWinds.prototype",           false, 0.05f),
            new("Powers/Player/Storm/Talents/LightningCharged.prototype",         false, 0.05f),
            new("Powers/Player/Storm/Talents/LightningSpec.prototype",            false, 0.05f),
            new("Powers/Player/Storm/Talents/LightningTempest.prototype",         false, 0.05f),
            new("Powers/Player/Storm/Talents/MaelstromBuff.prototype",            false, 0.02f),
            new("Powers/Player/Storm/Talents/MassiveLightningStrike.prototype",   false, 0.05f),
            new("Powers/Player/Storm/Talents/StormSurgeInstantFill.prototype",    false, 0.03f),
            new("Powers/Player/Storm/Talents/TyphoonAcidRain.prototype",          false, 0.0360f), // 2026-06-18
            new("Powers/Player/Storm/Talents/TyphoonPull.prototype",              false, 0.0360f), // 2026-06-18
            new("Powers/Player/Storm/Talents/WindSpec.prototype",                 false, 0.05f),
            new("Powers/Player/Storm/Talents/WindTempest.prototype",              false, 0.05f),
            new("Powers/Player/Storm/Tornado.prototype",                          true,  0.1385f), // 2026-06-17
            new("Powers/Player/Storm/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Storm/Traits/MechanicTraitSurge.prototype",        false, 0.05f),
            new("Powers/Player/Storm/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Storm/Typhoon.prototype",                          true,  0.0360f), // 2026-06-18
            new("Powers/Player/Storm/TyphoonHiddenPassive.prototype",             false, 0.0360f), // 2026-06-18
            new("Powers/Player/Storm/Ultimate.prototype",                         true,  0.0104f), // 2026-06-18
            new("Powers/Player/Storm/Zephyr.prototype",                           true,  0.1243f), // 2026-06-18
            new("Powers/Player/TravelPower/StormFlight.prototype",                false, 0.03f),
            new("Powers/StolenPowers/StealablePowers/StormStolenPower.prototype", false, 0.03f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                   false, 0.05f),
        };
    }
}
