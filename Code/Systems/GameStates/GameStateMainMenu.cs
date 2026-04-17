// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.States
{
    public class GameStateMainMenu : GameState
    {
        public override StateType StateType => StateType.MainMenu;
        public override StringName ScenePath => "res://Scenes/UI/MenuMain.tscn";

        public GameStateMainMenu()
        {
            AddTargetState(StateType.SettingsMenu);
            AddTargetState(StateType.Round);
        }

        /// <summary>
        /// Switches the level to the background level if necessary.
        /// </summary>
        protected override void OnEntered()
        {
            base.OnEntered();

            var levelManager = LevelManager.Active;
            var targetLevelType = LevelType.Background;
            MusicManager.Instance.KillAllMusic();
            MusicManager.Instance.MainThemePlayer.VolumeDb = 0f;
            MusicManager.Instance.PlayMusic(MusicManager.Instance.MainThemePlayer, MusicManager.Song.MainTheme);

            if (levelManager == null || levelManager.LevelType != targetLevelType)
            {
                GameManager.Instance.SwitchToLevelType(targetLevelType);
            }
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            base.OnExited(keepLoaded);
            MusicManager.Instance.FadeOutPlayer(MusicManager.Instance.MainThemePlayer);
        }
    }
}
