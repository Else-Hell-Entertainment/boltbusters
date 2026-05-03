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

        public override void _EnterTree()
        {
            _btnEnterNextRound.Pressed += OnBtnEnterNextRoundPressed;
            GameManager.Instance.WeaponUpgradeSucceeded += OnWeaponUpgradeSucceeded;
            GameManager.Instance.WeaponUpgradeFailed += OnWeaponUpgradeFailed;
            _animationPlayer.CallDeferred(AnimationPlayer.MethodName.Play, "slide_up");
        }

        public override void _ExitTree()
        {
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
    }
}
