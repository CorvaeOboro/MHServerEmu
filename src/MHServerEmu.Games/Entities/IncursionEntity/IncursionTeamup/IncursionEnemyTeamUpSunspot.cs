using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Sunspot - rendered as the Sunspot Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSunspot : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Sunspot.prototype");

        public IncursionEnemyTeamUpSunspot(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Sunspot Invader";

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
            new("Powers/TeamUps/Sunspot/FuriousLunge.prototype",  true,  0.0776f), // 2026-07-30
            new("Powers/TeamUps/Sunspot/DiveBomb.prototype",  true,  0.0330f), // 2026-07-30
            new("Powers/TeamUps/Sunspot/AwayDiveBombProc.prototype", false,  0.023333f),  // Sunspot/AwayDiveBombProc.prototype - away passive
            new("Powers/TeamUps/Sunspot/FuriousLungeHotspotTrigger.prototype", false,  0.023333f),  // Sunspot/FuriousLungeHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Sunspot/ChanneledBeam.prototype",  true,  0.0515f), // 2026-07-30
            new("Powers/TeamUps/Sunspot/TripleStrike.prototype",  true,  0.0686f), // 2026-07-30
            new("Powers/TeamUps/Sunspot/SolarSteroid.prototype", false,  0.023333f),  // Sunspot/SolarSteroid.prototype - defensive
            new("Powers/TeamUps/Sunspot/SolarSteroidDamageAuraTrigger.prototype", false,  0.023333f),  // Sunspot/SolarSteroidDamageAuraTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Sunspot/AwaySolarSteroidProc.prototype", false,  0.023333f),  // Sunspot/AwaySolarSteroidProc.prototype - away passive
            new("Powers/TeamUps/Sunspot/PBAoESignature.prototype",  true, 0.0129f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
