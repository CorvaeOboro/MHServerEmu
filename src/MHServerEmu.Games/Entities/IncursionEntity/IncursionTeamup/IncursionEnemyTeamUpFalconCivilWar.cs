using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// FalconCivilWar - rendered as the FalconCivilWar Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpFalconCivilWar : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/FalconCivilWar.prototype");

        public IncursionEnemyTeamUpFalconCivilWar(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Falcon Civil War Invader";

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
            new("Powers/TeamUps/FalconCivilWar/DashStrike.prototype",  true,  0.023333f),  // FalconCivilWar/DashStrike.prototype
            new("Powers/TeamUps/FalconCivilWar/ExplosiveRounds.prototype",  true,  0.023333f),  // FalconCivilWar/ExplosiveRounds.prototype
            new("Powers/TeamUps/Falcon/BulletSpray.prototype",  true,  0.023333f),  // Falcon/BulletSpray.prototype
            new("Powers/TeamUps/FalconCivilWar/MissileStrike.prototype",  true,  0.023333f),  // FalconCivilWar/MissileStrike.prototype
            new("Powers/TeamUps/FalconCivilWar/ExplosiveRoundsBleedTrigger.prototype", false,  0.023333f),  // FalconCivilWar/ExplosiveRoundsBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Falcon/AwayStrafeProc.prototype", false,  0.023333f),  // Falcon/AwayStrafeProc.prototype - away passive
            new("Powers/TeamUps/FalconCivilWar/DroneStrike.prototype",  true,  0.023333f),  // FalconCivilWar/DroneStrike.prototype
            new("Powers/TeamUps/FalconCivilWar/DroneStrikeBarrage.prototype",  true,  0.023333f),  // FalconCivilWar/DroneStrikeBarrage.prototype
            new("Powers/TeamUps/FalconCivilWar/AwayDroneProc.prototype", false,  0.023333f),  // FalconCivilWar/AwayDroneProc.prototype - away passive
            new("Powers/TeamUps/Falcon/UltimateStrafe.prototype",  true, 0.011667f),  // Falcon/UltimateStrafe.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
