using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// DrStrange - rendered as the DrStrange Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDrStrange : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/DrStrange.prototype");

        public IncursionEnemyTeamUpDrStrange(Game game) : base(game) { }

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
            new("Powers/TeamUps/DrStrange/Daggers.prototype",  true,  0.026667f),  // DrStrange/Daggers.prototype
            new("Powers/TeamUps/DrStrange/SummonFlame.prototype",  true,  0.0475f), // 2026-07-30
            new("Powers/TeamUps/DrStrange/AwaySummonFlames.prototype", false,  0.026667f),  // DrStrange/AwaySummonFlames.prototype - away passive
            new("Powers/TeamUps/DrStrange/ExplosiveOrb.prototype",  true,  0.026667f),  // DrStrange/ExplosiveOrb.prototype
            new("Powers/TeamUps/DrStrange/AstralClone.prototype",  true,  0.1328f), // 2026-07-30
            new("Powers/TeamUps/DrStrange/AwayAstralClone.prototype", false,  0.026667f),  // DrStrange/AwayAstralClone.prototype - away passive
            new("Powers/TeamUps/DrStrange/VishantiSeal.prototype",  true,  0.026667f),  // DrStrange/VishantiSeal.prototype
            new("Powers/TeamUps/DrStrange/SeraphimShield.prototype", false,  0.026667f),  // DrStrange/SeraphimShield.prototype - defensive
            new("Powers/TeamUps/DrStrange/AwaySeraphimShield.prototype", false,  0.026667f),  // DrStrange/AwaySeraphimShield.prototype - away passive
            new("Powers/TeamUps/DrStrange/Vapor.prototype",  true,  0.0305f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
