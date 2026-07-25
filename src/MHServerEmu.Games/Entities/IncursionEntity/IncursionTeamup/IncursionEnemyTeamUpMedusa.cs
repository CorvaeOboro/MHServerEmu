using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Medusa - rendered as the Medusa Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpMedusa : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Medusa.prototype");

        public IncursionEnemyTeamUpMedusa(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Medusa Invader";

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
            new("Powers/TeamUps/Medusa/Impale.prototype",  true,  0.023333f),  // Medusa/Impale.prototype
            new("Powers/TeamUps/Medusa/Pirouette.prototype",  true,  0.023333f),  // Medusa/Pirouette.prototype
            new("Powers/TeamUps/Medusa/AwayPirouette.prototype", false,  0.023333f),  // Medusa/AwayPirouette.prototype - away passive
            new("Powers/TeamUps/Medusa/HairCone.prototype",  true,  0.023333f),  // Medusa/HairCone.prototype
            new("Powers/TeamUps/Medusa/PBAoEPush.prototype",  true,  0.023333f),  // Medusa/PBAoEPush.prototype
            new("Powers/TeamUps/Medusa/Constrict.prototype",  true,  0.023333f),  // Medusa/Constrict.prototype
            new("Powers/TeamUps/Medusa/AutoSlap.prototype",  true,  0.023333f),  // Medusa/AutoSlap.prototype
            new("Powers/TeamUps/Medusa/HairThrow.prototype",  true,  0.023333f),  // Medusa/HairThrow.prototype
            new("Powers/TeamUps/Medusa/AwayHairThrow.prototype", false,  0.023333f),  // Medusa/AwayHairThrow.prototype - away passive
            new("Powers/TeamUps/Medusa/HairBarrage.prototype",  true, 0.011667f),  // Medusa/HairBarrage.prototype - signature / ultimate
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
