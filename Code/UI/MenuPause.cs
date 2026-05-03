// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Config;
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

        /// <inheritdoc />
        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                OnBtnResumePressed();
            }

            if (Input.IsActionJustPressed(ControlConfig.PAUSE_GAME))
            {
                OnBtnResumePressed();
            }
        }

        public override void _EnterTree()
        {
            base._EnterTree();

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
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.StateMachine.TransitionToPrevious();
        }

        private void OnBtnSettingsPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.StateMachine.TransitionTo(StateType.SettingsMenu);
        }

        private void OnBtnMainMenuPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.SaveGame();
            GameManager.Instance.StateMachine.TransitionTo(StateType.MainMenu);
        }

        private void OnBtnQuitPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer2.Play();
            GameManager.Instance.SaveAndQuit();
        }
    }
}
