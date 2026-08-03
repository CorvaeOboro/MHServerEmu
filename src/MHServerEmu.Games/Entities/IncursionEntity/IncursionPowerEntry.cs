using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// One row in an incursion enemy's power table: a power, whether it is assigned,
    /// the per-power outgoing damage multiplier, an optional max channel duration,
    /// an optional per-power cooldown override (0 = use default), and whether the
    /// enemy can move freely while the power is active.
    /// </summary>
    public readonly struct IncursionPowerEntry
    {
        /// <summary>The power prototype.</summary>
        public readonly PrototypeId Power;

        /// <summary>Whether this enemy assigns and uses the power.</summary>
        public readonly bool Enabled;

        /// <summary>Per-power outgoing damage multiplier (1.0 = unchanged).</summary>
        public readonly float DamageScale;

        /// <summary>
        /// If > 0, the power is treated as channeled / continuous and will be forcibly
        /// stopped after this many milliseconds. 0 = not channeled (normal single-fire).
        /// </summary>
        public readonly int MaxChannelMs;

        /// <summary>
        /// Per-power cooldown override in milliseconds. 0 = use the controller's default
        /// cooldown (or ultimate multiplier for ultimate powers).
        /// </summary>
        public readonly int CooldownMs;

        /// <summary>
        /// If true, the enemy can move freely while this power is active, overriding
        /// the class-level FreezeMovementDuringPower. Useful for channeled powers that
        /// should not root the boss in place (e.g. Grim Reaper's scythe attacks).
        /// </summary>
        public readonly bool CanMoveDuringPower;

        public IncursionPowerEntry(string powerPath, bool enabled, float damageScale, int maxChannelMs = 0, int cooldownMs = 0, bool canMoveDuringPower = false)
        {
            Power = GameDatabase.GetPrototypeRefByName(powerPath);
            Enabled = enabled;
            DamageScale = damageScale;
            MaxChannelMs = maxChannelMs;
            CooldownMs = cooldownMs;
            CanMoveDuringPower = canMoveDuringPower;
        }

        public IncursionPowerEntry(PrototypeId power, bool enabled, float damageScale, int maxChannelMs = 0, int cooldownMs = 0, bool canMoveDuringPower = false)
        {
            Power = power;
            Enabled = enabled;
            DamageScale = damageScale;
            MaxChannelMs = maxChannelMs;
            CooldownMs = cooldownMs;
            CanMoveDuringPower = canMoveDuringPower;
        }
    }
}
