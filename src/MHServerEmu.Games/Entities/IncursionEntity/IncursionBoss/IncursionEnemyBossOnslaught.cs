using System.Linq;
using System.Reflection;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Powers.Conditions;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion Boss Invader
    /// Onslaught - spawned as the actual boss entity (no render override).
    /// The boss renders and animates as itself; powers are harvested from its
    /// native power collection after spawn, or overridden by a power table.
    /// Controller disables native AI and drives behavior through the think loop.
    /// </summary>
    public class IncursionEnemyBossOnslaught : IncursionEnemyBoss
    {
        private static readonly PrototypeId BossRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Bosses/OnslaughtRaid/Onslaught.prototype");

        // Sentinel summon powers to exclude from the auto-harvested power list.
        // Resolved lazily from the prototype database by name pattern.
        private static readonly PrototypeId[] _excludedPowers = ResolveExcludedPowers();

        // Powers with distinct SizeDecrease UnrealClass visual effects.
        // The client deduplicates by UnrealClass, so we need different UnrealClass values.
        // We bypass power activation checks by creating conditions directly via InitializeFromPower.
        private static readonly PrototypeId[] SizeDecreasePowers = new PrototypeId[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/EnemyPowers/AffixPowers/Mob/DiminishedAffixCondition.prototype"),       // SizeDecrease25
            GameDatabase.GetPrototypeRefByName("Mods/Omega/Powers/Molecular46ShrinkPassivePower.prototype"),                    // SizeDecrease10
            GameDatabase.GetPrototypeRefByName("Mods/Omega/Powers/Molecular07ShrinkProcEffect.prototype"),                      // SizeDecrease05
            GameDatabase.GetPrototypeRefByName("Powers/ItemPowers/Test/FortuneCard2FastAndSmall.prototype"),                    // FortuneCardMark2SizeDecrease
        };

        private static PrototypeId[] ResolveExcludedPowers()
        {
            // Exclude the CallSentinel summon power and its child effect powers.
            // Paths confirmed from incursion log: Powers/EnemyPowers/Boss/OnslaughtRaid/CallSentinel.prototype
            var excluded = new System.Collections.Generic.List<PrototypeId>();
            foreach (var name in new string[]
            {
                "Powers/EnemyPowers/Boss/OnslaughtRaid/CallSentinel.prototype",
                "Powers/EnemyPowers/Boss/OnslaughtRaid/StarktechSentinelLaserbeamShort.prototype",
                "Powers/EnemyPowers/Boss/OnslaughtRaid/StarktechSentinelChargedLaserDoT.prototype",
                "Powers/EnemyPowers/Boss/OnslaughtRaid/StarktechSentinelLaserHotspotEffect.prototype",
                "Powers/EnemyPowers/Boss/OnslaughtRaid/StarktechSentinelGroundStomp.prototype",
            })
            {
                PrototypeId refId = GameDatabase.GetPrototypeRefByName(name);
                if (refId != PrototypeId.Invalid)
                    excluded.Add(refId);
            }
            return excluded.ToArray();
        }

        public IncursionEnemyBossOnslaught(Game game) : base(game) { }

        public override PrototypeId RenderBossRef => BossRef;
        public override string InvaderDisplayName => "Onslaught Invader";

        // 8x smaller than the default 1.5x scale → 1.5 / 8 ≈ 0.1875
        // NOTE: This only affects server-side collision bounds (BoundsScaleOverride).
        // Visual shrink is achieved via the SizeDecrease condition power in OnSetup.
        public override float VisualScaleOverride => 0.1875f;

        // 3x movement speed for a fast, aggressive mini-boss.
        protected override float MovementSpeedMult => 3.0f;

        // Exclude sentinel summon powers so Onslaught can't call in Starktech Sentinels.
        protected override PrototypeId[] ExcludedPowers => _excludedPowers;

        protected override int ThinkIntervalMs => 300;
        protected override float AttackRange => 300f;
        protected override float ChaseRange => 5000f;
        protected override float GlobalAttackCooldownMs => 800f;
        protected override float PerPowerCooldownMs => 6000f;
        protected override float DamageScale => 1.0f;

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);

            // Create SizeDecrease conditions directly via InitializeFromPower, bypassing
            // power activation checks (many of these powers are Instant/ProcEffect and fail
            // CanActivatePower validation). InitializeFromPower sets the creator power ref
            // and mixin index so the client can look up the UnrealClass visual effect.
            //
            // The client deduplicates by UnrealClass, so we use 4 distinct values:
            //   SizeDecrease25 (0.75x), SizeDecrease10 (0.90x), SizeDecrease05 (0.95x),
            //   FortuneCardMark2SizeDecrease (unknown %).
            // If all stack: 0.75 * 0.90 * 0.95 * ? ≈ 0.641 * ?
            var conditionCollection = agent.ConditionCollection;
            if (conditionCollection == null) return;

            foreach (PrototypeId powerRef in SizeDecreasePowers)
            {
                if (powerRef == PrototypeId.Invalid) continue;

                ConditionPrototype conditionProto = GetConditionPrototypeFromPower(powerRef);
                if (conditionProto == null) continue;

                // Build a minimal PowerPayload via reflection to set private setters.
                PowerPayload payload = CreateMinimalPayload(powerRef, agent);
                if (payload == null) continue;

                Condition condition = ConditionCollection.AllocateCondition();
                condition.InitializeFromPower(
                    conditionCollection.NextConditionId, payload, conditionProto, TimeSpan.Zero);
                conditionCollection.AddCondition(condition);
            }
        }

        /// <summary>
        /// Extracts the first ConditionPrototype from a power's AppliesConditions mixin list.
        /// </summary>
        private static ConditionPrototype GetConditionPrototypeFromPower(PrototypeId powerProtoRef)
        {
            if (powerProtoRef == PrototypeId.Invalid) return null;
            var powerProto = powerProtoRef.As<PowerPrototype>();
            if (powerProto?.AppliesConditions == null) return null;
            foreach (var item in powerProto.AppliesConditions)
            {
                if (item.Prototype is ConditionPrototype conditionProto)
                    return conditionProto;
            }
            return null;
        }

        /// <summary>
        /// Creates a minimal PowerPayload with just enough data for InitializeFromPower.
        /// Uses reflection to set private/protected setters on PowerPayload and PowerEffectsPacket.
        /// </summary>
        private PowerPayload CreateMinimalPayload(PrototypeId powerProtoRef, Agent agent)
        {
            PowerPrototype powerProto = powerProtoRef.As<PowerPrototype>();
            if (powerProto == null) return null;

            var payload = new PowerPayload();
            payload.Init(Game);

            // Set protected set properties on PowerEffectsPacket via reflection
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.PowerPrototype))
                .SetValue(payload, powerProto);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.PowerOwnerId))
                .SetValue(payload, agent.Id);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.UltimateOwnerId))
                .SetValue(payload, agent.Id);
            typeof(PowerEffectsPacket).GetProperty(nameof(PowerEffectsPacket.TargetId))
                .SetValue(payload, agent.Id);

            // Set private set property on PowerPayload via reflection
            typeof(PowerPayload).GetProperty(nameof(PowerPayload.PowerProtoRef))
                .SetValue(payload, powerProtoRef);

            return payload;
        }
    }
}
