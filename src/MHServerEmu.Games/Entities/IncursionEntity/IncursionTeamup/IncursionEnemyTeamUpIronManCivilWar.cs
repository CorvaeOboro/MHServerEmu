using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// IronManCivilWar - rendered as the IronManCivilWar Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpIronManCivilWar : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/IronManCivilWar.prototype");

        public IncursionEnemyTeamUpIronManCivilWar(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Iron Man Civil War Invader";
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
            new("Powers/TeamUps/IronManMark2/Micromissiles.prototype",  true,  0.0950f), // 2026-07-30
            new("Powers/TeamUps/IronManMark2/MicromissilesAoETrigger.prototype", false,  0.023333f),  // IronManMark2/MicromissilesAoETrigger.prototype - trigger/secondary
            new("Powers/TeamUps/IronManMark2/AwayMissileBombardment.prototype", false,  0.023333f),  // IronManMark2/AwayMissileBombardment.prototype - away passive
            new("Powers/TeamUps/IronManMark2/MissileSalvo.prototype",  true,  0.0798f), // 2026-07-30
            new("Powers/TeamUps/IronManMark2/ChanneledBeam.prototype",  true,  0.0428f), // 2026-07-30
            new("Powers/TeamUps/IronManMark2/DamageShield.prototype", false,  0.023333f),  // IronManMark2/DamageShield.prototype - defensive
            new("Powers/TeamUps/IronManMark2/MissileSalvoExtraShotsTrigger.prototype", false,  0.023333f),  // IronManMark2/MissileSalvoExtraShotsTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/IronManMark2/RapidFire.prototype",  true,  0.0339f), // 2026-07-30
            new("Powers/TeamUps/IronManMark2/AwayDamageShield.prototype", false,  0.023333f),  // IronManMark2/AwayDamageShield.prototype - away passive
            new("Powers/TeamUps/IronManMark2/OneOff.prototype",  true,  0.0187f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
