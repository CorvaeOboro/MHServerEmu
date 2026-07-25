using System.Linq;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Enemy rendered as an existing game boss entity.
    /// Unlike <see cref="IncursionEnemyAvatar"/> (illusion proxy on a generic combat body)
    /// and <see cref="IncursionEnemyTeamup"/> (Team-Up render), a Boss invader spawns the
    /// actual boss prototype as the combat body — the boss renders and animates as itself.
    /// Powers are harvested from the boss entity's native power collection after spawn,
    /// or overridden by an explicit <see cref="IncursionPowerEntry"/>[] power table.
    /// The controller disables the boss's native AI and drives it through the same
    /// think-loop (target, chase, activate powers) used for all incursion enemies.
    /// </summary>
    public abstract class IncursionEnemyBoss : IncursionEnemyController
    {
        protected IncursionEnemyBoss(Game game) : base(game) { }

        public override string EnemyType => "Boss";

        /// <summary>The boss entity prototype spawned as the combat body.</summary>
        public abstract override PrototypeId RenderBossRef { get; }

        // Bosses render as themselves — seal the illusion-proxy refs to Invalid.
        public sealed override PrototypeId RenderAvatarRef => PrototypeId.Invalid;
        public sealed override PrototypeId RenderTeamupRef => PrototypeId.Invalid;

        // Boss nameplate should vanish immediately on death — no outro delay.
        protected override int NameplateProxyDestroyDelayMs => 0;

        // Suppress intro overhead dialog for boss invaders.
        protected override bool SayIntroDialog => false;

        // Bosses take normal damage by default (2.0 base * 1.0 = 2.0 = double damage, same as all incursion enemies).
        // Individual boss subclasses can override to tune survivability.
        protected override float DamageTakenMultiplier => 1.0f;

        // Per-power damage scales from PowerTable (if provided).
        private readonly Dictionary<PrototypeId, float> _tableScales = new();

        /// <summary>
        /// Power prototype refs to exclude from the auto-harvested power list.
        /// Override in subclasses to suppress specific boss powers (e.g. summon adds).
        /// </summary>
        protected virtual PrototypeId[] ExcludedPowers => null;

        /// <summary>
        /// Pool of henchmen NPCs to spawn alongside this boss. Default is null (no henchmen).
        /// Override in a specific boss .cs file to define the boss's crew.
        /// Each entry specifies an NPC prototype, a count range (min/max), and an optional rank override.
        /// </summary>
        protected virtual IncursionHenchmanEntry[] HenchmenPool => null;

        protected override void OnSetup(Agent agent)
        {
            PopulatePowers(agent);
            SpawnHenchmen(agent);
        }

        /// <summary>
        /// Spawns henchmen NPCs around the boss. For each entry in <see cref="HenchmenPool"/>,
        /// rolls a random count between Min and Max and spawns that many NPCs in a ring around
        /// the boss. Henchmen keep their native AI — they are not incursion-controlled.
        /// </summary>
        private void SpawnHenchmen(Agent agent)
        {
            IncursionHenchmanEntry[] pool = HenchmenPool;
            if (pool == null || pool.Length == 0) return;

            Region region = agent.Region;
            if (region == null) return;

            var manager = region.PopulationManager;
            if (manager == null) return;

            Vector3 bossPos = agent.RegionLocation.Position;
            int level = agent.CharacterLevel;
            int totalSpawned = 0;

            foreach (IncursionHenchmanEntry entry in pool)
            {
                if (entry.Entity == PrototypeId.Invalid) continue;
                if (entry.Max <= 0) continue;

                int count = Game.Random.Next(entry.Min, entry.Max + 1);
                if (count <= 0) continue;

                for (int i = 0; i < count; i++)
                {
                    // Spread henchmen in a ring around the boss.
                    float angle = (float)(Game.Random.NextDouble() * Math.PI * 2);
                    float dist = 150f + (float)(Game.Random.NextDouble() * 250f);
                    Vector3 offset = new(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
                    Vector3 spawnPos = bossPos + offset;

                    spawnPos = RegionLocation.ProjectToFloor(region, spawnPos);
                    if (spawnPos == Vector3.Zero) continue;

                    var group = manager.CreateSpawnGroup();
                    group.Transform = Transform3.BuildTransform(spawnPos, Orientation.Zero);

                    var spec = manager.CreateSpawnSpec(group);
                    spec.EntityRef = entry.Entity;
                    spec.Transform = Transform3.Identity();
                    spec.SnapToFloor = true;

                    spec.Properties[PropertyEnum.CharacterLevel] = level;
                    spec.Properties[PropertyEnum.CombatLevel] = level;
                    spec.Properties[PropertyEnum.VariationSeed] = Game.Random.Next(1, 10000);

                    // Apply rank override if specified.
                    if (entry.RankOverride != PrototypeId.Invalid)
                        spec.Properties[PropertyEnum.Rank] = entry.RankOverride;

                    // Make hostile to players so they attack on sight.
                    spec.Properties[PropertyEnum.AllianceOverride] =
                        GameDatabase.GetPrototypeRefByName("Entity/Alliances/Enemies.prototype");

                    spec.Spawn();

                    var henchman = spec.ActiveEntity;
                    if (henchman != null)
                        totalSpawned++;
                    else
                        manager.RemoveSpawnGroup(group.Id);
                }
            }

            if (totalSpawned > 0)
            {
                string msg = $"[IncursionEnemy] {GetType().Name} spawned {totalSpawned} henchman(s).";
                if (IsIncursionLoggingEnabled)
                    Logger.Info(msg);
                IncursionLogCollator.WriteLine(EntityId, msg);
            }
        }

        /// <summary>
        /// Harvests usable offensive powers from the boss entity's native power collection.
        /// Boss prototypes come with their combat powers pre-assigned via their behavior profile;
        /// we filter for activated, non-movement, non-passive powers and add them to our power list.
        /// If a <see cref="PowerTable"/> is provided, enabled entries are assigned and table scales recorded.
        /// </summary>
        protected virtual void PopulatePowers(Agent agent)
        {
            // Table-driven mode: assign enabled powers from the explicit table.
            IncursionPowerEntry[] table = PowerTable;
            if (table != null)
            {
                PopulateFromTable(agent, table);
                return;
            }

            // Auto-harvest: scan the boss entity's existing power collection for usable offensive powers.
            var powerCollection = agent.PowerCollection;
            if (powerCollection == null)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: boss has no power collection; no powers assigned.");
                return;
            }

            // Diagnostic: dump ALL powers in the collection (before filtering) so we can
            // identify which powers to exclude (e.g. sentinel summon powers).
            if (IsIncursionLoggingEnabled)
            {
                int totalCount = 0;
                var allPowers = new System.Text.StringBuilder();
                allPowers.AppendLine($"[IncursionEnemy:PowerDump] {GetType().Name} ALL powers in collection:");
                foreach (var kvp in powerCollection)
                {
                    totalCount++;
                    PrototypeId pref = kvp.Key;
                    var proto = pref.As<PowerPrototype>();
                    allPowers.AppendLine($"  {GameDatabase.GetPrototypeName(pref)}  cat={proto?.PowerCategory}  activation={proto?.Activation}  travel={proto?.IsTravelPower}  toggle={proto?.IsToggled}  movement={proto is MovementPowerPrototype}");
                }
                allPowers.AppendLine($"  total: {totalCount}");
                Logger.Info(allPowers.ToString());
                IncursionLogCollator.WriteLine(EntityId, allPowers.ToString());
            }

            foreach (var kvp in powerCollection)
            {
                PrototypeId powerRef = kvp.Key;
                if (powerRef == PrototypeId.Invalid) continue;
                if (Powers.Contains(powerRef)) continue;
                if (IsExcludedPower(powerRef)) continue;

                var powerProto = powerRef.As<PowerPrototype>();
                if (IsUsableOffensivePower(powerProto) == false) continue;

                Powers.Add(powerRef);
            }

            if (Powers.Count == 0)
            {
                // Fallback: try the boss prototype's BehaviorProfile for equipped powers.
                var agentProto = agent.AgentPrototype;
                if (agentProto?.BehaviorProfile?.EquippedPassivePowers != null)
                {
                    foreach (PrototypeId powerRef in agentProto.BehaviorProfile.EquippedPassivePowers)
                    {
                        if (powerRef == PrototypeId.Invalid) continue;
                        if (Powers.Contains(powerRef)) continue;
                        if (IsExcludedPower(powerRef)) continue;

                        var powerProto = powerRef.As<PowerPrototype>();
                        if (IsUsableOffensivePower(powerProto) == false) continue;

                        Powers.Add(powerRef);

                        if (agent.GetPower(powerRef) == null)
                        {
                            PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                            agent.AssignPower(powerRef, indexProps);
                        }
                    }
                }
            }

            if (Powers.Count == 0)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: no usable offensive powers found for boss '{agent.PrototypeName}'.");
            }
            else
            {
                string powerMsg = $"[IncursionEnemy] {GetType().Name} powers from boss '{agent.PrototypeName}' ({Powers.Count}): " +
                                  string.Join(", ", Powers.Select(p => GameDatabase.GetPrototypeName(p)));
                if (IsIncursionLoggingEnabled)
                    Logger.Info(powerMsg);
                IncursionLogCollator.WriteLine(EntityId, powerMsg);
            }
        }

        /// <summary>
        /// Assigns enabled powers from <see cref="PowerTable"/> and records every entry's damage scale.
        /// </summary>
        protected void PopulateFromTable(Agent agent, IncursionPowerEntry[] table)
        {
            foreach (IncursionPowerEntry entry in table)
            {
                if (entry.Power == PrototypeId.Invalid) continue;

                _tableScales[entry.Power] = entry.DamageScale;

                if (entry.Enabled == false) continue;
                if (Powers.Contains(entry.Power)) continue;

                Powers.Add(entry.Power);

                if (agent.GetPower(entry.Power) == null)
                {
                    PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                    agent.AssignPower(entry.Power, indexProps);
                }
            }

            if (Powers.Count == 0)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: power table has no enabled powers; nothing assigned.");
            }
            else
            {
                string tableMsg = $"[IncursionEnemy] {GetType().Name} table powers ({Powers.Count}/{table.Length} enabled): " +
                                  string.Join(", ", Powers.Select(p => GameDatabase.GetPrototypeName(p)));
                if (IsIncursionLoggingEnabled)
                    Logger.Info(tableMsg);
                IncursionLogCollator.WriteLine(EntityId, tableMsg);
            }
        }

        /// <summary>
        /// Per-power damage scale. Resolution order:
        ///   1. Explicit table scale.
        ///   2. Combo child effect falls back to parent power's table scale.
        ///   3. Enemy-wide <see cref="IncursionEnemyController.DamageScale"/>.
        /// </summary>
        protected override float GetDamageScaleForPower(PrototypeId powerRef)
        {
            if (_tableScales.TryGetValue(powerRef, out float scale))
                return scale;

            if (_effectToParentPower.TryGetValue(powerRef, out PrototypeId parentRef))
                if (_tableScales.TryGetValue(parentRef, out scale))
                    return scale;

            return base.GetDamageScaleForPower(powerRef);
        }

        /// <summary>
        /// Filters for usable offensive powers. Same criteria as avatar powers but also
        /// allows boss-specific power categories that bosses use.
        /// </summary>
        protected static bool IsUsableOffensivePower(PowerPrototype proto)
        {
            if (proto == null) return false;
            if (proto is MovementPowerPrototype) return false;
            if (proto.Activation == PowerActivationType.Passive) return false;
            if (proto.IsToggled) return false;
            if (proto.IsTravelPower) return false;
            // Boss powers can be NormalPower or BasicPower; skip None.
            if (proto.PowerCategory == PowerCategoryType.None) return false;
            return true;
        }

        /// <summary>
        /// Returns true if the given power ref is in the <see cref="ExcludedPowers"/> list.
        /// </summary>
        protected bool IsExcludedPower(PrototypeId powerRef)
        {
            var excluded = ExcludedPowers;
            if (excluded == null || excluded.Length == 0) return false;
            foreach (var excludedRef in excluded)
                if (excludedRef == powerRef) return true;
            return false;
        }
    }
}
