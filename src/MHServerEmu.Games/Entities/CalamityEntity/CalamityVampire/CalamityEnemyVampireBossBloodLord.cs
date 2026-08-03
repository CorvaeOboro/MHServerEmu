#region BOSS BLOOD 
// =============================================================================
// MOD CALAMITY 
// =============================================================================
//   CALAMITY is a collection of custom encounters that are short, small 
//   and play like the games existing "terminals". 
//
//   Ritual Centerpiece is a prop of the boss fight to interact with , elude to lore , or be decorative 
//   for Example the "Amulet of Quiox" item is being showcased to elude to its lore 
//   this vampire coven is attempting to corrupt it so that it may not be used to slow vampirism
//   the player standing near the amulet long enough will purify it ( red to blue vfx ) , 
//   but entering into the ritual circle slows ( kacilius mirror dimension cirle )
//
//  VERSION:: 20260713
// =============================================================================

using Gazillion;
using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Entities.IncursionEntity;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.Network;
using MHServerEmu.Games.Entities.PowerCollections;
using MHServerEmu.Games.Powers;
using MHServerEmu.Games.Properties;
using MHServerEmu.Games.Regions;
using MHServerEmu.Games.Social;

namespace MHServerEmu.Games.Entities.CalamityEntity
{
    /// <summary>
    /// Vampire Blood Ritual - Final Boss
    /// "Vampire Blood Lord" - rendered as Scarlet Witch wearing the DarkWanda costume.
    /// Normal powers are Scarlet Witch powers so animations play correctly on the SW pawn.
    /// CalamityPowers are complex special abilities cast via invisible proxies (for non-SW
    /// powers) or directly (for SW powers), with patterns like cross AOE, radial bursts,
    /// and cascading radial waves.
    /// 3-phase enrage: normal (100-66%), enraged (66-33%), furious (33-0%).
    /// Summons vampire thrall adds at phase 2 (33% HP).
    /// </summary>
    public class CalamityEnemyVampireBossBloodLord : IncursionEnemyAvatar
    {
        #region Variables

        // Toggle: send augmented power activations as overhead chat text for in-game debugging.
        private static readonly bool DebugAugmentedPowers = false;
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/ScarletWitch.prototype");

        private static readonly PrototypeId DarkWandaCostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/ScarletWitch/DarkWanda.prototype");
            //"Entity/Items/Costumes/Prototypes/ScarletWitch/Ultimate.prototype" // zombie scarlet mod
            //Entity/Items/Costumes/Prototypes/ScarletWitch/DarkWanda.prototype

        // Henchman prototype for add summoning during phase 2.
        private static PrototypeId _vampireThrallRef = PrototypeId.Invalid;
        private static PrototypeId VampireThrallRef
        {
            get
            {
                if (_vampireThrallRef == PrototypeId.Invalid)
                    _vampireThrallRef = GameDatabase.GetPrototypeRefByName("Entity/Characters/Mobs/DarkElves/DarkElfSoldierBase.prototype");
                return _vampireThrallRef;
            }
        }

        // Buff power that produces a red visual aura (AvatarOfCyttorak ).
        // Other candidates commented out for individual evaluation.
        private static readonly PrototypeId[] _summonBuffPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/AvatarOfCyttorak.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Juggernaut/ImInvulnerable.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Wolverine/BloodySteroid.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Blade/SerumInjection.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Blade/BloodlustHiddenPassive.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/TeamUps/Drax/RageSteroid.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Wolverine/Frenzy.prototype"),
            // GameDatabase.GetPrototypeRefByName("Powers/Player/Thor/WarriorsWrath.prototype"),
        };

        // Random vampire-themed names for summoned adds.
        private static readonly string[] _summonVampireNames = new[]
        {
            "Vampire Thrall",

        };

        private static PrototypeId _hostileAllianceRef = PrototypeId.Invalid;
        private static PrototypeId HostileAllianceRef
        {
            get
            {
                if (_hostileAllianceRef == PrototypeId.Invalid)
                    _hostileAllianceRef = GameDatabase.GetPrototypeRefByName("Entity/Alliances/Enemies.prototype");
                return _hostileAllianceRef;
            }
        }

