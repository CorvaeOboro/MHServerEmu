using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// SamWilson - rendered as the SamWilson Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSamWilson : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/SamWilson.prototype");

        public IncursionEnemyTeamUpSamWilson(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Sam Wilson Invader";

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
            new("Powers/TeamUps/SamWilson/CallRedwing.prototype",  true,  0.0570f), // 2026-07-30
            new("Powers/TeamUps/SamWilson/BoomerangThrow.prototype",  true,  0.0646f), // 2026-07-30
            new("Powers/TeamUps/SamWilson/BouncingShield.prototype", false,  0.023333f),  // SamWilson/BouncingShield.prototype - defensive
            new("Powers/TeamUps/SamWilson/AwayCallRedwingProc.prototype", false,  0.023333f),  // SamWilson/AwayCallRedwingProc.prototype - away passive
            new("Powers/TeamUps/SamWilson/DeathFromAbove.prototype",  true,  0.0285f), // 2026-07-30
            new("Powers/TeamUps/SamWilson/BouncingShieldMoreBounces.prototype", false,  0.023333f),  // SamWilson/BouncingShieldMoreBounces.prototype - defensive
            new("Powers/TeamUps/SamWilson/PBAoE.prototype",  true,  0.0346f), // 2026-07-30
            new("Powers/TeamUps/SamWilson/AwayDFAProc.prototype", false,  0.023333f),  // SamWilson/AwayDFAProc.prototype - away passive
            new("Powers/TeamUps/SamWilson/ShieldExpertise.prototype", false,  0.023333f),  // SamWilson/ShieldExpertise.prototype - defensive
            new("Powers/TeamUps/SamWilson/Signature.prototype",  true, 0.0122f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
