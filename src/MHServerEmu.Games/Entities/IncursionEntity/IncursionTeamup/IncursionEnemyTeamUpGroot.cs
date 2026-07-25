using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Groot - rendered as the Groot Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpGroot : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Groot.prototype");

        public IncursionEnemyTeamUpGroot(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Groot Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 180f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 600f;
        protected override float PerPowerCooldownMs => 10000f;
        protected override float DamageScale => 0.033333f; // fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/TeamUps/Groot/DeathFromAbove.prototype",  true,  0.033333f),  // Groot/DeathFromAbove.prototype
            new("Powers/TeamUps/Groot/DirectedShockwave.prototype",  true,  0.033333f),  // Groot/DirectedShockwave.prototype
            new("Powers/TeamUps/Groot/WeAreGroot.prototype", false,  0.033333f),  // Groot/WeAreGroot.prototype - away passive
            new("Powers/TeamUps/Groot/HealingSpores.prototype", false,  0.033333f),  // Groot/HealingSpores.prototype - defensive
            new("Powers/TeamUps/Groot/GraspingRoots.prototype",  true,  0.033333f),  // Groot/GraspingRoots.prototype
            new("Powers/TeamUps/Groot/PBAoE.prototype",  true,  0.033333f),  // Groot/PBAoE.prototype
            new("Powers/TeamUps/Groot/AwayHealingSpores.prototype", false,  0.033333f),  // Groot/AwayHealingSpores.prototype - away passive
            new("Powers/TeamUps/Groot/AwayGraspingRoots.prototype", false,  0.033333f),  // Groot/AwayGraspingRoots.prototype - away passive
            new("Powers/TeamUps/Groot/BrambleToss.prototype",  true,  0.033333f),  // Groot/BrambleToss.prototype
            new("Powers/TeamUps/Groot/GrootOut.prototype",  true, 0.016667f),  // Groot/GrootOut.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.033333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
