#region Item Merge Best Affix
// =============================================================================
// MOD Item Merge Best Affix - Item Partial
// =============================================================================
//   exposes the private _affixProperties list on Item so that the merge logic
//   in Player.ModItemMergeBestAffix.cs can read rolled affix values for
//   eligibility checks, dry-run comparisons, and JSON dumps.
//
//   Item is declared as partial class in Item.cs:51, so we add a single
//   read-only accessor here without modifying the core file.
//
//  VERSION:: 20260805
// =============================================================================

using System.Collections.Generic;
using MHServerEmu.Games.Entities.Items;

namespace MHServerEmu.Games.Entities.Items
{
    public partial class Item
    {
        /// <summary>
        /// Read-only access to the rolled affix property entries.
        /// Used by the Item Merge Best Affix mod to compare affix values and
        /// build JSON diagnostic dumps. The merge itself no longer modifies
        /// affix values in-place - it searches for a seed that naturally
        /// produces near the desired values instead.
        /// </summary>
        public IReadOnlyList<AffixPropertiesCopyEntry> AffixPropertiesEntries => _affixProperties;

        #endregion Item Merge Best Affix
    }
}
