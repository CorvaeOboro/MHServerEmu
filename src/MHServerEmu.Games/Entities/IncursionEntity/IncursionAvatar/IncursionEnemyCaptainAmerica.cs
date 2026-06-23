using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// CaptainAmerica Invader
    /// Powers: 16 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyCaptainAmerica : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/CaptainAmerica.prototype");

        public IncursionEnemyCaptainAmerica(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "CaptainAmerica Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/ANADSteveRogers.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/AgeOfUltronMovie.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/AgeOfUltronMovieNoHelmet.prototype", true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/AmericanDream.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Arctic.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Avengers.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/CivilWarMovie.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Classic.prototype",                  true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/ClassicMH2013.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/EarthX.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/MarvelNOW.prototype",                true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/ModernSoldier.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Nomad.prototype",                    true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Original.prototype",                 true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/OriginalMH2013.prototype",           true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Reborn.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/ShaderCheck.prototype",              true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/Skrull.prototype",                   true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/SuperSoldier.prototype",             true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/TheCaptain.prototype",               true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/WinterSoldier.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/WinterSoldierRedShield.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/CaptainAmerica/WorldWar2.prototype",                true),
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
            new("Powers/Player/CaptainAmerica/BackwardsTumble.prototype",                      true,  0.0879f), // 2026-06-11
            new("Powers/Player/CaptainAmerica/BoomerangThrow.prototype",                       true,  0.0505f), // 2026-06-17
            new("Powers/Player/CaptainAmerica/BroadStrike.prototype",                          true,  0.0632f), // 2026-06-16
            new("Powers/Player/CaptainAmerica/BrutalStrike.prototype",                         true,  0.0486f), // 2026-06-16
            new("Powers/Player/CaptainAmerica/DeathFromAbove.prototype",                       true,  0.0360f), // 2026-06-17
            new("Powers/Player/CaptainAmerica/FinestHour.prototype",                           true,  0.0122f), // 2026-06-18
            new("Powers/Player/CaptainAmerica/FirstStrike.prototype",                          true,  0.0779f), // 2026-06-16
            new("Powers/Player/CaptainAmerica/FuriousLunge.prototype",                         true,  0.1151f), // 2026-06-18
            new("Powers/Player/CaptainAmerica/PatrioticTaunt.prototype",                       true,  0.05f),
            new("Powers/Player/CaptainAmerica/ShieldBash.prototype",                           true,  0.0249f), // 2026-06-17
            new("Powers/Player/CaptainAmerica/ShieldBlock.prototype",                          true,  0.05f),
            new("Powers/Player/CaptainAmerica/ShieldThrowPBAoE.prototype",                     true,  0.0352f), // 2026-06-18
            new("Powers/Player/CaptainAmerica/SoundRicochet.prototype",                        true,  0.0604f), // 2026-06-18
            new("Powers/Player/CaptainAmerica/Talents/GuardedTeamBuffSpec.prototype",          false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/HeroicStrikeShieldSwipeSpec.prototype",  false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/InspiredTeamBuffSpec.prototype",         false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/NoSerumSpec.prototype",                  false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/PBAoEShieldThrow.prototype",             false, 0.03f),
            new("Powers/Player/CaptainAmerica/Talents/Serum100PctSpenderSpec.prototype",       false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/SerumDoubleSpec.prototype",              false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/SerumShieldThrows.prototype",            false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/ShieldBleeds.prototype",                 false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/ShieldBlockCooldownSerumSpec.prototype", false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/ShieldBlockDeflect100PctSpec.prototype", false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/ShieldBlockDurationIncSpec.prototype",   false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/ShieldStrikeBonusDamageSpec.prototype",  false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/StrengthenedTeamBuffSpec.prototype",     false, 0.05f),
            new("Powers/Player/CaptainAmerica/Talents/VibraniumBashSerumSpec.prototype",       false, 0.0541f), // 2026-06-16
            new("Powers/Player/CaptainAmerica/Traits/DefenseTrait.prototype",                  false, 0.05f),
            new("Powers/Player/CaptainAmerica/Traits/MechanicTraitSerum.prototype",            false, 0.05f),
            new("Powers/Player/CaptainAmerica/Traits/OffenseTrait.prototype",                  false, 0.05f),
            new("Powers/Player/CaptainAmerica/Ultimate.prototype",                             true,  0.0100f), // 2026-06-18
            new("Powers/Player/CaptainAmerica/Vault.prototype",                                true,  0.05f),
            new("Powers/Player/CaptainAmerica/VibraniumBash.prototype",                        true,  0.0541f), // 2026-06-16
            new("Powers/Player/TravelPower/CaptainAmericaSprint.prototype",                    false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CaptainAmericaStolenPower.prototype",     false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                            false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                    false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                       false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                          false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                false, 0.05f),
        };
    }
}
