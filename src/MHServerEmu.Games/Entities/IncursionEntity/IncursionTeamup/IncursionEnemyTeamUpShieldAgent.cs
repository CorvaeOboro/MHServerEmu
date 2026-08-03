using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// ShieldAgent - rendered as the ShieldAgent Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpShieldAgent : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/ShieldAgent.prototype");

        public IncursionEnemyTeamUpShieldAgent(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Shield Agent Invader";

        // HardcodeExclude: lacks powers, uninteresting to fight. , maybe could summon more agents 
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
            new("Powers/TeamUps/ShieldAgent/Grenade.prototype", false,  0.026667f),  // ShieldAgent/Grenade.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/Turret.prototype", false,  0.026667f),  // ShieldAgent/Turret.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/AwayTurretProc.prototype", false,  0.026667f),  // ShieldAgent/AwayTurretProc.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/GrenadeDoTTrigger.prototype", false,  0.026667f),  // ShieldAgent/GrenadeDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/ShieldAgent/AwayPassive.prototype", false,  0.026667f),  // ShieldAgent/AwayPassive.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/SupportFire.prototype", false,  0.026667f),  // ShieldAgent/SupportFire.prototype - defensive
            new("Powers/TeamUps/AgentCoulson/ShotgunShieldAgentSummon.prototype", false,  0.026667f),  // AgentCoulson/ShotgunShieldAgentSummon.prototype - defensive
            new("Powers/TeamUps/AgentCoulson/MinigunAgentSummon.prototype",  true,  0.026667f),  // AgentCoulson/MinigunAgentSummon.prototype
            new("Powers/TeamUps/AgentCoulson/AwaySummonShieldAgentsProc.prototype", false,  0.026667f),  // AgentCoulson/AwaySummonShieldAgentsProc.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/Signature.prototype",  true, 0.0164f), // 2026-08-01
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
