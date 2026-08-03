using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Batroc - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossBatroc : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Batroc.prototype");

        public IncursionEnemyBossBatroc(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Batroc Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 150f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 2000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Batroc/DoubleKick.prototype",                    true,  0.9690f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/FuriousLungeStart.prototype",             true,  1.0f),
            new("Powers/EnemyPowers/Boss/Batroc/RoundhouseKick.prototype",                true,  0.8183f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/TripleKick.prototype",                    true,  0.8869f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/BrutalStrike.prototype",                  false, 1.5092f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Batroc/BrutalStrikeEffect.prototype",            false, 1.5092f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Batroc/DoubleKick2ndHit.prototype",              false, 0.9690f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/FlipKick.prototype",                      false, 1.0f),
            new("Powers/EnemyPowers/Boss/Batroc/FlipKickComboEffect.prototype",           false, 1.0f),
            new("Powers/EnemyPowers/Boss/Batroc/FuriousLunge.prototype",                  false, 1.5183f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/FuriousLungeEnd.prototype",               false, 1.0f),
            new("Powers/EnemyPowers/Boss/Batroc/TripleKick2ndHit.prototype",              false, 0.8869f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Batroc/TripleKick3rdHit.prototype",              false, 1.1424f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Batroc/TripleKick4thHit.prototype",              false, 0.8869f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
