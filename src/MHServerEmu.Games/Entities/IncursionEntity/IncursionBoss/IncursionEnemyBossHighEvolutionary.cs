using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// High Evolutionary - Savage Land patrol boss, uses MarvelAgent_HighEvolutionary art.
    /// Immobile (Speed 0, WalkSpeed 0) like Onslaught; parent is RaidBoss.defaults.
    /// Has OnslaughtStolenPower and PlayDramaticEntrance. The controller drives
    /// behavior through the think loop since native AI is disabled.
    /// </summary>
    public class IncursionEnemyBossHighEvolutionary : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolSavage/HighEvolutionary.prototype");

        public IncursionEnemyBossHighEvolutionary(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "High Evolutionary Invader";

        // HardcodeExclude: unfinished  
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/HighEvolutionary/PsychokineticBlastLeft.prototype",    true,  1.5457f), // 2026-07-28
            new("Powers/EnemyPowers/Boss/HighEvolutionary/DualEnergyBlast.prototype",           true,  1.0f),
            new("Powers/EnemyPowers/Boss/HighEvolutionary/MatterSpray.prototype",               true,  1.0f),
            new("Powers/EnemyPowers/Boss/HighEvolutionary/PsychokineticBlastRight.prototype",   true,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                          false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",         false, 1.0f),
        };
    }
}
