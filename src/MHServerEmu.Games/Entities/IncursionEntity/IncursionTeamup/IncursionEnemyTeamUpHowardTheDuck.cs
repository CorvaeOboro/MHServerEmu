using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// HowardTheDuck - rendered as the HowardTheDuck Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpHowardTheDuck : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/HowardTheDuck.prototype");

        public IncursionEnemyTeamUpHowardTheDuck(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Howard The Duck Invader";

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
            new("Powers/TeamUps/HowardTheDuck/FlyingDuckStart.prototype",  true,  0.026667f),  // HowardTheDuck/FlyingDuckStart.prototype
            new("Powers/TeamUps/HowardTheDuck/PerfectLanding.prototype",  true,  0.0398f), // 2026-07-30
            new("Powers/TeamUps/HowardTheDuck/AwayLanding.prototype", false,  0.026667f),  // HowardTheDuck/AwayLanding.prototype - away passive
            new("Powers/TeamUps/HowardTheDuck/SummonDoop.prototype",  true,  0.0576f), // 2026-07-30
            new("Powers/TeamUps/HowardTheDuck/AwayBuffProc.prototype", false,  0.026667f),  // HowardTheDuck/AwayBuffProc.prototype - away passive
            new("Powers/TeamUps/HowardTheDuck/BigPunch.prototype",  true,  0.0517f), // 2026-07-30
            new("Powers/TeamUps/HowardTheDuck/AwayDoop.prototype", false,  0.026667f),  // HowardTheDuck/AwayDoop.prototype - away passive
            new("Powers/TeamUps/HowardTheDuck/QuackAttack.prototype",  true,  0.0337f), // 2026-07-30
            new("Powers/TeamUps/HowardTheDuck/BigPunchExplosionTrigger.prototype", false,  0.0517f), // 2026-07-30
            new("Powers/TeamUps/HowardTheDuck/IronDuck.prototype",  true, 0.013333f),  // HowardTheDuck/IronDuck.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
