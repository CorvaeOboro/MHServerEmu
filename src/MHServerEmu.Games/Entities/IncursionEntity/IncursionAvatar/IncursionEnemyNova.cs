using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Nova Invader
    /// Powers: 17 / 45
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyNova : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Nova.prototype");

        public IncursionEnemyNova(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Nova Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Nova/RRNovaPrime.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Nova/RRNovaPrimeVU.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Nova/RROriginal.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Nova/SABlackVortex.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Nova/SAMarvelNOW.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Nova/SAMarvelNOWVU.prototype", true),
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
            new("Powers/Player/Nova/ArcBurst.prototype",                                   true,  0.0503f), // 2026-06-17
            new("Powers/Player/Nova/BasicPunch.prototype",                                 true,  0.1201f), // 2026-06-18
            new("Powers/Player/Nova/BasicSpiritBeam.prototype",                            true,  0.1108f), // 2026-06-18
            new("Powers/Player/Nova/BouncingStrike.prototype",                             true,  0.0477f), // 2026-06-20
            new("Powers/Player/Nova/ChanneledPulsarBeam.prototype",                        true,  0.0430f), // 2026-06-20
            new("Powers/Player/Nova/ChargedDash.prototype",                                true,  0.05f),
            new("Powers/Player/Nova/DeathFromAbove.prototype",                             true,  0.1104f), // 2026-06-20
            new("Powers/Player/Nova/FuriousLunge.prototype",                               true,  0.1048f), // 2026-06-20
            new("Powers/Player/Nova/HeavyBlast.prototype",                                 true,  0.1351f), // 2026-06-20
            new("Powers/Player/Nova/HeavyBlastHiddenPassive.prototype",                    false, 0.05f),
            new("Powers/Player/Nova/LungingPunch.prototype",                               true,  0.1237f), // 2026-06-20
            new("Powers/Player/Nova/MegaPunch.prototype",                                  true,  0.0774f), // 2026-06-18
            new("Powers/Player/Nova/PBAoENuke.prototype",                                  true,  0.0881f), // 2026-06-20
            new("Powers/Player/Nova/PassiveSRShieldHiddenPassive.prototype",               false, 0.05f),
            new("Powers/Player/Nova/PulsarExplosion.prototype",                            true,  0.0357f), // 2026-06-13
            new("Powers/Player/Nova/PulsarHotspot.prototype",                              true,  0.0931f), // 2026-06-20
            new("Powers/Player/Nova/PulsarImplosion.prototype",                            true,  0.1358f), // 2026-06-20
            new("Powers/Player/Nova/SignatureSupernova.prototype",                         true,  0.0174f), // 2026-06-20
            new("Powers/Player/Nova/Talents/Talent1ChanneledBeamDetonateBuff.prototype",   false, 0.025f),
            new("Powers/Player/Nova/Talents/Talent1MeleeFreeNovaPulse.prototype",          false, 0.01f),
            new("Powers/Player/Nova/Talents/Talent1MovementExplosion.prototype",           false, 0.01f),
            new("Powers/Player/Nova/Talents/Talent2MeleeBuffs.prototype",                  false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent2PulsarProximityBuffs.prototype",        false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent2PulsarSpiritRestoreDmgStack.prototype", false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent3MicroMagnetonPulsar.prototype",         false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent3MicroSupermassivePulsar.prototype",     false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent3MicroUnstablePulsar.prototype",         false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent4PulsarChargeDmg.prototype",             false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent4PulsarFastDetonation.prototype",        false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent4PulsarFreeSecond.prototype",            false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent5DetonationPowers.prototype",            false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent5MaxShieldDFACharge.prototype",          false, 0.05f),
            new("Powers/Player/Nova/Talents/Talent5SigCDPowers.prototype",                 false, 0.05f),
            new("Powers/Player/Nova/Traits/DefenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Nova/Traits/MechanicTrait.prototype",                       false, 0.05f),
            new("Powers/Player/Nova/Traits/OffenseTrait.prototype",                        false, 0.05f),
            new("Powers/Player/Nova/UltimateNovaCorps.prototype",                          true,  0.0097f), // 2026-06-20
            new("Powers/Player/TravelPower/NovaFlight.prototype",                          false, 0.01f),
            new("Powers/StolenPowers/StealablePowers/NovaStolenPower.prototype",           false, 0.01f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                 false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                   false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                      false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                            false, 0.05f),
        };
    }
}
