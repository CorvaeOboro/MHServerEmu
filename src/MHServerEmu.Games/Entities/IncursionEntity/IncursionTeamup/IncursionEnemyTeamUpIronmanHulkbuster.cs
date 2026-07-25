using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// IronmanHulkbuster - rendered as the IronmanHulkbuster Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpIronmanHulkbuster : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/IronmanHulkbuster.prototype");

        public IncursionEnemyTeamUpIronmanHulkbuster(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Ironman Hulkbuster Invader";

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
            new("Powers/TeamUps/Havok/PiercingBeam.prototype",  true,  0.026667f),  // Havok/PiercingBeam.prototype
            new("Powers/TeamUps/Havok/ExplodingChargeShot.prototype",  true,  0.026667f),  // Havok/ExplodingChargeShot.prototype
            new("Powers/TeamUps/Havok/ChanneledBeam.prototype",  true,  0.026667f),  // Havok/ChanneledBeam.prototype
            new("Powers/TeamUps/Havok/PiercingBeamDoTTrigger.prototype", false,  0.026667f),  // Havok/PiercingBeamDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Havok/ExplodingChargeShotEnergyBuffTrigger.prototype", false,  0.026667f),  // Havok/ExplodingChargeShotEnergyBuffTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Havok/MissileAbsorbPassive.prototype", false,  0.026667f),  // Havok/MissileAbsorbPassive.prototype - away passive
            new("Powers/TeamUps/Havok/ConeShot.prototype",  true,  0.026667f),  // Havok/ConeShot.prototype
            new("Powers/TeamUps/Havok/AwayExplodingShot.prototype", false,  0.026667f),  // Havok/AwayExplodingShot.prototype - away passive
            new("Powers/TeamUps/Havok/EnergyDamageBuffProc.prototype", false,  0.026667f),  // Havok/EnergyDamageBuffProc.prototype - away passive
            new("Powers/TeamUps/Havok/SpinShot.prototype",  true,  0.026667f),  // Havok/SpinShot.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
