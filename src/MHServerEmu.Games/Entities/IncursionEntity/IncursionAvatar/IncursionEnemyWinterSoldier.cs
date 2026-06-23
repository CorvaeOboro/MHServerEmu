using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// WinterSoldier Invader
    /// Powers: 17 / 43
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyWinterSoldier : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/WinterSoldier.prototype");

        public IncursionEnemyWinterSoldier(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "WinterSoldier Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/CivilWar.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/MovieCap2.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/MovieCap2NoMask.prototype", true),
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/OriginalSin.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/WinterSoldier/SnowGear.prototype",        true),
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
            new("Powers/Player/TravelPower/WinterSoldierSprint.prototype",                     false, 0.05f),
            new("Powers/Player/WinterSoldier/ArmBlast.prototype",                              true,  0.0239f), // 2026-06-17
            new("Powers/Player/WinterSoldier/ArmSmash.prototype",                              true,  0.0179f), // 2026-06-16
            new("Powers/Player/WinterSoldier/Assassinate.prototype",                           true,  0.0184f), // 2026-06-16
            new("Powers/Player/WinterSoldier/BulletSpray.prototype",                           true,  0.0397f), // 2026-06-16
            new("Powers/Player/WinterSoldier/FuriousLunge.prototype",                          true,  0.1613f), // 2026-06-03
            new("Powers/Player/WinterSoldier/GrenadeLauncher.prototype",                       true,  0.1192f), // 2026-06-17
            new("Powers/Player/WinterSoldier/Haymaker.prototype",                              true,  0.0777f), // 2026-06-17
            new("Powers/Player/WinterSoldier/KnifeThrow.prototype",                            true,  0.0822f), // 2026-06-17
            new("Powers/Player/WinterSoldier/PistolShot.prototype",                            true,  0.2141f), // 2026-06-17
            new("Powers/Player/WinterSoldier/RapidFire.prototype",                             true,  0.0500f), // 2026-06-16
            new("Powers/Player/WinterSoldier/SniperShot.prototype",                            true,  0.0382f), // 2026-06-17
            new("Powers/Player/WinterSoldier/SpinningMines.prototype",                         true,  0.05f),
            new("Powers/Player/WinterSoldier/StealthMineToss.prototype",                       true,  0.1247f), // 2026-06-17
            new("Powers/Player/WinterSoldier/StealthRoll.prototype",                           true,  0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent1BionicBrawling.prototype",         false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent1BionicallyChargedThrow.prototype", false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent1FirearmStabilizer.prototype",      false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent2BrutalKiller.prototype",           false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent2StealthCDRDmgBuff.prototype",      false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent2TargetDispatched.prototype",       false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent3BionicEMP.prototype",              false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent3InfiltrationGear.prototype",       false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent3RifleBonuses.prototype",           false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent4BionicsDmgBuff.prototype",         false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent4FirearmsDmgBuff.prototype",        false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent4SpinningMinesFreeCast.prototype",  false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent5DefBuffs.prototype",               false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent5StealthBuffs.prototype",           false, 0.05f),
            new("Powers/Player/WinterSoldier/Talents/Talent5TargetAcquired.prototype",         false, 0.05f),
            new("Powers/Player/WinterSoldier/TeamStealth.prototype",                           true,  0.05f),
            new("Powers/Player/WinterSoldier/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/WinterSoldier/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/WinterSoldier/TripleShot.prototype",                            true,  0.3573f), // 2026-06-16
            new("Powers/Player/WinterSoldier/Ultimate.prototype",                              true,  0.0094f), // 2026-06-16
            new("Powers/Player/WinterSoldier/UltimateHiddenPassive.prototype",                 false, 0.0094f), // 2026-06-16
            new("Powers/StolenPowers/StealablePowers/WinterSoldierStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                            false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                    false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                       false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                          false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                false, 0.05f),
        };
    }
}
