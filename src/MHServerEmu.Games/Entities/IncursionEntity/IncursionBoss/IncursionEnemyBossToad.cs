using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Toad - story boss (ToadOMCH6), uses MarvelAgent_Toad art.
    /// Has HitReactCondition and ToadStolenPower. Rank is ShowdownBoss.
    /// Standard boss controller; powers harvested from native power collection after spawn.
    /// </summary>
    public class IncursionEnemyBossToad : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/ToadOMCH6.prototype");

        public IncursionEnemyBossToad(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Toad Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Toad/ToadLeapAnticipation.prototype",            true,  0.7588f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Toad/ToadSwarm.prototype",                       true,  29.1738f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Toad/ToadTongueSS.prototype",                    true,  1.0f),
            new("Powers/EnemyPowers/Emotes/ToadFidget.prototype",                         true,  1.0f),
            new("Powers/EnemyPowers/Boss/Toad/ToadLeap.prototype",                        false, 0.7588f), // 2026-07-30
            new("Powers/EnemyPowers/Boss/Toad/ToadLeapEnd.prototype",                     false, 1.0966f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/Toad/ToadLeapGasSummon.prototype",               false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                    false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",   false, 1.0f),
        };
    }
}
