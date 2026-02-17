// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameStateRound : GameState
    {
        public override StateType StateType => StateType.Round;
        public override StringName ScenePath => "res://Scenes/UI/HUD.tscn";

        public GameStateRound()
        {
            AddTargetState(StateType.MenuPause);
            AddTargetState(StateType.Shop);
            AddTargetState(StateType.MenuGameOver);
        }

        protected override void OnEntered()
        {
            base.OnEntered();
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            base.OnExited(keepLoaded);
        }
    }
}
