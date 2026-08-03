using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// ShieldAirSupport - rendered as the ShieldAirSupport Team-Up actor.
    /// Powers: 0 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpShieldAirSupport : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/ShieldAirSupport.prototype");

        public IncursionEnemyTeamUpShieldAirSupport(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Shield Air Support Invader";

        // HardcodeExclude: lacks enough active skills (0 active / 11 total); passives appear bugged.
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
            new("Powers/TeamUps/ShieldAirSupport/SummonOverrideEnabler.prototype", false,  0.026667f),  // ShieldAirSupport/SummonOverrideEnabler.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/MissileStrike.prototype", false,  0.026667f),  // ShieldAirSupport/MissileStrike.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/SniperCover.prototype", false,  0.026667f),  // ShieldAirSupport/SniperCover.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/CallForHelp.prototype", false,  0.026667f),  // ShieldAirSupport/CallForHelp.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/EyesInTheSky.prototype", false,  0.026667f),  // ShieldAirSupport/EyesInTheSky.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/StarktechLaserStrike.prototype", false,  0.026667f),  // ShieldAirSupport/StarktechLaserStrike.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/MountedGunRun.prototype", false,  0.026667f),  // ShieldAirSupport/MountedGunRun.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/SweepAndClearTrigger.prototype", false,  0.026667f),  // ShieldAirSupport/SweepAndClearTrigger.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/MissileStrikeHotspotTrigger.prototype", false,  0.026667f),  // ShieldAirSupport/MissileStrikeHotspotTrigger.prototype - away passive
            new("Powers/TeamUps/ShieldAirSupport/TacticalNukeTrigger.prototype", false,  0.026667f),  // ShieldAirSupport/TacticalNukeTrigger.prototype - away passive
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
