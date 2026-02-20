// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuSettings : Menu
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
