using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Elektra Invader
    /// Powers: 18 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyElektra : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Elektra.prototype");

        public IncursionEnemyElektra(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Elektra Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Elektra/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Elektra/TVMDDS2.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Elektra/TVMDDS2Alternate.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Elektra/Ultimate.prototype",         true),
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
            new("Powers/Player/Elektra/Assassinate.prototype",                                true,  0.0180f), // 2026-06-16
            new("Powers/Player/Elektra/BamfDiveBomb.prototype",                               true,  0.0213f), // 2026-06-16
            new("Powers/Player/Elektra/BasicSai.prototype",                                   true,  0.1227f), // 2026-06-16
            new("Powers/Player/Elektra/BlowDart.prototype",                                   true,  0.1173f), // 2026-06-17
            new("Powers/Player/Elektra/CrossStrike.prototype",                                true,  0.0119f), // 2026-06-17
            new("Powers/Player/Elektra/KillCommand.prototype",                                true,  0.05f),
            new("Powers/Player/Elektra/KnifeRopeChain.prototype",                             true,  0.0331f), // 2026-06-17
            new("Powers/Player/Elektra/KnifeThrow.prototype",                                 true,  0.0175f), // 2026-06-16
            new("Powers/Player/Elektra/MarkForDeath.prototype",                               true,  0.05f),
            new("Powers/Player/Elektra/SaiStrike.prototype",                                  true,  0.0893f), // 2026-06-16
            new("Powers/Player/Elektra/ShadowStrike.prototype",                               true,  0.0515f), // 2026-06-17
            new("Powers/Player/Elektra/SpinningStrike.prototype",                             true,  0.0630f), // 2026-06-16
            new("Powers/Player/Elektra/StaffStrike.prototype",                                true,  0.1198f), // 2026-06-16
            new("Powers/Player/Elektra/Stealth.prototype",                                    true,  0.05f),
            new("Powers/Player/Elektra/Talents/CooldownResetOnKillMark.prototype",            false, 0.05f),
            new("Powers/Player/Elektra/Talents/KillCommandStealthTalent.prototype",           false, 0.05f),
            new("Powers/Player/Elektra/Talents/KnifeRopeMastery.prototype",                   false, 0.05f),
            new("Powers/Player/Elektra/Talents/KnifeThrowAssassinateStealthTalent.prototype", false, 0.05f),
            new("Powers/Player/Elektra/Talents/MarkSpreadingNoCharges.prototype",             false, 0.05f),
            new("Powers/Player/Elektra/Talents/NinjaMysticAlly.prototype",                    false, 0.05f),
            new("Powers/Player/Elektra/Talents/NinjaWarriorAllies.prototype",                 false, 0.05f),
            new("Powers/Player/Elektra/Talents/ProjectileMastery.prototype",                  false, 0.05f),
            new("Powers/Player/Elektra/Talents/SansetsukonMastery.prototype",                 false, 0.05f),
            new("Powers/Player/Elektra/Talents/ShadowStrikeDiveBombAutoMark.prototype",       false, 0.0515f), // 2026-06-17
            new("Powers/Player/Elektra/Talents/SilentScreamTalent.prototype",                 false, 0.05f),
            new("Powers/Player/Elektra/Talents/StealthMarkTalent.prototype",                  false, 0.05f),
            new("Powers/Player/Elektra/Talents/StealthNoBreakTalent.prototype",               false, 0.05f),
            new("Powers/Player/Elektra/Talents/StealthTeamBuffTalent.prototype",              false, 0.05f),
            new("Powers/Player/Elektra/Talents/TripleChainCDReset.prototype",                 false, 0.1280f), // 2026-06-16
            new("Powers/Player/Elektra/TeleportDash.prototype",                               true,  0.05f),
            new("Powers/Player/Elektra/ThrowShuriken.prototype",                              true,  0.1081f), // 2026-06-17
            new("Powers/Player/Elektra/Traits/DefenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Elektra/Traits/MechanicTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Elektra/Traits/OffenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Elektra/TripleChain.prototype",                                true,  0.1280f), // 2026-06-16
            new("Powers/Player/Elektra/Ultimate.prototype",                                   true,  0.0053f), // 2026-06-16
            new("Powers/Player/Elektra/UltimateHiddenPassive.prototype",                      false, 0.0053f), // 2026-06-16
            new("Powers/Player/TravelPower/ElektraSprint.prototype",                          false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ElektraStolenPower.prototype",           false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                           false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                   false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                      false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                         false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                               false, 0.05f),
        };
    }
}
