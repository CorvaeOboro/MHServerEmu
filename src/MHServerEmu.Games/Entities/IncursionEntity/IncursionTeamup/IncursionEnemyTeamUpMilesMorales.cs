using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// MilesMorales - rendered as the MilesMorales Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpMilesMorales : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/MilesMorales.prototype");

        public IncursionEnemyTeamUpMilesMorales(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Miles Morales Invader";

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
            new("Powers/TeamUps/Spiderman/MilesMorales/SwingingAssault.prototype",  true,  0.023333f),  // Spiderman/MilesMorales/SwingingAssault.prototype
            new("Powers/TeamUps/Spiderman/MilesMorales/WebSwing.prototype",  true,  0.0312f), // 2026-07-30
            new("Powers/TeamUps/Spiderman/MilesMorales/AwayWebSwingProc.prototype", false,  0.023333f),  // Spiderman/MilesMorales/AwayWebSwingProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/MilesMorales/VenomSting.prototype",  true,  0.0306f), // 2026-07-30
            new("Powers/TeamUps/Spiderman/MilesMorales/VenomStingDoTTrigger.prototype", false,  0.023333f),  // Spiderman/MilesMorales/VenomStingDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Spiderman/MilesMorales/AwayVenomStingProc.prototype", false,  0.023333f),  // Spiderman/MilesMorales/AwayVenomStingProc.prototype - away passive
            new("Powers/TeamUps/Spiderman/MilesMorales/WebSplat.prototype",  true,  0.0622f), // 2026-07-30
            new("Powers/TeamUps/Spiderman/MilesMorales/Camouflage.prototype",  true,  0.023333f),  // Spiderman/MilesMorales/Camouflage.prototype
            new("Powers/TeamUps/Spiderman/MilesMorales/MaximumSpider.prototype",  true,  0.0483f), // 2026-07-30
            new("Powers/TeamUps/Spiderman/MilesMorales/VenomBlast.prototype",  true,  0.0256f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
