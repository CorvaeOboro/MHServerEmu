using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// MoleMan - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMoleMan : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MoleMan.prototype");

        public IncursionEnemyBossMoleMan(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Mole Man Invader";

        // HardcodeExclude: invis , summons moloids but no body
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
