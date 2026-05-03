// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;
using GDCollections = Godot.Collections;
using GenSysCollections = System.Collections.Generic;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Handles upgrading the players weapons etc.
    /// </summary>
    public class PlayerUpgradeHandler
    {
        private GenSysCollections.Dictionary<WeaponType, PlayerWeaponGroupController> _weaponControllers = null;

        public PlayerUpgradeHandler()
        {
            _weaponControllers = new GenSysCollections.Dictionary<WeaponType, PlayerWeaponGroupController>();
        }

        /// <summary>
        ///  Registers the given weapon controller to the upgrade handler.
        /// </summary>
        ///
        /// <param name="controller">
        ///  Weapon controller to register.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if registration was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool RegisterWeaponController(PlayerWeaponGroupController controller)
        {
            var weaponType = controller.WeaponType;
            var success = _weaponControllers.TryAdd(weaponType, controller);

            if (!success)
            {
                GD.PushError($"Cannot add weapon controller for type '{weaponType}': already added?");
            }

            return success;
        }

        /// <summary>
        ///  Unregisters the given weapon controller from the upgrade handler.
        /// </summary>
        ///
        /// <param name="controller">
        ///  Weapon controller to unregister.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if registration was successfully removed,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool UnregisterWeaponController(PlayerWeaponGroupController controller)
        {
            var weaponType = controller.WeaponType;
            var success = _weaponControllers.Remove(weaponType);

            if (!success)
            {
                GD.PushError($"Cannot remove weapon controller for type '{weaponType}': not found.");
            }

            return success;
        }

        /// <summary>
        ///  Upgrades the given weapon controller if possible. If the upgrade
        ///  is performed successfully, records the new number of weapons to
        ///  <see cref="PlayerData"/>.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Type of the weapon controller to upgrade.
        /// </param>
        /// <param name="upgradeType">
        ///  Type of the upgrade to perform.
        /// </param>
        /// <param name="weaponUpgradeResult">
        ///  The result of the upgrade. Use this if different actions are
        ///  needed for different fail conditions.
        /// </param>
        /// <param name="ignorePrice">
        ///  Debug feature. Set this to true to allow purchases even if the
        ///  player doesn't have enough money.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool UpgradeWeapon(
            WeaponType weaponType,
            UpgradeType upgradeType,
            out WeaponUpgradeResult weaponUpgradeResult,
            bool ignorePrice = false
        )
        {
            // Get the controller that matches the given type.
            if (!_weaponControllers.TryGetValue(weaponType, out var weaponController))
            {
                this.LogError($"Failed to upgrade '{weaponType}': controller not found!");
                weaponUpgradeResult = WeaponUpgradeResult.None;
                return false;
            }

            // Get price info for the upgrade.
            var priceInfo = weaponController.GetPrice(upgradeType);

            if (priceInfo == null)
            {
                this.LogError($"Failed to upgrade '{weaponType}': Price undefined!");
                weaponUpgradeResult = WeaponUpgradeResult.None;
                return false;
            }

            // Check if the player has enough money to perform the upgrade.
            var playerData = GameManager.Instance.CurrentPlayerData;

            if (!ignorePrice)
            {
                // Check if the player has enough money to buy the upgrade.
                var currentAmount = playerData.GetCollectibleCount(priceInfo.RequiredItem);
                var hasEnoughMoney = currentAmount >= priceInfo.RequiredAmount;

                if (!hasEnoughMoney)
                {
                    weaponUpgradeResult = WeaponUpgradeResult.FailedNoMoney;
                    return false;
                }
            }

            if (!weaponController.Upgrade(upgradeType))
            {
                // The given upgrade is already maxed out.
                weaponUpgradeResult = WeaponUpgradeResult.FailedNoSlots;
                return false;
            }

            if (!ignorePrice)
            {
                playerData.DecreaseCollectibleCount(priceInfo.RequiredItem, priceInfo.RequiredAmount);
            }

            weaponUpgradeResult = WeaponUpgradeResult.Success;
            return true;
        }

        /// <summary>
        ///  Downgrades the given weapon controller if possible. If the
        ///  downgrade is performed successfully, records the new number of
        ///  weapons to <see cref="PlayerData"/>.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Type of the weapon controller to downgrade.
        /// </param>
        /// <param name="upgradeType">
        ///  Type of the downgrade to be performed.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool DowngradeWeapon(WeaponType weaponType, UpgradeType upgradeType)
        {
            if (!_weaponControllers.TryGetValue(weaponType, out var weaponController))
            {
                this.LogError($"Failed to downgrade '{weaponType}': controller not found!");
                return false;
            }

            return weaponController.Downgrade(upgradeType);
        }

        /// <summary>
        ///  Initializes the player's weapon controllers by setting the number
        ///  of weapons in each one of them.
        /// </summary>
        ///
        /// <param name="weaponCounts">
        ///  Dictionary containing the counts for each weapon type.
        /// </param>
        public void InitializeWeaponCounts(
            GDCollections.Dictionary<WeaponType, int> weaponCounts,
            GDCollections.Dictionary<WeaponType, int> secondaryUpgradeCounts
        )
        {
            foreach (var (type, controller) in _weaponControllers)
            {
                var primaryCount = weaponCounts[type];
                var secondaryCount = secondaryUpgradeCounts[type];
                controller.Initialize(primaryCount, secondaryCount);
            }
        }
    }
}
