using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Cyclops Invader
    /// Powers: 17 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyCyclops : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Cyclops.prototype");

        public IncursionEnemyCyclops(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Cyclops Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Cyclops/AgeOfApocalypse.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/AllNewXmen.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/Astonishing.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/ClassicXMen.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/MarvelNOW.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/MarvelNowVU.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/Noir.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/Original.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/PhoenixForce.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/Skrull.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/XFactor.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/XMen90s.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Cyclops/XMen90sVU.prototype",       true),
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
            new("Powers/Player/Cyclops/CallAngelMovement.prototype",                      false, 0.05f),
            new("Powers/Player/Cyclops/FocusBeamNew.prototype",                           true,  0.0802f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/AoEBeam.prototype",                         true,  0.1096f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/BasicBeam.prototype",                       true,  0.1213f), // 2026-06-17
            new("Powers/Player/Cyclops/Rework/BasicBouncingBeam.prototype",               true,  0.1291f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/CallBeast.prototype",                       true,  0.0571f), // 2026-06-17
            new("Powers/Player/Cyclops/Rework/CallIceman.prototype",                      true,  0.05f),
            new("Powers/Player/Cyclops/Rework/CallJean.prototype",                        true,  0.1221f), // 2026-06-17
            new("Powers/Player/Cyclops/Rework/ChanneledBeam.prototype",                   true,  0.0429f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/ChargeCone.prototype",                      true,  0.0422f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/ConeBeam.prototype",                        true,  0.0224f), // 2026-06-16
            new("Powers/Player/Cyclops/Rework/DisengagingShot.prototype",                 true,  0.05f),
            new("Powers/Player/Cyclops/Rework/PrismBeam.prototype",                       true,  0.0363f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/SignatureBeam.prototype",                   true,  0.0175f), // 2026-06-18
            new("Powers/Player/Cyclops/Rework/TacticalAnalysis.prototype",                true,  0.05f),
            new("Powers/Player/Cyclops/Rework/Tumble.prototype",                          true,  0.05f),
            new("Powers/Player/Cyclops/Rework/UltimateHiddenPassiveSigSynergy.prototype", false, 0.0286f), // 2026-06-18
            new("Powers/Player/Cyclops/Talents/BeamBleedTalent.prototype",                false, 0.05f),
            new("Powers/Player/Cyclops/Talents/BeamRefractionTalent.prototype",           false, 0.05f),
            new("Powers/Player/Cyclops/Talents/BeamToPunchTalent.prototype",              false, 0.05f),
            new("Powers/Player/Cyclops/Talents/CallinProcTalent.prototype",               false, 0.025f),
            new("Powers/Player/Cyclops/Talents/ChanneledBeamUpgradeTalent.prototype",     false, 0.0429f), // 2026-06-18
            new("Powers/Player/Cyclops/Talents/ChargeConeChargeTalent.prototype",         false, 0.0422f), // 2026-06-18
            new("Powers/Player/Cyclops/Talents/EmmaFrostCallInTalent.prototype",          false, 0.05f),
            new("Powers/Player/Cyclops/Talents/FocusBeamThirdStack.prototype",            false, 0.05f),
            new("Powers/Player/Cyclops/Talents/MagikCallinTalent.prototype",              false, 0.05f),
            new("Powers/Player/Cyclops/Talents/MagnetoCallinTalent.prototype",            false, 0.05f),
            new("Powers/Player/Cyclops/Talents/SigChannelTimeTalent.prototype",           false, 0.02f),
            new("Powers/Player/Cyclops/Talents/SigCooldownTimeTalent.prototype",          false, 0.02f),
            new("Powers/Player/Cyclops/Talents/SigMaximumOpticsTalent.prototype",         false, 0.02f),
            new("Powers/Player/Cyclops/Talents/TeamSteroidCDResetTalent.prototype",       false, 0.05f),
            new("Powers/Player/Cyclops/Talents/TeamSteroidGroupBuffsTalent.prototype",    false, 0.05f),
            new("Powers/Player/Cyclops/TeamSteroid.prototype",                            true,  0.05f),
            new("Powers/Player/Cyclops/Traits/DefenseTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Cyclops/Traits/OffenseTrait.prototype",                    false, 0.05f),
            new("Powers/Player/Cyclops/Ultimate.prototype",                               true,  0.0286f), // 2026-06-18
            new("Powers/Player/TravelPower/CyclopsRide.prototype",                        false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/CyclopsStolenPower.prototype",       false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                       false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",               false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                  false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                     false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                           false, 0.05f),
        };
    }
}
