// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <email>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameStatePaused : GameState
    {
        public override StateType StateType => StateType.Paused;
        public override StringName ScenePath => "res://Scenes/UI/MenuPause.tscn";
        public override bool IsAdditive => true;

        public GameStatePaused()
        {
            AddTargetState(StateType.Round);
            AddTargetState(StateType.Shop);
            AddTargetState(StateType.SettingsMenu);
            AddTargetState(StateType.MainMenu);
        }

        protected override void OnEntered()
        {
            base.OnEntered();
            GameManager.Instance.Pause();
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            // TODO: Make sure this works when going from pause menu to settings menu!
            base.OnExited(keepLoaded);

            if (!keepLoaded)
            {
                GameManager.Instance.Resume();
            }
        }
    }
}
