using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// RockTrollBerserker - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossRockTrollBerserker : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Mobs/RockTrolls/RockTrollBerserkerBase.prototype");

        public IncursionEnemyBossRockTrollBerserker(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Rock Troll Berserker Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerMeleeStrike1.prototype",   true,  0.8396f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerMeleeStrike2.prototype",   true,  0.8396f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerSpinAttack.prototype",     true,  0.9980f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerMeleeStrike3.prototype",   false, 0.8396f), // 2026-08-01
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerSlashHit.prototype",       false, 1.4752f), // 2026-07-28
            new("Powers/EnemyPowers/MobPowers/RockTrolls/RockTrollBerserkerSlashStart.prototype",     false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",               false, 1.0f),
        };
    }
}
