using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// SpiderWoman - rendered as the SpiderWoman Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSpiderWoman : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/SpiderWoman.prototype");

        public IncursionEnemyTeamUpSpiderWoman(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Spider Woman Invader";

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
            new("Powers/TeamUps/SpiderWoman/DashStrike.prototype",  true,  0.023333f),  // SpiderWoman/DashStrike.prototype
            new("Powers/TeamUps/SpiderWoman/VenomSpray.prototype",  true,  0.023333f),  // SpiderWoman/VenomSpray.prototype
            new("Powers/TeamUps/SpiderWoman/AwayStrafeProc.prototype", false,  0.023333f),  // SpiderWoman/AwayStrafeProc.prototype - away passive
            new("Powers/TeamUps/SpiderWoman/DashStrikeHotspotTrigger.prototype", false,  0.023333f),  // SpiderWoman/DashStrikeHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/SpiderWoman/FocusVenomBeam.prototype",  true,  0.023333f),  // SpiderWoman/FocusVenomBeam.prototype
            new("Powers/TeamUps/SpiderWoman/FocusVenomBeamSlowTrigger.prototype", false,  0.023333f),  // SpiderWoman/FocusVenomBeamSlowTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/SpiderWoman/TauntSilence.prototype",  true,  0.023333f),  // SpiderWoman/TauntSilence.prototype
            new("Powers/TeamUps/SpiderWoman/EnergyDoTProc.prototype", false,  0.023333f),  // SpiderWoman/EnergyDoTProc.prototype - trigger/secondary
            new("Powers/TeamUps/SpiderWoman/AwayEnergyDoTProc.prototype", false,  0.023333f),  // SpiderWoman/AwayEnergyDoTProc.prototype - away passive
            new("Powers/TeamUps/SpiderWoman/SuperVenomBeam.prototype",  true,  0.023333f),  // SpiderWoman/SuperVenomBeam.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
