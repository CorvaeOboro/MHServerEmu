using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Arachne - rendered as the Arachne Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpArachne : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Arachne.prototype");

        public IncursionEnemyTeamUpArachne(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Arachne Invader";

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
            new("Powers/TeamUps/Arachne/WebWhipCross.prototype",  true,  0.0929f), // 2026-07-24
            new("Powers/TeamUps/Arachne/Cocoon.prototype",  true,  0.0383f), // 2026-07-31
            new("Powers/TeamUps/Arachne/Counterattack.prototype", false,  0.026667f),  // Arachne/Counterattack.prototype - away passive
            new("Powers/TeamUps/Arachne/ConeYank.prototype",  true,  0.0505f), // 2026-07-24
            new("Powers/TeamUps/Arachne/AwayStrafe.prototype", false,  0.026667f),  // Arachne/AwayStrafe.prototype - away passive
            new("Powers/TeamUps/Arachne/TKToss.prototype",  true,  0.0627f), // 2026-07-31
            new("Powers/TeamUps/Arachne/BouncingWeb.prototype",  true,  0.0429f), // 2026-07-31
            new("Powers/TeamUps/Arachne/BouncingWebMoreBounces.prototype",  true,  0.0429f), // 2026-07-31
            new("Powers/TeamUps/Arachne/AwayBouncingWeb.prototype", false,  0.026667f),  // Arachne/AwayBouncingWeb.prototype - away passive
            new("Powers/TeamUps/Arachne/MegaStun.prototype",  true,  0.0223f), // 2026-07-31
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
