using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Quake - rendered as the Quake Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpQuake : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Quake.prototype");

        public IncursionEnemyTeamUpQuake(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Quake Invader";

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
            new("Powers/TeamUps/Quake/Tremors.prototype",  true,  0.026667f),  // Quake/Tremors.prototype
            new("Powers/TeamUps/Quake/TremorsDurationTrigger.prototype", false,  0.026667f),  // Quake/TremorsDurationTrigger.prototype - away passive
            new("Powers/TeamUps/Quake/AwayTremors.prototype", false,  0.026667f),  // Quake/AwayTremors.prototype - away passive
            new("Powers/TeamUps/Quake/ChanneledBeam.prototype",  true,  0.026667f),  // Quake/ChanneledBeam.prototype
            new("Powers/TeamUps/Quake/Shockwave.prototype",  true,  0.026667f),  // Quake/Shockwave.prototype
            new("Powers/TeamUps/Quake/ShockwaveSlowTrigger.prototype", false,  0.026667f),  // Quake/ShockwaveSlowTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Quake/InternalQuake.prototype",  true,  0.026667f),  // Quake/InternalQuake.prototype
            new("Powers/TeamUps/Quake/InternalQuakeDoTTrigger.prototype", false,  0.026667f),  // Quake/InternalQuakeDoTTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Quake/AwayInternalQuake.prototype", false,  0.026667f),  // Quake/AwayInternalQuake.prototype - away passive
            new("Powers/TeamUps/Quake/Signature.prototype",  true, 0.013333f),  // Quake/Signature.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
