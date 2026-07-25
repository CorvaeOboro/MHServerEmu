using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// CaptainAmericaCivilWar - rendered as the CaptainAmericaCivilWar Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpCaptainAmericaCivilWar : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/CaptainAmericaCivilWar.prototype");

        public IncursionEnemyTeamUpCaptainAmericaCivilWar(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Captain America Civil War Invader";

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
            new("Powers/TeamUps/CaptainAmerica/DeathFromAbove.prototype",  true,  0.023333f),  // CaptainAmerica/DeathFromAbove.prototype
            new("Powers/TeamUps/CaptainAmerica/BoomerangThrow.prototype",  true,  0.023333f),  // CaptainAmerica/BoomerangThrow.prototype
            new("Powers/TeamUps/CaptainAmerica/BouncingShield.prototype", false,  0.023333f),  // CaptainAmerica/BouncingShield.prototype - defensive
            new("Powers/TeamUps/CaptainAmericaCivilWar/AwayDFAProc.prototype", false,  0.023333f),  // CaptainAmericaCivilWar/AwayDFAProc.prototype - away passive
            new("Powers/TeamUps/CaptainAmerica/FuriousLunge.prototype",  true,  0.023333f),  // CaptainAmerica/FuriousLunge.prototype
            new("Powers/TeamUps/CaptainAmerica/BouncingShieldMoreBounces.prototype", false,  0.023333f),  // CaptainAmerica/BouncingShieldMoreBounces.prototype - defensive
            new("Powers/TeamUps/CaptainAmerica/PBAoE.prototype",  true,  0.023333f),  // CaptainAmerica/PBAoE.prototype
            new("Powers/TeamUps/CaptainAmericaCivilWar/AwayFuriousLungeProc.prototype", false,  0.023333f),  // CaptainAmericaCivilWar/AwayFuriousLungeProc.prototype - away passive
            new("Powers/TeamUps/CaptainAmerica/ShieldExpertise.prototype", false,  0.023333f),  // CaptainAmerica/ShieldExpertise.prototype - defensive
            new("Powers/TeamUps/CaptainAmerica/Signature.prototype",  true, 0.011667f),  // CaptainAmerica/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
