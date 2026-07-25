using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Rescue - rendered as the Rescue Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpRescue : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Rescue.prototype");

        public IncursionEnemyTeamUpRescue(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Rescue Invader";

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
            new("Powers/TeamUps/Rescue/NanobotStream.prototype",  true,  0.023333f),  // Rescue/NanobotStream.prototype
            new("Powers/TeamUps/Rescue/RTRegeneration.prototype", false,  0.023333f),  // Rescue/RTRegeneration.prototype - defensive
            new("Powers/TeamUps/Rescue/AwayRegeneration.prototype", false,  0.023333f),  // Rescue/AwayRegeneration.prototype - away passive
            new("Powers/TeamUps/Rescue/NanobotStreamDamageBuffTrigger.prototype", false,  0.023333f),  // Rescue/NanobotStreamDamageBuffTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Rescue/EMFShieldDashPower.prototype", false,  0.023333f),  // Rescue/EMFShieldDashPower.prototype - defensive
            new("Powers/TeamUps/Rescue/EMFShieldDamageHotspotTrigger.prototype", false,  0.023333f),  // Rescue/EMFShieldDamageHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Rescue/TotheRescue.prototype",  true,  0.023333f),  // Rescue/TotheRescue.prototype
            new("Powers/TeamUps/Rescue/AwayRescue.prototype", false,  0.023333f),  // Rescue/AwayRescue.prototype - away passive
            new("Powers/TeamUps/Rescue/StasisRay.prototype",  true,  0.023333f),  // Rescue/StasisRay.prototype
            new("Powers/TeamUps/Rescue/Signature.prototype",  true, 0.011667f),  // Rescue/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
