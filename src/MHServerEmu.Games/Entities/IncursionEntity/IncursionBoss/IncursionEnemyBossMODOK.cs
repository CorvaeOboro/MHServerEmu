using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// MODOK - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMODOK : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MODOKBase.prototype");

        public IncursionEnemyBossMODOK(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "M O D O K Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        // Henchmen: 2-3 elite AIM blasters, 3-5 champion AIM swarmbots, 4-6 normal AIM gunners.
        protected override IncursionHenchmanEntry[] HenchmenPool => _henchmen;
        private static readonly IncursionHenchmanEntry[] _henchmen =
        {
            new("Entity/Characters/Mobs/AIM/AIMBlasterBase", 2, 3, "Mods/Ranks/Elite.prototype"),
            new("Entity/Characters/Mobs/AIM/AIMSwarmbotBase", 3, 5, "Mods/Ranks/Champion.prototype"),
            new("Entity/Characters/Mobs/Hydra/HydraPlasmaCasterBase", 4, 6),
        };
    }
}
