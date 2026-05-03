// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Tells how the upgrade went. Has different values for different fail
    ///  conditions.
    /// </summary>
    public enum WeaponUpgradeResult
    {
        /// <summary>
        ///  Used when upgrade was not performed due to an internal error.
        ///  For example, there was no valid weapon controller.
        /// </summary>
        None = 0,

        /// <summary>
        ///  The upgrade was performed successfully.
        /// </summary>
        Success,

        /// <summary>
        ///  The upgrade failed because the player did not have enough money.
        /// </summary>
        FailedNoMoney,

        /// <summary>
        ///  The upgrade failed because the weapon was already maxed out.
        /// </summary>
        FailedNoSlots,
    }
}
