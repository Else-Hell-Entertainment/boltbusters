// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Rihu, Miska <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameOverState : GameState
    {
        /// <inheritdoc />
        public override StateType StateType => StateType.GameOver;

        /// <inheritdoc />
        public override StringName ScenePath => "res://Scenes/UI/MenuGameOver.tscn";

        /// <inheritdoc />
        public override bool IsAdditive => true;

        public GameOverState()
        {
            AddTargetState(StateType.MainMenu);
            AddTargetState(StateType.SettingsMenu);
            AddTargetState(StateType.Round);
        }
    }
}
