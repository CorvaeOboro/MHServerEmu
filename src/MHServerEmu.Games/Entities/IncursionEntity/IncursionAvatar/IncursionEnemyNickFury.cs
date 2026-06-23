using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// NickFury Invader
    /// Powers: 16 / 42
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyNickFury : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/NickFury.prototype");

        public IncursionEnemyNickFury(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "NickFury Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/NickFury/SHIELD.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/NickFury/Ultimate.prototype", true),
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
            new("Powers/Player/NickFury/BasicPistol.prototype",                                   true,  0.3121f), // 2026-06-17
            new("Powers/Player/NickFury/BulletSpray.prototype",                                   true,  0.0338f), // 2026-06-17
            new("Powers/Player/NickFury/ChanneledBeam.prototype",                                 true,  0.0106f), // 2026-06-17
            new("Powers/Player/NickFury/CommandingShout.prototype",                               true,  0.05f),
            new("Powers/Player/NickFury/DangerClose.prototype",                                   true,  0.0417f), // 2026-06-17
            new("Powers/Player/NickFury/DriveByAnimStart.prototype",                              true,  0.0443f), // 2026-06-17
            new("Powers/Player/NickFury/Execute.prototype",                                       true,  0.0203f), // 2026-06-20
            new("Powers/Player/NickFury/EyesEverywhere.prototype",                                true,  0.0120f), // 2026-06-13
            new("Powers/Player/NickFury/HeadsDownRanged.prototype",                               true,  0.0643f), // 2026-06-17
            new("Powers/Player/NickFury/Microdrones.prototype",                                   true,  0.0681f), // 2026-06-17
            new("Powers/Player/NickFury/MolecularGrenade.prototype",                              true,  0.0898f), // 2026-06-20
            new("Powers/Player/NickFury/RapidFire.prototype",                                     true,  0.1554f), // 2026-06-17
            new("Powers/Player/NickFury/Reload.prototype",                                        true,  0.05f),
            new("Powers/Player/NickFury/RocketLauncher.prototype",                                true,  0.0529f), // 2026-06-20
            new("Powers/Player/NickFury/SniperShot.prototype",                                    true,  0.0672f), // 2026-06-20
            new("Powers/Player/NickFury/Talents/Talent1CommandingShoutRemap.prototype",           false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent1FireteamRifles.prototype",                 false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent1LifeModelDecoy.prototype",                 false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent2FireteamShotgun.prototype",                false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent2RedwingRemap.prototype",                   false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent2TumbleCharges.prototype",                  false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent3Bandolier.prototype",                      false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent3FireteamMedic.prototype",                  false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent3RocketLauncherRemap.prototype",            false, 0.0529f), // 2026-06-20
            new("Powers/Player/NickFury/Talents/Talent4ExecuteCooldownReset.prototype",           false, 0.0203f), // 2026-06-20
            new("Powers/Player/NickFury/Talents/Talent4FireteamMinigun.prototype",                false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent4HeadsDownRangedRemap.prototype",           false, 0.0643f), // 2026-06-17
            new("Powers/Player/NickFury/Talents/Talent5SigCooldownResets.prototype",              false, 0.05f),
            new("Powers/Player/NickFury/Talents/Talent5SignatureDangerCloseExtraShots.prototype", false, 0.02f),
            new("Powers/Player/NickFury/Talents/Talent5SignatureFireteamSteroid.prototype",       false, 0.02f),
            new("Powers/Player/NickFury/Traits/DefenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/NickFury/Traits/MechanicTraitAmmo.prototype",                      false, 0.05f),
            new("Powers/Player/NickFury/Traits/OffenseTrait.prototype",                           false, 0.05f),
            new("Powers/Player/NickFury/Tumble.prototype",                                        true,  0.05f),
            new("Powers/Player/TravelPower/NickFuryRide.prototype",                               false, 0.02f),
            new("Powers/StolenPowers/StealablePowers/NickFuryStolenPower.prototype",              false, 0.02f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",                        false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                               false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",                       false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",                          false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                             false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                                   false, 0.05f),
        };
    }
}
