using MHServerEmu.Core.Helpers;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Common;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.Entities.Inventories;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;

namespace MHServerEmu.Games.Powers
{
    public partial class PowerPayload
    {
        /// <summary>
        /// Resolves the per-ability damage scale for an incursion enemy. Returns 1.0 when the payload
        /// does not originate from an incursion enemy.
        /// </summary>
        private float GetIncursionEnemyDamageScale(out WorldEntity ultimateOwner, out PrototypeId rootPowerRef, out bool indirect)
        {
            rootPowerRef = PrototypeId.Invalid;
            indirect = false;
            ultimateOwner = null;

            WorldEntity immediateOwner = Game.EntityManager.GetEntity<WorldEntity>(UltimateOwnerId);
            WorldEntity invader = ResolveIncursionInvader(immediateOwner);
            if (invader == null)
                return 1f;

            ultimateOwner = invader;

            // Determine the root (activated) power: missiles/summons stamp CreatorPowerPrototype.
            WorldEntity propertySource = Game.EntityManager.GetEntity<WorldEntity>(_propertySourceEntityId);
            if (propertySource != null)
            {
                rootPowerRef = propertySource.Properties[PropertyEnum.CreatorPowerPrototype];
                indirect = propertySource != invader;
            }

            if (rootPowerRef == PrototypeId.Invalid)
                rootPowerRef = PowerProtoRef;

            // Resolve the scale through the IncursionManager registry.
            float scale = Game.IncursionManager != null
                ? Game.IncursionManager.GetOutgoingDamageScale(invader.Id, rootPowerRef)
                : 1f;

            return scale > 0f ? scale : 0f;
        }

        /// <summary>
        /// Walks the ownership chain to find the incursion invader ultimately responsible for this damage.
        /// </summary>
        private WorldEntity ResolveIncursionInvader(WorldEntity entity)
        {
            EntityManager entityManager = Game.EntityManager;

            // Bounded walk guards against any unexpected cycle in the ownership links.
            for (int hop = 0; entity != null && hop < 16; hop++)
            {
                // Avatar-type incursion enemies set IsClientRenderedAsAvatar.
                // TeamUp-type incursion enemies do not, so check the IncursionManager registry.
                if (entity.IsClientRenderedAsAvatar)
                    return entity;

                if (Game.IncursionManager != null && Game.IncursionManager.IsIncursionEntity(entity.Id))
                    return entity;

                ulong nextId = entity.PowerUserOverrideId;

                if (nextId == Entity.InvalidId || nextId == entity.Id)
                {
                    // Summoned pets/gadgets are tracked in their summoner's Summoned inventory even
                    // when PowerUserOverrideID is unset; use that container as the owner link.
                    ref InventoryLocation invLoc = ref entity.InventoryLocation;
                    if (invLoc.IsValid && invLoc.InventoryConvenienceLabel == InventoryConvenienceLabel.Summoned)
                        nextId = invLoc.ContainerId;
                }

                if (nextId == Entity.InvalidId || nextId == entity.Id)
                    break;

                entity = entityManager.GetEntity<WorldEntity>(nextId);
            }

            return null;
        }

        /// <summary>
        /// Scales an incursion enemy's outgoing damage by the per-ability scale.
        /// </summary>
        private void CalculateResultDamageIncursionScale(PowerResults results)
        {
            float scale = GetIncursionEnemyDamageScale(out _, out _, out _);
            if (scale >= 1f)
                return;

            ApplyDamageMultiplier(results.Properties, scale);
        }

