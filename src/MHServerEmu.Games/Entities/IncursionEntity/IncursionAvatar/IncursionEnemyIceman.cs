using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Iceman Invader
    /// Powers: 16 / 44
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyIceman : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Iceman.prototype");

        public IncursionEnemyIceman(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Iceman Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Iceman/AgeOfApocalypse.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Iceman/AllNewXmen.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Iceman/Classic.prototype",         true),
            new("Entity/Items/Costumes/Prototypes/Iceman/Original.prototype",        true),
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
            new("Powers/Player/Iceman/AbsoluteZero.prototype",                          true,  0.0091f), // 2026-06-18
            new("Powers/Player/Iceman/BasicBeam.prototype",                             true,  0.1236f), // 2026-06-10
            new("Powers/Player/Iceman/ChanneledBeam.prototype",                         true,  0.0695f), // 2026-06-10
            new("Powers/Player/Iceman/DeathFromAbove.prototype",                        true,  0.0344f), // 2026-06-18
            new("Powers/Player/Iceman/FocusBeam.prototype",                             true,  0.1022f), // 2026-06-09
            new("Powers/Player/Iceman/FrostWedgeNoMovement.prototype",                  false, 0.05f),
            new("Powers/Player/Iceman/FrozenOrb.prototype",                             true,  0.0750f), // 2026-06-10
            new("Powers/Player/Iceman/FuriousLunge.prototype",                          true,  0.1885f), // 2026-06-10
            new("Powers/Player/Iceman/HotspotBeam.prototype",                           true,  0.0998f), // 2026-06-10
            new("Powers/Player/Iceman/IceBlock.prototype",                              true,  0.05f),
            new("Powers/Player/Iceman/IceGolem.prototype",                              true,  0.0529f), // 2026-06-18
            new("Powers/Player/Iceman/Icewall.prototype",                               true,  0.05f),
            new("Powers/Player/Iceman/Icicle.prototype",                                true,  0.0676f), // 2026-06-09
            new("Powers/Player/Iceman/RapidFire.prototype",                             true,  0.4392f), // 2026-06-10
            new("Powers/Player/Iceman/ShowOff.prototype",                               true,  0.0376f), // 2026-06-10
            new("Powers/Player/Iceman/SpikePunch.prototype",                            true,  0.0604f), // 2026-06-10
            new("Powers/Player/Iceman/Talents/BeamSpec.prototype",                      false, 0.05f),
            new("Powers/Player/Iceman/Talents/ChillFreezePotency.prototype",            false, 0.05f),
            new("Powers/Player/Iceman/Talents/ChilledDoT.prototype",                    false, 0.05f),
            new("Powers/Player/Iceman/Talents/DFAArmorSpend.prototype",                 false, 0.05f),
            new("Powers/Player/Iceman/Talents/FocusBeamToFrostNova.prototype",          false, 0.1022f), // 2026-06-09
            new("Powers/Player/Iceman/Talents/FrozenOrbSummon.prototype",               false, 0.0750f), // 2026-06-10
            new("Powers/Player/Iceman/Talents/HailBallIcicleBall.prototype",            false, 0.0676f), // 2026-06-09
            new("Powers/Player/Iceman/Talents/HotspotBeamHealing.prototype",            false, 0.0998f), // 2026-06-10
            new("Powers/Player/Iceman/Talents/IceBlockCDDefensiveBuff.prototype",       false, 0.05f),
            new("Powers/Player/Iceman/Talents/IceGolemBuff.prototype",                  false, 0.0529f), // 2026-06-18
            new("Powers/Player/Iceman/Talents/IcemanClones.prototype",                  false, 0.05f),
            new("Powers/Player/Iceman/Talents/MeleeWeapons.prototype",                  false, 0.05f),
            new("Powers/Player/Iceman/Talents/ShatterBonus.prototype",                  false, 0.05f),
            new("Powers/Player/Iceman/Talents/ShowOffIceWallFreeHotspotBeam.prototype", false, 0.0998f), // 2026-06-10
            new("Powers/Player/Iceman/Talents/SignatureIceGolemBuff.prototype",         false, 0.0529f), // 2026-06-18
            new("Powers/Player/Iceman/Traits/DefenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/Iceman/Traits/MechanicTraitChillShatter.prototype",      false, 0.05f),
            new("Powers/Player/Iceman/Traits/OffenseTrait.prototype",                   false, 0.05f),
            new("Powers/Player/Iceman/UltimateHiddenPassive.prototype",                 false, 0.006f),
            new("Powers/Player/Iceman/UltimateStart.prototype",                         true,  0.0262f), // 2026-06-10
            new("Powers/Player/TravelPower/IcemanFlight.prototype",                     false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/IcemanStolenPower.prototype",      false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",              false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                     false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",             false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                   false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                         false, 0.05f),
        };
    }
}
