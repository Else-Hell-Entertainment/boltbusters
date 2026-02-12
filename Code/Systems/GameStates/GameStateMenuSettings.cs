// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameStateMenuSettings : GameState
    {
        public override StateType StateType => StateType.MenuSettings;
        public override StringName ScenePath => "res://Scenes/UI/MenuSettings.tscn";
        public override bool IsAdditive => true;

        public GameStateMenuSettings()
        {
            AddTargetState(StateType.MenuMain);
        }

        protected override void OnEntered()
        {
            base.OnEntered();
        }

        protected override void OnExited()
        {
            base.OnExited();
        }
    }
}
