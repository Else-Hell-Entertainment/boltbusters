// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
using EHE.BoltBusters.Systems;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// WIP. Only dummy implementation with the minimum necessary logic to test
    /// the GameState class.
    /// </summary>
    public partial class GameManager : Node
    {
        private SceneTree _sceneTree = null;

        public static GameManager Instance = null;

        public SceneTree SceneTree
        {
            get
            {
                if (_sceneTree == null)
                {
                    _sceneTree = GetTree();
                }

                return _sceneTree;
            }
        }

        public GameloopStateMachine StateMachine { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            StateMachine = new GameloopStateMachine(new GameStateMenuMain(), new GameStateMenuSettings(), new GameStatePaused());
            ProcessMode = ProcessModeEnum.Always;
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void Pause()
        {
            SceneTree.Paused = true;
        }

        /// <summary>
        /// Unpauses the game.
        /// </summary>
        public void Resume()
        {
            SceneTree.Paused = false;
        }

        /// <summary>
        /// Toggles the pause state of the game.
        /// </summary>
        /// <returns>
        ///  <c>true</c> if the game is currently paused,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool TogglePaused()
        {
            SceneTree.Paused = SceneTree.Paused;
            return SceneTree.Paused;
        }
    }
}
