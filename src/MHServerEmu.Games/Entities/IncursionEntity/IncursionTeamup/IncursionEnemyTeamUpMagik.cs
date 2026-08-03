using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Magik - rendered as the Magik Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpMagik : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Magik.prototype");

        public IncursionEnemyTeamUpMagik(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Magik Invader";

        // HardcodeExclude: Magik has Avatar version better power set
        public override bool HardcodeExclude => true;

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
            new("Powers/TeamUps/Magik/SkullCrack.prototype",  true,  0.1018f), // 2026-07-29
            new("Powers/TeamUps/Magik/SorcerousEruption.prototype",  true,  0.0550f), // 2026-07-29
            new("Powers/TeamUps/Magik/AwaySorcerousEruption.prototype", false,  0.023333f),  // Magik/AwaySorcerousEruption.prototype - away passive
            new("Powers/TeamUps/Magik/DarkReaping.prototype",  true,  0.023333f),  // Magik/DarkReaping.prototype
            new("Powers/TeamUps/Magik/BounceStrike.prototype",  true,  0.0607f), // 2026-07-29
            new("Powers/TeamUps/Magik/BounceStrikeMoreBounces.prototype",  true,  0.0607f), // 2026-07-29
            new("Powers/TeamUps/Magik/DemonicArmy.prototype",  true,  0.023333f),  // Magik/DemonicArmy.prototype
            new("Powers/TeamUps/Magik/ImprovedDemons.prototype",  true,  0.023333f),  // Magik/ImprovedDemons.prototype
            new("Powers/TeamUps/Magik/AwayDemonicArmy.prototype", false,  0.023333f),  // Magik/AwayDemonicArmy.prototype - away passive
            new("Powers/TeamUps/Magik/Ultimate.prototype",  true, 0.011667f),  // Magik/Ultimate.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
