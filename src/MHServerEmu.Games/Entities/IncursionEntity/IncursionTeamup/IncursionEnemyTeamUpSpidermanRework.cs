using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// SpidermanRework - rendered as the SpidermanRework Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSpidermanRework : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/SpidermanRework.prototype");

        public IncursionEnemyTeamUpSpidermanRework(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Spiderman Invader";

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
            new("Powers/TeamUps/Spiderman/SwingingAssault.prototype",  true,  0.026667f),  // Spiderman/SwingingAssault.prototype
            new("Powers/TeamUps/Spiderman/WebWhirlwind.prototype",  true,  0.026667f),  // Spiderman/WebWhirlwind.prototype
            new("Powers/TeamUps/Spiderman/AwayWebWhirlwindProc.prototype", false,  0.026667f),  // Spiderman/AwayWebWhirlwindProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/Cocoon.prototype",  true,  0.026667f),  // Spiderman/Cocoon.prototype
            new("Powers/TeamUps/Spiderman/AwayCocoonRetaliationProc.prototype", false,  0.026667f),  // Spiderman/AwayCocoonRetaliationProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/DiveKick.prototype",  true,  0.026667f),  // Spiderman/DiveKick.prototype
            new("Powers/TeamUps/Spiderman/Taunt.prototype",  true,  0.026667f),  // Spiderman/Taunt.prototype
            new("Powers/TeamUps/Spiderman/WebSwing.prototype",  true,  0.026667f),  // Spiderman/WebSwing.prototype
            new("Powers/TeamUps/Spiderman/AwayWebSwingProc.prototype", false,  0.026667f),  // Spiderman/AwayWebSwingProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/AmazingSmash.prototype",  true, 0.013333f),  // Spiderman/AmazingSmash.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
