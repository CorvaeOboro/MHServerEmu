using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// WarMachine Invader
    /// Powers: 20 / 48
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyWarMachine : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/WarMachine.prototype");

        public IncursionEnemyWarMachine(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "WarMachine Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/WarMachine/AgeOfUltronMovie.prototype", true),
            new("Entity/Items/Costumes/Prototypes/WarMachine/CivilWarMovie.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/WarMachine/Initiative.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/WarMachine/IronMan3Movie.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/WarMachine/IronPatriot.prototype",      true),
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
            new("Powers/Player/WarMachine/AlphaStrike.prototype",                           true,  0.0473f), // 2026-06-20
            new("Powers/Player/WarMachine/AutogunPassive.prototype",                        false, 0.05f),
            new("Powers/Player/WarMachine/BasicLaserBlade.prototype",                       true,  0.0685f), // 2026-06-20
            new("Powers/Player/WarMachine/BulletOneOff.prototype",                          true,  0.0490f), // 2026-06-20
            new("Powers/Player/WarMachine/ChaingunBulletSpray.prototype",                   true,  0.1427f), // 2026-06-18
            new("Powers/Player/WarMachine/ChaingunBurst.prototype",                         true,  0.2397f), // 2026-06-20
            new("Powers/Player/WarMachine/ChaingunFullAuto.prototype",                      true,  0.1690f), // 2026-06-20
            new("Powers/Player/WarMachine/ChainsawImpale.prototype",                        true,  0.0537f), // 2026-06-20
            new("Powers/Player/WarMachine/Chainsaws.prototype",                             true,  0.0904f), // 2026-06-18
            new("Powers/Player/WarMachine/DeathFromAbove.prototype",                        true,  0.0352f), // 2026-06-20
            new("Powers/Player/WarMachine/EMP.prototype",                                   true,  0.0598f), // 2026-06-20
            new("Powers/Player/WarMachine/FlameThrower.prototype",                          true,  0.0356f), // 2026-06-18
            new("Powers/Player/WarMachine/HeatDecay.prototype",                             true,  0.05f),
            new("Powers/Player/WarMachine/LaserBladeDash.prototype",                        true,  0.0886f), // 2026-06-18
            new("Powers/Player/WarMachine/Overheat.prototype",                              true,  0.05f),
            new("Powers/Player/WarMachine/PlasmaCannon.prototype",                          true,  0.0846f), // 2026-06-19
            new("Powers/Player/WarMachine/Talents/Talent1AutoGun.prototype",                false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent1LaserBlade.prototype",             false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent1PlasmaCannon.prototype",           false, 0.0846f), // 2026-06-19
            new("Powers/Player/WarMachine/Talents/Talent2Chainsaws.prototype",              false, 0.0904f), // 2026-06-18
            new("Powers/Player/WarMachine/Talents/Talent2MissileTargeting.prototype",       false, 0.01f),
            new("Powers/Player/WarMachine/Talents/Talent2ThermalExpulsion.prototype",       false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent3ArcReactor.prototype",             false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent3MachineGuns.prototype",            false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent3NanoEnhancers.prototype",          false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent4CoolantSystems.prototype",         false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent4Overclocking.prototype",           false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent4ReactiveShockArmor.prototype",     false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent5EnergyBarrier.prototype",          false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent5ReinforcedArmorPlating.prototype", false, 0.05f),
            new("Powers/Player/WarMachine/Talents/Talent5StealthSuit.prototype",            false, 0.05f),
            new("Powers/Player/WarMachine/TearGas.prototype",                               true,  0.05f),
            new("Powers/Player/WarMachine/ThermalShot.prototype",                           true,  0.05f),
            new("Powers/Player/WarMachine/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/WarMachine/Traits/MechanicTrait.prototype",                  false, 0.05f),
            new("Powers/Player/WarMachine/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/WarMachine/UltimateHiddenPassive.prototype",                 false, 0.006f),
            new("Powers/Player/WarMachine/UltimateSidekick.prototype",                      true,  0.0074f), // 2026-06-18
            new("Powers/Player/WarMachine/WarMachineArmor.prototype",                       true,  0.0715f), // 2026-06-20
            new("Powers/Player/WarMachine/Warhead.prototype",                               true,  0.0369f), // 2026-06-20
            new("Powers/StolenPowers/StealablePowers/WarMachineStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
            new("Powers/Player/TravelPower/WarMarchineFlight.prototype",                    false, 0.05f),
        };
    }
}
