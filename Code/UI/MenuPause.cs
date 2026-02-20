// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuPause : Menu
    {
        [Export]
        private Button _btnResume;

        [Export]
        private Button _btnSettings;

        [Export]
        private Button _btnMainMenu;

        [Export]
        private Button _btnQuit;

        public override void _EnterTree()
        {
            _btnResume.Pressed += OnBtnResumePressed;
            _btnSettings.Pressed += OnBtnSettingsPressed;
            _btnMainMenu.Pressed += OnBtnMainMenuPressed;
            _btnQuit.Pressed += OnBtnQuitPressed;
        }

        public override void _ExitTree()
        {
            _btnResume.Pressed -= OnBtnResumePressed;
            _btnSettings.Pressed -= OnBtnSettingsPressed;
            _btnMainMenu.Pressed -= OnBtnMainMenuPressed;
            _btnQuit.Pressed -= OnBtnQuitPressed;
        }

        private void OnBtnResumePressed()
        {
            GameManager.Instance.StateMachine.TransitionToPrevious();
        }

        private void OnBtnSettingsPressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.SettingsMenu);
        }

        private void OnBtnMainMenuPressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.MainMenu);
        }

        private void OnBtnQuitPressed()
        {
            // TODO: Move this to Game Manager!
            GetTree().Quit();
        }
    }
}
