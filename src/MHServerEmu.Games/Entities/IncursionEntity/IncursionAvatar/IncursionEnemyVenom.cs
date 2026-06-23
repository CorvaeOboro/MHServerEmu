using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Venom Invader
    /// Powers: 18 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyVenom : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Venom.prototype");

        public IncursionEnemyVenom(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Venom Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Venom/AntiVenom.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Venom/Classic.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Venom/Hydra.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Venom/SpaceKnight.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Venom/Toxin.prototype",       true),
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
            new("Powers/Player/TravelPower/VenomFlight.prototype",                 false, 0.05f),
            new("Powers/Player/Venom/BigPunch.prototype",                          true,  0.0246f), // 2026-06-18
            new("Powers/Player/Venom/BigWebShoot.prototype",                       true,  0.1335f), // 2026-06-20
            new("Powers/Player/Venom/ConeDrain.prototype",                         true,  0.1209f), // 2026-06-18
            new("Powers/Player/Venom/DoubleSlash.prototype",                       true,  0.1367f), // 2026-06-18
            new("Powers/Player/Venom/FuriousLunge.prototype",                      true,  0.1497f), // 2026-06-20
            new("Powers/Player/Venom/IchorSpike.prototype",                        true,  0.0453f), // 2026-06-20
            new("Powers/Player/Venom/MawFromAbove.prototype",                      true,  0.0285f), // 2026-06-20
            new("Powers/Player/Venom/MeleeBasic.prototype",                        true,  0.1455f), // 2026-06-18
            new("Powers/Player/Venom/PBAoEBlob.prototype",                         true,  0.0669f), // 2026-06-18
            new("Powers/Player/Venom/RangedBasic.prototype",                       true,  0.1406f), // 2026-06-20
            new("Powers/Player/Venom/SigFreakout.prototype",                       true,  0.0200f), // 2026-06-18
            new("Powers/Player/Venom/SwingingAssault.prototype",                   true,  0.1360f), // 2026-06-11
            new("Powers/Player/Venom/SymbioteDrain.prototype",                     true,  0.0391f), // 2026-06-18
            new("Powers/Player/Venom/SymbioteDrainHiddenPassive.prototype",        false, 0.0391f), // 2026-06-18
            new("Powers/Player/Venom/Talents/BigPunchBuff.prototype",              false, 0.0246f), // 2026-06-18
            new("Powers/Player/Venom/Talents/BuffAtLowHealth.prototype",           false, 0.05f),
            new("Powers/Player/Venom/Talents/DefenseBuff.prototype",               false, 0.05f),
            new("Powers/Player/Venom/Talents/DoubleSlashIchorSpearBuff.prototype", false, 0.1367f), // 2026-06-18
            new("Powers/Player/Venom/Talents/HealthCostIncrease.prototype",        false, 0.05f),
            new("Powers/Player/Venom/Talents/HealthRestoreBuff.prototype",         false, 0.05f),
            new("Powers/Player/Venom/Talents/IchorCostIncrease.prototype",         false, 0.05f),
            new("Powers/Player/Venom/Talents/IchorCostReduction.prototype",        false, 0.05f),
            new("Powers/Player/Venom/Talents/IchorSpikeBuff.prototype",            false, 0.0453f), // 2026-06-20
            new("Powers/Player/Venom/Talents/InfectBuff.prototype",                false, 0.05f),
            new("Powers/Player/Venom/Talents/SymbioteDrainBuff.prototype",         false, 0.0391f), // 2026-06-18
            new("Powers/Player/Venom/Talents/SymbioteSpawns.prototype",            false, 0.05f),
            new("Powers/Player/Venom/Talents/TentacleImpaleBuff.prototype",        false, 0.0614f), // 2026-06-20
            new("Powers/Player/Venom/Talents/WrithingTendrilsBuff.prototype",      false, 0.0440f), // 2026-06-18
            new("Powers/Player/Venom/Talents/YankBuff.prototype",                  false, 0.0771f), // 2026-06-18
            new("Powers/Player/Venom/TentacleImpale.prototype",                    true,  0.0614f), // 2026-06-20
            new("Powers/Player/Venom/Traits/DefenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Venom/Traits/MechanicTraitIchor.prototype",         false, 0.05f),
            new("Powers/Player/Venom/Traits/OffenseTrait.prototype",               false, 0.05f),
            new("Powers/Player/Venom/Ultimate.prototype",                          true,  0.0189f), // 2026-06-20
            new("Powers/Player/Venom/WebSplat.prototype",                          true,  0.0936f), // 2026-06-20
            new("Powers/Player/Venom/WrithingTendrils.prototype",                  true,  0.0440f), // 2026-06-18
            new("Powers/Player/Venom/Yank.prototype",                              true,  0.0771f), // 2026-06-18
            new("Powers/StolenPowers/StealablePowers/VenomStolenPower.prototype",  false, 0.05f),
            new("Powers/SynergyPowers/SynergyVenomLowHealthBuff.prototype",        false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",         false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",        false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",           false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",              false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                    false, 0.05f),
        };
    }
}
