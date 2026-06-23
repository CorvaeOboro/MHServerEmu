using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// IronFist Invader
    /// Powers: 11 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyIronFist : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/IronFist.prototype");

        public IncursionEnemyIronFist(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "IronFist Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/IronFist/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/IronFist/HeroesForHire.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/IronFist/Immortal.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/IronFist/ImmortalVariant.prototype", true),
            new("Entity/Items/Costumes/Prototypes/IronFist/Netflix.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/IronFist/Skrull.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/IronFist/White.prototype",           true),
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
            new("Powers/Player/IronFist/BlackBlackPoisonTouch.prototype",                           true,  0.0321f), // 2026-06-17
            new("Powers/Player/IronFist/ChiBlast.prototype",                                        true,  0.0529f), // 2026-06-17
            new("Powers/Player/IronFist/ChiBurst.prototype",                                        true,  0.0950f), // 2026-06-17
            new("Powers/Player/IronFist/ChiMastery.prototype",                                      true,  0.05f),
            new("Powers/Player/IronFist/CraneStance.prototype",                                     false, 0.05f),
            new("Powers/Player/IronFist/DragonSliceStance.prototype",                               false, 0.05f),
            new("Powers/Player/IronFist/FlyingKick.prototype",                                      false, 0.05f),
            new("Powers/Player/IronFist/IronFistPunchStartMove.prototype",                          true,  0.0139f), // 2026-06-17
            new("Powers/Player/IronFist/KunlunStrike.prototype",                                    true,  0.1386f), // 2026-06-13
            new("Powers/Player/IronFist/LeopardSlashStance.prototype",                              false, 0.05f),
            new("Powers/Player/IronFist/NinjutsuDash.prototype",                                    true,  0.1373f), // 2026-06-17
            new("Powers/Player/IronFist/Pummel.prototype",                                          true,  0.1220f), // 2026-06-13
            new("Powers/Player/IronFist/SevenSidedStrike.prototype",                                true,  0.0212f), // 2026-06-17
            new("Powers/Player/IronFist/ShaolinStrike.prototype",                                   true,  0.0854f), // 2026-06-17
            new("Powers/Player/IronFist/SnakeStance.prototype",                                     false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent1OpenerCDR.prototype",                        false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent1OpenerDamageMult.prototype",                 false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent1StancePassiveBoost.prototype",               false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent2ComboChiBurst.prototype",                    false, 0.0950f), // 2026-06-17
            new("Powers/Player/IronFist/Talents/Talent2ComboFlow.prototype",                        false, 0.025f),
            new("Powers/Player/IronFist/Talents/Talent2PummelDamageMult.prototype",                 false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent3OpenerChiBurstCombo.prototype",              false, 0.0950f), // 2026-06-17
            new("Powers/Player/IronFist/Talents/Talent3ShaolinStrikeBonusDamageIncrease.prototype", false, 0.0854f), // 2026-06-17
            new("Powers/Player/IronFist/Talents/Talent3StanceComboMult.prototype",                  false, 0.025f),
            new("Powers/Player/IronFist/Talents/Talent4ChiOverload.prototype",                      false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent4ChiPunch.prototype",                         false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent4HarmonyChi.prototype",                       false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent5DualStance.prototype",                       false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent5FiveStance.prototype",                       false, 0.05f),
            new("Powers/Player/IronFist/Talents/Talent5SingleStance.prototype",                     false, 0.05f),
            new("Powers/Player/IronFist/TigerClawStance.prototype",                                 false, 0.05f),
            new("Powers/Player/IronFist/Traits/DefenseTrait.prototype",                             false, 0.05f),
            new("Powers/Player/IronFist/Traits/MechanicTraitChi.prototype",                         false, 0.05f),
            new("Powers/Player/IronFist/Traits/OffenseTrait.prototype",                             false, 0.05f),
            new("Powers/Player/IronFist/Ultimate.prototype",                                        true,  0.0085f), // 2026-06-17
            new("Powers/Player/TravelPower/IronFistSprint.prototype",                               false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IronFistStolenPower.prototype",                false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                          false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                                 false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                         false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                            false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                     false, 0.05f),
        };
    }
}
