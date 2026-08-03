using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Nightcrawler Invader
    /// Powers: 15 / 40
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyNightcrawler : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Nightcrawler.prototype");

        public IncursionEnemyNightcrawler(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Nightcrawler Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/AgeOfApocalypse.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/AgeOfApocalypseVariant.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/Classic.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/HouseOfM.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/Modern.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/Nightcrawler/XForce.prototype",                 true),
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
            new("Powers/Player/Nightcrawler/Bamf.prototype",                                      true,  0.0647f), // 2026-06-10
            new("Powers/Player/Nightcrawler/BamfDiveBomb.prototype",                              true,  0.0416f), // 2026-07-14
            new("Powers/Player/Nightcrawler/BamfFrenzy.prototype",                                true,  0.0453f), // 2026-07-14
            new("Powers/Player/Nightcrawler/BamfYank.prototype",                                  true,  0.1639f), // 2026-06-10
            new("Powers/Player/Nightcrawler/BasicPunch.prototype",                                true,  0.1338f), // 2026-06-10
            new("Powers/Player/Nightcrawler/BasicStealthPunch.prototype",                         true,  0.2359f), // 2026-06-10
            new("Powers/Player/Nightcrawler/BasicSwordSlash.prototype",                           true,  0.2134f), // 2026-06-10
            new("Powers/Player/Nightcrawler/BrimstoneBlitz.prototype",                            true,  0.0744f), // 2026-06-10
            new("Powers/Player/Nightcrawler/DoubleSlash.prototype",                               true,  0.1748f), // 2026-06-08
            new("Powers/Player/Nightcrawler/Execute.prototype",                                   true,  0.0502f), // 2026-07-14
            new("Powers/Player/Nightcrawler/NewUltimate.prototype",                               true,  0.0151f), // 2026-07-01
            new("Powers/Player/Nightcrawler/PBAoESwordSlash.prototype",                           true,  0.1372f), // 2026-06-10
            new("Powers/Player/Nightcrawler/SwordPummel.prototype",                               true,  0.0507f), // 2026-07-14
            new("Powers/Player/Nightcrawler/Talents/BackstabStealthTalent.prototype",             false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/FlourishDeflectChanceTalent.prototype",       false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/FlourishTeleportComboTalent.prototype",       false, 0.025f),
            new("Powers/Player/Nightcrawler/Talents/InfernalBrimstoneTalent.prototype",           false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/ObscuringBrimstoneTalent.prototype",          false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/ShadowmeldTalent.prototype",                  false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/SigCooldownReduction.prototype",              false, 0.02f),
            new("Powers/Player/Nightcrawler/Talents/SignatureRestoreTalent.prototype",            false, 0.02f),
            new("Powers/Player/Nightcrawler/Talents/Talent1BamfDiveBombStealthBuff.prototype",    false, 0.0483f), // 2026-07-01
            new("Powers/Player/Nightcrawler/Talents/Talent1FlashandGrabCooldown.prototype",       false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/Talent1FlourishPowerCooldownReset.prototype", false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/Talent3SwordPowerTweaks.prototype",           false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/Talent5FlourishBuff.prototype",               false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/TeleportDamageBuffTalent.prototype",          false, 0.05f),
            new("Powers/Player/Nightcrawler/Talents/TeleportDodgeChanceTalent.prototype",         false, 0.05f),
            new("Powers/Player/Nightcrawler/TeleportBackstab.prototype",                          true,  0.0390f), // 2026-07-14
            new("Powers/Player/Nightcrawler/Traits/DefenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Nightcrawler/Traits/OffenseTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Nightcrawler/ValiantLeap.prototype",                               true,  0.0448f), // 2026-07-14
            new("Powers/Player/TravelPower/NightcrawlerSprint.prototype",                         false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/NightcrawlerStolenPower.prototype",          false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                   false, 0.05f),
        };
    }
}
