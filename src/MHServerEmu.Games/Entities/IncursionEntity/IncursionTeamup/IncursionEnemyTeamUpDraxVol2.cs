using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Team-Up Invader
    /// DraxVol2 - rendered as the DraxVol2 Team-Up actor.
    /// Powers: 8 active / 11 total
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyTeamUpDraxVol2 : IncursionEnemyTeamup
    {
        private static readonly PrototypeId TeamUpRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/DraxVol2.prototype");

        public IncursionEnemyTeamUpDraxVol2(Game game) : base(game) { }

        public override PrototypeId RenderTeamupRef => TeamUpRef;
        public override string InvaderDisplayName => "Drax Vol2 Invader";

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
            new("Powers/TeamUps/Drax/SwordLeap.prototype",  true,  0.023333f),  // Drax/SwordLeap.prototype
            new("Powers/TeamUps/Drax/KnifeThrow.prototype",  true,  0.023333f),  // Drax/KnifeThrow.prototype
            new("Powers/TeamUps/Drax/MeleeBuffProc.prototype", false,  0.023333f),  // Drax/MeleeBuffProc.prototype - away passive
            new("Powers/TeamUps/Drax/SwordLeapBleedTrigger.prototype", false,  0.023333f),  // Drax/SwordLeapBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Drax/PBAoE.prototype",  true,  0.023333f),  // Drax/PBAoE.prototype
            new("Powers/TeamUps/Drax/PBAoEBleedTrigger.prototype", false,  0.023333f),  // Drax/PBAoEBleedTrigger.prototype - trigger/secondary
            new("Powers/TeamUps/Drax/AwaySwordLeapVol2.prototype", false,  0.023333f),  // Drax/AwaySwordLeapVol2.prototype - away passive
            new("Powers/TeamUps/Drax/ChuckConcrete.prototype",  true,  0.023333f),  // Drax/ChuckConcrete.prototype
            new("Powers/TeamUps/Drax/Whirlwind.prototype",  true,  0.023333f),  // Drax/Whirlwind.prototype
            new("Powers/TeamUps/Drax/RageSteroid.prototype", false,  0.023333f),  // Drax/RageSteroid.prototype - defensive
            new("Powers/TeamUps/TeamUpSynergyHeroPassive.prototype", false,  0.023333f),  // TeamUpSynergyHeroPassive.prototype - synergy passive
        };
    }
}
