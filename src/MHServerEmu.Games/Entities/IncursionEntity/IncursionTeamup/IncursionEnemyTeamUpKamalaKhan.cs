using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// KamalaKhan - rendered as the KamalaKhan Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpKamalaKhan : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/KamalaKhan.prototype");

        public IncursionEnemyTeamUpKamalaKhan(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Kamala Khan Invader";

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
            new("Powers/TeamUps/KamalaKhan/RoundAboutPunch.prototype",  true,  0.026667f),  // KamalaKhan/RoundAboutPunch.prototype
            new("Powers/TeamUps/KamalaKhan/BigKick.prototype",  true,  0.026667f),  // KamalaKhan/BigKick.prototype
            new("Powers/TeamUps/KamalaKhan/AwayBigKick.prototype", false,  0.026667f),  // KamalaKhan/AwayBigKick.prototype - away passive
            new("Powers/TeamUps/KamalaKhan/AwayRoundAboutPunch.prototype", false,  0.026667f),  // KamalaKhan/AwayRoundAboutPunch.prototype - away passive
            new("Powers/TeamUps/KamalaKhan/Flick.prototype",  true,  0.026667f),  // KamalaKhan/Flick.prototype
            new("Powers/TeamUps/KamalaKhan/AwayFlick.prototype", false,  0.026667f),  // KamalaKhan/AwayFlick.prototype - away passive
            new("Powers/TeamUps/KamalaKhan/MachineGunFists.prototype",  true,  0.026667f),  // KamalaKhan/MachineGunFists.prototype
            new("Powers/TeamUps/KamalaKhan/Slap.prototype",  true,  0.026667f),  // KamalaKhan/Slap.prototype
            new("Powers/TeamUps/KamalaKhan/Healing.prototype", false,  0.026667f),  // KamalaKhan/Healing.prototype - defensive
            new("Powers/TeamUps/KamalaKhan/Signature.prototype",  true, 0.013333f),  // KamalaKhan/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
