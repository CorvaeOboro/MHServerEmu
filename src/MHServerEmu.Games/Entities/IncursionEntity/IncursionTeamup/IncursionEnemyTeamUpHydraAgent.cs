using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// HydraAgent - rendered as the HydraAgent Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpHydraAgent : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/HydraAgent.prototype");

        public IncursionEnemyTeamUpHydraAgent(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Hydra Agent Invader";

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
            new("Powers/TeamUps/ShieldAgent/HydraAgent/Grenade.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/Grenade.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/Turret.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/Turret.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/AwayTurretProc.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/AwayTurretProc.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/GrenadeDoTTrigger.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/GrenadeDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/ShieldAgent/HydraAgent/AwayPassive.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/AwayPassive.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/SupportFire.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/SupportFire.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/SummonHydraBrawler.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/SummonHydraBrawler.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/SummonHydraGunner.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/SummonHydraGunner.prototype - defensive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/AwaySummonHydraAgents.prototype", false,  0.026667f),  // ShieldAgent/HydraAgent/AwaySummonHydraAgents.prototype - away passive
            new("Powers/TeamUps/ShieldAgent/HydraAgent/Signature.prototype",  true, 0.0133f), // 2026-08-01
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
