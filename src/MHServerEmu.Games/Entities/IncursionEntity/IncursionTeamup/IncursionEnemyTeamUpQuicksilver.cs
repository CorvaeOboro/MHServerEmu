using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Quicksilver - rendered as the Quicksilver Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpQuicksilver : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Quicksilver.prototype");

        public IncursionEnemyTeamUpQuicksilver(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Quicksilver Invader";

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
            new("Powers/TeamUps/Quicksilver/Pummel.prototype",  true,  0.0284f), // 2026-07-30
            new("Powers/TeamUps/Quicksilver/AwayPummel.prototype", false,  0.026667f),  // Quicksilver/AwayPummel.prototype - away passive
            new("Powers/TeamUps/Quicksilver/BounceStrikeStart.prototype",  true,  0.0565f), // 2026-07-30
            new("Powers/TeamUps/Quicksilver/Taunt.prototype",  true,  0.026667f),  // Quicksilver/Taunt.prototype
            new("Powers/TeamUps/Quicksilver/AwayTaunt.prototype", false,  0.026667f),  // Quicksilver/AwayTaunt.prototype - away passive
            new("Powers/TeamUps/Quicksilver/BounceStrikeMoreBounces.prototype",  true,  0.026667f),  // Quicksilver/BounceStrikeMoreBounces.prototype
            new("Powers/TeamUps/Quicksilver/WindTunnel.prototype",  true,  0.0618f), // 2026-07-30
            new("Powers/TeamUps/Quicksilver/SuperSonicCyclone.prototype",  true,  0.0354f), // 2026-07-30
            new("Powers/TeamUps/Quicksilver/AwayCyclone.prototype", false,  0.026667f),  // Quicksilver/AwayCyclone.prototype - away passive
            new("Powers/TeamUps/Quicksilver/Signature.prototype",  true, 0.0330f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