        private static PrototypeId _championRankRef = PrototypeId.Invalid;
        private static PrototypeId ChampionRankRef
        {
            get
            {
                if (_championRankRef == PrototypeId.Invalid)
                    _championRankRef = GameDatabase.GetPrototypeRefByName("Mods/Ranks/BossNoOverheadInfo.prototype");
                return _championRankRef;
            }
        }

        #endregion

        #region Stats

        public CalamityEnemyVampireBossBloodLord(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => DarkWandaCostumeRef;
        public override string InvaderDisplayName => "Vampire Blood Lord";
        public override string LogFilePrefix => "Calamity_Vampire";
        public override string LogTrueName => "BossBloodLord";

        // Combat tuning
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 400f;
        protected override float ChaseRange => 99999f;
        protected override float GlobalAttackCooldownMs => 500f;
        protected override float PerPowerCooldownMs => 4000f;
        protected override float DamageScale => 0.02f;
        protected override float DamageTakenMultiplier => 0.35f;
        protected override bool CanRegainHealth => false;

        #endregion

        #region Power Table

        // Power table: Scarlet Witch powers for normal casting (animations match the SW pawn).
        // Non-SW CalamityPowers are cast via invisible proxies and are NOT in this table.
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            // Single-target / direct-cast SW powers
            new("Powers/Player/ScarletWitch/Rework/ChaosBlast.prototype",    true,  0.0549f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/Implosion.prototype",     true,  0.0958f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/HexBolt.prototype",       true,  0.0974f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/BouncingHex.prototype",   true,  0.1418f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/UnmakeReality.prototype", false,  0.0300f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/ShadowBolt.prototype",    true,  0.1856f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/AlterReality.prototype",  true,  0.03f),
            new("Powers/Player/ScarletWitch/Rework/ChaosHex.prototype",      true,  0.02f),
            new("Powers/Player/ScarletWitch/Rework/IronMaiden.prototype",    true,  0.02f),

            // SW powers also used as CalamityPower cross-pattern AOE (in table for damage scaling)
            new("Powers/Player/ScarletWitch/Rework/HexSphere.prototype",     true,  0.0430f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/ChaosRift.prototype",     true,  0.1063f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/DarkHex.prototype",       true,  0.1781f), // 2026-08-01
            new("Powers/Player/ScarletWitch/Rework/UnmakeReality.prototype", true,  0.0300f), // 2026-08-01

            // Non-SW CalamityPower sources (in table for damage scaling, but NOT assigned
            // to the boss agent - they are activated via the augmented power system).
            new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtMentalBombStartFront.prototype", false, 0.04f),
            new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtSummonMentalVoidFront.prototype", false, 0.14f),
            new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtSpikedanceDelay.prototype",   false, 0.03f),
            new("Powers/Player/Blade/AllOutAssault.prototype",                        false, 0.1116f),
            new("Powers/Player/Blade/UnleashGlaive.prototype",                        false, 0.1479f), // 2026-08-01
            new("Powers/Player/Blade/JustStayDown.prototype",                         false, 0.0047f), // 2026-08-01
            new("Powers/Player/Elektra/Ultimate.prototype",                           false, 0.0034f), // 2026-08-01
            new("Powers/Player/Elektra/TripleChain.prototype",                        false, 0.02f),
            new("Powers/Player/Elektra/SpinningStrike.prototype",                     false, 0.02f),
            new("Powers/Player/Carnage/Ultimate.prototype",                           false, 0.0046f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Venom/VenomOMTripleShot.prototype",           false, 0.3418f), // 2026-08-01
            new("Powers/EnemyPowers/Boss/Kurse/KurseDarkVoidToss.prototype",             false, 0.8329f), // 2026-08-01

            // Kaecilius MorphEnvironment - visual darkness effect (0 damage, cast on setup)
            new("Powers/EnemyPowers/Boss/Kaecilius/SummonMorphZone.prototype",        false, 0.0f),
        };

        #endregion

        #region Power Augment

        // --- Augmented power system ---

        private AugmentedPowerController _augmentedCtrl;

        // Round-robin index: each successful augmented power cast advances this so the
        // boss cycles through her kit rather than always prioritizing the first entry.
        private int _augmentedPowerIndex = 0;

        // Augmented power definitions: special complex abilities with patterns.
        // All powers are cast directly by the boss (no proxy). Non-SW powers may briefly
        // T-pose the boss, but a SW animation reset is fired immediately after to mask it.
        private static readonly AugmentedPowerEntry[] _augmentedPowers = new AugmentedPowerEntry[]
        {
            // Cross-pattern AOE (SW powers, direct cast - animations match)
            new("Powers/Player/ScarletWitch/Rework/HexSphere.prototype",
                AugmentedPattern.Cross, 0.03f, cooldownMs: 5000, useProxy: false,
                radius: 300f),
            //new("Powers/Player/ScarletWitch/Rework/ChaosRift.prototype", // chaos rift lasts too long
            //    AugmentedPattern.Cross, 0.03f, cooldownMs: 8000, useProxy: false,
            //    radius: 300f),
            new("Powers/Player/ScarletWitch/Rework/DarkHex.prototype",
                AugmentedPattern.Cross, 0.04f, cooldownMs: 10000, useProxy: false,
                radius: 350f),

            // Radial burst AOE (Onslaught spike dance - ground spike AOE, direct cast)
            //new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtSpikedanceDelay.prototype",
            //    AugmentedPattern.RadialBurst, 0.03f, cooldownMs: 12000, useProxy: false,
            //    radius: 400f, pointCount: 6),

            // Cascading radial waves (Onslaught mental bomb - expanding AOE, direct cast)
            new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtMentalBombStartFront.prototype",
                AugmentedPattern.SingleAtPosition, 0.0038f, cooldownMs: 20000, useProxy: false),
                //AugmentedPattern.CascadingRadial, 0.04f, cooldownMs: 10000, useProxy: false,
                //radius: 200f, pointCount: 6, rings: 3, radiusStep: 250f, delayMs: 500),

            // Onslaught mental void summon - another candidate for red orb visual.
            //new("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtSummonMentalVoidFront.prototype",
            //   AugmentedPattern.SingleAtPosition, 0.04f, cooldownMs: 22000, useProxy: false),

            // Elektra swirling red AOE ultimate (direct cast, half the  damage scale from IncursionEnemyElektra)
            new("Powers/Player/Elektra/Ultimate.prototype",
                AugmentedPattern.SingleAtPosition, 0.003f, cooldownMs: 11000, useProxy: false), // half dmg of incursion elktra

            // Carnage ultimate - massive symbiote AOE (direct cast, non-SW rig)
            new("Powers/Player/Carnage/Ultimate.prototype",
                AugmentedPattern.SingleAtPosition, 0.0038f, cooldownMs: 20000, useProxy: false),

            // Venom black projectile - cross pattern (direct cast, non-SW rig)
            new("Powers/EnemyPowers/Boss/Venom/VenomOMTripleShot.prototype",
                AugmentedPattern.Cross, 0.04f, cooldownMs: 6000, useProxy: false,
                radius: 100f),

            // Blade UnleashGlaive - cross pattern (direct cast, non-SW rig)
            new("Powers/Player/Blade/UnleashGlaive.prototype",
                AugmentedPattern.Cross, 0.0961f, cooldownMs: 8000, useProxy: false,
                radius: 100f),

            // Kurse Dark Void - cascading line of 3 in boss facing direction (direct cast)
            new("Powers/EnemyPowers/Boss/Kurse/KurseDarkVoidToss.prototype",
                AugmentedPattern.CascadingLine, 0.4f, cooldownMs: 10000, useProxy: false,
                radius: 150, pointCount: 3, radiusStep: 170f, delayMs: 500),
        };

