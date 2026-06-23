using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// GreenGoblin Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyGreenGoblin : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/GreenGoblin.prototype");

        public IncursionEnemyGreenGoblin(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "GreenGoblin Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/GreenGoblin/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/GreenGoblin/JackOLantern.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/GreenGoblin/MarvelKnights.prototype", true),
            new("Entity/Items/Costumes/Prototypes/GreenGoblin/Thunderbolts.prototype",  true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.0333f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/GreenGoblin/BombingCircle.prototype",                       true,  0.0153f), // 2026-06-19
            new("Powers/Player/GreenGoblin/Dash.prototype",                                true,  0.1116f), // 2026-06-19
            new("Powers/Player/GreenGoblin/DeathFromAboveV2.prototype",                    true,  0.0496f), // 2026-06-19
            new("Powers/Player/GreenGoblin/ExplosivePumpkinBase.prototype",                true,  0.1308f), // 2026-06-19
            new("Powers/Player/GreenGoblin/FlyingFlamethrower.prototype",                  false, 0.05f),
            new("Powers/Player/GreenGoblin/GasPumpkin.prototype",                          true,  0.0971f), // 2026-06-18
            new("Powers/Player/GreenGoblin/GhostBomb.prototype",                           true,  0.1182f), // 2026-06-19
            new("Powers/Player/GreenGoblin/GoblinBlast.prototype",                         true,  0.1028f), // 2026-06-19
            new("Powers/Player/GreenGoblin/GoblinCannon.prototype",                        true,  0.0076f), // 2026-06-19
            new("Powers/Player/GreenGoblin/GoblinLaser.prototype",                         true,  0.0764f), // 2026-06-19
            new("Powers/Player/GreenGoblin/HallucinogenicPumpkin.prototype",               true,  0.0784f), // 2026-06-18
            new("Powers/Player/GreenGoblin/MachineGuns.prototype",                         true,  0.0366f), // 2026-06-19
            new("Powers/Player/GreenGoblin/PbAoESpin.prototype",                           true,  0.1224f), // 2026-06-19
            new("Powers/Player/GreenGoblin/RazorBat.prototype",                            true,  0.1533f), // 2026-06-18
            new("Powers/Player/GreenGoblin/Rockets.prototype",                             true,  0.1396f), // 2026-06-19
            new("Powers/Player/GreenGoblin/SonicToads.prototype",                          true,  0.1140f), // 2026-06-19
            new("Powers/Player/GreenGoblin/Talents/ExplosivePumpkinInnerDamage.prototype", false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/GasPumpkinIgniteTalent.prototype",      false, 0.0971f), // 2026-06-18
            new("Powers/Player/GreenGoblin/Talents/GliderBladeTalent.prototype",           false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/GoblinBlastExtraHitTalent.prototype",   false, 0.1028f), // 2026-06-19
            new("Powers/Player/GreenGoblin/Talents/GoblinLaserDamageMultTalent.prototype", false, 0.0764f), // 2026-06-19
            new("Powers/Player/GreenGoblin/Talents/HallucinogenicPumpkinTalent.prototype", false, 0.0784f), // 2026-06-18
            new("Powers/Player/GreenGoblin/Talents/MoreGhostsTalent.prototype",            false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/MoreToadsTalent.prototype",             false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/NitrousTalent.prototype",               false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/RazorBatsBleed.prototype",              false, 0.1533f), // 2026-06-18
            new("Powers/Player/GreenGoblin/Talents/SignatureMovementBuffTalent.prototype", false, 0.02f),
            new("Powers/Player/GreenGoblin/Talents/SignatureResistanceTalent.prototype",   false, 0.02f),
            new("Powers/Player/GreenGoblin/Talents/SignatureRestoreTalent.prototype",      false, 0.02f),
            new("Powers/Player/GreenGoblin/Talents/SuperSpinTalent.prototype",             false, 0.05f),
            new("Powers/Player/GreenGoblin/Talents/TheBigOneBuffTalent.prototype",         false, 0.05f),
            new("Powers/Player/GreenGoblin/TheBigOneSummon.prototype",                     true,  0.0494f), // 2026-06-19
            new("Powers/Player/GreenGoblin/Traits/DefenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/GreenGoblin/Traits/OffenseTrait.prototype",                 false, 0.05f),
            new("Powers/Player/TravelPower/GreenGoblinFlight.prototype",                   false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/GreenGoblinStolenPower.prototype",    false, 0.05f),
            new("Powers/SynergyPowers/SynergyGreenGoblinBonusDamageMoving.prototype",      true,  0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                   false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                      false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                            false, 0.05f),
        };
    }
}
