using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Mordo - rendered as the Mordo Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpMordo : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Mordo.prototype");

        public IncursionEnemyTeamUpMordo(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Mordo Invader";

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
            new("Powers/TeamUps/Mordo/Mists.prototype",  true,  0.026667f),  // Mordo/Mists.prototype
            new("Powers/TeamUps/Mordo/Shield.prototype", false,  0.026667f),  // Mordo/Shield.prototype - defensive
            new("Powers/TeamUps/Mordo/AwayShield.prototype", false,  0.026667f),  // Mordo/AwayShield.prototype - away passive
            new("Powers/TeamUps/Mordo/SevenSuns.prototype",  true,  0.026667f),  // Mordo/SevenSuns.prototype
            new("Powers/TeamUps/Mordo/AstralDash.prototype",  true,  0.026667f),  // Mordo/AstralDash.prototype
            new("Powers/TeamUps/Mordo/AwayAstralDashTrigger.prototype", false,  0.026667f),  // Mordo/AwayAstralDashTrigger.prototype - away passive
            new("Powers/TeamUps/Mordo/BurningSunTrigger.prototype", false,  0.026667f),  // Mordo/BurningSunTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Mordo/DemonsOfDenak.prototype",  true,  0.026667f),  // Mordo/DemonsOfDenak.prototype
            new("Powers/TeamUps/Mordo/AwayDemonsOfDenak.prototype", false,  0.026667f),  // Mordo/AwayDemonsOfDenak.prototype - away passive
            new("Powers/TeamUps/Mordo/AstralStrike.prototype",  true,  0.026667f),  // Mordo/AstralStrike.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
