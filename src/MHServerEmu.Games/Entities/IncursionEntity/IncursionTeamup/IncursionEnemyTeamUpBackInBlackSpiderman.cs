using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// BackInBlackSpiderman - rendered as the BackInBlackSpiderman Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpBackInBlackSpiderman : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/BackInBlackSpiderman.prototype");

        public IncursionEnemyTeamUpBackInBlackSpiderman(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Dark Spiderman Invader";

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
            new("Powers/TeamUps/Spiderman/BlackSuit/BlackSuitSwingingAssaut.prototype",  true,  0.026667f),  // Spiderman/BlackSuit/BlackSuitSwingingAssaut.prototype
            new("Powers/TeamUps/Spiderman/WebSplat.prototype",  true,  0.026667f),  // Spiderman/WebSplat.prototype
            new("Powers/TeamUps/Spiderman/AwayWebSplatProc.prototype", false,  0.026667f),  // Spiderman/AwayWebSplatProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/Cocoon.prototype",  true,  0.026667f),  // Spiderman/Cocoon.prototype
            new("Powers/TeamUps/Spiderman/BlackSuit/BlackSuitAwayCocoonRetaliationProc.prototype", false,  0.026667f),  // Spiderman/BlackSuit/BlackSuitAwayCocoonRetaliationProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/BlackSuit/MaximumSpider.prototype",  true,  0.026667f),  // Spiderman/BlackSuit/MaximumSpider.prototype
            new("Powers/TeamUps/Spiderman/BlackSuit/BlackSuitTaunt.prototype",  true,  0.026667f),  // Spiderman/BlackSuit/BlackSuitTaunt.prototype
            new("Powers/TeamUps/Spiderman/BlackSuit/BlackSuitWebSwing.prototype",  true,  0.026667f),  // Spiderman/BlackSuit/BlackSuitWebSwing.prototype
            new("Powers/TeamUps/Spiderman/BlackSuit/BlackSuitAwayWebSwingProc.prototype", false,  0.026667f),  // Spiderman/BlackSuit/BlackSuitAwayWebSwingProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/AmazingSmash.prototype",  true, 0.013333f),  // Spiderman/AmazingSmash.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
