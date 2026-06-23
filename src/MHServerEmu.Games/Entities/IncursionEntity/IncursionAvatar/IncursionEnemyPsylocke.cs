using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Psylocke Invader
    /// Powers: 15 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyPsylocke : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Psylocke.prototype");

        public IncursionEnemyPsylocke(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Psylocke Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Psylocke/Classic.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Psylocke/HouseofM.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Psylocke/LadyMandarin.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Psylocke/Skrull.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Psylocke/XForce.prototype",       true),
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
            new("Powers/Player/Psylocke/AoEDoT.prototype",                                  true,  0.0821f), // 2026-06-20
            new("Powers/Player/Psylocke/Bow.prototype",                                     true,  0.05f),
            new("Powers/Player/Psylocke/Butterflynado.prototype",                           false, 0.05f),
            new("Powers/Player/Psylocke/ConeBlast.prototype",                               true,  0.0602f), // 2026-06-20
            new("Powers/Player/Psylocke/DashBackstab.prototype",                            true,  0.0891f), // 2026-06-18
            new("Powers/Player/Psylocke/DashStealth.prototype",                             true,  0.1613f), // 2026-06-18
            new("Powers/Player/Psylocke/Implosion.prototype",                               true,  0.1231f), // 2026-06-18
            new("Powers/Player/Psylocke/KatanaDoubleStrike.prototype",                      true,  0.0627f), // 2026-06-18
            new("Powers/Player/Psylocke/KatanaLeapSlashAoE.prototype",                      true,  0.0474f), // 2026-06-20
            new("Powers/Player/Psylocke/KatanaPBAoE.prototype",                             true,  0.0644f), // 2026-06-17
            new("Powers/Player/Psylocke/KickPunch.prototype",                               true,  0.1093f), // 2026-06-20
            new("Powers/Player/Psylocke/Lunge.prototype",                                   true,  0.1603f), // 2026-06-20
            new("Powers/Player/Psylocke/PassiveDecoys.prototype",                           false, 0.05f),
            new("Powers/Player/Psylocke/PsiBolt.prototype",                                 true,  0.1311f), // 2026-06-18
            new("Powers/Player/Psylocke/PsiKatanaCone.prototype",                           true,  0.1012f), // 2026-06-20
            new("Powers/Player/Psylocke/PsiKnifeTripleStrike.prototype",                    true,  0.0971f), // 2026-06-18
            new("Powers/Player/Psylocke/SeekerButterflies.prototype",                       true,  0.0213f), // 2026-06-20
            new("Powers/Player/Psylocke/StealthMechanicHiddenPassive.prototype",            false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent1MeleeBuff.prototype",                false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent1MentalBuff.prototype",               false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent1RangedBuff.prototype",               false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent2DefenseBuffGuardedAllies.prototype", false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent2SigButterflyCDR.prototype",          false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent2SigPowersCDR.prototype",             false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent3BetterMovement.prototype",           false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent3PsionicBarrierBuff.prototype",       false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent3PsychoBlast.prototype",              false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent4DancingKatana.prototype",            false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent4MaxProjections.prototype",           false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent4PsionicBow.prototype",               false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent5AoEDoTBuff.prototype",               false, 0.0821f), // 2026-06-20
            new("Powers/Player/Psylocke/Talents/Talent5AssassinateCDR.prototype",           false, 0.05f),
            new("Powers/Player/Psylocke/Talents/Talent5ProjectionsWeapons.prototype",       false, 0.05f),
            new("Powers/Player/Psylocke/Traits/DefenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/Psylocke/Traits/MechanicTraitPsiBarrier.prototype",          false, 0.05f),
            new("Powers/Player/Psylocke/Traits/OffenseTrait.prototype",                     false, 0.05f),
            new("Powers/Player/TravelPower/PsylockeSprint.prototype",                       false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/PsylockeStolenPower.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                         false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                 false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                    false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                       false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                             false, 0.05f),
        };
    }
}
