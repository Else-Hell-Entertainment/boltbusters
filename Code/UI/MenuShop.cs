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
        private AnimationPlayer _animationPlayer;

        [Export]
        private Button _btnEnterNextRound;

        [Export]
        private ToastLabel _toastLabel;

        [ExportGroup("Primary Upgrade Buttons")]
        [Export]
        private Button _btnPrimaryUpgradeChaingun;

        [Export]
        private Button _btnPrimaryUpgradeRailgun;

        [Export]
        private Button _btnPrimaryUpgradeRocket;

        [ExportGroup("Secondary Upgrade Buttons")]
        [Export]
        private Button _btnSecondaryUpgradeChaingun;

        [Export]
        private Button _btnSecondaryUpgradeRailgun;

        [Export]
        private Button _btnSecondaryUpgradeRocket;

        public override void _EnterTree()
        {
            base._EnterTree();

            _btnPrimaryUpgradeChaingun.Pressed += OnBtnPrimaryUpgradeChaingunPressed;
            _btnPrimaryUpgradeRailgun.Pressed += OnBtnPrimaryUpgradeRailgunPressed;
            _btnPrimaryUpgradeRocket.Pressed += OnBtnPrimaryUpgradeRocketPressed;

            _btnSecondaryUpgradeChaingun.Pressed += OnBtnSecondaryUpgradeChaingunPressed;
            _btnSecondaryUpgradeRailgun.Pressed += OnBtnSecondaryUpgradeRailgunPressed;
            _btnSecondaryUpgradeRocket.Pressed += OnBtnSecondaryUpgradeRocketPressed;

            _btnEnterNextRound.Pressed += OnBtnEnterNextRoundPressed;

            GameManager.Instance.WeaponUpgradeSucceeded += OnWeaponUpgradeSucceeded;
            GameManager.Instance.WeaponUpgradeFailed += OnWeaponUpgradeFailed;

            _animationPlayer.CallDeferred(AnimationPlayer.MethodName.Play, "slide_up");
        }

        public override void _ExitTree()
        {
            _btnPrimaryUpgradeChaingun.Pressed -= OnBtnPrimaryUpgradeChaingunPressed;
            _btnPrimaryUpgradeRailgun.Pressed -= OnBtnPrimaryUpgradeRailgunPressed;
            _btnPrimaryUpgradeRocket.Pressed -= OnBtnPrimaryUpgradeRocketPressed;

            _btnSecondaryUpgradeChaingun.Pressed -= OnBtnSecondaryUpgradeChaingunPressed;
            _btnSecondaryUpgradeRailgun.Pressed -= OnBtnSecondaryUpgradeRailgunPressed;
            _btnSecondaryUpgradeRocket.Pressed -= OnBtnSecondaryUpgradeRocketPressed;

            _btnEnterNextRound.Pressed -= OnBtnEnterNextRoundPressed;

            GameManager.Instance.WeaponUpgradeSucceeded -= OnWeaponUpgradeSucceeded;
            GameManager.Instance.WeaponUpgradeFailed -= OnWeaponUpgradeFailed;
        }

        private async void OnBtnEnterNextRoundPressed()
        {
            _animationPlayer.PlayBackwards("slide_up");
            await ToSignal(_animationPlayer, AnimationMixer.SignalName.AnimationFinished);
            GameManager.Instance.StateMachine.TransitionTo(StateType.Round);
            LevelManager.Active.InitializeLevel(GameManager.Instance.RoundIndex);
            LevelManager.Active.StartRound();
        }

        private void OnWeaponUpgradeSucceeded(int weaponType)
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            _toastLabel.Text = "Weapon upgraded!";
            _toastLabel.Toast();
        }

        private void OnWeaponUpgradeFailed(int weaponType, int reason)
        {
            MusicManager.Instance.ButtonSoundPlayer2.Play();
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

        private void RequestWeaponUpgrade(WeaponType weaponType, UpgradeType upgradeType)
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.EmitSignal(
                GameManager.SignalName.RequestWeaponUpgrade,
                (int)weaponType,
                (int)upgradeType
            );
        }

        #region Primary Upgrades

        private void OnBtnPrimaryUpgradeChaingunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Chaingun, UpgradeType.Primary);
        }

        private void OnBtnPrimaryUpgradeRailgunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Railgun, UpgradeType.Primary);
        }

        private void OnBtnPrimaryUpgradeRocketPressed()
        {
            RequestWeaponUpgrade(WeaponType.Rocket, UpgradeType.Primary);
        }

        #endregion Primary Upgrades

        #region Secondary Upgrades

        private void OnBtnSecondaryUpgradeChaingunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Chaingun, UpgradeType.Secondary);
        }

        private void OnBtnSecondaryUpgradeRailgunPressed()
        {
            RequestWeaponUpgrade(WeaponType.Railgun, UpgradeType.Secondary);
        }

        private void OnBtnSecondaryUpgradeRocketPressed()
        {
            RequestWeaponUpgrade(WeaponType.Rocket, UpgradeType.Secondary);
        }

        #endregion Secondary Upgrades
    }
}
