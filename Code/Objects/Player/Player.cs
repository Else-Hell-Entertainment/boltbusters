using System;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        private PlayerChaingunController _chaingunController;
        private PlayerRailgunController _railgunController;
        private PlayerRocketLauncherController _rocketLauncherController;
        private PlayerUpgradeHandler _upgradeHandler;

        /// <summary>
        /// (Parent) _EnterTree runs first and is good for registering and subscribing to services.
        /// </summary>
        public override void _EnterTree()
        {
            if (TargetProvider.Instance == null)
            {
                GD.PushWarning($"Player: TargetProvider.Instance is null. Player was not registered.");
                return;
            }

            TargetProvider.Instance.RegisterPlayer(this);

            GameManager.Instance.RequestWeaponUpgrade += OnWeaponUpgradeRequested;
            GameManager.Instance.RequestWeaponDowngrade += OnWeaponDowngradeRequested;
        }

        public override void _Input(InputEvent inputEvent)
        {
#if DEBUG
            if (inputEvent.IsActionPressed("DebugDowngradeChaingun"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Chaingun);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeChaingun"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Chaingun);
            }

            if (inputEvent.IsActionPressed("DebugDowngradeRailgun"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Railgun);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeRailgun"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Railgun);
            }

            if (inputEvent.IsActionPressed("DebugDowngradeMissile"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Rocket);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeMissile"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Rocket);
            }
#endif
        }

        public override void _Ready()
        {
            _chaingunController = this.GetFirstChildOfType<PlayerChaingunController>(true);
            _railgunController = this.GetFirstChildOfType<PlayerRailgunController>(true);
            _rocketLauncherController = this.GetFirstChildOfType<PlayerRocketLauncherController>(true);
            _upgradeHandler = new PlayerUpgradeHandler();
            _upgradeHandler.RegisterWeaponController(_chaingunController);
            _upgradeHandler.RegisterWeaponController(_railgunController);
            _upgradeHandler.RegisterWeaponController(_rocketLauncherController);
        }

        /// <summary>
        /// Remove player from TargetProvider when exiting tree.
        /// </summary>
        public override void _ExitTree()
        {
            TargetProvider.Instance?.UnregisterPlayer(this);

            GameManager.Instance.RequestWeaponUpgrade -= OnWeaponUpgradeRequested;
            GameManager.Instance.RequestWeaponDowngrade -= OnWeaponDowngradeRequested;
        }

        public override void TakeDamage(DamageData damageData)
        {
            base.TakeDamage(damageData);
            GD.Print("Aaaa I'm taking damage! ");
        }

        public override void OnSpawn() { }

        public override void OnDespawn() { }

        /// <summary>
        ///  Handles the <see cref="GameManager.RequestWeaponUpgrade"/> event.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Integer representation of the weapon type to upgrade.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool OnWeaponUpgradeRequested(int weaponType)
        {
            return _upgradeHandler.UpgradeWeapon((WeaponType)weaponType);
        }

        /// <summary>
        ///  Handles the <see cref="GameManager.RequestWeaponDowngrade"/> event.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Integer representation of the weapon type to downgrade.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool OnWeaponDowngradeRequested(int weaponType)
        {
            return _upgradeHandler.DowngradeWeapon((WeaponType)weaponType);
        }
    }
}
