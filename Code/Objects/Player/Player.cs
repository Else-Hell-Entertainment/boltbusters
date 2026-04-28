// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//            Pekka Heljakka <pekka.heljakka@tuni.fi>
//            TimeForNano <tuominen.mika-95@hotmail.com>

using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        /// <summary>
        ///  Emitted when the <see cref="HandleDeath"/> method of the
        ///  <see cref="Player"/> is called.
        /// </summary>
        ///
        /// <param name="player">
        ///  Reference to the player object that died.
        /// </param>
        [Signal]
        public delegate void PlayerDiedEventHandler(Player player);

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
            // Primary upgrades
            // Chaingun
            if (inputEvent.IsActionPressed("DebugDowngradeChaingun"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Chaingun, UpgradeType.Primary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeChaingun"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Chaingun, UpgradeType.Primary, out _, true);
            }

            // Railgun
            if (inputEvent.IsActionPressed("DebugDowngradeRailgun"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Railgun, UpgradeType.Primary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeRailgun"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Railgun, UpgradeType.Primary, out _, true);
            }

            // Rocket Launcher
            if (inputEvent.IsActionPressed("DebugDowngradeMissile"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Rocket, UpgradeType.Primary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeMissile"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Rocket, UpgradeType.Primary, out _, true);
            }

            // Secondary upgrades
            // Chaingun
            if (inputEvent.IsActionPressed("DebugDowngradeChaingunSecondary"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Chaingun, UpgradeType.Secondary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeChaingunSecondary"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Chaingun, UpgradeType.Secondary, out _, true);
            }

            // Railgun
            if (inputEvent.IsActionPressed("DebugDowngradeRailgunSecondary"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Railgun, UpgradeType.Secondary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeRailgunSecondary"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Railgun, UpgradeType.Secondary, out _, true);
            }

            // Rocket Launcher
            if (inputEvent.IsActionPressed("DebugDowngradeMissileSecondary"))
            {
                _upgradeHandler.DowngradeWeapon(WeaponType.Rocket, UpgradeType.Secondary);
            }
            else if (inputEvent.IsActionPressed("DebugUpgradeMissileSecondary"))
            {
                _upgradeHandler.UpgradeWeapon(WeaponType.Rocket, UpgradeType.Secondary, out _, true);
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
            GameManager.Instance.RoundStateChanged += OnRoundStateChanged;
        }

        // TODO: Convert this to a public Reset method that can be called from LevelManager.
        private void OnRoundStateChanged(bool inProgress)
        {
            if (!inProgress)
            {
                ResetWeapons();
                HealthComponent.RestoreToInitial();
            }
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

        public override void HandleDeath()
        {
            MusicManager.Instance.PlayPlayerDeathSound();
            EmitSignal(SignalName.PlayerDied, this);
            // OnDespawn();
        }

        // Add additional logic if it differs from default (Node.QueueFree) method.
        public override void OnDespawn()
        {
            base.OnDespawn();
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
            // TODO: Move these to a Reset method?
            HealthComponent.RestoreToInitial();
            _upgradeHandler.InitializeWeaponCounts(
                playerData.GetWeaponCounts(),
                playerData.GetSecondaryUpgradeCounts()
            );
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

        public void ResetWeapons()
        {
            RailgunController.ResetWeapons();
            RocketLauncherController.ResetWeapons();
            ChaingunController.ResetWeapons();
        }

        /// <summary>
        ///  Handles the <see cref="GameManager.RequestWeaponUpgrade"/> event.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Integer representation of the weapon type to upgrade.
        /// </param>
        /// <param name="upgradeType">
        ///  Integer representation of the upgrade type to perform.
        ///  Must be castable to <see cref="UpgradeType"/>!
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool OnWeaponUpgradeRequested(int weaponType, int upgradeType)
        {
            var isSuccess = _upgradeHandler.UpgradeWeapon(
                (WeaponType)weaponType,
                (UpgradeType)upgradeType,
                out var upgradeResult
            );

            if (isSuccess)
            {
                GameManager.Instance.EmitSignal(
                    GameManager.SignalName.WeaponUpgradeSucceeded,
                    weaponType,
                    (int)upgradeResult
                );
            }
            else
            {
                GameManager.Instance.EmitSignal(
                    GameManager.SignalName.WeaponUpgradeFailed,
                    weaponType,
                    (int)upgradeResult
                );
            }

            return isSuccess;
        }

        /// <summary>
        ///  Handles the <see cref="GameManager.RequestWeaponDowngrade"/> event.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Integer representation of the weapon type to downgrade.
        /// </param>
        /// <param name="upgradeType">
        ///  Integer representation of the downgrade type to be performed.
        ///  Must be castable to <see cref="UpgradeType"/>!
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool OnWeaponDowngradeRequested(int weaponType, int upgradeType)
        {
            return _upgradeHandler.DowngradeWeapon((WeaponType)weaponType, (UpgradeType)upgradeType);
        }
    }
}
