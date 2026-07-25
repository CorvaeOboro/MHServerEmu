using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Jubilee - rendered as the Jubilee Team-Up actor.
    /// Powers: 7 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpJubilee : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Jubilee.prototype");

        public IncursionEnemyTeamUpJubilee(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Jubilee Invader";

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
            new("Powers/TeamUps/Jubilee/HomingMissiles.prototype",  true,  0.026667f),  // Jubilee/HomingMissiles.prototype
            new("Powers/TeamUps/Jubilee/Hotspot.prototype", false,  0.026667f),  // Jubilee/Hotspot.prototype - trigger/secondary
            new("Powers/TeamUps/Jubilee/AwayHotspot.prototype", false,  0.026667f),  // Jubilee/AwayHotspot.prototype - away passive
            new("Powers/TeamUps/Jubilee/TripleWave.prototype",  true,  0.026667f),  // Jubilee/TripleWave.prototype
            new("Powers/TeamUps/Jubilee/Boom.prototype",  true,  0.026667f),  // Jubilee/Boom.prototype
            new("Powers/TeamUps/Jubilee/AwayBoom.prototype", false,  0.026667f),  // Jubilee/AwayBoom.prototype - away passive
            new("Powers/TeamUps/Jubilee/Superglob.prototype",  true,  0.026667f),  // Jubilee/Superglob.prototype
            new("Powers/TeamUps/Jubilee/Multiglob.prototype",  true,  0.026667f),  // Jubilee/Multiglob.prototype
            new("Powers/TeamUps/Jubilee/AwaySuperglob.prototype", false,  0.026667f),  // Jubilee/AwaySuperglob.prototype - away passive
            new("Powers/TeamUps/Jubilee/GrandFinale.prototype",  true, 0.013333f),  // Jubilee/GrandFinale.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.026667f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
