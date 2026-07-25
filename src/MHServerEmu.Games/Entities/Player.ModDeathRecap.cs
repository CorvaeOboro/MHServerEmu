#region DeathRecap
// =============================================================================
// MOD Death Recap - Player-side chat + banner output
// =============================================================================
//   Formats the death recap buffer into chat lines and sends them to the
//   deceased player via ChatManager.SendChatFromCustomSystem.
//   Optionally sends a banner message via NetMessageBannerMessage for visual impact.
//   Also stores the last recap so /recap can re-display it after respawn.
//
//  Integration:
//   - WorldEntity.ApplyPowerResultsInternal calls SendDeathRecap() on death.
//   - DeathRecapCommands.cs provides the /recap slash command.
//
//  VERSION:: 20260721
// =============================================================================

using System.Collections.Generic;
using System.Text;
using MHServerEmu.Core.Logging;
using MHServerEmu.Games.Entities.Avatars;
using MHServerEmu.Games.GameData;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Powers;

namespace MHServerEmu.Games.Entities
{
    public partial class Player
    {
        private string _lastDeathRecapText;

        // NOTE: Color tag prepending removed — the client chat system does not support
        // color markup, so prepending hex color codes (e.g. "FF0000") showed as literal
        // text in chat. DeathRecapColorEnable is kept as a config no-op for compatibility.

        // --- Number formatting ---

        /// <summary>
        /// Abbreviates a damage value: 8470 → "8k", 1200000 → "1.2M".
        /// </summary>
        private static string FormatDamageShort(float damage)
        {
            if (damage >= 1000000f) return $"{damage / 1000000f:F1}M";
            if (damage >= 1000f) return $"{MathF.Round(damage / 1000f)}k";
            return $"{damage:F0}";
        }

        // --- Name formatting ---

        /// <summary>
        /// Shortens a source name by removing spaces: "Scarlet Witch" → "ScarletWitch".
        /// </summary>
        private static string FormatSourceName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";
            return name.Replace(" ", "");
        }

        // --- Damage type label ---

        /// <summary>
        /// Short label for a damage type: Physical → "phys", Energy → "energy", Mental → "ment".
        /// </summary>
        private static string FormatDamageTypeLabel(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => "phys",
                DamageType.Energy => "energy",
                DamageType.Mental => "ment",
                _ => "dmg"
            };
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

        // --- Ultra-compact name formatting ---

        /// <summary>
        /// Truncates a source name to first 3 letters: "Scarlet Witch" → "Sca".
        /// </summary>
        private static string FormatSourceNameUltra(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unk";
            name = name.Replace(" ", "");
            return name.Length <= 3 ? name : name[..3];
        }

        // --- Ultra-compact damage type label ---

