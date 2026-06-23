using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// DoctorStrange Invader
    /// Powers: 18 / 47
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyDoctorStrange : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/DoctorStrange.prototype");

        public IncursionEnemyDoctorStrange(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "DoctorStrange Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/DrStrange/Classic.prototype", true),
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
            new("Powers/Player/DoctorStrange/AstralCloneProjection.prototype",              true,  0.1586f), // 2026-06-17
            new("Powers/Player/DoctorStrange/AstralForm.prototype",                         true,  0.05f),
            new("Powers/Player/DoctorStrange/BasicBolts.prototype",                         true,  0.1126f), // 2026-06-18
            new("Powers/Player/DoctorStrange/BasicBoltsHiddenPassive.prototype",            false, 0.1126f), // 2026-06-18
            new("Powers/Player/DoctorStrange/BasicDaggers.prototype",                       true,  0.2220f), // 2026-06-18
            new("Powers/Player/DoctorStrange/BasicDaggersHiddenPassive.prototype",          false, 0.05f),
            new("Powers/Player/DoctorStrange/ConeShards.prototype",                         true,  0.0496f), // 2026-06-18
            new("Powers/Player/DoctorStrange/CrimsonBands.prototype",                       true,  0.0998f), // 2026-06-18
            new("Powers/Player/DoctorStrange/DemonsOfDenak.prototype",                      true,  0.1098f), // 2026-06-18
            new("Powers/Player/DoctorStrange/DemonsOfDenakHiddenPassive.prototype",         false, 0.1098f), // 2026-06-18
            new("Powers/Player/DoctorStrange/EssenceOfZom.prototype",                       true,  0.0304f), // 2026-06-18
            new("Powers/Player/DoctorStrange/EyeOfAgamotto.prototype",                      true,  0.0464f), // 2026-06-18
            new("Powers/Player/DoctorStrange/FangNuke.prototype",                           true,  0.1150f), // 2026-06-18
            new("Powers/Player/DoctorStrange/IcyTendrils.prototype",                        true,  0.0422f), // 2026-06-18
            new("Powers/Player/DoctorStrange/MirrorImage.prototype",                        true,  0.05f),
            new("Powers/Player/DoctorStrange/SeraphimShield.prototype",                     true,  0.05f),
            new("Powers/Player/DoctorStrange/SummonFlames.prototype",                       true,  0.0862f), // 2026-06-18
            new("Powers/Player/DoctorStrange/Talents/AstralFormBonus.prototype",            false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/AstralLegion.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/AstralProjectionTwincast.prototype",   false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/FireAndIce.prototype",                 false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/LightAndDarkness.prototype",           false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismDamagePulse.prototype",       false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismNoReset.prototype",           false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/MysticismRestoration.prototype",       false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/RitualCircle.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SealOfHoggoth.prototype",              false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SealOfOshtur.prototype",               false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/SevenRings.prototype",                 false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/ShieldOfSeraphimAutoshield.prototype", false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/ShieldOfSeraphimTeamBuff.prototype",   false, 0.05f),
            new("Powers/Player/DoctorStrange/Talents/WindAndLightning.prototype",           false, 0.05f),
            new("Powers/Player/DoctorStrange/Teleport.prototype",                           true,  0.0718f), // 2026-06-18
            new("Powers/Player/DoctorStrange/Traits/DefenseTrait.prototype",                false, 0.05f),
            new("Powers/Player/DoctorStrange/Traits/MechanicTraitMysticism.prototype",      false, 0.05f),
            new("Powers/Player/DoctorStrange/Traits/OffenseTrait.prototype",                false, 0.05f),
            new("Powers/Player/DoctorStrange/Vapors.prototype",                             true,  0.0846f), // 2026-06-18
            new("Powers/Player/DoctorStrange/VishantiSeal.prototype",                       true,  0.05f),
            new("Powers/Player/DoctorStrange/WindsOfWatoomb.prototype",                     true,  0.0388f), // 2026-06-18
            new("Powers/Player/TravelPower/DoctorStrangeFlight.prototype",                  false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/DoctorStrangeStolenPower.prototype",   false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
