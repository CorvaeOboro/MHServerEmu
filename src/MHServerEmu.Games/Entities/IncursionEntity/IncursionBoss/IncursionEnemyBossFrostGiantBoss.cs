using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// FrostGiantBoss - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossFrostGiantBoss : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/Limbo/LimboEvent05GlacialLordFrostGiantBoss.prototype");

        public IncursionEnemyBossFrostGiantBoss(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Frost Giant Boss Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/FrostGiants/FGIceShaperIceCageSummonMarker.prototype",   true,  1.0f),
            new("Powers/EnemyPowers/MobPowers/FrostGiants/FGShamanIceOrb.prototype",                   true,  1.1897f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                 false, 1.0f),
            new("Powers/EnemyPowers/MobPowers/FrostGiants/FGIceShaperFrostNova.prototype",             true,  1.1708f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/FrostGiants/FGIceShaperIceShard.prototype",              true,  0.9831f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",                false, 1.0f),
            new("Powers/EnemyPowers/MobPowers/FrostGiants/FGSummonBlizzardHotspot.prototype",          true,  1.5193f), // 2026-08-01
        };
    }
}