        /// <summary>
        /// Single capital letter for a damage type: Physical → "P", Energy → "E", Mental → "M".
        /// </summary>
        private static string FormatDamageTypeLabelUltra(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => "P",
                DamageType.Energy => "E",
                DamageType.Mental => "M",
                _ => "D"
            };
        }

        // --- Format a single damage source for single-line output ---

        /// <summary>
        /// Formats one damage source as "Name 8k phys" (with optional color tags).
        /// </summary>
        private string FormatSingleSource(DeathRecapSummary source, bool useColor)
        {
            string name = FormatSourceName(source.SourceName);
            var dominantType = GetDominantDamageType(source.PhysicalDamage, source.EnergyDamage, source.MentalDamage);
            string dmgStr = FormatDamageShort(source.TotalDamage);
            string typeLabel = FormatDamageTypeLabel(dominantType);

            return $"{name} {dmgStr} {typeLabel}";
        }

        // --- Format a single damage source for ultra-compact output ---

        /// <summary>
        /// Formats one damage source as "Sca 8k E" (with optional color tags).
        /// </summary>
        private string FormatSingleSourceUltra(DeathRecapSummary source, bool useColor)
        {
            string name = FormatSourceNameUltra(source.SourceName);
            var dominantType = GetDominantDamageType(source.PhysicalDamage, source.EnergyDamage, source.MentalDamage);
            string dmgStr = FormatDamageShort(source.TotalDamage);
            string typeLabel = FormatDamageTypeLabelUltra(dominantType);

            return $"{name} {dmgStr} {typeLabel}";
        }

        // --- Single-line recap format ---

        /// <summary>
        /// Formats the recap as a single line: "DEATH = ScarletWitch 8k energy | Venom 3k phys | Loki 2k ment"
        /// </summary>
        private string FormatSingleLineRecap(DeathRecapBuffer buffer, int topN, bool useColor)
        {
            var topSources = buffer.GetTopDamageSources(topN);
            if (topSources.Length == 0) return "DEATH = No damage recorded.";

            var parts = new List<string>(topSources.Length);
            foreach (var s in topSources)
                parts.Add(FormatSingleSource(s, useColor));

            return "DEATH = " + string.Join("   |   ", parts);
        }

        // --- Ultra-compact recap format ---

        /// <summary>
        /// Formats the recap as an ultra-compact single line: "DEATH = Sca 8k E | Ven 3k P | Lok 2k M"
        /// </summary>
        private string FormatUltraCompactRecap(DeathRecapBuffer buffer, int topN, bool useColor)
        {
            var topSources = buffer.GetTopDamageSources(topN);
            if (topSources.Length == 0) return "DEATH = No dmg.";

            var parts = new List<string>(topSources.Length);
            foreach (var s in topSources)
                parts.Add(FormatSingleSourceUltra(s, useColor));

            return "DEATH = " + string.Join(" | ", parts);
        }

        // --- Detailed multi-line recap format ---

        /// <summary>
        /// Formats the recap as a detailed multi-line report (the original format).
        /// </summary>
        private string FormatDetailedRecap(DeathRecapBuffer buffer, int topN, bool showHeals, bool useColor)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DEATH RECAP ===");

            var timeSpan = buffer.GetTimeSpan();
            if (timeSpan > TimeSpan.Zero)
                sb.AppendLine($"Killed in {timeSpan.TotalSeconds:F1}s - Top {topN} damage sources:");

            var topSources = buffer.GetTopDamageSources(topN);
            if (topSources.Length > 0)
            {
                for (int i = 0; i < topSources.Length; i++)
                {
                    var s = topSources[i];
                    var dominantType = GetDominantDamageType(s.PhysicalDamage, s.EnergyDamage, s.MentalDamage);
                    string typeLabel = FormatDamageTypeLabel(dominantType);
                    string flags = "";
                    if (s.IsCrit) flags += " CRIT";
                    if (s.IsDoT) flags += " DoT";

                    string dmgStr = FormatDamageShort(s.TotalDamage);

                    sb.AppendLine($"#{i + 1} {s.SourceName} - {dmgStr} {typeLabel} dmg ({s.HitCount} hits{flags})");
                }
            }
            else
            {
                sb.AppendLine("No damage recorded.");
            }

            if (showHeals)
            {
                var heals = buffer.GetHealEntries();
                if (heals.Length > 0)
                {
                    float totalHeals = 0;
                    foreach (var h in heals) totalHeals += h.Healing;
                    sb.AppendLine($"Incoming healing: {FormatDamageShort(totalHeals)} ({heals.Length} events)");
                }
            }

            var chrono = buffer.ToChronologicalArray();
            int recentCount = Math.Min(5, chrono.Length);
            if (recentCount > 0)
            {
                sb.AppendLine("--- Last hits ---");
                for (int i = chrono.Length - recentCount; i < chrono.Length; i++)
                {
                    var e = chrono[i];
                    if (e.Healing > 0) continue;
                    var dominantType = GetDominantDamageType(e.PhysicalDamage, e.EnergyDamage, e.MentalDamage);
                    string typeShort = dominantType switch
                    {
                        DamageType.Energy => "Ene",
                        DamageType.Mental => "Men",
                        _ => "Phy"
                    };
                    string critTag = (e.Flags.HasFlag(PowerResultFlags.Critical) || e.Flags.HasFlag(PowerResultFlags.SuperCritical)) ? "!" : "";
                    string dmgStr = FormatDamageShort(e.TotalDamage);
                    sb.AppendLine($"{e.SourceName} {dmgStr}{typeShort}{critTag} [{e.PowerName}] HP:{e.HealthBefore}->{e.HealthAfter}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // --- Banner message ---

        /// <summary>
        /// Sends a banner message to the player for visual impact on death.
        /// Uses NetMessageBannerMessage with an existing game LocaleStringId.
        /// Banner position and size are client-side controlled; we can choose TextStyle
        /// (Standard, Large, Error, Alert) and MessageStyle (Standard, FlyIn, Error).
        /// Note: bannerText must be a LocaleStringId that exists in the client's locale table.
        /// We use a known death-related string if available, otherwise skip.
        /// </summary>
        private void SendDeathRecapBanner()
        {
            // LocaleStringId for "You have been defeated" — this is a known game string
            // used in PvP death scenarios. If it doesn't resolve on the client, the banner
            // will simply not display (no crash).
            // TODO: Find the exact LocaleStringId for "You have been defeated" from client data.
            // For now we use LocaleStringId.Invalid to skip banner until we identify the right ID.
            LocaleStringId defeatTextId = LocaleStringId.Invalid;

            if (defeatTextId == LocaleStringId.Invalid) return;

            SendBannerMessage(
                bannerText: defeatTextId,
                textStyle: TextStylePrototype.BannerMessageAlert,
                timeToLiveMS: 4000,
                messageStyle: BannerMessageStyle.FlyIn,
                doNotQueue: true,
                showImmediately: true);
        }

        // --- Main entry: format and send ---

        /// <summary>
        /// Formats the death recap buffer and sends it to the player's chat.
        /// Optionally sends a banner message for visual impact.
        /// Called from WorldEntity.ApplyPowerResultsInternal on avatar death.
        /// </summary>
        internal void SendDeathRecap(DeathRecapBuffer buffer, Avatar killedAvatar)
        {
            if (buffer == null || buffer.Count == 0) return;

            var customOptions = Game?.CustomGameOptions;
            if (customOptions == null || customOptions.DeathRecapEnable == false) return;

            int topN = customOptions.DeathRecapTopN;
            bool showHeals = customOptions.DeathRecapShowHeals;
            bool useColor = customOptions.DeathRecapColorEnable;
            bool singleLine = customOptions.DeathRecapSingleLine;
            bool ultraCompact = customOptions.DeathRecapUltraCompact;
            bool sendBanner = customOptions.DeathRecapBannerEnable;

            // Send banner first (appears immediately, chat follows)
            if (sendBanner)
                SendDeathRecapBanner();

            string recapText;
            if (ultraCompact)
                recapText = FormatUltraCompactRecap(buffer, topN, useColor);
            else if (singleLine)
                recapText = FormatSingleLineRecap(buffer, topN, useColor);
            else
                recapText = FormatDetailedRecap(buffer, topN, showHeals, useColor);

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