        /// <summary>
        /// Logs incursion enemy damage for tuning.
        /// </summary>
        private void LogIncursionEnemyDamage(PowerResults results, WorldEntity target)
        {
            float scale = GetIncursionEnemyDamageScale(out WorldEntity ultimateOwner, out PrototypeId rootPowerRef, out bool indirect);
            if (ultimateOwner == null)
                return;

            float totalAfter = 0f;
            foreach (var kvp in results.Properties.IteratePropertyRange(PropertyEnum.Damage))
                totalAfter += kvp.Value;

            if (totalAfter <= 0f)
                return;

            // Only log damage to player avatars by default.
            if (target is not Avatar && Game?.CustomGameOptions?.IncursionLogAllDamageTargetsEnable == false)
                return;

            float totalBefore = scale > 0f ? totalAfter / scale : totalAfter;

            // Resolve the "table power" for logging - when a combo child has no CreatorPowerPrototype,
            // map it back to the parent so the log parser groups hits correctly.
            PrototypeId logAbilityRef = rootPowerRef;
            if (Game?.IncursionManager != null)
            {
                PrototypeId parentRef = Game.IncursionManager.GetParentPowerForEffect(ultimateOwner.Id, rootPowerRef);
                if (parentRef != PrototypeId.Invalid)
                    logAbilityRef = parentRef;
            }

            // Resolve the controller class name and enemy type for precise log parsing.
            string enemyType = "Unknown";
            string controllerCls = "Unknown";
            if (Game?.IncursionManager != null)
            {
                Game.IncursionManager.TryGetControllerInfo(ultimateOwner.Id, out controllerCls, out enemyType);
            }

            string damageMsg = $"[IncursionEnemy] DAMAGE: '{ultimateOwner.PrototypeName}' (id {ultimateOwner.Id}) " +
                               $"[type={enemyType} cls={controllerCls}] " +
                               $"{(indirect ? "indirect" : "direct")} ability '{GameDatabase.GetPrototypeName(logAbilityRef)}' " +
                               $"(deliver '{GameDatabase.GetPrototypeName(PowerProtoRef)}') -> after={MathHelper.RoundToInt(totalAfter)} " +
                               $"(unscaled~{MathHelper.RoundToInt(totalBefore)}, scale x{scale:0.###}) " +
                               $"to '{target?.PrototypeName}' (id {target?.Id}).";
            if (Game?.CustomGameOptions?.IncursionLoggingEnable == true)
                Logger.Info(damageMsg);
            IncursionLogCollator.WriteLine(ultimateOwner.Id, damageMsg);
            if (target != null) IncursionLogCollator.WriteLine(target.Id, damageMsg);
        }

        /// <summary>
        /// Normalizes difficulty multiplier to Red (Hard) tier for IncursionEnemies.
        /// IncursionEnemies are balanced for Hard mode; Cosmic tier's extra damage scaling
        /// makes them vastly too strong. This counteracts only the tier-specific portion
        /// (DamageMobToPlayerPct / DamagePlayerToMobPct) by ratioing back to Red tier values.
        /// Called from CalculateResultDamageDifficultyScaling().
        /// </summary>
        private void ApplyIncursionDifficultyNormalization(PowerResults results, WorldEntity target)
        {
            if (Game.IncursionManager == null)
                return;

            bool isIncursionSource = false;
            bool isIncursionTarget = false;

            // Outgoing: IncursionEnemy -> player
            GetIncursionEnemyDamageScale(out WorldEntity invader, out _, out _);
            if (invader != null && Game.IncursionManager.IsIncursionEntity(invader.Id))
                isIncursionSource = true;

            // Incoming: player -> IncursionEnemy
            if (Game.IncursionManager.IsIncursionEntity(target.Id))
                isIncursionTarget = true;

            if (isIncursionSource || isIncursionTarget)
            {
                Region region = target.Region;
                if (region != null)
                {
                    DifficultyTierPrototype currentTier = region.DifficultyTierRef.As<DifficultyTierPrototype>();
                    DifficultyTierPrototype redTier = GameDatabase.GlobalsPrototype?.GetDifficultyTierByEnum(DifficultyTier.Red);

                    if (currentTier != null && redTier != null && currentTier != redTier)
                    {
                        float ratio;
                        if (IsPlayerPayload)
                            ratio = redTier.DamagePlayerToMobPct / currentTier.DamagePlayerToMobPct;
                        else
                            ratio = redTier.DamageMobToPlayerPct / currentTier.DamageMobToPlayerPct;

                        ApplyDamageMultiplier(results.Properties, ratio);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the per-enemy incoming damage scale (DamageTakenScale * DamageTakenMultiplier)
        /// for incursion/calamity enemies directly in the damage pipeline. This bypasses the
        /// DamagePctVulnerability property, which can be overridden by conditions (e.g. AvatarOfCyttorak).
        /// Called from CalculateResultDamage() after difficulty scaling.
        /// </summary>
        private void ApplyIncursionIncomingDamageScale(PowerResults results, WorldEntity target)
        {
            if (Game.IncursionManager == null) return;
            if (Game.IncursionManager.IsIncursionEntity(target.Id) == false) return;

            float scale = Game.IncursionManager.GetIncomingDamageScale(target.Id);
            if (scale != 1f && scale > 0f)
                ApplyDamageMultiplier(results.Properties, scale);
        }
    }
}