        // SW powers for normal single-target casting (direct, no proxy).
        private static readonly PrototypeId[] _targetPowers = new[]
        {
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/ChaosBlast.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/HexBolt.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/BouncingHex.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/Implosion.prototype"),
            //GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/UnmakeReality.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/ShadowBolt.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/AlterReality.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/ChaosHex.prototype"),
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/IronMaiden.prototype"),
        };

        #endregion

        #region Power Casting

        protected override bool TryCustomPowerCast(Agent agent, Avatar target)
        {
            TimeSpan now = Game.CurrentTime;
            if (now < _globalAttackCooldownEnd) return false;

            // During intro, let the agent settle.
            if (IsInIntroState() && (now - _spawnTime).TotalMilliseconds < 1500)
                return false;

            float effectiveRange = GetEffectiveAttackRange();
            float distSq = Vector3.DistanceSquared2D(agent.RegionLocation.Position, target.RegionLocation.Position);
            if (distSq > effectiveRange * effectiveRange) return false;

            // Process pending Augmented power delayed casts and clean up expired proxies.
            _augmentedCtrl?.Update();

            // Try Augmented powers in round-robin order starting from the rotating index.
            // Each power has its own cooldown so ultimates fire less frequently than cross patterns.
            for (int i = 0; i < _augmentedPowers.Length; i++)
            {
                int idx = (_augmentedPowerIndex + i) % _augmentedPowers.Length;
                var entry = _augmentedPowers[idx];
                if (IsPowerReady(entry.PowerRef, now) == false) continue;

                bool fired = false;
                int hits = 0;

                switch (entry.Pattern)
                {
                    case AugmentedPattern.Cross:
                        hits = _augmentedCtrl.CastCrossPattern(entry.PowerRef, entry.Radius, entry.UseProxy);
                        fired = hits > 0;
                        break;

                    case AugmentedPattern.RadialBurst:
                        hits = _augmentedCtrl.CastRadialBurst(entry.PowerRef, entry.PointCount, entry.Radius, entry.UseProxy);
                        fired = hits > 0;
                        break;

                    case AugmentedPattern.CascadingRadial:
                        _augmentedCtrl.CastCascadingRadial(entry.PowerRef, entry.Rings, entry.PointCount,
                            entry.Radius, entry.RadiusStep, entry.DelayMs, entry.UseProxy);
                        fired = true; // scheduled, not immediate
                        break;

                    case AugmentedPattern.SingleAtPosition:
                        // Cast at target position for offensive ultimates.
                        fired = _augmentedCtrl.CastSingleAtPosition(entry.PowerRef, target.RegionLocation.Position, entry.UseProxy);
                        break;

                    case AugmentedPattern.CascadingLine:
                        _augmentedCtrl.CastCascadingLine(entry.PowerRef, entry.PointCount, entry.Radius,
                            entry.RadiusStep, entry.DelayMs, entry.UseProxy);
                        fired = true; // scheduled, not immediate
                        break;
                }

                if (fired)
                {
                    SetCooldown(entry.PowerRef, now);
                    // Advance the round-robin index so the next think starts from the next power.
                    _augmentedPowerIndex = (idx + 1) % _augmentedPowers.Length;
                    float cdMult = (entry.Pattern == AugmentedPattern.CascadingRadial || entry.Pattern == AugmentedPattern.CascadingLine) ? 3f : 2f;
                    _globalAttackCooldownEnd = now + TimeSpan.FromMilliseconds(GlobalAttackCooldownMs * Math.Max(0.05f, PhaseCooldownScale()) * cdMult);
                    if (IsIncursionLoggingEnabled)
                        Logger.Info($"[CalamityEnemy:VampireBloodLord] Augmented power '{GameDatabase.GetPrototypeName(entry.PowerRef)}' ({entry.Pattern}, hits={hits}).");

                    if (DebugAugmentedPowers)
                    {
                        string powerName = GameDatabase.GetPrototypeName(entry.PowerRef);
                        string shortName = powerName.Substring(powerName.LastIndexOf('/') + 1).Replace(".prototype", "");
                        string debugText = $"[AUG] {entry.Pattern} -> {shortName} (hits={hits})";
                        Logger.Info($"[CalamityEnemy:VampireBloodLord] DEBUG CHAT: {debugText}");
                        SendDebugChat(agent, debugText);
                    }

                    // If the power is not a Scarlet Witch power, immediately activate a SW
                    // power to snap the boss out of the T-pose caused by the non-matching animation.
                    string firedPowerName = GameDatabase.GetPrototypeName(entry.PowerRef);
                    if (firedPowerName.Contains("ScarletWitch") == false)
                        PlayAnimReset(agent, target.RegionLocation.Position);

                    return true;
                }
            }

            // Fallback: normal single-target SW power cast directly by the boss.
            PrototypeId targetPower = _targetPowers[Game.Random.Next(_targetPowers.Length)];
            if (IsPowerReady(targetPower, now) && ActivatePowerOnTarget(agent, targetPower, target))
            {
                SetCooldown(targetPower, now);
                _globalAttackCooldownEnd = now + TimeSpan.FromMilliseconds(GlobalAttackCooldownMs * Math.Max(0.05f, PhaseCooldownScale()));
                _lastUsedPowerRef = targetPower;
                return true;
            }

            return false;
        }

