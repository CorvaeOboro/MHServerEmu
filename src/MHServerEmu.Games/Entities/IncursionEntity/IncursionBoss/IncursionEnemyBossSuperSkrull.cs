using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// SuperSkrull - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossSuperSkrull : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/KlrtSuperSkrullCH10.prototype");

        public IncursionEnemyBossSuperSkrull(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Super Skrull Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 200f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;
        protected override float DamageTakenMultiplier => 3.0f; // really tanky , we reduce for incursion

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/SuperSkrull/BasicMelee2.prototype",                          true,  1.0528f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SuperSkrull/BasicRangedStealthMode.prototype",               false,  0.9616f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SuperSkrull/FlamingShockwaveStealthMode.prototype",          false,  1.5223f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SuperSkrull/RapidPunch.prototype",                           true,  1.4513f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSmashStart.prototype",                      true,  1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/Whirlwind.prototype",                            false,  1.0732f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SuperSkrull/YankPunch.prototype",                            true,  1.2429f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/SkrullBosses/SuperSkrullBeacon.prototype",                   false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/BasicMeleeExtraHit.prototype",                   false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullFireRockArmsVisual.prototype",        false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullFireRockArmsVisualShort.prototype",   false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullHideMesh.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullRockArmRightOnly.prototype",          false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullRockArmsVisualBase.prototype",        false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSkrullShowMesh.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSmashComboFireball.prototype",              false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSmashDrop.prototype",                       false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/SuperSmashEnd.prototype",                        false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/TeleportSelf.prototype",                         false, 1.0f),
            new("Powers/EnemyPowers/Boss/SuperSkrull/YankPunch2ndHit.prototype",                      false, 1.2429f), // 2026-08-01
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                                false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",               false, 1.0f),
        };
    }
}
