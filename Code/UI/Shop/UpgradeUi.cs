// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Rihu, Miska <email>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.Ui
{
    [GlobalClass]
    public partial class UpgradeUi : Control
    {
        private PriceInfo _primaryPrice = new() { RequiredItem = CollectibleType.None, RequiredAmount = -1 };
        private PriceInfo _secondaryPrice = new() { RequiredItem = CollectibleType.None, RequiredAmount = -1 };
        private PlayerData _playerData;

        [Export]
        private WeaponType _weaponType = WeaponType.None;

        [ExportGroup("Primary Upgrade")]
        [Export]
        private Label _lblCurrentPrimaryCount;

        [Export]
        private Label _lblMaxPrimaryCount;

        [Export]
        private Button _btnPrimaryUpgrade;

        [ExportGroup("Secondary Upgrade")]
        [Export]
        private Label _lblCurrentSecondaryCount;

        [Export]
        private Label _lblMaxSecondaryCount;

        [Export]
        private Button _btnSecondaryUpgrade;

        public override void _EnterTree()
        {
            // Set max upgrade counts and prices.
            // TODO:
            // Create a config resource for weapons that holds max weapon
            // counts and prices!
            var controller = LevelManager.Active?.Player?.GetWeaponController(_weaponType);

            if (controller != null)
            {
                _lblMaxPrimaryCount.Text = $"{controller.MaxWeaponCount}";
                _lblMaxSecondaryCount.Text = $"{controller.MaxSecondaryUpgradeCount}";
                _primaryPrice = controller.GetPrice(UpgradeType.Primary);
                _secondaryPrice = controller.GetPrice(UpgradeType.Secondary);
            }
            else
            {
                this.LogError($"Cannot fetch information for controller of type '{_weaponType}'!");
            }

            // Cache current player data and connect its signals.
            _playerData = GameManager.Instance?.CurrentPlayerData;

            if (_playerData != null)
            {
                _playerData.WeaponCountChanged += OnPrimaryUpgradeCountChanged;
                _playerData.SecondaryUpgradeCountChanged += OnSecondaryUpgradeCountChanged;

                // Initialize current upgrade counts.
                OnPrimaryUpgradeCountChanged((int)_weaponType, _playerData.GetWeaponCount(_weaponType));
                OnSecondaryUpgradeCountChanged((int)_weaponType, _playerData.GetSecondaryUpgradeCount(_weaponType));
            }
            else
            {
                this.LogError("Current player data is null!");
            }

            // Connect button signals.
            _btnPrimaryUpgrade.Pressed += OnBtnPrimaryUpgradePressed;
            _btnSecondaryUpgrade.Pressed += OnBtnSecondaryUpgradePressed;
        }

        /// <summary>
        ///  Disconnects signals.<br/>
        ///  <inheritdoc/>
        /// </summary>
        public override void _ExitTree()
        {
            if (_playerData != null)
            {
                _playerData.WeaponCountChanged -= OnPrimaryUpgradeCountChanged;
                _playerData.SecondaryUpgradeCountChanged -= OnSecondaryUpgradeCountChanged;
            }

            _btnPrimaryUpgrade.Pressed -= OnBtnPrimaryUpgradePressed;
            _btnSecondaryUpgrade.Pressed -= OnBtnSecondaryUpgradePressed;
        }

        private void RequestUpgrade(UpgradeType upgradeType)
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.EmitSignal(
                GameManager.SignalName.RequestWeaponUpgrade,
                (int)_weaponType,
                (int)upgradeType
            );
        }

        private void OnPrimaryUpgradeCountChanged(int weaponType, int newValue)
        {
            if ((WeaponType)weaponType == _weaponType)
            {
                _lblCurrentPrimaryCount.Text = $"{newValue}";
            }
        }

        private void OnSecondaryUpgradeCountChanged(int weaponType, int newValue)
        {
            if ((WeaponType)weaponType == _weaponType)
            {
                _lblCurrentSecondaryCount.Text = $"{newValue}";
            }
        }

        private void OnBtnPrimaryUpgradePressed()
        {
            RequestUpgrade(UpgradeType.Primary);
        }

        private void OnBtnSecondaryUpgradePressed()
        {
            RequestUpgrade(UpgradeType.Secondary);
        }
    }
}
