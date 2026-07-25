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
            new("Powers/TeamUps/RobbieReyes/ChainRoot.prototype",  true,  0.023333f),  // RobbieReyes/ChainRoot.prototype
            new("Powers/TeamUps/RobbieReyes/FirePillar.prototype",  true,  0.023333f),  // RobbieReyes/FirePillar.prototype
            new("Powers/TeamUps/RobbieReyes/AwayFirePillar.prototype", false,  0.023333f),  // RobbieReyes/AwayFirePillar.prototype - away passive
            new("Powers/TeamUps/RobbieReyes/TireIron.prototype",  true,  0.023333f),  // RobbieReyes/TireIron.prototype
            new("Powers/TeamUps/RobbieReyes/DriveBy.prototype",  true,  0.023333f),  // RobbieReyes/DriveBy.prototype
            new("Powers/TeamUps/RobbieReyes/FireBreath.prototype",  true,  0.023333f),  // RobbieReyes/FireBreath.prototype
            new("Powers/TeamUps/RobbieReyes/AwayTireIron.prototype", false,  0.023333f),  // RobbieReyes/AwayTireIron.prototype - away passive
            new("Powers/TeamUps/RobbieReyes/DriveByHotspotTrigger.prototype", false,  0.023333f),  // RobbieReyes/DriveByHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/RobbieReyes/Hellfire.prototype",  true,  0.023333f),  // RobbieReyes/Hellfire.prototype
            new("Powers/TeamUps/RobbieReyes/Signature.prototype",  true, 0.011667f),  // RobbieReyes/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
