// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <email>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameStatePaused : GameState
    {
        public override StateType StateType => StateType.MenuPause;
        public override StringName ScenePath => "res://Scenes/UI/MenuPause.tscn";
        public override bool IsAdditive => true;

        public GameStatePaused()
        {
            AddTargetState(StateType.Round);
            AddTargetState(StateType.Shop);
            AddTargetState(StateType.MenuSettings);
            AddTargetState(StateType.MenuMain);
        }

        protected override void OnEntered()
        {
            base.OnEntered();
            GameManager.Instance.Pause();
        }

        protected override void OnExited()
        {
            // TODO: Make sure this works when going from pause menu to settings menu!
            base.OnExited();
            GameManager.Instance.Resume();
        }
    }
}
