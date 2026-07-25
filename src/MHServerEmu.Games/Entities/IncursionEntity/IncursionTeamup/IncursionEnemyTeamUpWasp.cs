using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Wasp - rendered as the Wasp Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpWasp : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Wasp.prototype");

        public IncursionEnemyTeamUpWasp(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Wasp Invader";

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
            new("Powers/TeamUps/Wasp/Biospray.prototype",  true,  0.023333f),  // Wasp/Biospray.prototype
            new("Powers/TeamUps/Wasp/ShrinkAttack.prototype",  true,  0.023333f),  // Wasp/ShrinkAttack.prototype
            new("Powers/TeamUps/Wasp/AwayShrinkAttack.prototype", false,  0.023333f),  // Wasp/AwayShrinkAttack.prototype - away passive
            new("Powers/TeamUps/Wasp/BiospraySlowTrigger.prototype", false,  0.023333f),  // Wasp/BiospraySlowTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Wasp/TripleBioball.prototype",  true,  0.023333f),  // Wasp/TripleBioball.prototype
            new("Powers/TeamUps/Wasp/TripleBioballDoTTrigger.prototype", false,  0.023333f),  // Wasp/TripleBioballDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Wasp/Flyby.prototype",  true,  0.023333f),  // Wasp/Flyby.prototype
            new("Powers/TeamUps/Wasp/AwayFlyby.prototype", false,  0.023333f),  // Wasp/AwayFlyby.prototype - away passive
            new("Powers/TeamUps/Wasp/SpinAttack.prototype",  true,  0.023333f),  // Wasp/SpinAttack.prototype
            new("Powers/TeamUps/Wasp/BarragePBAoE.prototype",  true,  0.023333f),  // Wasp/BarragePBAoE.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
