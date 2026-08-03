using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// OldManLogan - rendered as the OldManLogan Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpOldManLogan : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/OldManLogan.prototype");

        public IncursionEnemyTeamUpOldManLogan(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Old Man Logan Invader";

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
            new("Powers/TeamUps/WolverineBrood/CrimsonLeap.prototype",  true,  0.0420f), // 2026-07-30
            new("Powers/TeamUps/WolverineBrood/FuriousLunge.prototype",  true,  0.0384f), // 2026-07-30
            new("Powers/TeamUps/WolverineBrood/UnstoppableBeast.prototype", false,  0.023333f),  // WolverineBrood/UnstoppableBeast.prototype - away passive
            new("Powers/TeamUps/WolverineBrood/HasteSteroidTrigger.prototype", false,  0.023333f),  // WolverineBrood/HasteSteroidTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/WolverineBrood/BloodySweep.prototype",  true,  0.0516f), // 2026-07-30
            new("Powers/TeamUps/WolverineBrood/BloodyClaws.prototype",  true,  0.023333f),  // WolverineBrood/BloodyClaws.prototype
            new("Powers/TeamUps/WolverineBrood/Eviscerate.prototype",  true,  0.0234f), // 2026-07-30
            new("Powers/TeamUps/WolverineBrood/EviscerateTrigger.prototype", false,  0.0234f), // 2026-07-30
            new("Powers/TeamUps/WolverineBrood/WetworkBuffProc.prototype", false,  0.023333f),  // WolverineBrood/WetworkBuffProc.prototype - away passive
            new("Powers/TeamUps/WolverineBrood/FuriousAssault.prototype",  true,  0.0244f), // 2026-07-30
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
