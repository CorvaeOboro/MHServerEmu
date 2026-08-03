using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// SpiderGwen - rendered as the SpiderGwen Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSpiderGwen : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/SpiderGwen.prototype");

        public IncursionEnemyTeamUpSpiderGwen(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Spider Gwen Invader";

        protected override int ThinkIntervalMs => 200;
        protected override float AttackRange => 200f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 400f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 0.026667f; // fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/TeamUps/Spiderman/WebSplat.prototype",  true,  0.1095f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/DisengagingShot.prototype",  true,  0.0632f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/AwayDisengagingShot.prototype", false,  0.026667f),  // GwenStacy/AwayDisengagingShot.prototype - away passive
            new("Powers/TeamUps/GwenStacy/AwayWebSplat.prototype", false,  0.026667f),  // GwenStacy/AwayWebSplat.prototype - away passive
            new("Powers/TeamUps/GwenStacy/WebSpray.prototype",  true,  0.0549f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/Wrap.prototype",  true,  0.0700f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/Slingshot.prototype",  true,  0.0307f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/CorrosiveWebbing.prototype",  true,  0.0358f), // 2026-07-29
            new("Powers/TeamUps/GwenStacy/AwayCorrosiveWebbing.prototype", false,  0.026667f),  // GwenStacy/AwayCorrosiveWebbing.prototype - away passive
            new("Powers/TeamUps/Spiderman/AmazingSmash.prototype",  true, 0.0130f), // 2026-07-29
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
