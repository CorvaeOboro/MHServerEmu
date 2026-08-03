using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// RobbieReyes - rendered as the RobbieReyes Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpRobbieReyes : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/RobbieReyes.prototype");

        public IncursionEnemyTeamUpRobbieReyes(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Robbie Reyes Invader";

        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 200f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 500f;
        protected override float PerPowerCooldownMs => 8000f;
        protected override float DamageScale => 0.023333f; // fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/TeamUps/RobbieReyes/ChainRoot.prototype",  true,  0.0534f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/FirePillar.prototype",  true,  0.0463f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/AwayFirePillar.prototype", false,  0.023333f),  // RobbieReyes/AwayFirePillar.prototype - away passive
            new("Powers/TeamUps/RobbieReyes/TireIron.prototype",  true,  0.0433f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/DriveBy.prototype",  true,  0.0359f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/FireBreath.prototype",  true,  0.0521f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/AwayTireIron.prototype", false,  0.023333f),  // RobbieReyes/AwayTireIron.prototype - away passive
            new("Powers/TeamUps/RobbieReyes/DriveByHotspotTrigger.prototype", false,  0.0359f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/Hellfire.prototype",  true,  0.0392f), // 2026-07-30
            new("Powers/TeamUps/RobbieReyes/Signature.prototype",  true, 0.0161f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
