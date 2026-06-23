using System.Linq;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Enemies are rendered as a playable avatar.
    /// Powers are harvested from the rendered avatar, but can be overridden by a power table.
    /// Per power damage scaling , with logging thru PowerPayload , to try and balance  
    /// </summary>
    public abstract class IncursionEnemyAvatar : IncursionEnemyController
    {
        protected IncursionEnemyAvatar(Game game) : base(game) { }

        /// <summary>The avatar this invader is rendered as .</summary>
        public abstract override PrototypeId RenderAvatarRef { get; }

        /// <summary>
        /// Stealable power exposed to Rogue. Derived from the rendered avatar's prototype
        /// </summary>
        public override PrototypeId StealablePowerInfoRef
        {
            get
            {
                var avatarProto = RenderAvatarRef.As<AvatarPrototype>();
                if (avatarProto != null && avatarProto.StealablePower != PrototypeId.Invalid)
                    return avatarProto.StealablePower;
                return PrototypeId.Invalid;
            }
        }

        // Per-power damage scales from PowerTable. Covers enabled and disabled entries
        private readonly Dictionary<PrototypeId, float> _tableScales = new();

        /// <summary>
        ///  explicit power table. When non-null, every enabled power is assigned
        ///  When null, powers are harvested from the rendered avatar.
        /// </summary>
        protected override IncursionPowerEntry[] PowerTable => null;

        protected override void OnSetup(Agent agent)
        {
            PopulatePowers(agent);
        }

        /// <summary>
        /// Harvests powers from the rendered avatar, assigns usable offensive ones to the agent.
        /// </summary>
        protected virtual void PopulatePowers(Agent agent)
        {
            // Table-driven mode: a generated controller declared an explicit power table.
            IncursionPowerEntry[] table = PowerTable;
            if (table != null)
            {
                PopulateFromTable(agent, table);
                return;
            }

            // Prefer the controller's declared avatar; fall back to the agent's render override.
            PrototypeId powerSourceRef = RenderAvatarRef;
            if (powerSourceRef == PrototypeId.Invalid || powerSourceRef.As<AvatarPrototype>() == null)
                powerSourceRef = agent.ClientPrototypeRefOverride;

            var avatarProto = powerSourceRef.As<AvatarPrototype>();
            if (avatarProto == null)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: no avatar power source resolved; no powers assigned.");
                return;
            }

            List<PowerProgressionEntryPrototype> entries = new();
            avatarProto.GetPowersUnlockedAtLevel(entries, -1, true);

            foreach (PowerProgressionEntryPrototype entry in entries)
            {
                PrototypeId powerRef = entry?.PowerAssignment?.Ability ?? PrototypeId.Invalid;
                if (powerRef == PrototypeId.Invalid) continue;
                if (Powers.Contains(powerRef)) continue;

                var powerProto = powerRef.As<PowerPrototype>();
                if (IsUsableOffensivePower(powerProto) == false) continue;

                Powers.Add(powerRef);

                // Pre-assign so the power is ready and in the collection from the start.
                if (agent.GetPower(powerRef) == null)
                {
                    PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                    agent.AssignPower(powerRef, indexProps);
                }
            }

            if (Powers.Count == 0)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: no usable offensive powers found for '{GameDatabase.GetPrototypeName(powerSourceRef)}'.");
            }
            else
            {
                string powerMsg = $"[IncursionEnemy] {GetType().Name} powers from '{GameDatabase.GetPrototypeName(powerSourceRef)}' ({Powers.Count}): " +
                                  string.Join(", ", Powers.Select(p => GameDatabase.GetPrototypeName(p)));
                if (IsIncursionLoggingEnabled)
                    Logger.Info(powerMsg);
                IncursionLogCollator.WriteLine(EntityId, powerMsg);
            }
        }

        /// <summary>
        /// Assigns enabled powers from <see cref="PowerTable"/> and records every entry's damage scale.
        /// </summary>
        private void PopulateFromTable(Agent agent, IncursionPowerEntry[] table)
        {
            foreach (IncursionPowerEntry entry in table)
            {
                if (entry.Power == PrototypeId.Invalid) continue;

                // Record scale for every entry regardless of enabled state.
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
        ///   2. Keyword tier (<see cref="IncursionPowerScaling"/>) for untabled secondary effects.
        ///   3. Enemy-wide <see cref="IncursionEnemyController.DamageScale"/>.
        /// </summary>
        protected override float GetDamageScaleForPower(PrototypeId powerRef)
        {
            if (_tableScales.TryGetValue(powerRef, out float scale))
                return scale;

            // Combo and multi-hit child effects fall back to their parent power's scale.
            if (_effectToParentPower.TryGetValue(powerRef, out PrototypeId parentRef))
                if (_tableScales.TryGetValue(parentRef, out scale))
                    return scale;

            if (IncursionPowerScaling.TryGetKeywordScale(powerRef, out float keywordScale))
                return keywordScale;

            return base.GetDamageScaleForPower(powerRef);
        }

        /// <summary> If Autopopulate powers then use the normal, activated, single-shot offensive powers; skip passives/toggles/travel/movement.</summary>
        protected static bool IsUsableOffensivePower(PowerPrototype proto)
        {
            if (proto == null) return false;
            if (proto is MovementPowerPrototype) return false;
            if (proto.PowerCategory != PowerCategoryType.NormalPower) return false;
            if (proto.Activation == PowerActivationType.Passive) return false;
            if (proto.IsToggled) return false;
            if (proto.IsTravelPower) return false;
            return true;
        }
    }
}
