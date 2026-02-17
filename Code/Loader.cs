// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//
// Based on Sami Kojo's implementation.
// Source:
// License: https://github.com/samikojo-tuni/GArkanoid-2025/blob/c53ac400cc9fbd8855bc59f907ab751aba83c205/LICENSE

using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Loader : Control
    {
        [Export]
        private StateType _initialState = StateType.MainMenu;

        public override void _Ready()
        {
            GameManager.Instance.StateMachine.TransitionTo(_initialState);
        }
    }
}
