using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// BlackWidow Invader
    /// Powers: 19 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyBlackWidow : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/BlackWidow.prototype");

        public IncursionEnemyBlackWidow(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "BlackWidow Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/BlackWidow/AgeOfUltronComic.prototype", true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/AgeOfUltronMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/Avengers.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/AvengersMH2013.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/CivilWar.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/ClassicMH2013.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/ClassicWhite.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/FearItself.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/GrayBodysuit.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/Original.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/BlackWidow/Thunderbolts.prototype",     true),
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
            new("Powers/Player/BlackWidow/CoupDeGrace.prototype",                           true,  0.0387f), // 2026-06-20
            new("Powers/Player/BlackWidow/ElectricBatons.prototype",                        true,  0.0910f), // 2026-06-16
            new("Powers/Player/BlackWidow/FlashGrenade.prototype",                          true,  0.2636f), // 2026-06-20
            new("Powers/Player/BlackWidow/FlipKick.prototype",                              true,  0.0283f), // 2026-06-20
            new("Powers/Player/BlackWidow/Microdrones.prototype",                           true,  0.0590f), // 2026-06-20
            new("Powers/Player/BlackWidow/PBAoETaser.prototype",                            true,  0.0243f), // 2026-06-17
            new("Powers/Player/BlackWidow/PistolShot.prototype",                            true,  0.1586f), // 2026-06-20
            new("Powers/Player/BlackWidow/Plastique.prototype",                             true,  0.0712f), // 2026-06-20
            new("Powers/Player/BlackWidow/Punch.prototype",                                 true,  0.0647f), // 2026-06-20
            new("Powers/Player/BlackWidow/RapidShot.prototype",                             true,  0.1633f), // 2026-06-20
            new("Powers/Player/BlackWidow/RollingGrenades.prototype",                       true,  0.1286f), // 2026-06-17
            new("Powers/Player/BlackWidow/RoundhouseKick.prototype",                        true,  0.0332f), // 2026-06-20
            new("Powers/Player/BlackWidow/SniperShot.prototype",                            true,  0.05f),
            new("Powers/Player/BlackWidow/SweepingKick.prototype",                          true,  0.0737f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/FightingFocus.prototype",                 false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/FlashGrenadeConductiveGrenade.prototype", false, 0.2636f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/FlipKickExplosives.prototype",            false, 0.0283f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/MicrodronesSecondWave.prototype",         false, 0.0590f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/NeverKnowWhatHitThem.prototype",          false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/PBAoEChargeFullSpend.prototype",          false, 0.03f),
            new("Powers/Player/BlackWidow/Talents/PunchElectricBatons.prototype",           false, 0.0910f), // 2026-06-16
            new("Powers/Player/BlackWidow/Talents/PunchKnife.prototype",                    false, 0.0647f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/PunchStingProc.prototype",                false, 0.0647f), // 2026-06-20
            new("Powers/Player/BlackWidow/Talents/RollingGrenadesBonus.prototype",          false, 0.01f),
            new("Powers/Player/BlackWidow/Talents/SniperNest.prototype",                    false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleAcrobaticAttack.prototype",         false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleHaste.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/TumbleKineticBattery.prototype",          false, 0.05f),
            new("Powers/Player/BlackWidow/Talents/WidowsBootSpec.prototype",                false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/MechanicTraitElectricCharge.prototype",    false, 0.05f),
            new("Powers/Player/BlackWidow/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/BlackWidow/Tumble.prototype",                                true,  0.05f),
            new("Powers/Player/BlackWidow/TwilightInitiative.prototype",                    true,  0.0346f), // 2026-06-20
            new("Powers/Player/BlackWidow/Ultimate.prototype",                              true,  0.0251f), // 2026-06-20
            new("Powers/Player/BlackWidow/WidowsBite.prototype",                            true,  0.0427f), // 2026-06-20
            new("Powers/Player/BlackWidow/WidowsKiss.prototype",                            true,  0.0716f), // 2026-06-20
            new("Powers/Player/TravelPower/BlackWidowRide.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/BlackWidowStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
