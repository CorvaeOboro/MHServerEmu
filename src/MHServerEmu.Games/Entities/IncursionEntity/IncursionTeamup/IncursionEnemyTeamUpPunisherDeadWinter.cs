using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// PunisherDeadWinter - rendered as the PunisherDeadWinter Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpPunisherDeadWinter : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/PunisherDeadWinter.prototype");

        public IncursionEnemyTeamUpPunisherDeadWinter(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Punisher Dead Winter Invader";

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
            new("Powers/TeamUps/FrankenCastle/ShotgunBlast.prototype",  true,  0.023333f),  // FrankenCastle/ShotgunBlast.prototype
            new("Powers/TeamUps/FrankenCastle/RPG.prototype",  true,  0.023333f),  // FrankenCastle/RPG.prototype
            new("Powers/TeamUps/FrankenCastle/AwayMissileProc.prototype", false,  0.023333f),  // FrankenCastle/AwayMissileProc.prototype - away passive
            new("Powers/TeamUps/FrankenCastle/ShotgunBlastBleedTrigger.prototype", false,  0.023333f),  // FrankenCastle/ShotgunBlastBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/FrankenCastle/ChemicalBomb.prototype",  true,  0.023333f),  // FrankenCastle/ChemicalBomb.prototype
            new("Powers/TeamUps/FrankenCastle/ChemicalBombHotspotTrigger.prototype", false,  0.023333f),  // FrankenCastle/ChemicalBombHotspotTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/FrankenCastle/ArmorPiercing.prototype",  true,  0.023333f),  // FrankenCastle/ArmorPiercing.prototype
            new("Powers/TeamUps/FrankenCastle/AwaySniperProc.prototype", false,  0.023333f),  // FrankenCastle/AwaySniperProc.prototype - away passive
            new("Powers/TeamUps/FrankenCastle/Flamethrower.prototype",  true,  0.023333f),  // FrankenCastle/Flamethrower.prototype
            new("Powers/TeamUps/FrankenCastle/Bazooka.prototype",  true,  0.023333f),  // FrankenCastle/Bazooka.prototype
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
