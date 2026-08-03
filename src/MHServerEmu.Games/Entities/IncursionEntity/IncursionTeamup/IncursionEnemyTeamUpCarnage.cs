using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// Carnage - rendered as the Carnage Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpCarnage : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Carnage.prototype");

        public IncursionEnemyTeamUpCarnage(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Carnage Invader";

                        // HardcodeExclude: Carnage has Avatar version better power set
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
            new("Powers/TeamUps/Carnage/Impale.prototype",  true,  0.0577f), // 2026-07-30
            new("Powers/TeamUps/Carnage/DeathFromAbove.prototype",  true,  0.0260f), // 2026-07-30
            new("Powers/TeamUps/Carnage/AwayDFAProc.prototype", false,  0.023333f),  // Carnage/AwayDFAProc.prototype - away passive
            new("Powers/TeamUps/Carnage/PBAoE.prototype",  true,  0.0355f), // 2026-07-30
            new("Powers/TeamUps/Carnage/AxeSweep.prototype",  true,  0.0389f), // 2026-07-30
            new("Powers/TeamUps/Carnage/AxeThrow.prototype",  true,  0.0580f), // 2026-07-30
            new("Powers/TeamUps/Carnage/AwayPBAoEProc.prototype", false,  0.023333f),  // Carnage/AwayPBAoEProc.prototype - away passive
            new("Powers/TeamUps/Carnage/Hamstring.prototype",  true,  0.023333f),  // Carnage/Hamstring.prototype
            new("Powers/TeamUps/Carnage/BleedingAxe.prototype",  true,  0.023333f),  // Carnage/BleedingAxe.prototype
            new("Powers/TeamUps/Carnage/MeleeOneOff.prototype",  true,  0.0193f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
