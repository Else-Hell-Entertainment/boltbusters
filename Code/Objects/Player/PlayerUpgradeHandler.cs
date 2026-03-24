// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Handles upgrading the players weapons etc.
    /// </summary>
    public class PlayerUpgradeHandler
    {
        private Dictionary<WeaponType, IUpgradeable> _weaponControllers = null;

        public PlayerUpgradeHandler()
        {
            _weaponControllers = new Dictionary<WeaponType, IUpgradeable>();
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
        ///  Upgrades the given weapon controller if possible.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Type of the weapon controller to upgrade.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool UpgradeWeapon(WeaponType weaponType)
        {
            if (!_weaponControllers.TryGetValue(weaponType, out var weaponController))
            {
                GD.PushWarning($"Cannot upgrade weapon controller for type '{weaponType}': not found.");
                return false;
            }

            return weaponController.Upgrade();
        }

        /// <summary>
        ///  Downgrades the given weapon controller if possible.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Type of the weapon controller to downgrade.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool DowngradeWeapon(WeaponType weaponType)
        {
            if (!_weaponControllers.TryGetValue(weaponType, out var weaponController))
            {
                GD.PushWarning($"Cannot downgrade weapon controller for type '{weaponType}': not found.");
                return false;
            }

            return weaponController.Downgrade();
        }
    }
}
