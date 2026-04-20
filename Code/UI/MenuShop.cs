// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuShop : Menu
    {
        [Export]
        private Button _btnUpgradeChaingun;

        [Export]
        private Button _btnUpgradeRailgun;

        [Export]
        private Button _btnUpgradeRocketLauncher;

        [Export]
        private Button _btnDowngradeChaingun;

        [Export]
        private Button _btnDowngradeRailgun;

        [Export]
        private Button _btnDowngradeRocketLauncher;

        [Export]
        private Button _btnEnterNextRound;

        [Export]
        private ToastLabel _toastLabel;

        public override void _EnterTree()
        {
            _btnUpgradeChaingun.Pressed += OnBtnUpgradeChaingunPressed;
            _btnUpgradeRailgun.Pressed += OnBtnUpgradeRailgunPressed;
            _btnUpgradeRocketLauncher.Pressed += OnBtnUpgradeRocketLauncherPressed;
            _btnDowngradeChaingun.Pressed += OnBtnDowngradeChaingunPressed;
            _btnDowngradeRailgun.Pressed += OnBtnDowngradeRailgunPressed;
            _btnDowngradeRocketLauncher.Pressed += OnBtnDowngradeRocketLauncherPressed;
            _btnEnterNextRound.Pressed += OnBtnEnterNextRoundPressed;

            GameManager.Instance.WeaponUpgradeSucceeded += OnWeaponUpgradeSucceeded;
            GameManager.Instance.WeaponUpgradeFailed += OnWeaponUpgradeFailed;
        }

        public override void _ExitTree()
        {
            _btnUpgradeChaingun.Pressed -= OnBtnUpgradeChaingunPressed;
            _btnUpgradeRailgun.Pressed -= OnBtnUpgradeRailgunPressed;
            _btnUpgradeRocketLauncher.Pressed -= OnBtnUpgradeRocketLauncherPressed;
            _btnDowngradeChaingun.Pressed -= OnBtnDowngradeChaingunPressed;
            _btnDowngradeRailgun.Pressed -= OnBtnDowngradeRailgunPressed;
            _btnDowngradeRocketLauncher.Pressed -= OnBtnDowngradeRocketLauncherPressed;
            _btnEnterNextRound.Pressed -= OnBtnEnterNextRoundPressed;

            GameManager.Instance.WeaponUpgradeSucceeded -= OnWeaponUpgradeSucceeded;
            GameManager.Instance.WeaponUpgradeFailed -= OnWeaponUpgradeFailed;
        }

        private void RequestWeaponUpgrade(WeaponType weaponType)
        {
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestWeaponUpgrade, (int)weaponType);
        }

        private void RequestWeaponDowngrade(WeaponType weaponType)
        {
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestWeaponDowngrade, (int)weaponType);
        }

        private void OnBtnUpgradeChaingunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Chaingun);
        }

        private void OnBtnUpgradeRailgunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Railgun);
        }

        private void OnBtnUpgradeRocketLauncherPressed()
        {
            RequestWeaponUpgrade(WeaponType.Rocket);
        }

        private void OnBtnDowngradeChaingunPressed()
        {
            RequestWeaponDowngrade(WeaponType.Chaingun);
        }

        private void OnBtnDowngradeRailgunPressed()
        {
            RequestWeaponDowngrade(WeaponType.Railgun);
        }

        private void OnBtnDowngradeRocketLauncherPressed()
        {
            RequestWeaponDowngrade(WeaponType.Rocket);
        }

        private void OnBtnEnterNextRoundPressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.Round);
            LevelManager.Active.InitializeLevel(GameManager.Instance.RoundIndex);
            GameManager.Instance.SceneTree.CreateTimer(2).Timeout += LevelManager.Active.StartRound;
        }

        private void OnWeaponUpgradeSucceeded(int weaponType)
        {
            _toastLabel.Text = "Weapon upgraded!";
            _toastLabel.Toast();
        }

        private void OnWeaponUpgradeFailed(int weaponType, int reason)
        {
            switch ((WeaponUpgradeResult)reason)
            {
                case WeaponUpgradeResult.FailedNoMoney:
                    _toastLabel.Text = "Not enough money!";
                    break;
                case WeaponUpgradeResult.FailedNoSlots:
                    _toastLabel.Text = "Not enough slots!";
                    break;
                default:
                    _toastLabel.Text = "Cannot upgrade this weapon right now!";
                    break;
            }

            _toastLabel.Toast();
        }
    }
}
