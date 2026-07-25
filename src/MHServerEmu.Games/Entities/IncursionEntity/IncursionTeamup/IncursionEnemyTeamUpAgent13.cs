using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Agent13 - rendered as the Agent13 Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpAgent13 : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Agent13.prototype");

        public IncursionEnemyTeamUpAgent13(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Agent 13 Invader";

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
            new("Powers/TeamUps/Agent13/ReboundingClub.prototype",  true,  0.026667f),  // Agent13/ReboundingClub.prototype
            new("Powers/TeamUps/Agent13/NoScope.prototype",  true,  0.026667f),  // Agent13/NoScope.prototype
            new("Powers/TeamUps/Agent13/AwayPassive.prototype", false,  0.026667f),  // Agent13/AwayPassive.prototype - away passive
            new("Powers/TeamUps/Agent13/ReboundingClubMoreBounces.prototype",  true,  0.026667f),  // Agent13/ReboundingClubMoreBounces.prototype
            new("Powers/TeamUps/Agent13/AwaySniperShot.prototype", false,  0.026667f),  // Agent13/AwaySniperShot.prototype - away passive
            new("Powers/TeamUps/Agent13/RollingGrenades.prototype",  true,  0.026667f),  // Agent13/RollingGrenades.prototype
            new("Powers/TeamUps/AgentCoulson/ShotgunShieldAgentSummon.prototype", false,  0.026667f),  // AgentCoulson/ShotgunShieldAgentSummon.prototype - defensive
            new("Powers/TeamUps/AgentCoulson/MinigunAgentSummon.prototype",  true,  0.026667f),  // AgentCoulson/MinigunAgentSummon.prototype
            new("Powers/TeamUps/AgentCoulson/AwaySummonShieldAgentsProc.prototype", false,  0.026667f),  // AgentCoulson/AwaySummonShieldAgentsProc.prototype - away passive
            new("Powers/TeamUps/Agent13/Signature.prototype",  true, 0.013333f),  // Agent13/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