        private static readonly PrototypeId _animResetPowerRef =
            GameDatabase.GetPrototypeRefByName("Powers/Player/ScarletWitch/Rework/HexBolt.prototype");

        private void PlayAnimReset(Agent agent, Vector3 targetPos)
        {
            if (_animResetPowerRef == PrototypeId.Invalid) return;

            Power power = agent.GetPower(_animResetPowerRef);
            if (power == null)
            {
                PowerIndexProperties indexProps = new(0, agent.CharacterLevel, agent.CombatLevel);
                if (agent.AssignPower(_animResetPowerRef, indexProps) == null) return;
                power = agent.GetPower(_animResetPowerRef);
                if (power == null) return;
            }

            PowerActivationSettings settings = new(Entity.InvalidId, targetPos, agent.RegionLocation.Position);
            settings.Flags |= PowerActivationSettingsFlags.NotifyOwner;
            agent.ActivatePower(_animResetPowerRef, ref settings);
        }

        #endregion

        #region Helpers

        private void SendDebugChat(Agent agent, string text)
        {
            var chatMsg = ChatMessage.CreateBuilder()
                .SetBody(text)
                .Build();

            var message = ChatNormalMessage.CreateBuilder()
                .SetRoomType(ChatRoomTypes.CHAT_ROOM_TYPE_LOCAL)
                .SetFromPlayerName("VampireBloodLord")
                .SetTheMessage(chatMsg)
                .SetPrestigeLevel(0)
                .Build();

            Region region = agent.Region;
            if (region == null) return;

            foreach (Player player in new PlayerIterator(region))
                player.PlayerConnection.SendMessage(message);
        }

