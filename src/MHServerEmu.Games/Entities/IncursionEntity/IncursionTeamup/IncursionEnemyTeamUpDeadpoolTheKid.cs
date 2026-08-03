using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// DeadpoolTheKid - rendered as the DeadpoolTheKid Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDeadpoolTheKid : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/DeadpoolTheKid.prototype");

        public IncursionEnemyTeamUpDeadpoolTheKid(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Deadpool the Kid Invader";

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
            new("Powers/TeamUps/DeadpoolTheKid/BulletSpray.prototype",  true,  0.0554f), // 2026-07-30
            new("Powers/TeamUps/DeadpoolTheKid/Caltrops.prototype",  true,  0.0639f), // 2026-07-30
            new("Powers/TeamUps/DeadpoolTheKid/AwayCaltropsProc.prototype", false,  0.026667f),  // DeadpoolTheKid/AwayCaltropsProc.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/TNT.prototype",  true,  0.026667f),  // DeadpoolTheKid/TNT.prototype
            new("Powers/TeamUps/DeadpoolTheKid/Decoy.prototype",  true,  0.0188f), // 2026-07-30
            new("Powers/TeamUps/DeadpoolTheKid/AwayDecoyProc.prototype", false,  0.026667f),  // DeadpoolTheKid/AwayDecoyProc.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/TNTDoTTrigger.prototype", false,  0.026667f),  // DeadpoolTheKid/TNTDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/DeadpoolTheKid/GatlingGun.prototype",  true,  0.0490f), // 2026-07-30
            new("Powers/TeamUps/DeadpoolTheKid/Godmode.prototype", false,  0.026667f),  // DeadpoolTheKid/Godmode.prototype - away passive
            new("Powers/TeamUps/DeadpoolTheKid/ServerLag.prototype",  true,  0.026667f),  // DeadpoolTheKid/ServerLag.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
