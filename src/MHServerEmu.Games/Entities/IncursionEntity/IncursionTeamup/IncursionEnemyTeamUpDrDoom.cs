using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// DrDoom - rendered as the DrDoom Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDrDoom : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/DrDoom.prototype");

        public IncursionEnemyTeamUpDrDoom(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Dr Doom Invader";

                // HardcodeExclude: DrDoom has Avatar version better power set
        public override bool HardcodeExclude => true;

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
            new("Powers/TeamUps/DrDoom/MagicLance.prototype",  true,  0.026667f),  // DrDoom/MagicLance.prototype
            new("Powers/TeamUps/DrDoom/Multishot.prototype",  true,  0.026667f),  // DrDoom/Multishot.prototype
            new("Powers/TeamUps/DrDoom/AwayEmpowerProc.prototype", false,  0.026667f),  // DrDoom/AwayEmpowerProc.prototype - away passive
            new("Powers/TeamUps/DrDoom/AoEDebuff.prototype",  true,  0.026667f),  // DrDoom/AoEDebuff.prototype
            new("Powers/TeamUps/DrDoom/MultishotExtraShots.prototype",  true,  0.026667f),  // DrDoom/MultishotExtraShots.prototype
            new("Powers/TeamUps/DrDoom/AwayArmorTrigger.prototype", false,  0.026667f),  // DrDoom/AwayArmorTrigger.prototype - away passive
            new("Powers/TeamUps/DrDoom/AwayAoEDebuff.prototype", false,  0.026667f),  // DrDoom/AwayAoEDebuff.prototype - away passive
            new("Powers/TeamUps/DrDoom/BallLightning.prototype",  true,  0.026667f),  // DrDoom/BallLightning.prototype
            new("Powers/TeamUps/DrDoom/BallLightningArcTrigger.prototype", false,  0.026667f),  // DrDoom/BallLightningArcTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/DrDoom/ShockNova.prototype",  true,  0.026667f),  // DrDoom/ShockNova.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