        private bool IsPowerReady(PrototypeId powerRef, TimeSpan now)
        {
            if (_cooldownEndTimes.TryGetValue(powerRef, out TimeSpan end) && now < end)
                return false;
            return true;
        }

        private void SetCooldown(PrototypeId powerRef, TimeSpan now)
        {
            _cooldownEndTimes[powerRef] = now + TimeSpan.FromMilliseconds(GetCooldownMsForPower(powerRef));
        }

        #endregion

        #region Phases

        // 3-phase enrage: normal -> enraged (66%) -> furious (33%)
        protected override int GetPhaseForHealthPct(float healthPct)
        {
            if (healthPct < 0.33f) return 2;
            if (healthPct < 0.66f) return 1;
            return 0;
        }

        protected override float PhaseCooldownScale() => CurrentPhase switch
        {
            1 => 0.7f,
            2 => 0.4f,
            _ => 1.0f,
        };

        #endregion

        #region Setup 

        protected override void OnSetup(Agent agent)
        {
            base.OnSetup(agent);
            _augmentedCtrl = new AugmentedPowerController(Game, agent, this);

        }

        private bool _mentalOrbCast = false;
        private bool _justStayDownCast = false;

        // Fraction of max health healed on each phase transition (15%).
        private const float PhaseHealPct = 0.15f;

