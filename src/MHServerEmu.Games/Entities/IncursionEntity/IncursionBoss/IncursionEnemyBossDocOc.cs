using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// DocOc - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossDocOc : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/XMansionDefense/XMansionDefenseEventDocOctopus.prototype");

        public IncursionEnemyBossDocOc(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Doc Oc Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleSlamStart.prototype",            true,  1.1130f), // 2026-07-25
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleSwipe.prototype",                true,  0.9807f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleThrow.prototype",                true,  1.3620f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleSlam.prototype",                 false, 1.1130f), // 2026-07-25
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleSlamKnockdownCombo.prototype",   false, 1.1130f), // 2026-07-25
            new("Powers/EnemyPowers/Boss/DocOc/DocOcTentacleThrowEnd.prototype",             false, 1.3620f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",      false, 1.0f),
        };
    }
}
