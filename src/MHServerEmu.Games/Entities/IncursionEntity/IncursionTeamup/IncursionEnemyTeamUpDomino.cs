using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Domino - rendered as the Domino Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDomino : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Domino.prototype");

        public IncursionEnemyTeamUpDomino(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Domino Invader";

        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 200f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 500f;
        protected override float PerPowerCooldownMs => 8000f;
        protected override float DamageScale => 0.026667f; // fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/TeamUps/Domino/Grenade.prototype",  true,  0.0302f), // 2026-07-30
            new("Powers/TeamUps/Domino/BrutalTakedown.prototype",  true,  0.0487f), // 2026-07-30
            new("Powers/TeamUps/Domino/SnowballsChance.prototype", false,  0.026667f),  // Domino/SnowballsChance.prototype - away passive
            new("Powers/TeamUps/Domino/MagicBullet.prototype",  true,  0.0245f), // 2026-07-30
            new("Powers/TeamUps/Domino/NoScope.prototype",  true,  0.0435f), // 2026-07-30
            new("Powers/TeamUps/Domino/AwaySniperShot.prototype", false,  0.026667f),  // Domino/AwaySniperShot.prototype - away passive
            new("Powers/TeamUps/Domino/MagicBulletMoreBounces.prototype",  true,  0.026667f),  // Domino/MagicBulletMoreBounces.prototype
            new("Powers/TeamUps/Domino/BombSummoner.prototype",  true,  0.026667f),  // Domino/BombSummoner.prototype
            new("Powers/TeamUps/Domino/AwayBombSummoner.prototype", false,  0.026667f),  // Domino/AwayBombSummoner.prototype - away passive
            new("Powers/TeamUps/Domino/LuckSteroid.prototype",  true, 0.013333f),  // Domino/LuckSteroid.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
