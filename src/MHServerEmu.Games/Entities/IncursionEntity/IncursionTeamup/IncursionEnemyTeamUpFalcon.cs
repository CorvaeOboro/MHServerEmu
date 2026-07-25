using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Falcon - rendered as the Falcon Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpFalcon : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Falcon.prototype");

        public IncursionEnemyTeamUpFalcon(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Falcon Invader";

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
            new("Powers/TeamUps/Falcon/DashStrike.prototype",  true,  0.026667f),  // Falcon/DashStrike.prototype
            new("Powers/TeamUps/Falcon/BulletSpray.prototype",  true,  0.026667f),  // Falcon/BulletSpray.prototype
            new("Powers/TeamUps/Falcon/AwayStrafeProc.prototype", false,  0.026667f),  // Falcon/AwayStrafeProc.prototype - away passive
            new("Powers/TeamUps/Falcon/DashStrikeBleedTrigger.prototype", false,  0.026667f),  // Falcon/DashStrikeBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Falcon/ExplosiveRounds.prototype",  true,  0.026667f),  // Falcon/ExplosiveRounds.prototype
            new("Powers/TeamUps/Falcon/ExplosiveRoundsDurationTrigger.prototype", false,  0.026667f),  // Falcon/ExplosiveRoundsDurationTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Falcon/AwayHasteProc.prototype", false,  0.026667f),  // Falcon/AwayHasteProc.prototype - away passive
            new("Powers/TeamUps/Falcon/DeathFromAbove.prototype",  true,  0.026667f),  // Falcon/DeathFromAbove.prototype
            new("Powers/TeamUps/Falcon/AwayDFAProc.prototype", false,  0.026667f),  // Falcon/AwayDFAProc.prototype - away passive
            new("Powers/TeamUps/Falcon/UltimateStrafe.prototype",  true, 0.013333f),  // Falcon/UltimateStrafe.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
