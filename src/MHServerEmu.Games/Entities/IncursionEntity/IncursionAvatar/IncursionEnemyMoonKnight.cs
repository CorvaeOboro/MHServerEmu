using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// MoonKnight Invader
    /// Powers: 19 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyMoonKnight : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/MoonKnight.prototype");

        public IncursionEnemyMoonKnight(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "MoonKnight Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/MoonKnight/EarthX.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/MoonKnight/MarvelNOW.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/MoonKnight/Modern.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/MoonKnight/MrKnightCoat.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/MoonKnight/MrKnightVest.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/MoonKnight/SecretAvengers.prototype", true),
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
            new("Powers/Player/MoonKnight/BasicCrescentDart.prototype",                             true,  0.1733f), // 2026-06-20
            new("Powers/Player/MoonKnight/BasicGauntletPunch.prototype",                            true,  0.0973f), // 2026-06-17
            new("Powers/Player/MoonKnight/BasicStaffStrike.prototype",                              true,  0.1271f), // 2026-06-17
            new("Powers/Player/MoonKnight/CestusGauntletPunch.prototype",                           true,  0.0879f), // 2026-06-14
            new("Powers/Player/MoonKnight/ConeYank.prototype",                                      true,  0.0990f), // 2026-06-17
            new("Powers/Player/MoonKnight/CrescentBola.prototype",                                  true,  0.3780f), // 2026-06-17
            new("Powers/Player/MoonKnight/CrescentDartFan.prototype",                               true,  0.1464f), // 2026-06-20
            new("Powers/Player/MoonKnight/DeathFromAbove.prototype",                                true,  0.0708f), // 2026-06-17
            new("Powers/Player/MoonKnight/HighlightSteroids.prototype",                             true,  0.05f),
            new("Powers/Player/MoonKnight/KhonshuSteroidHealth.prototype",                          true,  0.05f),
            new("Powers/Player/MoonKnight/NunchuckBulldoze.prototype",                              true,  0.0430f), // 2026-06-17
            new("Powers/Player/MoonKnight/RapidFire.prototype",                                     true,  0.05f),
            new("Powers/Player/MoonKnight/Ricochet.prototype",                                      true,  0.05f),
            new("Powers/Player/MoonKnight/SignatureFrenzy.prototype",                               true,  0.0613f), // 2026-06-05
            new("Powers/Player/MoonKnight/StaffPBAoE.prototype",                                    true,  0.0094f), // 2026-06-05
            new("Powers/Player/MoonKnight/Strafe.prototype",                                        true,  0.1390f), // 2026-06-05
            new("Powers/Player/MoonKnight/SummonKhonshuStatue.prototype",                           true,  0.05f),
            new("Powers/Player/MoonKnight/Talents/AngelwingStrafeDFACharges.prototype",             false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/BasicCrescentExplosiveRapidFireBounce.prototype", false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/BrutalChanceTerrify.prototype",                   false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/CestusPunchLayer.prototype",                      false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/CestusUppercutTribute.prototype",                 false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/ConeYankNunchuckBulldozeBonus.prototype",         false, 0.0990f), // 2026-06-17
            new("Powers/Player/MoonKnight/Talents/CrescentFanCooldown.prototype",                   false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/HealthDefenseSelfRez.prototype",                  false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuStatueSteroidCombined.prototype",          false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuStatueTerrify.prototype",                  false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/KhonshuSteroidCastSpeedMult.prototype",           false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/RangedSignature.prototype",                       false, 0.02f),
            new("Powers/Player/MoonKnight/Talents/RicochetCharges.prototype",                       false, 0.05f),
            new("Powers/Player/MoonKnight/Talents/SignatureTributeGain.prototype",                  false, 0.02f),
            new("Powers/Player/MoonKnight/Talents/StaffPBAoEBleed.prototype",                       false, 0.0094f), // 2026-06-05
            new("Powers/Player/MoonKnight/Traits/DefenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/MoonKnight/Traits/MechanicTrait.prototype",                          false, 0.05f),
            new("Powers/Player/MoonKnight/Traits/OffenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/MoonKnight/Tumble.prototype",                                        true,  0.05f),
            new("Powers/Player/MoonKnight/Ultimate.prototype",                                      true,  0.006f),
            new("Powers/Player/MoonKnight/UltimateHiddenPassive.prototype",                         false, 0.006f),
            new("Powers/Player/TravelPower/MoonKnightFlight.prototype",                             false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MoonKnightStolenPower.prototype",              false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                          false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                            false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                               false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                     false, 0.05f),
        };
    }
}
