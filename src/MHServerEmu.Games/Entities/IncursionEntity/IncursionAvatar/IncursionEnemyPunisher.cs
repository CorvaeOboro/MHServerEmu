using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Punisher Invader
    /// Powers: 19 / 46
    /// Damage scale per ability is listed below.
    /// </summary>
    public class IncursionEnemyPunisher : IncursionEnemyAvatar
    {
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Punisher.prototype");

        public IncursionEnemyPunisher(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override string InvaderDisplayName => "Punisher Invader";

        // Costume pool: one enabled entry is rolled at random per spawn.
        protected override IncursionCostumeEntry[] CostumeTable => _costumeTable;

        private static readonly IncursionCostumeEntry[] _costumeTable =
        {
            new("Entity/Items/Costumes/Prototypes/Punisher/Classic.prototype",       true),
            new("Entity/Items/Costumes/Prototypes/Punisher/DeadWinter.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Punisher/FrankenCastle.prototype", true),
            new("Entity/Items/Costumes/Prototypes/Punisher/MarvelNow.prototype",     true),
            new("Entity/Items/Costumes/Prototypes/Punisher/Modern.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Punisher/ModernVU.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Punisher/Noir.prototype",          true),
            new("Entity/Items/Costumes/Prototypes/Punisher/OmegaEffect.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Punisher/Original.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Punisher/RachelAlves.prototype",   true),
            new("Entity/Items/Costumes/Prototypes/Punisher/Skrull.prototype",        true),
            new("Entity/Items/Costumes/Prototypes/Punisher/TESTONLY.prototype",      true),
            new("Entity/Items/Costumes/Prototypes/Punisher/TV.prototype",            true),
            new("Entity/Items/Costumes/Prototypes/Punisher/TrenchCoat.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Punisher/WarJournal.prototype",    true),
            new("Entity/Items/Costumes/Prototypes/Punisher/WarTorn.prototype",       true),
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
            new("Powers/CharacterSelectOnly/HeroProfilePowerPunisher.prototype",     true,  0.05f),
            new("Powers/Player/Punisher/Rework/ArmorPiercing.prototype",             true,  0.0834f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/BackwardsTumble.prototype",           true,  0.05f),
            new("Powers/Player/Punisher/Rework/Bazooka.prototype",                   true,  0.0785f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/BulletSpray.prototype",               true,  0.0572f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/ChemicalBomb.prototype",              true,  0.0995f), // 2026-06-10
            new("Powers/Player/Punisher/Rework/Flamethrower.prototype",              true,  0.0649f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/Flashbang.prototype",                 true,  0.1007f), // 2026-06-03
            new("Powers/Player/Punisher/Rework/Magnum.prototype",                    true,  0.05f),
            new("Powers/Player/Punisher/Rework/PineappleGrenade.prototype",          true,  0.1113f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/Reload.prototype",                    true,  0.05f),
            new("Powers/Player/Punisher/Rework/Rpg.prototype",                       true,  0.05f),
            new("Powers/Player/Punisher/Rework/SawedOff.prototype",                  true,  0.1071f), // 2026-06-18
            new("Powers/Player/Punisher/Rework/Sidearms.prototype",                  true,  0.05f),
            new("Powers/Player/Punisher/Rework/ThreeRoundBurst.prototype",           true,  0.05f),
            new("Powers/Player/Punisher/Rework/Tumble.prototype",                    true,  0.05f),
            new("Powers/Player/Punisher/Rework/UltimateHiddenPassive.prototype",     false, 0.0126f), // 2026-06-18
            new("Powers/Player/Punisher/Talents/AutomaticShotgun.prototype",         false, 0.05f),
            new("Powers/Player/Punisher/Talents/CombatKnife.prototype",              false, 0.05f),
            new("Powers/Player/Punisher/Talents/FlamethrowerBuff.prototype",         false, 0.0649f), // 2026-06-18
            new("Powers/Player/Punisher/Talents/GrenadeLauncher.prototype",          false, 0.01f),
            new("Powers/Player/Punisher/Talents/HighCapacityMagazine.prototype",     false, 0.05f),
            new("Powers/Player/Punisher/Talents/HollowPointRounds.prototype",        false, 0.05f),
            new("Powers/Player/Punisher/Talents/InYourFace.prototype",               false, 0.05f),
            new("Powers/Player/Punisher/Talents/IncendiaryGrenades.prototype",       false, 0.01f),
            new("Powers/Player/Punisher/Talents/LighterMags.prototype",              false, 0.05f),
            new("Powers/Player/Punisher/Talents/Minigun.prototype",                  false, 0.05f),
            new("Powers/Player/Punisher/Talents/NuclearOption.prototype",            false, 0.05f),
            new("Powers/Player/Punisher/Talents/SMG.prototype",                      false, 0.05f),
            new("Powers/Player/Punisher/Talents/SniperRifleBuff.prototype",          false, 0.05f),
            new("Powers/Player/Punisher/Talents/TacticalShotgun.prototype",          false, 0.05f),
            new("Powers/Player/Punisher/Talents/TriBarrelRPG.prototype",             false, 0.05f),
            new("Powers/Player/Punisher/Traits/DefenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Punisher/Traits/MechanicTraitAmmo.prototype",         false, 0.05f),
            new("Powers/Player/Punisher/Traits/OffenseTrait.prototype",              false, 0.05f),
            new("Powers/Player/Punisher/Ultimate.prototype",                         true,  0.0126f), // 2026-06-18
            new("Powers/Player/TravelPower/PunisherSprint.prototype",                false, 0.05f),
            new("Powers/StolenPowers/StealablePowers/PunisherStolenPower.prototype", false, 0.05f),
            new("Powers/SynergyPowers/SynergyPunisherHealOnKill.prototype",          true,  0.05f),
            new("Powers/SynergyPowers/SynergyPunisherSpiritOnKill.prototype",        true,  0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",           false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                  false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",          false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",             false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                      false, 0.05f),
        };
    }
}
