#region POWER AUGMENT
// =============================================================================
// MOD CALAMITY 
// =============================================================================
//   CALAMITY is a collection of custom encounters that are short, small 
//   and play like the games existing "terminals". 
//
//   AugmentedPowerController manages dynamic power augmentation for Calamity bosses.
//   It intercepts the boss's power collection to inject bonus powers, schedule timed
//   casts, and spawn visual proxy entities for powers that need a separate render body.
//   Supports pattern-based casting sequences and cooldown management per augmented power.
//
//  VERSION:: 20260713
// =============================================================================

using MHServerEmu.Core.Logging;
using MHServerEmu.Core.Memory;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.Populations;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    #region Structs

    /// <summary>
    /// Patterns available for Augmented power casting.
    /// </summary>
    public enum AugmentedPattern
    {
        /// <summary>4 points on the ground in a cross around the boss.</summary>
        Cross,
        /// <summary>N points evenly distributed in a circle around the boss.</summary>
        RadialBurst,
        /// <summary>Multiple rings at increasing radius with delays between rings.</summary>
        CascadingRadial,
        /// <summary>Single cast at a specific position (e.g. target or boss position).</summary>
        SingleAtPosition,
        /// <summary>N points in a line along the boss's facing direction, with delays between each cast.</summary>
        CascadingLine,
    }

    /// <summary>
    /// Defines an Augmented power: a special complex boss ability with a pattern, cooldown,
    /// and optional proxy casting for powers that don't match the boss's animation rig.
    /// </summary>
    public readonly struct AugmentedPowerEntry
    {
        public readonly PrototypeId PowerRef;
        public readonly AugmentedPattern Pattern;
        public readonly float DamageScale;
        public readonly int CooldownMs;
        public readonly bool UseProxy;

        // Pattern parameters
        public readonly float Radius;
        public readonly int PointCount;
        public readonly int Rings;
        public readonly float RadiusStep;
        public readonly int DelayMs;

        public AugmentedPowerEntry(string powerPath, AugmentedPattern pattern, float damageScale,
            int cooldownMs, bool useProxy,
            float radius = 300f, int pointCount = 6, int rings = 3, float radiusStep = 200f, int delayMs = 400)
        {
            PowerRef = GameDatabase.GetPrototypeRefByName(powerPath);
            if (PowerRef == PrototypeId.Invalid)
                System.Diagnostics.Trace.WriteLine($"[AugmentedPowerEntry] WARNING: Power path '{powerPath}' resolved to PrototypeId.Invalid - power does not exist.");
            Pattern = pattern;
            DamageScale = damageScale;
            CooldownMs = cooldownMs;
            UseProxy = useProxy;
            Radius = radius;
            PointCount = pointCount;
            Rings = rings;
            RadiusStep = radiusStep;
            DelayMs = delayMs;
        }
    }

    #endregion

    /// <summary>
    /// Manages complex "Augmented" power patterns for boss fights.
    /// Supports cross patterns, radial bursts, cascading radials, and single-position casts.
    /// Non-matching powers (different animation rig) are cast via invisible proxy agents
    /// to prevent T-posing on the boss's rendered avatar.
    /// All cast positions are projected to the floor so AOE effects appear on the ground.
    /// </summary>
    public class AugmentedPowerController
    {
        #region Constructor

        private static readonly Logger Logger = LogManager.CreateLogger();

        private readonly Game _game;
        private readonly Agent _bossAgent;
        private readonly IncursionEnemyController _bossController;
        private readonly Region _region;

        private readonly List<PendingCast> _pendingCasts = new();
        private readonly List<ProxyEntry> _activeProxies = new();

        // Proxy body for augmented power casting.
        // Uses SpidermanClone (an Agent, not Avatar) because Avatar prototypes require a player owner
        // and fail power assignment when spawned without one. The proxy is hidden anyway
        // (IsClientEntityHidden + Visible=false), so T-posing on non-matching powers is irrelevant.
        private const string ProxyBodyProtoName = "Entity/Characters/Mobs/SpiderClones/SpidermanCloneSuperiorBase.prototype";
        private const string HostileAllianceName = "Entity/Alliances/Enemies.prototype";

        private struct PendingCast
        {
            public PrototypeId PowerRef;
            public Vector3 Position;
            public TimeSpan ActivateAt;
            public Agent ProxyAgent;  // null = cast from boss
        }

        private struct ProxyEntry
        {
            public Agent Agent;
            public TimeSpan CleanupAt;
        }

        public AugmentedPowerController(Game game, Agent bossAgent, IncursionEnemyController bossController)
        {
            _game = game;
            _bossAgent = bossAgent;
            _bossController = bossController;
            _region = bossAgent.Region;
        }

        #endregion

        #region  Tick

        /// <summary>
        /// Process pending delayed casts and clean up expired proxies.
        /// Call from the boss's Think loop every tick.
        /// </summary>
        public void Update()
        {
            if (_disposed) return;
            TimeSpan now = _game.CurrentTime;

            for (int i = _pendingCasts.Count - 1; i >= 0; i--)
            {
                if (now >= _pendingCasts[i].ActivateAt)
                {
                    var cast = _pendingCasts[i];
                    _pendingCasts.RemoveAt(i);
                    ActivatePower(cast.ProxyAgent ?? _bossAgent, cast.PowerRef, cast.Position, skipCheck: true);
                }
            }

            for (int i = _activeProxies.Count - 1; i >= 0; i--)
            {
                if (now >= _activeProxies[i].CleanupAt)
                {
                    CleanupProxy(_activeProxies[i].Agent);
                    _activeProxies.RemoveAt(i);
                }
            }
        }

        #endregion

        // --- Pattern Casting ---

        #region Pattern 

        /// <summary>
        /// Cast a power at 4 points on the ground in a cross pattern around the boss.
        /// </summary>
        public int CastCrossPattern(PrototypeId powerRef, float radius, bool useProxy = false)
        {
            Vector3 bossPos = _bossAgent.RegionLocation.Position;
            // X-Y plane (horizontal). Z is up in this engine, so using Z would make the cross vertical.
            Vector3[] offsets =
            {
                new(radius, 0f, 0f),
                new(-radius, 0f, 0f),
                new(0f, radius, 0f),
                new(0f, -radius, 0f),
            };

            Agent proxy = useProxy ? SpawnProxy(bossPos) : null;
            Agent caster = proxy ?? _bossAgent;
            int activated = 0;

            foreach (var offset in offsets)
            {
                Vector3 castPos = ProjectToGround(bossPos + offset);
                if (castPos == Vector3.Zero) continue;
                if (ActivatePower(caster, powerRef, castPos, skipCheck: activated > 0))
                    activated++;
            }

            if (proxy != null)
                ScheduleProxyCleanup(proxy, 5000);

            return activated;
        }

        /// <summary>
        /// Cast a power at N points evenly distributed in a circle on the ground around the boss.
        /// </summary>
        public int CastRadialBurst(PrototypeId powerRef, int pointCount, float radius, bool useProxy = false)
        {
            Vector3 bossPos = _bossAgent.RegionLocation.Position;
            Agent proxy = useProxy ? SpawnProxy(bossPos) : null;
            Agent caster = proxy ?? _bossAgent;
            int activated = 0;

            for (int i = 0; i < pointCount; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / pointCount);
                // X-Y plane (horizontal). Z is up.
                Vector3 offset = new(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius, 0f);
                Vector3 castPos = ProjectToGround(bossPos + offset);
                if (castPos == Vector3.Zero) continue;
                if (ActivatePower(caster, powerRef, castPos, skipCheck: activated > 0))
                    activated++;
            }

            if (proxy != null)
                ScheduleProxyCleanup(proxy, 5000);

            return activated;
        }

        /// <summary>
        /// Cast expanding rings of AOE at increasing radius with delays between rings.
        /// Creates a cascading wave effect. Points in alternating rings are offset
        /// so they interleave visually.
        /// </summary>
        public void CastCascadingRadial(PrototypeId powerRef, int rings, int pointsPerRing,
            float startRadius, float radiusStep, int delayMs, bool useProxy = false)
        {
            Vector3 bossPos = _bossAgent.RegionLocation.Position;
            Agent proxy = useProxy ? SpawnProxy(bossPos) : null;
            Agent caster = proxy ?? _bossAgent;
            TimeSpan now = _game.CurrentTime;

            for (int ring = 0; ring < rings; ring++)
            {
                float radius = startRadius + (ring * radiusStep);
                TimeSpan activateAt = now + TimeSpan.FromMilliseconds(ring * delayMs);
                float angleOffset = (ring % 2 == 0) ? 0f : (float)(Math.PI / pointsPerRing);

                for (int i = 0; i < pointsPerRing; i++)
                {
                    float angle = angleOffset + (float)(i * 2.0 * Math.PI / pointsPerRing);
                    // X-Y plane (horizontal). Z is up.
                    Vector3 offset = new(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius, 0f);
                    Vector3 castPos = ProjectToGround(bossPos + offset);
                    if (castPos == Vector3.Zero) continue;

                    _pendingCasts.Add(new PendingCast
                    {
                        PowerRef = powerRef,
                        Position = castPos,
                        ActivateAt = activateAt,
                        ProxyAgent = proxy,
                    });
                }
            }

            if (proxy != null)
                ScheduleProxyCleanup(proxy, (rings * delayMs) + 5000);
        }

        /// <summary>
        /// Cast a power at N points in a line along the boss's facing direction.
        /// Starts at startDistance from the boss, each subsequent point is stepDistance further,
        /// with delayMs between each cast. Creates a sequential line of AOE effects.
        /// </summary>
        public void CastCascadingLine(PrototypeId powerRef, int count, float startDistance, float stepDistance,
            int delayMs, bool useProxy = false)
        {
            Vector3 bossPos = _bossAgent.RegionLocation.Position;
            float yaw = _bossAgent.RegionLocation.Orientation.Yaw;
            Vector3 forward = new(MathF.Cos(yaw), MathF.Sin(yaw), 0f);

            Agent proxy = useProxy ? SpawnProxy(bossPos) : null;
            Agent caster = proxy ?? _bossAgent;
            TimeSpan now = _game.CurrentTime;

            for (int i = 0; i < count; i++)
            {
                float distance = startDistance + (i * stepDistance);
                Vector3 castPos = ProjectToGround(bossPos + forward * distance);
                if (castPos == Vector3.Zero) continue;

                TimeSpan activateAt = now + TimeSpan.FromMilliseconds(i * delayMs);
                _pendingCasts.Add(new PendingCast
                {
                    PowerRef = powerRef,
                    Position = castPos,
                    ActivateAt = activateAt,
                    ProxyAgent = proxy,
                });
            }

            if (proxy != null)
                ScheduleProxyCleanup(proxy, (count * delayMs) + 5000);
        }

        /// <summary>
        /// Cast a single power at a specific position (e.g. target position or boss position).
        /// Used for ultimates and single-shot special abilities.
        /// </summary>
        public bool CastSingleAtPosition(PrototypeId powerRef, Vector3 position, bool useProxy = false, bool skipCheck = false)
        {
            Vector3 castPos = ProjectToGround(position);
            if (castPos == Vector3.Zero) return false;

            Agent proxy = useProxy ? SpawnProxy(_bossAgent.RegionLocation.Position) : null;
            Agent caster = proxy ?? _bossAgent;

            bool activated = ActivatePower(caster, powerRef, castPos, skipCheck: skipCheck);

            if (proxy != null)
                ScheduleProxyCleanup(proxy, 5000);

            return activated;
        }

        #endregion

        // --- Core Activation ---

        #region Core 

        private bool ActivatePower(Agent caster, PrototypeId powerRef, Vector3 targetPos, bool skipCheck)
        {
            if (powerRef == PrototypeId.Invalid)
            {
                Logger.Warn($"[AugmentedPowerController] ActivatePower called with PrototypeId.Invalid - power path does not exist.");
                return false;
            }

            Power power = caster.GetPower(powerRef);
            if (power == null)
            {
                PowerIndexProperties indexProps = new(0, caster.CharacterLevel, caster.CombatLevel);
                if (caster.AssignPower(powerRef, indexProps) == null) return false;
                power = caster.GetPower(powerRef);
                if (power == null) return false;
            }

            if (skipCheck == false &&
                caster.CanActivatePower(power, Entity.InvalidId, targetPos) != PowerUseResult.Success)
                return false;

            PowerActivationSettings settings = new(Entity.InvalidId, targetPos, caster.RegionLocation.Position);
            settings.Flags |= PowerActivationSettingsFlags.NotifyOwner;
            return caster.ActivatePower(powerRef, ref settings) == PowerUseResult.Success;
        }

        #endregion

        // --- Proxy Management ---

        #region Proxy 

        private Agent SpawnProxy(Vector3 position)
        {
            var manager = _region.PopulationManager;
            if (manager == null) return null;

            var group = manager.CreateSpawnGroup();
            group.Transform = Transform3.BuildTransform(position, Orientation.Zero);

            var spec = manager.CreateSpawnSpec(group);
            spec.EntityRef = GameDatabase.GetPrototypeRefByName(ProxyBodyProtoName);
            spec.Transform = Transform3.Identity();
            spec.SnapToFloor = true;

            // Hide the proxy using IsClientEntityHidden - the same mechanism used by
            // the nameplate proxy and PlayableExpandedController. This tells the client
            // to create the pawn but NOT add it to the visible render scene.
            spec.OptionFlagsOverride = EntitySettingsOptionFlags.IsClientEntityHidden;
            spec.Properties[PropertyEnum.Visible] = false;
            spec.BoundsScaleOverride = 0.001f;

            spec.Properties[PropertyEnum.Untargetable] = true;
            spec.Properties[PropertyEnum.NoEntityCollide] = true;
            spec.Properties[PropertyEnum.CharacterLevel] = _bossAgent.CharacterLevel;
            spec.Properties[PropertyEnum.CombatLevel] = _bossAgent.CombatLevel;

            // Hostile alliance so proxy-cast powers damage players.
            spec.Properties[PropertyEnum.AllianceOverride] =
                GameDatabase.GetPrototypeRefByName(HostileAllianceName);

            spec.Spawn();

            var proxy = spec.ActiveEntity as Agent;
            if (proxy == null)
            {
                manager.RemoveSpawnGroup(group.Id);
                return null;
            }

            // Disable native AI so the proxy doesn't think or move.
            proxy.AIController?.SetIsEnabled(false);
            proxy.SetDormant(true);
            proxy.SetSimulated(false);

            // Strip native powers to prevent intro voice lines and unwanted animations.
            // Augmented powers are assigned dynamically in ActivatePower().
            // Check ContainsPower before each UnassignPower because unassigning a combo
            // power can condemn related powers, removing them from the collection.
            if (proxy.PowerCollection != null)
            {
                using var powersHandle = ListPool<PrototypeId>.Instance.Get(out List<PrototypeId> powerRefs);
                foreach (var kvp in proxy.PowerCollection)
                    powerRefs.Add(kvp.Value.PowerPrototypeRef);
                foreach (var powerRef in powerRefs)
                {
                    if (proxy.PowerCollection.ContainsPower(powerRef))
                        proxy.UnassignPower(powerRef);
                }
            }

            // Register with IncursionManager so damage is scaled via the BloodLord's controller.
            _game.IncursionManager?.RegisterProxyEntity(proxy.Id, _bossController);

            return proxy;
        }

        private void ScheduleProxyCleanup(Agent proxy, int delayMs)
        {
            _activeProxies.Add(new ProxyEntry
            {
                Agent = proxy,
                CleanupAt = _game.CurrentTime + TimeSpan.FromMilliseconds(delayMs),
            });
        }

        private void CleanupProxy(Agent proxy)
        {
            if (proxy == null) return;
            _game.IncursionManager?.UnregisterProxyEntity(proxy.Id);
            proxy.Kill(null, KillFlags.NoLoot | KillFlags.NoExp | KillFlags.NoDeadEvent | KillFlags.DestroyImmediate);
        }

        #endregion

        // --- Helpers ---

        #region Cleanup

        private Vector3 ProjectToGround(Vector3 pos)
        {
            if (_region == null) return pos;
            return RegionLocation.ProjectToFloor(_region, pos);
        }

        private bool _disposed;

        public void CleanupAll()
        {
            if (_disposed) return;
            _disposed = true;
            _pendingCasts.Clear();

            foreach (var pe in _activeProxies)
                CleanupProxy(pe.Agent);
            _activeProxies.Clear();
        }

        #endregion

        #endregion
    }
}
