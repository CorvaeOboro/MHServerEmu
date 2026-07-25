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
    /// Incursion Enemies rendered as a Team-Up character (an <see cref="AgentTeamUpPrototype"/>).
    /// Powers are harvested from the Team-Up's power progression, but can be overridden by a power table.
    /// This mirrors <see cref="IncursionEnemyAvatar"/> but sources powers and rendering from
    /// Team-Up prototypes instead of playable Avatar prototypes.
    /// </summary>
    public abstract class IncursionEnemyTeamup : IncursionEnemyAvatar
    {
        protected IncursionEnemyTeamup(Game game) : base(game) { }

        public override string EnemyType => "TeamUp";

        public override int NameplatePrestigeLevel => 5;

        // TeamUp nameplate should vanish at half the invisible time (1500ms vs 3000ms).
        protected override int NameplateProxyDestroyDelayMs => 1500;

        /// <summary>The Team-Up prototype this invader is rendered as.</summary>
        public abstract override PrototypeId RenderTeamupRef { get; }

        /// <summary>
        /// Not an avatar render; return Invalid so <see cref="IncursionManager.ApplyRenderSkin"/>
        /// falls through to the Team-Up path.
        /// </summary>
        public sealed override PrototypeId RenderAvatarRef => PrototypeId.Invalid;

        /// <summary>
        /// Stealable power from the Team-Up prototype (AgentPrototype.StealablePower).
        /// </summary>
        public override PrototypeId StealablePowerInfoRef
        {
            get
            {
                var teamUpProto = RenderTeamupRef.As<AgentTeamUpPrototype>();
                if (teamUpProto != null && teamUpProto.StealablePower != PrototypeId.Invalid)
                    return teamUpProto.StealablePower;
                return PrototypeId.Invalid;
            }
        }

        /// <summary>
        /// Harvests powers from the rendered Team-Up's power progression, assigns usable
        /// offensive ones to the agent. Falls back to the agent's render override when
        /// <see cref="RenderTeamupRef"/> is not resolvable at build time.
        /// </summary>
        protected override void PopulatePowers(Agent agent)
        {
            // Table-driven mode: a generated controller declared an explicit power table.
            IncursionPowerEntry[] table = PowerTable;
            if (table != null)
            {
                PopulateFromTable(agent, table);
                return;
            }

            // Prefer the controller's declared Team-Up; fall back to the agent's render override.
            PrototypeId powerSourceRef = RenderTeamupRef;
            if (powerSourceRef == PrototypeId.Invalid || powerSourceRef.As<AgentTeamUpPrototype>() == null)
                powerSourceRef = agent.ClientPrototypeRefOverride;

            var teamUpProto = powerSourceRef.As<AgentTeamUpPrototype>();
            if (teamUpProto?.PowerProgression == null)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: no Team-Up power source resolved; no powers assigned.");
                return;
            }

            // Collect active (non-passive, non-away/summoned) powers from the Team-Up progression.
            List<(PrototypeId PowerRef, int Level)> entries = new();
            foreach (TeamUpPowerProgressionEntryPrototype entry in teamUpProto.PowerProgression)
            {
                if (entry.Power == PrototypeId.Invalid)
                    continue;

                // Away/summoned passives belong to the real Team-Up pet pipeline, not to us.
                if (entry.IsPassiveOnAvatarWhileAway || entry.IsPassiveOnAvatarWhileSummoned)
                    continue;

                var powerProto = entry.Power.As<PowerPrototype>();
                if (IsUsableOffensivePower(powerProto) == false)
                    continue;

                entries.Add((entry.Power, entry.GetRequiredLevel()));
            }

            entries.Sort((a, b) => a.Level.CompareTo(b.Level));

            foreach (var (powerRef, _) in entries)
            {
                if (Powers.Contains(powerRef)) continue;

                Powers.Add(powerRef);

                if (agent.GetPower(powerRef) == null)
                {
                    PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                    agent.AssignPower(powerRef, indexProps);
                }
            }

            if (Powers.Count == 0)
            {
                Logger.Warn($"[IncursionEnemy] {GetType().Name}: no usable offensive powers found for Team-Up '{GameDatabase.GetPrototypeName(powerSourceRef)}'.");
            }
            else
            {
                string powerMsg = $"[IncursionEnemy] {GetType().Name} powers from Team-Up '{GameDatabase.GetPrototypeName(powerSourceRef)}' ({Powers.Count}): " +
                                  string.Join(", ", Powers.Select(p => GameDatabase.GetPrototypeName(p)));
                if (IsIncursionLoggingEnabled)
                    Logger.Info(powerMsg);
                IncursionLogCollator.WriteLine(EntityId, powerMsg);
            }
        }
    }
}
