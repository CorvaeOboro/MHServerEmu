using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// BetaRayBill - rendered as the BetaRayBill Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpBetaRayBill : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/BetaRayBill.prototype");

        public IncursionEnemyTeamUpBetaRayBill(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Beta Ray Bill Invader";

        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 200f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 500f;
        protected override float PerPowerCooldownMs => 8000f;
        protected override float DamageScale => 0.03f; // fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/TeamUps/BetaRayBill/DeathFromAbove.prototype",  true,  0.03f),  // BetaRayBill/DeathFromAbove.prototype
            new("Powers/TeamUps/BetaRayBill/DeathFromAboveHotspotTrigger.prototype", false,  0.03f),  // BetaRayBill/DeathFromAboveHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/AwayThunderstormProc.prototype", false,  0.03f),  // BetaRayBill/AwayThunderstormProc.prototype - away passive
            new("Powers/TeamUps/BetaRayBill/HammerSmash.prototype",  true,  0.03f),  // BetaRayBill/HammerSmash.prototype
            new("Powers/TeamUps/BetaRayBill/StormBreaker.prototype",  true,  0.03f),  // BetaRayBill/StormBreaker.prototype
            new("Powers/TeamUps/BetaRayBill/StormBreakerArcTrigger.prototype", false,  0.03f),  // BetaRayBill/StormBreakerArcTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/LightningBarrage.prototype",  true,  0.03f),  // BetaRayBill/LightningBarrage.prototype
            new("Powers/TeamUps/BetaRayBill/LightningBarrageDoTTrigger.prototype", false,  0.03f),  // BetaRayBill/LightningBarrageDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/AwaySkuttlebuttProc.prototype", false,  0.03f),  // BetaRayBill/AwaySkuttlebuttProc.prototype - away passive
            new("Powers/TeamUps/BetaRayBill/Antiforce.prototype",  true, 0.015f),  // BetaRayBill/Antiforce.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.03f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
