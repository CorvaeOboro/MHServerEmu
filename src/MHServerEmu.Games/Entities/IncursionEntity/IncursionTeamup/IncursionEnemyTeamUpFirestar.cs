using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Firestar - rendered as the Firestar Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpFirestar : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Firestar.prototype");

        public IncursionEnemyTeamUpFirestar(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Firestar Invader";

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
            new("Powers/TeamUps/Firestar/ChanneledBeam.prototype",  true,  0.0765f), // 2026-07-29
            new("Powers/TeamUps/Firestar/SummonFireHotspot.prototype", false,  0.026667f),  // Firestar/SummonFireHotspot.prototype - trigger/secondary
            new("Powers/TeamUps/Firestar/AwayFireHotspot.prototype", false,  0.026667f),  // Firestar/AwayFireHotspot.prototype - away passive
            new("Powers/TeamUps/Firestar/ChanneledBeamDoTTrigger.prototype", false,  0.0765f), // 2026-07-29
            new("Powers/TeamUps/Firestar/EnergyRainStart.prototype",  true,  0.026667f),  // Firestar/EnergyRainStart.prototype
            new("Powers/TeamUps/Firestar/AwayEnergyRain.prototype", false,  0.026667f),  // Firestar/AwayEnergyRain.prototype - away passive
            new("Powers/TeamUps/Firestar/MicrowaveShield.prototype", false,  0.026667f),  // Firestar/MicrowaveShield.prototype - defensive
            new("Powers/TeamUps/Firestar/MicrowaveShieldDamageAuraTrigger.prototype", false,  0.026667f),  // Firestar/MicrowaveShieldDamageAuraTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Firestar/AwayShield.prototype", false,  0.026667f),  // Firestar/AwayShield.prototype - away passive
            new("Powers/TeamUps/Firestar/Ultimate.prototype",  true, 0.0159f), // 2026-07-29
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
