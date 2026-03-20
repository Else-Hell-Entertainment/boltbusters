// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.BoltBusters
{
    /// <summary>
    ///  An interface for handling upgrading components.
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>
        ///  Use this method to perform upgrades.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool Upgrade();

        /// <summary>
        ///  Use this method to perform downgrades.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool Downgrade();
    }
}
