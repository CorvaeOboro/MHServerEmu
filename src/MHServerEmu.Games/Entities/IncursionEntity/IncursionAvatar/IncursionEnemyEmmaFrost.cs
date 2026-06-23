using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// EmmaFrost Invader
    /// Powers: 21 / 49
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyEmmaFrost : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/EmmaFrost.prototype");

        public IncursionEnemyEmmaFrost(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "EmmaFrost Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/MarvelNOW.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/Modern.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/Modern2013.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/NewXMen.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/OldManLogan.prototype",  true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/PhoenixForce.prototype", true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/Punk.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/EmmaFrost/WhiteQueen.prototype",   true),
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
            new("Powers/Player/EmmaFrost/BasicSpiritGain.prototype",                         true,  0.1167f), // 2026-06-19
            new("Powers/Player/EmmaFrost/ControlMob.prototype",                              true,  0.0585f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/AmpControlledMobCDR.prototype",             false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/AmpControlledMobMentalOverload.prototype",  false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/AmpControlledMobNoKill.prototype",          false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/AmpControlledMobUnlockPotential.prototype", false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/AoEFearMassConfusion.prototype",            false, 0.0801f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/ControlledMobToIllusion.prototype",         false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/DamageConeIronMaiden.prototype",            false, 0.0725f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/DiamondArmorAlwaysOn.prototype",            false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/DiamondHeartBonus.prototype",               false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/DiamondWhirlwindReflectBuff.prototype",     false, 0.0522f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/KneelBeforeMeCDR.prototype",                false, 0.0195f), // 2026-06-18
            new("Powers/Player/EmmaFrost/Talents/MaxDiamondArmorBonus.prototype",            false, 0.05f),
            new("Powers/Player/EmmaFrost/Talents/MegaSlapChargeAxeHeelDropBuff.prototype",   false, 0.0396f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/PsychicSpearChargeWhipVuln.prototype",      false, 0.0347f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Talents/TeamBuffer.prototype",                      false, 0.05f),
            new("Powers/Player/EmmaFrost/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/EmmaFrost/Traits/DiamondFormCondition.prototype",             false, 0.05f),
            new("Powers/Player/EmmaFrost/Traits/MechanicTraitDiamondForm.prototype",         false, 0.05f),
            new("Powers/Player/EmmaFrost/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/EmmaFrost/Ultimate.prototype",                                true,  0.0110f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/AmpControlledMob.prototype",                 true,  0.05f),
            new("Powers/Player/EmmaFrost/Update/AoEFear.prototype",                          true,  0.0801f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/AreaDoT.prototype",                          true,  0.0834f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/AstralWhip.prototype",                       true,  0.0285f), // 2026-06-18
            new("Powers/Player/EmmaFrost/Update/AxeHeelDrop.prototype",                      true,  0.0396f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/BigPunch.prototype",                         true,  0.0489f), // 2026-06-18
            new("Powers/Player/EmmaFrost/Update/ControlledMobHiddenPassive.prototype",       false, 0.05f),
            new("Powers/Player/EmmaFrost/Update/DamageCone.prototype",                       true,  0.0725f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/DiamondArmorCondition.prototype",            true,  0.05f),
            new("Powers/Player/EmmaFrost/Update/DiamondHeart.prototype",                     true,  0.05f),
            new("Powers/Player/EmmaFrost/Update/DiamondKnee.prototype",                      true,  0.05f),
            new("Powers/Player/EmmaFrost/Update/DiamondStrike.prototype",                    true,  0.0904f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/DiamondSweepKick.prototype",                 true,  0.0868f), // 2026-06-18
            new("Powers/Player/EmmaFrost/Update/DiamondWhirlwind.prototype",                 true,  0.0522f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/Drain.prototype",                            true,  0.0489f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/KneelBeforeMe.prototype",                    true,  0.0195f), // 2026-06-18
            new("Powers/Player/EmmaFrost/Update/MegaSlap.prototype",                         true,  0.0623f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/PsychicSpear.prototype",                     true,  0.0347f), // 2026-06-19
            new("Powers/Player/EmmaFrost/Update/RangedSplashShot.prototype",                 true,  0.05f),
            new("Powers/Player/TravelPower/EmmaFrostSprint.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/EmmaFrostStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                   false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                          false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                  false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                     false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                        false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                              false, 0.05f),
        };
    }
}
