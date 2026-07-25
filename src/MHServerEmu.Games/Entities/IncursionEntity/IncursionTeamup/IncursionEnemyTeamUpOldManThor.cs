using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// OldManThor - rendered as the OldManThor Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpOldManThor : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/OldManThor.prototype");

        public IncursionEnemyTeamUpOldManThor(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Old Man Thor Invader";

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
            new("Powers/TeamUps/BetaRayBill/KingThor/DeathFromAbove.prototype",  true,  0.023333f),  // BetaRayBill/KingThor/DeathFromAbove.prototype
            new("Powers/TeamUps/BetaRayBill/KingThor/DeathFromAboveHotspotTrigger.prototype", false,  0.023333f),  // BetaRayBill/KingThor/DeathFromAboveHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/KingThor/AwayThunderstormProc.prototype", false,  0.023333f),  // BetaRayBill/KingThor/AwayThunderstormProc.prototype - away passive
            new("Powers/TeamUps/BetaRayBill/HammerSmash.prototype",  true,  0.023333f),  // BetaRayBill/HammerSmash.prototype
            new("Powers/TeamUps/BetaRayBill/KingThor/StormBreaker.prototype",  true,  0.023333f),  // BetaRayBill/KingThor/StormBreaker.prototype
            new("Powers/TeamUps/BetaRayBill/KingThor/StormBreakerArcTrigger.prototype", false,  0.023333f),  // BetaRayBill/KingThor/StormBreakerArcTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/KingThor/LightningBarrage.prototype",  true,  0.023333f),  // BetaRayBill/KingThor/LightningBarrage.prototype
            new("Powers/TeamUps/BetaRayBill/LightningBarrageDoTTrigger.prototype", false,  0.023333f),  // BetaRayBill/LightningBarrageDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/BetaRayBill/KingThor/AwaySkuttlebuttProc.prototype", false,  0.023333f),  // BetaRayBill/KingThor/AwaySkuttlebuttProc.prototype - away passive
            new("Powers/TeamUps/BetaRayBill/Antiforce.prototype",  true, 0.011667f),  // BetaRayBill/Antiforce.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
