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
            AddTargetState(StateType.Paused);
            AddTargetState(StateType.Shop);
            AddTargetState(StateType.GameOver);
        }

        /// <summary>
        /// Switches the level to the gameplay level if necessary.
        /// </summary>
        protected override async void OnEntered()
        {
            base.OnEntered();

            var levelManager = LevelManager.Active;
            var targetLevelType = LevelType.Gameplay;
            var song = PickSong(GameManager.Instance.RoundIndex);

            if (levelManager == null || levelManager.LevelType != targetLevelType)
            {
                GameManager.Instance.SwitchToLevelType(targetLevelType);

                if (levelManager == null)
                {
                    // Instantly play music if this is the first level manager to be loaded.
                    GD.Print("[RoundState] Playing music instantly.");
                    MusicManager.Instance.PlaySong(song);
                }
                else
                {
                    // When transitioning to another level manager, fade out the music.
                    GD.Print("[RoundState] Stopping music with fade out.");
                    await MusicManager.Instance.StopCurrentSongWithFadeOut(5.0f);
                }
            }

            GD.Print("[RoundState] Playing music with fade in.");
            await MusicManager.Instance.PlaySongWithFadeIn(song, 1.0f);
        }

        protected override void OnExited(bool keepLoaded = false)
        {
            base.OnExited(keepLoaded);
        }

        private MusicManager.Song PickSong(int roundIndex)
        {
            var song = MusicManager.Song.MainTheme;

            switch (roundIndex % 4)
            {
                case 1:
                    song = MusicManager.Song.StageTheme1;
                    break;
                case 2:
                    song = MusicManager.Song.StageTheme2;
                    break;
                case 3:
                    song = MusicManager.Song.StageTheme3;
                    break;
                case 0:
                    song = MusicManager.Song.StageTheme4;
                    break;
            }

            return song;
        }
    }
}
