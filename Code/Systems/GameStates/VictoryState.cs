// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Rihu, Miska <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class VictoryState : GameState
    {
        /// <inheritdoc />
        public override StateType StateType => StateType.Victory;

        /// <inheritdoc />
        public override StringName ScenePath => "res://Scenes/UI/Victory/MenuVictory.tscn";

        public VictoryState()
        {
            AddTargetState(StateType.MainMenu);
        }
    }
}
