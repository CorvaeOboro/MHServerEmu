using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Spiderman Invader
    /// Powers: 19 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemySpiderman : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Spiderman.prototype");

        public IncursionEnemySpiderman(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Spiderman Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Spiderman/Amazing.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/AmazingBagMan.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/BackInBlack.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/BattleDamaged.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/BigTime.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/BigTimeBlue.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/BigTimeGreen.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/CivilWarMovie.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/EndsOfTheEarth.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/FFInverted.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/FutureFoundation.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Homecoming.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Homemade.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/HomemadeHoodUp.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/IronSpider.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/MilesMorales.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Modern.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Noir.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/ScarletSpider.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Sensational.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/SpiderGirl.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/SpiderGwen.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Superior.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Spiderman/Symbiote.prototype",         true),
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
            new("Powers/Player/Spiderman/Rework/AmazingSmash.prototype",                          true,  0.05f),
            new("Powers/Player/Spiderman/Rework/BasicRanged.prototype",                           true,  0.05f),
            new("Powers/Player/Spiderman/Rework/Cocoon.prototype",                                true,  0.0578f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/DisengagingShot.prototype",                       true,  0.05f),
            new("Powers/Player/Spiderman/Rework/DiveKick.prototype",                              true,  0.0481f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/FuriousLungeFlip.prototype",                      true,  0.05f),
            new("Powers/Player/Spiderman/Rework/FuriousLungeWebZip.prototype",                    true,  0.05f),
            new("Powers/Player/Spiderman/Rework/RapidFire.prototype",                             true,  0.05f),
            new("Powers/Player/Spiderman/Rework/Slingshot.prototype",                             true,  0.0396f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/SpiderBamf.prototype",                            true,  0.0687f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/SpiderCombo.prototype",                           true,  0.025f),
            new("Powers/Player/Spiderman/Rework/Uppercut.prototype",                              true,  0.05f),
            new("Powers/Player/Spiderman/Rework/WebSpray.prototype",                              true,  0.0324f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/WebWhirlwind.prototype",                          true,  0.0369f), // 2026-06-17
            new("Powers/Player/Spiderman/Rework/Wrap.prototype",                                  true,  0.0357f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent1MeleeBuff.prototype",                     false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent1MovementBuff.prototype",                  false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent1RangedBuff.prototype",                    false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent2CocoonBuff.prototype",                    false, 0.0578f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent2DiveKickDisengageCDRWebSwing.prototype",  false, 0.0481f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent2StickAroundintoWebYank.prototype",        false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent3DiveKickDisengageCharges.prototype",      false, 0.0481f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent3SpiderBamfDodgeBuff.prototype",           false, 0.0687f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent3WebEmAllBuff.prototype",                  false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent4WbZipAcrobaticAttackChargeCDR.prototype", false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent4WebSlingCharge.prototype",                false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent4WebWhirlwindBuff.prototype",              false, 0.0369f), // 2026-06-17
            new("Powers/Player/Spiderman/Talents/Talent5AmazingSmashDmgBuffNoDoT.prototype",      false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent5AmazingSmashDodgeTauntBuff.prototype",    false, 0.05f),
            new("Powers/Player/Spiderman/Talents/Talent5AmazingSmashKnockbackDoTBuff.prototype",  false, 0.05f),
            new("Powers/Player/Spiderman/Taunt.prototype",                                        true,  0.05f),
            new("Powers/Player/Spiderman/Traits/DefenseTrait.prototype",                          false, 0.05f),
            new("Powers/Player/Spiderman/Traits/MechanicTraitWebFluid.prototype",                 false, 0.05f),
            new("Powers/Player/Spiderman/Traits/OffenseTrait.prototype",                          false, 0.05f),
            new("Powers/Player/Spiderman/Ultimate.prototype",                                     true,  0.0103f), // 2026-06-17
            new("Powers/Player/Spiderman/UltimateHiddenPassive.prototype",                        false, 0.0103f), // 2026-06-17
            new("Powers/Player/Spiderman/WebSplat.prototype",                                     true,  0.05f),
            new("Powers/Player/Spiderman/WebSwing.prototype",                                     true,  0.05f),
            new("Powers/Player/TravelPower/SpidermanFlight.prototype",                            false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/SpidermanStolenPower.prototype",             false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                   false, 0.05f),
        };
    }
}
