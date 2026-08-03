using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Corvus - Black Order boss, reuses Malekith art (MarvelAgent_MalekithCH9Instance).
    /// Has a death-summon proc power (MalekithOnDeathSummonProc) and NoStealablePowerBlank.
    /// Standard boss controller; powers harvested from native power collection after spawn.
    /// 
    /// DISABLED - its just malekith for now 
    /// </summary>
    public class IncursionEnemyBossCorvus : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/BlackOrder/Corvus.prototype");

        public IncursionEnemyBossCorvus(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Corvus Invader";

        // HardcodeExclude: unfinished - currently just a Malekith placeholder.
        public override bool HardcodeExclude => true;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
