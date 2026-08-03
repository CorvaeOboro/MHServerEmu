using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// HolidayDoctorDoom - rendered as the HolidayDoctorDoom Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpHolidayDoctorDoom : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/HolidayDoctorDoom.prototype");

        public IncursionEnemyTeamUpHolidayDoctorDoom(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Doctor Doom Invader";

        // HardcodeExclude: no holiday skins ; DoctorDoom already has a TeamUp and Avatar and Boss entry.
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
            new("Powers/TeamUps/DrDoom/MagicLance.prototype",  true,  0.0817f), // 2026-07-31
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/Multishot.prototype",  true,  0.026667f),  // DrDoom/HolidayDrDoom/Multishot.prototype
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/AwayEmpowerProc.prototype", false,  0.026667f),  // DrDoom/HolidayDrDoom/AwayEmpowerProc.prototype - away passive
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/AoEDebuff.prototype",  true,  0.026667f),  // DrDoom/HolidayDrDoom/AoEDebuff.prototype
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/MultishotExtraShots.prototype",  true,  0.026667f),  // DrDoom/HolidayDrDoom/MultishotExtraShots.prototype
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/AwayArmorTrigger.prototype", false,  0.026667f),  // DrDoom/HolidayDrDoom/AwayArmorTrigger.prototype - away passive
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/AwayAoEDebuff.prototype", false,  0.026667f),  // DrDoom/HolidayDrDoom/AwayAoEDebuff.prototype - away passive
            new("Powers/TeamUps/DrDoom/BallLightning.prototype",  true,  0.026667f),  // DrDoom/BallLightning.prototype
            new("Powers/TeamUps/DrDoom/BallLightningArcTrigger.prototype", false,  0.026667f),  // DrDoom/BallLightningArcTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/DrDoom/HolidayDrDoom/ShockNova.prototype",  true,  0.0252f), // 2026-07-31
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
