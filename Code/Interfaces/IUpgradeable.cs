// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  An interface for handling upgrading components.
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>
        ///  Price of the upgrade.
        /// </summary>
        [Obsolete("Use GetPrice and SetPrice with an internal data structure.")]
        PriceInfo PriceInfo { get; }

        /// <summary>
        ///  Returns the price for the given upgrade.
        /// </summary>
        ///
        /// <param name="upgradeType">
        ///  The upgrade whose price is to be queried.
        /// </param>
        ///
        /// <returns>
        ///  A <see cref="PriceInfo"/> object representing the price of the
        ///  given <see cref="UpgradeType"/>.
        /// </returns>
        PriceInfo GetPrice(UpgradeType upgradeType);

        /// <summary>
        ///  Sets the price for the given upgrade type.
        /// </summary>
        ///
        /// <param name="upgradeType">
        ///  The upgrade whose price is to be modified.
        /// </param>
        /// <param name="priceInfo">The new price for the upgrade.</param>
        ///
        /// <returns>
        ///  <c>true</c> if the price was set successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        bool SetPrice(UpgradeType upgradeType, PriceInfo priceInfo);

        /// <summary>
        ///  DEPRECATED! Use this method to perform upgrades.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        [Obsolete("Use Upgrade(UpgradeType).")]
        bool Upgrade();

        /// <summary>
        ///  Performs an upgrade of the given type.
        /// </summary>
        ///
        /// <param name="type">
        ///  The type of the upgrade to perform.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        bool Upgrade(UpgradeType type);

        /// <summary>
        ///  DEPRECATED! Use this method to perform downgrades.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        [Obsolete("Use Downgrade(UpgradeType).")]
        bool Downgrade();

        /// <summary>
        ///  Performs a downgrade of the given type.
        /// </summary>
        ///
        /// <param name="type">
        ///  The type of the downgrade to perform.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        bool Downgrade(UpgradeType type);
    }
}
