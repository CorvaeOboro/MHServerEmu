using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// SheHulk - rendered as the SheHulk Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpSheHulk : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/SheHulk.prototype");

        public IncursionEnemyTeamUpSheHulk(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "She Hulk Invader";

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
            new("Powers/TeamUps/SheHulk/PBAoE.prototype",  true,  0.0703f), // 2026-07-30
            new("Powers/TeamUps/SheHulk/GammaElbowDrop.prototype",  true,  0.0262f), // 2026-07-30
            new("Powers/TeamUps/SheHulk/AwayElbowDropProc.prototype", false,  0.023333f),  // SheHulk/AwayElbowDropProc.prototype - away passive
            new("Powers/TeamUps/SheHulk/BarristerBeatdown.prototype",  true,  0.023333f),  // SheHulk/BarristerBeatdown.prototype
            new("Powers/TeamUps/SheHulk/BriefcaseThrow.prototype",  true,  0.0434f), // 2026-07-30
            new("Powers/TeamUps/SheHulk/MoveToStrike.prototype",  true,  0.0317f), // 2026-07-30
            new("Powers/TeamUps/SheHulk/Taunt.prototype",  true,  0.023333f),  // SheHulk/Taunt.prototype
            new("Powers/TeamUps/SheHulk/HulkOutSteroid.prototype", false,  0.023333f),  // SheHulk/HulkOutSteroid.prototype - defensive
            new("Powers/TeamUps/SheHulk/AwayLawyerUpProc.prototype", false,  0.023333f),  // SheHulk/AwayLawyerUpProc.prototype - away passive
            new("Powers/TeamUps/SheHulk/Conviction.prototype",  true,  0.0181f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
