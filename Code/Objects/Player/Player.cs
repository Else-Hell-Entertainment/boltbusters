using System;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        [Export]
        private EntityController _playerController;
        public PlayerChaingunController ChaingunController { get; private set; }

        public PlayerRailgunController RailgunController { get; private set; }

        public PlayerRocketLauncherController RocketLauncherController { get; private set; }

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
            ChaingunController = this.GetFirstChildOfType<PlayerChaingunController>(true);
            RailgunController = this.GetFirstChildOfType<PlayerRailgunController>(true);
            RocketLauncherController = this.GetFirstChildOfType<PlayerRocketLauncherController>(true);
            _upgradeHandler = new PlayerUpgradeHandler();
            _upgradeHandler.RegisterWeaponController(ChaingunController);
            _upgradeHandler.RegisterWeaponController(RailgunController);
            _upgradeHandler.RegisterWeaponController(RocketLauncherController);

            // Signal to let other elements (mainly UI) know the player is now ready.
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
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
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
        }

        public override void OnSpawn() { }

        public override void OnDespawn()
        {
            QueueFree();
        }

        /// <summary>
        ///  Initializes the player using values from
        ///  <paramref name="playerData"/>.
        /// </summary>
        ///
        /// <param name="playerData">
        ///  Current player data.
        /// </param>
        public void Initialize(PlayerData playerData)
        {
            // TODO: Init HP.
            _upgradeHandler.InitializeWeaponCounts(playerData.GetWeaponCounts());
        }

        /// <summary>
        /// Toggle true/false to listen/ignore all inputs from InputHandler.
        /// </summary>
        /// <param name="isListening">Are player inputs being listened at all.</param>
        public void ToggleInputListening(bool isListening)
        {
            _playerController.AcceptCommands = isListening;
        }

        /// <summary>
        /// Is player currently taking in input commands at all.
        /// </summary>
        /// <returns>The assigned EntityController's AcceptCommand state.</returns>
        public bool IsPlayerListeningInput()
        {
            return _playerController.AcceptCommands;
        }

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
