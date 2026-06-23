using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Daredevil Invader
    /// Powers: 17 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyDaredevil : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Daredevil.prototype");

        public IncursionEnemyDaredevil(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Daredevil Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Daredevil/Armored.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/EarthX.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/ImNotDaredevil.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/ManWOFear.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/ManWOFearBattleDamaged.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/Modern.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/ModernMH2013.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/NetflixFinal.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/NetflixMaskless.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/NetflixSeason1.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/Noir.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/Original.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/SecretWar.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/Daredevil/Shadowland.prototype",             true),
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
            new("Powers/Player/Daredevil/Talents/BouncingStrikeAdditionalHitsTalents.prototype", false, 0.05f),
            new("Powers/Player/Daredevil/Talents/BrutalStrikeFinisherCritDamage.prototype",      false, 0.05f),
            new("Powers/Player/Daredevil/Talents/ComboHealTalent.prototype",                     false, 0.025f),
            new("Powers/Player/Daredevil/Talents/ComboInvulnTalent.prototype",                   false, 0.025f),
            new("Powers/Player/Daredevil/Talents/DamageCritBrutBuffTalent.prototype",            false, 0.05f),
            new("Powers/Player/Daredevil/Talents/NoComboPointsTalent.prototype",                 false, 0.025f),
            new("Powers/Player/Daredevil/Talents/NormalPointsBuffTalent.prototype",              false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerCaneSlowTalent.prototype",                false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerClubWeakenTalent.prototype",              false, 0.05f),
            new("Powers/Player/Daredevil/Talents/OpenerNunchuckStunTalent.prototype",            false, 0.05f),
            new("Powers/Player/Daredevil/Talents/SigBuffTalent.prototype",                       false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SigCooldownReductionTalent.prototype",          false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SigDoubleDamageCenterTalent.prototype",         false, 0.02f),
            new("Powers/Player/Daredevil/Talents/SlowComboPointTalent.prototype",                false, 0.025f),
            new("Powers/Player/Daredevil/Talents/WhirlingClubStaminaCancelTalent.prototype",     false, 0.1633f), // 2026-06-18
            new("Powers/Player/Daredevil/Traits/DefenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/Daredevil/Traits/MechanicTraitComboPoints.prototype",             false, 0.025f),
            new("Powers/Player/Daredevil/Traits/OffenseTrait.prototype",                         false, 0.05f),
            new("Powers/Player/Daredevil/Ultimate.prototype",                                    true,  0.0173f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/BillyClubSweep.prototype",                       true,  0.1513f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/BouncingStrike.prototype",                       true,  0.05f),
            new("Powers/Player/Daredevil/Update/BrutalStrike.prototype",                         true,  0.05f),
            new("Powers/Player/Daredevil/Update/CaneAttack.prototype",                           true,  0.1156f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/ClubAttack.prototype",                           true,  0.1415f), // 2026-06-16
            new("Powers/Player/Daredevil/Update/ClubRicochet.prototype",                         true,  0.05f),
            new("Powers/Player/Daredevil/Update/ComboPointGainMechanic.prototype",               true,  0.025f),
            new("Powers/Player/Daredevil/Update/ComboPointHiddenPassive.prototype",              false, 0.025f),
            new("Powers/Player/Daredevil/Update/ConeYank.prototype",                             true,  0.0436f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/NunchuckAttack.prototype",                       true,  0.1307f), // 2026-06-17
            new("Powers/Player/Daredevil/Update/NunchuckBulldoze.prototype",                     true,  0.0655f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/OpeningLunge.prototype",                         true,  0.0499f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/RoundhouseKick.prototype",                       true,  0.05f),
            new("Powers/Player/Daredevil/Update/ShadowStrike.prototype",                         true,  0.0202f), // 2026-06-18
            new("Powers/Player/Daredevil/Update/Tumble.prototype",                               true,  0.05f),
            new("Powers/Player/Daredevil/Update/Vault.prototype",                                true,  0.05f),
            new("Powers/Player/Daredevil/Update/WhirlingClub.prototype",                         true,  0.1633f), // 2026-06-18
            new("Powers/Player/TravelPower/DaredevilFlight.prototype",                           false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DaredevilStolenPower.prototype",            false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                              false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                      false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                         false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                            false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                  false, 0.05f),
        };
    }
}
