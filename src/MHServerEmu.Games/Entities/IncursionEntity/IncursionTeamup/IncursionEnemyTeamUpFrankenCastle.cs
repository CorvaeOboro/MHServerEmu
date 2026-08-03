using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// FrankenCastle - rendered as the FrankenCastle Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpFrankenCastle : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/FrankenCastle.prototype");

        public IncursionEnemyTeamUpFrankenCastle(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Franken Castle Invader";

                 // HardcodeExclude: has avatar version 
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
            new("Powers/TeamUps/FrankenCastle/ShotgunBlast.prototype",  true,  0.0559f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/RPG.prototype",  true,  0.0352f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/AwayMissileProc.prototype", false,  0.023333f),  // FrankenCastle/AwayMissileProc.prototype - away passive
            new("Powers/TeamUps/FrankenCastle/ShotgunBlastBleedTrigger.prototype", false,  0.0559f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/ChemicalBomb.prototype",  true,  0.0337f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/ChemicalBombHotspotTrigger.prototype", false,  0.0337f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/ArmorPiercing.prototype",  true,  0.0352f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/AwaySniperProc.prototype", false,  0.023333f),  // FrankenCastle/AwaySniperProc.prototype - away passive
            new("Powers/TeamUps/FrankenCastle/Flamethrower.prototype",  true,  0.1038f), // 2026-07-30
            new("Powers/TeamUps/FrankenCastle/Bazooka.prototype",  true,  0.0191f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
