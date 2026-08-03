using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Malekith - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossMalekith : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/MalekithCh9.prototype");

        public IncursionEnemyBossMalekith(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Malekith Invader";

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            new("Powers/EnemyPowers/Boss/Malekith/MalekithSummonDarkElves.prototype",              true,  1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithOnDeathSummonProc.prototype",            false, 1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithChargeStart.prototype",                  false, 1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDarkBeam.prototype",                     true,  1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithChargeAttack.prototype",                 true,  1.0f),
            new("Powers/EnemyPowers/Boss/Elektra/ElektraShadowStrikeReappear.prototype",           false, 1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDeathFromAboveDrop.prototype",           false, 1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnHeal.prototype",                             false, 1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDarkShroudStart.prototype",              true,  1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithSummonVoidHotspot.prototype",            true,  1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDeathFromAboveMovement.prototype",       false, 1.0f),
            new("Powers/EnemyPowers/Boss/Malekith/MalekithDeathFromAboveStart.prototype",          false,  1.0f),
            new("Powers/EnemyPowers/Shared/LeashReturnNegStatusEffectImmune.prototype",            false, 1.0f),
        };
    }
}
