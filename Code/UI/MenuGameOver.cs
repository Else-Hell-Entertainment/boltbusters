// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuGameOver : Menu
    {
        [Export]
        private Button _btnRestart;

        [Export]
        private Button _btnSettings;

        [Export]
        private Button _btnMainMenu;

        [Export]
        private Button _btnQuit;

        public override void _EnterTree()
        {
            base._EnterTree();

            _btnRestart.Pressed += OnBtnRestartPressed;
            _btnSettings.Pressed += OnBtnSettingsPressed;
            _btnMainMenu.Pressed += OnBtnMainMenuPressed;
            _btnQuit.Pressed += OnBtnQuitPressed;
        }

        public override void _ExitTree()
        {
            _btnRestart.Pressed -= OnBtnRestartPressed;
            _btnSettings.Pressed -= OnBtnSettingsPressed;
            _btnMainMenu.Pressed -= OnBtnMainMenuPressed;
            _btnQuit.Pressed -= OnBtnQuitPressed;
        }

        private void OnBtnRestartPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.LoadGame();
        }

        private void OnBtnSettingsPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.StateMachine.TransitionTo(StateType.SettingsMenu);
        }

        private void OnBtnMainMenuPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.StateMachine.TransitionTo(StateType.MainMenu);
        }

        private void OnBtnQuitPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer2.Play();
            GameManager.Instance.Quit();
        }
    }
}
