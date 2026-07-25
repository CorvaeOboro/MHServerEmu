using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// PirateDeadpool - rendered as the PirateDeadpool Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpPirateDeadpool : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/PirateDeadpool.prototype");

        public IncursionEnemyTeamUpPirateDeadpool(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Pirate Deadpool Invader";

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
            new("Powers/TeamUps/DeadpoolTheKid/BulletSpray.prototype",  true,  0.026667f),  // DeadpoolTheKid/BulletSpray.prototype
            new("Powers/TeamUps/DeadpoolTheKid/Caltrops.prototype",  true,  0.026667f),  // DeadpoolTheKid/Caltrops.prototype
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpool/AwayCaltropsProc.prototype", false,  0.026667f),  // DeadpoolTheKid/PirateDeadpool/AwayCaltropsProc.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpoolGrenado.prototype",  true,  0.026667f),  // DeadpoolTheKid/PirateDeadpoolGrenado.prototype
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpool/Decoy.prototype",  true,  0.026667f),  // DeadpoolTheKid/PirateDeadpool/Decoy.prototype
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpool/AwayDecoyProc.prototype", false,  0.026667f),  // DeadpoolTheKid/PirateDeadpool/AwayDecoyProc.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpool/GrenadoDoTTrigger.prototype", false,  0.026667f),  // DeadpoolTheKid/PirateDeadpool/GrenadoDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/DeadpoolTheKid/GatlingGun.prototype",  true,  0.026667f),  // DeadpoolTheKid/GatlingGun.prototype
            new("Powers/TeamUps/DeadpoolTheKid/PirateDeadpool/Godmode.prototype", false,  0.026667f),  // DeadpoolTheKid/PirateDeadpool/Godmode.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/ServerLag.prototype",  true,  0.026667f),  // DeadpoolTheKid/ServerLag.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
