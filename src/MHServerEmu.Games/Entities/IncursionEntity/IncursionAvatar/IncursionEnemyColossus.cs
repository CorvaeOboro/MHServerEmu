using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Colossus Invader
    /// Powers: 15 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyColossus : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Colossus.prototype");

        public IncursionEnemyColossus(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Colossus Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Colossus/AgeOfApocalypse.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Juggernaut.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Colossus/MagnetosAcolyte.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Colossus/MarvelNOW.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Modern.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Origins.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Outback.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Colossus/PhoenixForce.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Colossus/Ultimate.prototype",        true),
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
            new("Powers/Player/Colossus/ArmoringPunch.prototype",                                  true,  0.0242f), // 2026-06-17
            new("Powers/Player/Colossus/CallKittyAoE.prototype",                                   true,  0.0229f), // 2026-06-18
            new("Powers/Player/Colossus/CallNightcrawler.prototype",                               true,  0.0247f), // 2026-06-18
            new("Powers/Player/Colossus/DeathFromAbove.prototype",                                 true,  0.0226f), // 2026-06-18
            new("Powers/Player/Colossus/FastballSpecial.prototype",                                true,  0.1205f), // 2026-06-18
            new("Powers/Player/Colossus/GroundStomp.prototype",                                    true,  0.0591f), // 2026-06-18
            new("Powers/Player/Colossus/GroupTaunt.prototype",                                     true,  0.05f),
            new("Powers/Player/Colossus/MagikEldritchArmor.prototype",                             true,  0.05f),
            new("Powers/Player/Colossus/MetalCharge.prototype",                                    true,  0.1599f), // 2026-06-18
            new("Powers/Player/Colossus/MetalRegeneration.prototype",                              true,  0.0091f), // 2026-06-18
            new("Powers/Player/Colossus/MovementSpin.prototype",                                   false, 0.05f),
            new("Powers/Player/Colossus/PickUpTerrain.prototype",                                  true,  0.05f),
            new("Powers/Player/Colossus/Punch.prototype",                                          true,  0.0249f), // 2026-06-18
            new("Powers/Player/Colossus/Shockwave.prototype",                                      true,  0.0590f), // 2026-06-18
            new("Powers/Player/Colossus/Talents/Talent1DamageMultWithNoArmor.prototype",           false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent1HealthToArmorConversionInCombat.prototype", false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent1RegenArmor.prototype",                      false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent2ColossalWhirlwind.prototype",               false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent2NoCallinSpec.prototype",                    false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent2TauntBuff.prototype",                       false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent3CallinSharedCooldown.prototype",            false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent3DeathFromAboveArmorSpend.prototype",        false, 0.0226f), // 2026-06-18
            new("Powers/Player/Colossus/Talents/Talent3GroundStompFissureLayers.prototype",        false, 0.0591f), // 2026-06-18
            new("Powers/Player/Colossus/Talents/Talent4CallInBuffs.prototype",                     false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent4DamageBasedOnArmor.prototype",              false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent4MovementBuildBuffs.prototype",              false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent5ArmorRegeneratesAfterSig.prototype",        false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent5SigCooldownResets.prototype",               false, 0.05f),
            new("Powers/Player/Colossus/Talents/Talent5SigRestoresFullHealth.prototype",           false, 0.05f),
            new("Powers/Player/Colossus/Traits/DefenseTrait.prototype",                            false, 0.05f),
            new("Powers/Player/Colossus/Traits/MechanicTraitArmor.prototype",                      false, 0.05f),
            new("Powers/Player/Colossus/Traits/OffenseTrait.prototype",                            false, 0.05f),
            new("Powers/Player/Colossus/TroyPunch.prototype",                                      true,  0.0291f), // 2026-06-17
            new("Powers/Player/Colossus/Ultimate.prototype",                                       true,  0.006f),
            new("Powers/Player/TravelPower/ColossusSprint.prototype",                              false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ColossusStolenPower.prototype",               false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                        false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                           false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                              false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                    false, 0.05f),
        };
    }
}
