using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// AgentCoulson - rendered as the AgentCoulson Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpAgentCoulson : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/AgentCoulson.prototype");

        public IncursionEnemyTeamUpAgentCoulson(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Agent Coulson Invader";

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
            new("Powers/TeamUps/AgentCoulson/RollingGrenades.prototype",  true,  0.023333f),  // AgentCoulson/RollingGrenades.prototype
            new("Powers/TeamUps/AgentCoulson/ChanneledBeam.prototype",  true,  0.023333f),  // AgentCoulson/ChanneledBeam.prototype
            new("Powers/TeamUps/AgentCoulson/IntenseTraining.prototype", false,  0.023333f),  // AgentCoulson/IntenseTraining.prototype - away passive
            new("Powers/TeamUps/AgentCoulson/RollingGrenadesDoTTrigger.prototype", false,  0.023333f),  // AgentCoulson/RollingGrenadesDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/AgentCoulson/ChanneledBeamDoTTrigger.prototype", false,  0.023333f),  // AgentCoulson/ChanneledBeamDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/AgentCoulson/DestroyerBeamSweepStart.prototype",  true,  0.023333f),  // AgentCoulson/DestroyerBeamSweepStart.prototype
            new("Powers/TeamUps/AgentCoulson/ShotgunShieldAgentSummon.prototype", false,  0.023333f),  // AgentCoulson/ShotgunShieldAgentSummon.prototype - defensive
            new("Powers/TeamUps/AgentCoulson/MinigunAgentSummon.prototype",  true,  0.023333f),  // AgentCoulson/MinigunAgentSummon.prototype
            new("Powers/TeamUps/AgentCoulson/AwaySummonShieldAgentsProc.prototype", false,  0.023333f),  // AgentCoulson/AwaySummonShieldAgentsProc.prototype - away passive
            new("Powers/TeamUps/AgentCoulson/SummonLola.prototype",  true,  0.023333f),  // AgentCoulson/SummonLola.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
