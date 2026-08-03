using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// MagikLimboReward - rendered as the MagikLimboReward Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpMagikLimboReward : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/MagikLimboReward.prototype");

        public IncursionEnemyTeamUpMagikLimboReward(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Magik of Limbo Invader";

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
            new("Powers/TeamUps/Magik/SkullCrack.prototype",  true,  0.023333f),  // Magik/SkullCrack.prototype
            new("Powers/TeamUps/Magik/MagikLimboReward/SorcerousEruption.prototype",  true,  0.023333f),  // Magik/MagikLimboReward/SorcerousEruption.prototype
            new("Powers/TeamUps/Magik/MagikLimboReward/AwaySorcerousEruption.prototype", false,  0.023333f),  // Magik/MagikLimboReward/AwaySorcerousEruption.prototype - away passive
            new("Powers/TeamUps/Magik/DarkReaping.prototype",  true,  0.023333f),  // Magik/DarkReaping.prototype
            new("Powers/TeamUps/Magik/BounceStrike.prototype",  true,  0.023333f),  // Magik/BounceStrike.prototype
            new("Powers/TeamUps/Magik/BounceStrikeMoreBounces.prototype",  true,  0.023333f),  // Magik/BounceStrikeMoreBounces.prototype
            new("Powers/TeamUps/Magik/DemonicArmy.prototype",  true,  0.023333f),  // Magik/DemonicArmy.prototype
            new("Powers/TeamUps/Magik/ImprovedDemons.prototype",  true,  0.023333f),  // Magik/ImprovedDemons.prototype
            new("Powers/TeamUps/Magik/AwayDemonicArmy.prototype", false,  0.023333f),  // Magik/AwayDemonicArmy.prototype - away passive
            new("Powers/TeamUps/Magik/Ultimate.prototype",  true, 0.011667f),  // Magik/Ultimate.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
