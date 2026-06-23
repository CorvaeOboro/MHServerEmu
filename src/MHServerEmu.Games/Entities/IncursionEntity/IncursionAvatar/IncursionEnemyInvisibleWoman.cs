using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// InvisibleWoman Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyInvisibleWoman : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/InvisibleWoman.prototype");

        public IncursionEnemyInvisibleWoman(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "InvisibleWoman Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/InvisibleWoman/AllNewMarvelNOW.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/InvisibleWoman/Classic.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/InvisibleWoman/FFInverted.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/InvisibleWoman/FutureFoundation.prototype", true),
            new("Entity/Items/Costumes/Prototypes/InvisibleWoman/UltimateFF.prototype",       true),
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
            new("Powers/Player/InvisibleWoman/BoomerangBubble.prototype",                  true,  0.1579f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/BouncingBubble.prototype",                   true,  0.1514f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/BubbleSpray.prototype",                      true,  0.0492f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/DefenseHotspot.prototype",                   true,  0.008f),
            new("Powers/Player/InvisibleWoman/ForceDash.prototype",                        true,  0.1399f), // 2026-06-18
            new("Powers/Player/InvisibleWoman/ForcePillar.prototype",                      true,  0.0820f), // 2026-06-18
            new("Powers/Player/InvisibleWoman/ForceWall.prototype",                        true,  0.05f),
            new("Powers/Player/InvisibleWoman/ImplodeExplode.prototype",                   true,  0.0307f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/Invisibility.prototype",                     true,  0.05f),
            new("Powers/Player/InvisibleWoman/OrbStorm.prototype",                         true,  0.1211f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/Pancake.prototype",                          true,  0.1092f), // 2026-06-18
            new("Powers/Player/InvisibleWoman/SeekerOrbs.prototype",                       true,  0.0695f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/ShieldedFist.prototype",                     true,  0.0928f), // 2026-06-16
            new("Powers/Player/InvisibleWoman/ShockwavePBAoE.prototype",                   true,  0.0633f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/StealthToggle.prototype",                    false, 0.05f),
            new("Powers/Player/InvisibleWoman/Suffocate.prototype",                        true,  0.0736f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/SummonSpikedBall.prototype",                 true,  0.05f),
            new("Powers/Player/InvisibleWoman/Talents/AOESpamBuff.prototype",              false, 0.03f),
            new("Powers/Player/InvisibleWoman/Talents/AppliedBarrier.prototype",           false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/DefenseHotspotBuff.prototype",       false, 0.008f),
            new("Powers/Player/InvisibleWoman/Talents/Focus.prototype",                    false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/ForcePillarBonus.prototype",         false, 0.0820f), // 2026-06-18
            new("Powers/Player/InvisibleWoman/Talents/ForceWallBonus.prototype",           false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/InvisibilityAutoProc.prototype",     false, 0.025f),
            new("Powers/Player/InvisibleWoman/Talents/InvisibilityStackingBuff.prototype", false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/OutOfCombatStealth.prototype",       false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/PainfulForce.prototype",             false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/SeekerOrbsBonus.prototype",          false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/ShockwavePBAOEBonus.prototype",      false, 0.0633f), // 2026-06-19
            new("Powers/Player/InvisibleWoman/Talents/SpikedBallChanneled.prototype",      false, 0.025f),
            new("Powers/Player/InvisibleWoman/Talents/SpikedBallSkillshot.prototype",      false, 0.05f),
            new("Powers/Player/InvisibleWoman/Talents/StealthySupport.prototype",          false, 0.05f),
            new("Powers/Player/InvisibleWoman/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/InvisibleWoman/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/InvisibleWoman/UltimateBubblestorm.prototype",              true,  0.0140f), // 2026-06-19
            new("Powers/Player/TravelPower/InvisibleWomanFlight.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/InvisibleWomanStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                   false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                      false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                            false, 0.05f),
        };
    }
}
