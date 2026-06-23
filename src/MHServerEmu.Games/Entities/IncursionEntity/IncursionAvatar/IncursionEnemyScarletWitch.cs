using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// ScarletWitch Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyScarletWitch : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/ScarletWitch.prototype");

        public IncursionEnemyScarletWitch(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "ScarletWitch Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/ANAD.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/AgeOfUltronMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/CivilWar.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/DarkWanda.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/ForceWorks.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/HeroesReborn.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/HouseOfM.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/MarvelNOW.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/Modern.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/ModernVU.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/Romani.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/Ultimate.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/ScarletWitch/Wiccan.prototype",           true),
        };

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 120.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 100.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.05f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/Player/ScarletWitch/Rework/AlterReality.prototype",              true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/ArmyFromNothing.prototype",           true,  0.0413f), // 2026-06-08
            new("Powers/Player/ScarletWitch/Rework/BouncingHex.prototype",               true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/ChaosBlast.prototype",                true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/ChaosHex.prototype",                  true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/ChaosRift.prototype",                 true,  0.1338f), // 2026-06-10
            new("Powers/Player/ScarletWitch/Rework/DarkHex.prototype",                   true,  0.1488f), // 2026-06-17
            new("Powers/Player/ScarletWitch/Rework/HexBolt.prototype",                   true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/HexSphere.prototype",                 true,  0.1382f), // 2026-06-18
            new("Powers/Player/ScarletWitch/Rework/Implosion.prototype",                 true,  0.1356f), // 2026-06-10
            new("Powers/Player/ScarletWitch/Rework/IronMaiden.prototype",                true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/Obfuscation.prototype",               true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/RavenousBinding.prototype",           true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/ShadowBolt.prototype",                true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/Teleport.prototype",                  true,  0.05f),
            new("Powers/Player/ScarletWitch/Rework/UnmakeReality.prototype",             true,  0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent1ChaosBuff.prototype",         false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent1NonChaosBuff.prototype",      false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent1Support.prototype",           false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent2SigCDR.prototype",            false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent2SigChaosCosts.prototype",     false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent2SigItemProcs.prototype",      false, 0.025f),
            new("Powers/Player/ScarletWitch/Talents/Talent3BadLuck.prototype",           false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent3FurtherChaos.prototype",      false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent3RipApartReality.prototype",   false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent4Restoration.prototype",       false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent4RuinousFlux.prototype",       false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent4WitheringAgony.prototype",    false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent5AlterRealityBuff.prototype",  false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent5ChaosBlast.prototype",        false, 0.05f),
            new("Powers/Player/ScarletWitch/Talents/Talent5ImplosionBuff.prototype",     false, 0.1356f), // 2026-06-10
            new("Powers/Player/ScarletWitch/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/ScarletWitch/Traits/MechanicTraitChaosEnergy.prototype",  false, 0.05f),
            new("Powers/Player/ScarletWitch/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/ScarletWitch/Ultimate.prototype",                         true,  0.0084f), // 2026-06-10
            new("Powers/Player/TravelPower/ScarletWitchFlight.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/ScarletWitchStolenPower.prototype", false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",               false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                      false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",              false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                 false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                    false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                          false, 0.05f),
        };
    }
}
