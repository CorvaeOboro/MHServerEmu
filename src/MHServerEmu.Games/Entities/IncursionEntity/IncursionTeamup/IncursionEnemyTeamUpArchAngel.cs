using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// ArchAngel - rendered as the ArchAngel Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpArchAngel : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/ArchAngel.prototype");

        public IncursionEnemyTeamUpArchAngel(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Arch Angel Invader";

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
            new("Powers/TeamUps/ArchAngel/DeathFromAbove.prototype",  true,  0.026667f),  // ArchAngel/DeathFromAbove.prototype
            new("Powers/TeamUps/ArchAngel/AwayDeathFromAbove.prototype", false,  0.026667f),  // ArchAngel/AwayDeathFromAbove.prototype - away passive
            new("Powers/TeamUps/ArchAngel/SlowSteroid.prototype", false,  0.026667f),  // ArchAngel/SlowSteroid.prototype - defensive
            new("Powers/TeamUps/ArchAngel/BulletSpray.prototype",  true,  0.026667f),  // ArchAngel/BulletSpray.prototype
            new("Powers/TeamUps/ArchAngel/AwayBulletSpray.prototype", false,  0.026667f),  // ArchAngel/AwayBulletSpray.prototype - away passive
            new("Powers/TeamUps/ArchAngel/SlowSteroidHealTrigger.prototype", false,  0.026667f),  // ArchAngel/SlowSteroidHealTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/ArchAngel/WingShards.prototype",  true,  0.026667f),  // ArchAngel/WingShards.prototype
            new("Powers/TeamUps/ArchAngel/ToxicFeathers.prototype",  true,  0.026667f),  // ArchAngel/ToxicFeathers.prototype
            new("Powers/TeamUps/ArchAngel/AwaySlowSteroid.prototype", false,  0.026667f),  // ArchAngel/AwaySlowSteroid.prototype - away passive
            new("Powers/TeamUps/ArchAngel/CircleStrafe.prototype",  true,  0.026667f),  // ArchAngel/CircleStrafe.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
