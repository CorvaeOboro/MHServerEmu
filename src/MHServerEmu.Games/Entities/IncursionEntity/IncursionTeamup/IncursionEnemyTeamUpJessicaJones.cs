using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// JessicaJones - rendered as the JessicaJones Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpJessicaJones : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/JessicaJones.prototype");

        public IncursionEnemyTeamUpJessicaJones(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Jessica Jones Invader";

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
            new("Powers/TeamUps/JessicaJones/DeathFromAbove.prototype",  true,  0.023333f),  // JessicaJones/DeathFromAbove.prototype
            new("Powers/TeamUps/JessicaJones/DeathFromAboveQuakeTrigger.prototype", false,  0.023333f),  // JessicaJones/DeathFromAboveQuakeTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/JessicaJones/AwayDeathFromAbove.prototype", false,  0.023333f),  // JessicaJones/AwayDeathFromAbove.prototype - away passive
            new("Powers/TeamUps/JessicaJones/Taunt.prototype",  true,  0.023333f),  // JessicaJones/Taunt.prototype
            new("Powers/TeamUps/JessicaJones/ThrowConcrete.prototype",  true,  0.023333f),  // JessicaJones/ThrowConcrete.prototype
            new("Powers/TeamUps/JessicaJones/ThrowConcreteBleedTrigger.prototype", false,  0.023333f),  // JessicaJones/ThrowConcreteBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/JessicaJones/Pummel.prototype",  true,  0.023333f),  // JessicaJones/Pummel.prototype
            new("Powers/TeamUps/JessicaJones/PummelBleedTrigger.prototype", false,  0.023333f),  // JessicaJones/PummelBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/JessicaJones/AwayThrowConcrete.prototype", false,  0.023333f),  // JessicaJones/AwayThrowConcrete.prototype - away passive
            new("Powers/TeamUps/JessicaJones/KickCar.prototype",  true,  0.023333f),  // JessicaJones/KickCar.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
