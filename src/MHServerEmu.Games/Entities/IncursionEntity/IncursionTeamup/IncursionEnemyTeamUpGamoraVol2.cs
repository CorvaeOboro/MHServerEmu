using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// GamoraVol2 - rendered as the GamoraVol2 Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpGamoraVol2 : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/GamoraVol2.prototype");

        public IncursionEnemyTeamUpGamoraVol2(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Gamora Vol2 Invader";

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
            new("Powers/TeamUps/Gamora/OpeningShot.prototype",  true,  0.023333f),  // Gamora/OpeningShot.prototype
            new("Powers/TeamUps/Gamora/HunterKiller.prototype",  true,  0.023333f),  // Gamora/HunterKiller.prototype
            new("Powers/TeamUps/Gamora/AwayHunterKiller.prototype", false,  0.023333f),  // Gamora/AwayHunterKiller.prototype - away passive
            new("Powers/TeamUps/Gamora/Homerun.prototype",  true,  0.023333f),  // Gamora/Homerun.prototype
            new("Powers/TeamUps/Gamora/HomerunBleedTrigger.prototype", false,  0.023333f),  // Gamora/HomerunBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Gamora/BulletSpray.prototype",  true,  0.023333f),  // Gamora/BulletSpray.prototype
            new("Powers/TeamUps/Gamora/SwordLeap.prototype",  true,  0.023333f),  // Gamora/SwordLeap.prototype
            new("Powers/TeamUps/Gamora/DeathDealer.prototype",  true,  0.023333f),  // Gamora/DeathDealer.prototype
            new("Powers/TeamUps/Gamora/AssassinInstincts.prototype", false,  0.023333f),  // Gamora/AssassinInstincts.prototype - away passive
            new("Powers/TeamUps/Gamora/BladeDash.prototype",  true,  0.023333f),  // Gamora/BladeDash.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
