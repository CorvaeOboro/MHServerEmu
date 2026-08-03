using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Cosmic War Skrull - Hightown patrol boss, uses MarvelAgent_CosmicWarSkrull art.
    /// Parent chain goes through mob CosmicWarSkrull, not Boss.defaults.
    /// ModifierSetEnable is Yes. Standard controller with AI override.
    /// </summary>
    public class IncursionEnemyBossCosmicWarskrull : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/PatrolHightown/HightownEventCosmicWarskrull.prototype");

        public IncursionEnemyBossCosmicWarskrull(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Cosmic War Skrull Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
