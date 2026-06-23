using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Deadpool Invader
    /// Powers: 20 / 47
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyDeadpool : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Deadpool.prototype");

        public IncursionEnemyDeadpool(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Deadpool Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Deadpool/BOTA.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Classic.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/ClassicVU.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/DeadpoolTheKid.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/DeadpoolZenVariant.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Detective.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/LadyDeadpool.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Pirate.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Pulp.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Sheriff.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Ultimate.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Unmasked.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/XForce.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/XMen.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Deadpool/Zen.prototype",                true),
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
            new("Powers/Player/Deadpool/Rework/ArmorBuster.prototype",                 true,  0.0285f), // 2026-06-17
            new("Powers/Player/Deadpool/Rework/AwesomeHiddenPassive.prototype",        false, 0.05f),
            new("Powers/Player/Deadpool/Rework/BangBang.prototype",                    true,  0.05f),
            new("Powers/Player/Deadpool/Rework/BasicBleed.prototype",                  true,  0.1159f), // 2026-06-18
            new("Powers/Player/Deadpool/Rework/Bazooka.prototype",                     true,  0.0367f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/CaltropsRework.prototype",              true,  0.0827f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/DeadlyBarrage.prototype",               true,  0.0318f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/Deadpoolnado.prototype",                true,  0.0206f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/GiantMallet.prototype",                 true,  0.0728f), // 2026-06-17
            new("Powers/Player/Deadpool/Rework/HulkHandArrow.prototype",               true,  0.0812f), // 2026-06-18
            new("Powers/Player/Deadpool/Rework/LilDeadpool.prototype",                 true,  0.05f),
            new("Powers/Player/Deadpool/Rework/Lunge.prototype",                       true,  0.05f),
            new("Powers/Player/Deadpool/Rework/MercWithaMouth.prototype",              true,  0.05f),
            new("Powers/Player/Deadpool/Rework/Minigun.prototype",                     true,  0.05f),
            new("Powers/Player/Deadpool/Rework/OmnislashTeleport.prototype",           true,  0.0438f), // 2026-06-16
            new("Powers/Player/Deadpool/Rework/SaiAssault.prototype",                  true,  0.1335f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/ServerLag.prototype",                   true,  0.0411f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/StabbyFlip.prototype",                  true,  0.0608f), // 2026-06-18
            new("Powers/Player/Deadpool/Rework/Strafe.prototype",                      true,  0.0636f), // 2026-06-20
            new("Powers/Player/Deadpool/Rework/SuperiorHealingFactor.prototype",       true,  0.05f),
            new("Powers/Player/Deadpool/Rework/Teleport.prototype",                    true,  0.05f),
            new("Powers/Player/Deadpool/Talents/BazookaTalent.prototype",              false, 0.0367f), // 2026-06-20
            new("Powers/Player/Deadpool/Talents/BleedEmDryTalent.prototype",           false, 0.05f),
            new("Powers/Player/Deadpool/Talents/GodModeTalent.prototype",              false, 0.05f),
            new("Powers/Player/Deadpool/Talents/GunsGloriousGunsTalent.prototype",     false, 0.05f),
            new("Powers/Player/Deadpool/Talents/HulkHandArrowNapalmTalent.prototype",  false, 0.0812f), // 2026-06-18
            new("Powers/Player/Deadpool/Talents/LilDeadpoolTalent.prototype",          false, 0.05f),
            new("Powers/Player/Deadpool/Talents/MinibossTalent.prototype",             false, 0.05f),
            new("Powers/Player/Deadpool/Talents/MultiplayerTalent.prototype",          false, 0.05f),
            new("Powers/Player/Deadpool/Talents/OrbHealTalent.prototype",              false, 0.05f),
            new("Powers/Player/Deadpool/Talents/PowerUpsTalent.prototype",             false, 0.05f),
            new("Powers/Player/Deadpool/Talents/SelfDestructBangBangTalent.prototype", false, 0.05f),
            new("Powers/Player/Deadpool/Talents/SmellsLikeVictoryTalent.prototype",    false, 0.05f),
            new("Powers/Player/Deadpool/Talents/StrafeExtraDefenseTalent.prototype",   false, 0.0636f), // 2026-06-20
            new("Powers/Player/Deadpool/Talents/StrafeSlamExplosionsTalent.prototype", false, 0.0636f), // 2026-06-20
            new("Powers/Player/Deadpool/Talents/TenTonHammerTalent.prototype",         false, 0.05f),
            new("Powers/Player/Deadpool/Traits/DefenseTrait.prototype",                false, 0.05f),
            new("Powers/Player/Deadpool/Traits/MechanicTraitAwesome.prototype",        false, 0.05f),
            new("Powers/Player/Deadpool/Traits/OffenseTrait.prototype",                false, 0.05f),
            new("Powers/Player/TravelPower/DeadpoolRide.prototype",                    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DeadpoolStolenPower.prototype",   false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",             false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",            false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",               false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                  false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                        false, 0.05f),
        };
    }
}
