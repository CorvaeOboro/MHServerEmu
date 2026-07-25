using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Angel - rendered as the Angel Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpAngel : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Angel.prototype");

        public IncursionEnemyTeamUpAngel(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Angel Invader";

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
            new("Powers/TeamUps/Angel/DeathFromAbove.prototype",  true,  0.026667f),  // Angel/DeathFromAbove.prototype
            new("Powers/TeamUps/Angel/AwayDFAProc.prototype", false,  0.026667f),  // Angel/AwayDFAProc.prototype - away passive
            new("Powers/TeamUps/Angel/SpeedSteroid.prototype", false,  0.026667f),  // Angel/SpeedSteroid.prototype - defensive
            new("Powers/TeamUps/Angel/AngelicBombardmentStart.prototype",  true,  0.026667f),  // Angel/AngelicBombardmentStart.prototype
            new("Powers/TeamUps/Angel/AwayAngelicBombardmentProc.prototype", false,  0.026667f),  // Angel/AwayAngelicBombardmentProc.prototype - away passive
            new("Powers/TeamUps/Angel/SpeedSteroidHealTrigger.prototype", false,  0.026667f),  // Angel/SpeedSteroidHealTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Angel/WingSweep.prototype",  true,  0.026667f),  // Angel/WingSweep.prototype
            new("Powers/TeamUps/Angel/WingSweepSlowTrigger.prototype", false,  0.026667f),  // Angel/WingSweepSlowTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Angel/AwaySpeedSteroidProc.prototype", false,  0.026667f),  // Angel/AwaySpeedSteroidProc.prototype - away passive
            new("Powers/TeamUps/Angel/SwordFlurryStart.prototype",  true,  0.026667f),  // Angel/SwordFlurryStart.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
