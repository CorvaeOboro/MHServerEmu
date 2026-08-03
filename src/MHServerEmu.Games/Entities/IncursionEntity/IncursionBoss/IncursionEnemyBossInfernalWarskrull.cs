using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Infernal War Skrull - Hightown patrol boss, uses MarvelAgent_InfernalWarSkrull art.
    /// Parent chain goes through mob InfernalWarSkrull, not Boss.defaults.
    /// Has a custom loot table. Standard controller with AI override.
    /// </summary>
    public class IncursionEnemyBossInfernalWarskrull : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolHightown/HightownEventIncursionInfernalWarskrull.prototype");

        public IncursionEnemyBossInfernalWarskrull(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Infernal War Skrull Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/MobPowers/Skrulls/InfernalWarSkrull/ChainMelee.prototype",                 true,  0.8990f), // 2026-07-30
            new("Powers/EnemyPowers/MobPowers/Skrulls/InfernalWarSkrull/FlameCircle.prototype",                true,  0.9131f), // 2026-07-30
            new("Powers/EnemyPowers/MobPowers/Skrulls/InfernalWarSkrull/FlameCircleSummonLocusCombo.prototype", false, 0.9131f), // 2026-07-30
            new("Powers/EnemyPowers/MobPowers/Skrulls/InfernalWarSkrull/InfernalDecoy.prototype",              true,  5.4189f), // 2026-07-30
        };
    }
}
