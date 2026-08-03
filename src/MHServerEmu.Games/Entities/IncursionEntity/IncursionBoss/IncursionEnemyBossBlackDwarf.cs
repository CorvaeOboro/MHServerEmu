using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Black Dwarf - Black Order boss, uses RockTroll_Hammer art
    /// (MarvelAgent_RockTroll_Hammer). WalkEnabled, Speed 210.
    /// NoStealablePowerBlank; powers harvested from native collection.
    /// </summary>
    public class IncursionEnemyBossBlackDwarf : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/BlackOrder/BlackDwarf.prototype");

        public IncursionEnemyBossBlackDwarf(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Black Dwarf Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
    }
}
