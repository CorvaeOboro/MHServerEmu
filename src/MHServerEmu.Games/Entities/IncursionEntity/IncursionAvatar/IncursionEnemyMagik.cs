using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Magik Invader
    /// Powers: 18 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyMagik : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Magik.prototype");

        public IncursionEnemyMagik(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Magik Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Magik/MarvelNOW.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Magik/NewMutants.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Magik/PhoenixForce.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Magik/SoulArmor.prototype",    true),
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
            new("Powers/Player/Magik/Assassinate.prototype",                              true,  0.0825f), // 2026-06-18
            new("Powers/Player/Magik/BoneSpirit.prototype",                               true,  0.05f),
            new("Powers/Player/Magik/BoneSpiritHiddenPassive.prototype",                  false, 0.05f),
            new("Powers/Player/Magik/BoneWall.prototype",                                 true,  0.05f),
            new("Powers/Player/Magik/BounceStrikeStart.prototype",                        true,  0.0507f), // 2026-06-18
            new("Powers/Player/Magik/DarkPact.prototype",                                 true,  0.05f),
            new("Powers/Player/Magik/LifeTap.prototype",                                  true,  0.0819f), // 2026-06-20
            new("Powers/Player/Magik/LifeTapHiddenPassive.prototype",                     false, 0.0819f), // 2026-06-20
            new("Powers/Player/Magik/OtherworldlyNova.prototype",                         true,  0.0144f), // 2026-06-20
            new("Powers/Player/Magik/SorcerousEruption.prototype",                        true,  0.0739f), // 2026-06-16
            new("Powers/Player/Magik/SoulCapture.prototype",                              true,  0.0461f), // 2026-06-20
            new("Powers/Player/Magik/SoulCone.prototype",                                 true,  0.0992f), // 2026-06-18
            new("Powers/Player/Magik/SoulShockwave.prototype",                            true,  0.0552f), // 2026-06-18
            new("Powers/Player/Magik/SoulswordBasic.prototype",                           true,  0.1083f), // 2026-06-17
            new("Powers/Player/Magik/SoulswordWideSlash.prototype",                       true,  0.1001f), // 2026-06-20
            new("Powers/Player/Magik/SummonLimboDemon.prototype",                         true,  0.0466f), // 2026-06-20
            new("Powers/Player/Magik/SummonNastirh.prototype",                            true,  0.0556f), // 2026-06-20
            new("Powers/Player/Magik/Talents/Talent1LimboDemonIntoSpitter.prototype",     false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent1SoulConeLayer.prototype",             false, 0.0992f), // 2026-06-18
            new("Powers/Player/Magik/Talents/Talent1SoulConeProjectiles.prototype",       false, 0.0992f), // 2026-06-18
            new("Powers/Player/Magik/Talents/Talent2LifeTapAmpDamage.prototype",          false, 0.0819f), // 2026-06-20
            new("Powers/Player/Magik/Talents/Talent2LifeTapConfuse.prototype",            false, 0.0819f), // 2026-06-20
            new("Powers/Player/Magik/Talents/Talent2LifeTapWeaken.prototype",             false, 0.0819f), // 2026-06-20
            new("Powers/Player/Magik/Talents/Talent3MagicalProjection.prototype",         false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent3NastirthIntoBFLD.prototype",          false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent3SteppingMastery.prototype",           false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent4AssassinateSoulCollection.prototype", false, 0.0825f), // 2026-06-18
            new("Powers/Player/Magik/Talents/Talent4AutoBoneSpirit.prototype",            false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent4BloodSpirit.prototype",               false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent5DarkPactIntoDarkAlliance.prototype",  false, 0.05f),
            new("Powers/Player/Magik/Talents/Talent5OtherworldlyNovaDemonBuff.prototype", false, 0.0144f), // 2026-06-20
            new("Powers/Player/Magik/Talents/Talent5ReviveEnslavedMinionBuff.prototype",  false, 0.05f),
            new("Powers/Player/Magik/Teleport.prototype",                                 true,  0.1427f), // 2026-06-18
            new("Powers/Player/Magik/TeleportOther.prototype",                            true,  0.0784f), // 2026-06-20
            new("Powers/Player/Magik/Traits/DefenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/Magik/Traits/OffenseTrait.prototype",                      false, 0.05f),
            new("Powers/Player/Magik/Ultimate.prototype",                                 true,  0.0118f), // 2026-06-20
            new("Powers/Player/TravelPower/MagikFlight.prototype",                        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/MagikStolenPower.prototype",         false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
        };
    }
}
