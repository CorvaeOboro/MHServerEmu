using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Kaecilius - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossKaecilius : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/KaeciliusBase.prototype");

        public IncursionEnemyBossKaecilius(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Kaecilius Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Kaecilius/PortalsVisualStart.prototype",           true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/KaeciliusSummonMagicOrb.prototype",      true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/DeathFromAboveComboEffect.prototype",    true,  0.9872f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Kaecilius/HealChannelEruptionSummon.prototype",    true,  0.6612f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Kaecilius/HealChannel.prototype",                  true,  0.6612f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Kaecilius/KaeciliusScytheThrow.prototype",         true,  1.0454f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Kaecilius/SummonMirrorImages.prototype",           true,  1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/Portals.prototype",                      false, 1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/ScythePortalBlocker.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/KillMirrorImagesSummon.prototype",       false, 1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/KaeciliusHealthMinPassive.prototype",    false, 1.0f),
            new("Powers/EnemyPowers/Boss/Kaecilius/DeathFromAbove.prototype",               false, 0.9872f), // 2026-07-30
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                      false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",     false, 1.0f),
        };
    }
}
