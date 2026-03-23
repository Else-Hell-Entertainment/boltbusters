// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class ShopState : GameState
    {
        public override StateType StateType => StateType.Shop;
        public override StringName ScenePath => "res://Scenes/UI/Shop.tscn";
        public override bool IsAdditive => true;

        public ShopState()
        {
            AddTargetState(StateType.Paused);
            AddTargetState(StateType.Round);
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            GameManager.Instance.CurrentPlayerData.IsLevelCleared = false;
            GameManager.Instance.SaveGame();
        }
    }
}
