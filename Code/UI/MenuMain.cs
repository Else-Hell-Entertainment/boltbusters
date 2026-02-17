using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuMain : Menu
    {
        [Export]
        private Button _btnNewGame;

        [Export]
        private Button _btnSettings;

        [Export]
        private Button _btnQuit;

        public override void _EnterTree()
        {
            _btnNewGame.Pressed += OnBtnNewGamePressed;
            _btnSettings.Pressed += OnBtnSettingsPressed;
            _btnQuit.Pressed += OnBtnQuitPressed;
        }

        public override void _ExitTree()
        {
            _btnNewGame.Pressed -= OnBtnNewGamePressed;
            _btnSettings.Pressed -= OnBtnSettingsPressed;
            _btnQuit.Pressed -= OnBtnQuitPressed;
        }

        private void OnBtnNewGamePressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.Round);
        }

        private void OnBtnSettingsPressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.SettingsMenu);
        }

        private void OnBtnQuitPressed()
        {
            GetTree().Quit();
        }
    }
}
