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

        protected override void OnEntered()
        {
            // Await is not needed since there is nothing done afterward.
            GD.Print("[ShopState] Fading out level music.");
            MusicManager.Instance.FadeOutCurrentSong(5.0f);
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            // Only executed when the shop menu is removed. This prevents
            // the music from fading in/out when pause menu is
            // opened/closed.
            if (!keepLoaded)
            {
                // Await is not needed since there is nothing done afterward.
                GD.Print("[ShopState] Fading in level music.");
                MusicManager.Instance.FadeInCurrentSong(1.0f);
            }
        }
    }
}
