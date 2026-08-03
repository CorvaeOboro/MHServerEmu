#region DeathRecap
// =============================================================================
// MOD Death Recap - Player-side chat output
// =============================================================================
//   Formats the death recap buffer into chat lines and sends them to the
//   deceased player via ChatManager.SendChatFromCustomSystem.
//   Also stores the last recap so /recap can re-display it after respawn.
//
//  Integration:
//   - WorldEntity.ApplyPowerResultsInternal calls SendDeathRecap() on death.
//   - DeathRecapCommands.cs provides the /recap slash command.
//
//  VERSION:: 20260721
// =============================================================================

using System.Collections.Generic;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private string _lastDeathRecapText;

        // --- Number formatting ---

        /// <summary>
        /// Abbreviates a damage value: 8470 -> "8k", 1200000 -> "1.2M".
        /// </summary>
        private static string FormatDamageShort(float damage)
        {
            if (damage >= 1000000f) return $"{damage / 1000000f:F1}M";
            if (damage >= 1000f) return $"{MathF.Round(damage / 1000f)}k";
            return $"{damage:F0}";
        }

        // --- Dominant damage type ---

        /// <summary>
        /// Returns the damage type with the highest value.
        /// </summary>
        private static DamageType GetDominantDamageType(float physical, float energy, float mental)
        {
            if (energy >= physical && energy >= mental) return DamageType.Energy;
            if (mental >= physical && mental >= energy) return DamageType.Mental;
            return DamageType.Physical;
        }

        // --- Compact name formatting ---

        /// <summary>
        /// Truncates a source name to the first N letters after removing spaces: "Scarlet Witch" -> "Sca" (N=3).
        /// </summary>
        private static string FormatSourceNameCompact(string name, int nameLength)
        {
            if (string.IsNullOrEmpty(name)) return new string('?', nameLength);
            name = name.Replace(" ", "");
            return name.Length <= nameLength ? name : name[..nameLength];
        }

        // --- Compact damage type label ---

        /// <summary>
        /// First N letters of a damage type: Physical -> "P" (N=1), "Phy" (N=3).
        /// </summary>
        private static string FormatDamageTypeLabelCompact(DamageType damageType, int typeLength)
        {
            string fullName = damageType switch
            {
                DamageType.Physical => "Physical",
                DamageType.Energy => "Energy",
                DamageType.Mental => "Mental",
                _ => "Damage"
            };
            return fullName.Length <= typeLength ? fullName : fullName[..typeLength];
        }

        // --- Format a single damage source ---

        /// <summary>
        /// Formats one damage source as "Sca 8k E" using configurable name/type lengths.
        /// </summary>
        private string FormatSingleSource(DeathRecapSummary source, int nameLength, int typeLength)
        {
            string name = FormatSourceNameCompact(source.SourceName, nameLength);
            var dominantType = GetDominantDamageType(source.PhysicalDamage, source.EnergyDamage, source.MentalDamage);
            string dmgStr = FormatDamageShort(source.TotalDamage);
            string typeLabel = FormatDamageTypeLabelCompact(dominantType, typeLength);

            return $"{name} {dmgStr} {typeLabel}";
        }

        // --- Recap format ---

        /// <summary>
        /// Formats the recap as a compact single line: "DEATH = Sca 8k E | Ven 3k P | Lok 2k M"
        /// </summary>
        private string FormatRecap(DeathRecapBuffer buffer, int topN, int nameLength, int typeLength)
        {
            var topSources = buffer.GetTopDamageSources(topN);
            if (topSources.Length == 0) return "DEATH = No dmg.";

            var parts = new List<string>(topSources.Length);
            foreach (var s in topSources)
                parts.Add(FormatSingleSource(s, nameLength, typeLength));

            return "DEATH = " + string.Join(" | ", parts);
        }

        // --- Main entry: format and send ---

        /// <summary>
        /// Formats the death recap buffer and sends it to the player's chat.
        /// Called from WorldEntity.ApplyPowerResultsInternal on avatar death.
        /// </summary>
        internal void SendDeathRecap(DeathRecapBuffer buffer, Avatar killedAvatar)
        {
            if (buffer == null || buffer.Count == 0) return;

            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.DeathRecapEnable == false) return;

            int topN = customOptions.DeathRecapTopN;
            int nameLength = customOptions.DeathRecapNameLength;
            int typeLength = customOptions.DeathRecapDamageTypeLength;

            string recapText = FormatRecap(buffer, topN, nameLength, typeLength);

            _lastDeathRecapText = recapText;

            SendRecapLines(recapText);

            if (customOptions.DeathRecapLoggingEnable)
                Logger.Info($"[DeathRecap] Sent recap to player [{this}] for avatar [{killedAvatar?.PrototypeName}].");
        }

        // --- Send text as chat lines ---

        /// <summary>
        /// Splits text on newlines and sends each line as a separate chat message.
        /// The first line shows the sender name; subsequent lines do not.
        /// </summary>
        private void SendRecapLines(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            bool firstLine = true;
            foreach (string line in lines)
            {
                Game.ChatManager.SendChatFromCustomSystem(this, line, firstLine);
                firstLine = false;
            }
        }

        // --- Re-send last recap (for /recap command) ---

        /// <summary>
        /// Re-sends the last death recap to the player's chat (for /recap command).
        /// </summary>
        public bool ResendLastDeathRecap()
        {
            if (string.IsNullOrEmpty(_lastDeathRecapText)) return false;
            SendRecapLines(_lastDeathRecapText);
            return true;
        }
    }
}
#endregion
