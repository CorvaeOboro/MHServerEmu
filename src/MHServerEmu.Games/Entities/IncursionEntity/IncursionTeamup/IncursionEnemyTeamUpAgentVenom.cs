using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// AgentVenom - rendered as the AgentVenom Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpAgentVenom : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/AgentVenom.prototype");

        public IncursionEnemyTeamUpAgentVenom(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Agent Venom Invader";

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
            new("Powers/TeamUps/AgentVenom/Grenades.prototype",  true,  0.026667f),  // AgentVenom/Grenades.prototype
            new("Powers/TeamUps/AgentVenom/WebSplat.prototype",  true,  0.026667f),  // AgentVenom/WebSplat.prototype
            new("Powers/TeamUps/AgentVenom/AwayWebSplatProc.prototype", false,  0.026667f),  // AgentVenom/AwayWebSplatProc.prototype - away passive
            new("Powers/TeamUps/AgentVenom/AwayGrenades.prototype", false,  0.026667f),  // AgentVenom/AwayGrenades.prototype - away passive
            new("Powers/TeamUps/AgentVenom/SniperShot.prototype",  true,  0.026667f),  // AgentVenom/SniperShot.prototype
            new("Powers/TeamUps/AgentVenom/AwaySniperShot.prototype", false,  0.026667f),  // AgentVenom/AwaySniperShot.prototype - away passive
            new("Powers/TeamUps/AgentVenom/BulletSpray.prototype",  true,  0.026667f),  // AgentVenom/BulletSpray.prototype
            new("Powers/TeamUps/AgentVenom/Impale.prototype",  true,  0.026667f),  // AgentVenom/Impale.prototype
            new("Powers/TeamUps/AgentVenom/ImpaleHealTrigger.prototype", false,  0.026667f),  // AgentVenom/ImpaleHealTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/AgentVenom/Ultimate.prototype",  true, 0.013333f),  // AgentVenom/Ultimate.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
