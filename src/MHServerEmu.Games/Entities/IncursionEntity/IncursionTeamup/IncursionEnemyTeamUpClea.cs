using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Clea - rendered as the Clea Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpClea : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Clea.prototype");

        public IncursionEnemyTeamUpClea(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Clea Invader";

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
            new("Powers/TeamUps/Clea/Daggers.prototype",  true,  0.026667f),  // Clea/Daggers.prototype
            new("Powers/TeamUps/Clea/SummonFlames.prototype",  true,  0.026667f),  // Clea/SummonFlames.prototype
            new("Powers/TeamUps/Clea/AwaySummonFlames.prototype", false,  0.026667f),  // Clea/AwaySummonFlames.prototype - away passive
            new("Powers/TeamUps/Clea/ExplosiveOrbs.prototype",  true,  0.026667f),  // Clea/ExplosiveOrbs.prototype
            new("Powers/TeamUps/Clea/AstralClone.prototype",  true,  0.026667f),  // Clea/AstralClone.prototype
            new("Powers/TeamUps/Clea/AwayAstralClone.prototype", false,  0.026667f),  // Clea/AwayAstralClone.prototype - away passive
            new("Powers/TeamUps/Clea/VishantiSeal.prototype",  true,  0.026667f),  // Clea/VishantiSeal.prototype
            new("Powers/TeamUps/Clea/SeraphimShield.prototype", false,  0.026667f),  // Clea/SeraphimShield.prototype - defensive
            new("Powers/TeamUps/Clea/AwaySeraphimShield.prototype", false,  0.026667f),  // Clea/AwaySeraphimShield.prototype - away passive
            new("Powers/TeamUps/Clea/Vapors.prototype",  true,  0.026667f),  // Clea/Vapors.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
