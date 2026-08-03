using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// HolidayGroot - rendered as the HolidayGroot Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpHolidayGroot : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/HolidayGroot.prototype");

        public IncursionEnemyTeamUpHolidayGroot(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Holiday Groot Invader";

        // HardcodeExclude: no holiday skins ; Groot already has a TeamUp entry.
        public override bool HardcodeExclude => true;

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
            new("Powers/TeamUps/Groot/DeathFromAbove.prototype",  true,  0.0460f), // 2026-07-30
            new("Powers/TeamUps/Groot/DirectedShockwave.prototype",  true,  0.0327f), // 2026-07-30
            new("Powers/TeamUps/Groot/WeAreGroot.prototype", false,  0.026667f),  // Groot/WeAreGroot.prototype - away passive
            new("Powers/TeamUps/Groot/HealingSpores.prototype", false,  0.026667f),  // Groot/HealingSpores.prototype - defensive
            new("Powers/TeamUps/Groot/GraspingRoots.prototype",  true,  0.026667f),  // Groot/GraspingRoots.prototype
            new("Powers/TeamUps/Groot/PBAoE.prototype",  true,  0.0295f), // 2026-07-30
            new("Powers/TeamUps/Groot/AwayHealingSpores.prototype", false,  0.026667f),  // Groot/AwayHealingSpores.prototype - away passive
            new("Powers/TeamUps/Groot/AwayGraspingRoots.prototype", false,  0.026667f),  // Groot/AwayGraspingRoots.prototype - away passive
            new("Powers/TeamUps/Groot/BrambleToss.prototype",  true,  0.0332f), // 2026-07-30
            new("Powers/TeamUps/Groot/GrootOut.prototype",  true, 0.013333f),  // Groot/GrootOut.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
