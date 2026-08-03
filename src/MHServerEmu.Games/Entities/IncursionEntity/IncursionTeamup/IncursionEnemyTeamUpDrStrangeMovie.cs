using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// DrStrangeMovie - rendered as the DrStrangeMovie Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDrStrangeMovie : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/DrStrangeMovie.prototype");

        public IncursionEnemyTeamUpDrStrangeMovie(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Dr Strange Invader";

         // HardcodeExclude: has avatar version 
        public override bool HardcodeExclude => true;
        
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
            new("Powers/TeamUps/DrStrange/Daggers.prototype",  true,  0.0590f), // 2026-07-31
            new("Powers/TeamUps/DrStrange/SummonFlame.prototype",  true,  0.0596f), // 2026-07-31
            new("Powers/TeamUps/DrStrangeMovie/AwaySummonFlames.prototype", false,  0.026667f),  // DrStrangeMovie/AwaySummonFlames.prototype - away passive
            new("Powers/TeamUps/DrStrange/ExplosiveOrb.prototype",  true,  0.026667f),  // DrStrange/ExplosiveOrb.prototype
            new("Powers/TeamUps/DrStrangeMovie/AstralClone.prototype",  true,  0.1451f), // 2026-07-30
            new("Powers/TeamUps/DrStrangeMovie/AwayAstralClone.prototype", false,  0.026667f),  // DrStrangeMovie/AwayAstralClone.prototype - away passive
            new("Powers/TeamUps/DrStrange/VishantiSeal.prototype",  true,  0.026667f),  // DrStrange/VishantiSeal.prototype
            new("Powers/TeamUps/DrStrangeMovie/SeraphimShield.prototype", false,  0.026667f),  // DrStrangeMovie/SeraphimShield.prototype - defensive
            new("Powers/TeamUps/DrStrangeMovie/AwaySeraphimShield.prototype", false,  0.026667f),  // DrStrangeMovie/AwaySeraphimShield.prototype - away passive
            new("Powers/TeamUps/DrStrange/Vapor.prototype",  true,  0.0233f), // 2026-07-31
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
