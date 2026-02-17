using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuSettings : Control
    {
        [Export]
        private Button _btnBack;

        public override void _EnterTree()
        {
            _btnBack.Pressed += OnBtnBackPressed;
        }

        public override void _ExitTree()
        {
            _btnBack.Pressed -= OnBtnBackPressed;
        }

        private void OnBtnBackPressed()
        {
            GameManager.Instance.StateMachine.TransitionToPrevious();
        }
    }
}
