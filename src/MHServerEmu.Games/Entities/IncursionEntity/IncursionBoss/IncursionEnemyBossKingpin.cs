using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Kingpin - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossKingpin : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/KingpinBase.prototype");

        public IncursionEnemyBossKingpin(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Kingpin Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        // Henchmen: 2-3 elite Hand ninjas, 3-5 champion Maggia bats, 4-6 normal Maggia pistoleros.
        protected override IncursionHenchmanEntry[] HenchmenPool => _henchmen;
        private static readonly IncursionHenchmanEntry[] _henchmen =
        {
            new("Entity/Characters/Mobs/Hand/Redacted/ZZZHandNinjaBase", 2, 3, "Mods/Ranks/Elite.prototype"),
            new("Entity/Characters/Mobs/Maggia/MaggiaBatBlkBase", 3, 5, "Mods/Ranks/Champion.prototype"),
            new("Entity/Characters/Mobs/Maggia/MaggiaPistolBlkBase", 4, 6),
        };
    }
}