        protected override void OnPhaseChanged(Agent agent, int newPhase)
        {
            if (newPhase == 1)
            {
                SummonVampireAdds(agent, 2, 3);

                // One-time mental orb cast at 66% health (phase 1 transition).
                if (_mentalOrbCast == false && _augmentedCtrl != null)
                {
                    _mentalOrbCast = true;
                    PrototypeId orbPower = GameDatabase.GetPrototypeRefByName("Powers/EnemyPowers/Boss/OnslaughtRaid/OnslaughtSummonMentalOrb.prototype");
                    if (orbPower != PrototypeId.Invalid)
                    {
                        _augmentedCtrl.CastCascadingRadial(orbPower, 3, 6, 200f, 250f, 500, false);
                        if (DebugAugmentedPowers)
                        {
                            Logger.Info("[CalamityEnemy:VampireBloodLord] PHASE 1: One-time Mental Orb cast at 66% health.");
                            SendDebugChat(agent, "[PHASE] Mental Orb unleashed!");
                        }
                    }
                }
            }

            if (newPhase == 2)
            {
                // One-time Blade JustStayDown cast at 33% health (phase 2 transition).
                if (_justStayDownCast == false && _augmentedCtrl != null)
                {
                    _justStayDownCast = true;
                    PrototypeId justStayDown = GameDatabase.GetPrototypeRefByName("Powers/Player/Blade/JustStayDown.prototype");
                    if (justStayDown != PrototypeId.Invalid)
                    {
                        _augmentedCtrl.CastSingleAtPosition(justStayDown, agent.RegionLocation.Position, useProxy: false, skipCheck: true);
                        if (DebugAugmentedPowers)
                        {
                            Logger.Info("[CalamityEnemy:VampireBloodLord] PHASE 2: One-time JustStayDown cast at 33% health.");
                            SendDebugChat(agent, "[PHASE] JustStayDown unleashed!");
                        }
                    }
                }
            }

            // Refresh additional resilience and apply a small heal on every phase transition.
            if (newPhase > 0)
            {
                RefreshAdditionalResilience();

                long healthMax = agent.Properties[PropertyEnum.HealthMax];
                long healAmount = Math.Max(1L, (long)(healthMax * PhaseHealPct));
                long newHealth = Math.Min(agent.Properties[PropertyEnum.Health] + healAmount, healthMax);
                agent.Properties[PropertyEnum.Health] = newHealth;

                if (IsIncursionLoggingEnabled)
                    Logger.Info($"[CalamityEnemy:VampireBloodLord] PHASE {newPhase}: resilience refreshed, healed {healAmount} HP ({agent.Properties[PropertyEnum.Health]}/{healthMax}).");
            }
        }

        #endregion

        #region Add Summoning

        /// <summary>
        /// Spawns vampire thrall adds around the boss at ~800 unit radius using the region's
        /// spawn pipeline. Summoned thralls use CalamityEnemyVampireThrallSummoned (basic,
        /// no nameplate) via SpawnVampireBloodRitualThrall, ensuring correct rendering, loot
        /// stripping, death VFX suppression, damage scaling, and buff power application.
        /// Positions are projected to floor and cell-validated by the spawn pipeline; invalid
        /// positions are skipped.
        /// </summary>
        private void SummonVampireAdds(Agent agent, int minCount, int maxCount)
        {
            Region region = agent.Region;
            if (region == null) return;

            Vector3 bossPos = agent.RegionLocation.Position;
            int count = Game.Random.Next(minCount, maxCount + 1);
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = count * 4;  // try extra times to find valid positions

            for (int i = 0; i < count; i++)
            {
                bool spawnedThisIter = false;
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    attempts++;
                    float angle = (float)(Game.Random.NextDouble() * Math.PI * 2);
                    float dist = 400f + (float)(Game.Random.NextDouble() * 400f);  // 400-800 units
                    Vector3 offset = new(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
                    Vector3 spawnPos = bossPos + offset;

                    // Delegate to the region's spawn pipeline - ProjectToFloor + cell validation.
                    Agent thrall = region.SpawnVampireBloodRitualThrall(spawnPos);
                    if (thrall != null)
                    {
                        // Force permanent aggro so summoned adds never leash.
                        thrall.Properties[PropertyEnum.AIAlwaysAggroed] = true;
                        thrall.Properties[PropertyEnum.AIAggroState] = true;
                        spawned++;
                        spawnedThisIter = true;
                        break;
                    }
                }
                if (spawnedThisIter == false && IsIncursionLoggingEnabled)
                    Logger.Info($"[CalamityEnemy:VampireBloodLord] Failed to find valid spawn position for add #{i} after 4 attempts.");
            }

            if (spawned > 0 && IsIncursionLoggingEnabled)
                Logger.Info($"[CalamityEnemy:VampireBloodLord] Summoned {spawned} vampire adds at phase 1 (attempts={attempts}).");
        }

        #endregion

        #endregion
    }
}
