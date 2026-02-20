// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using EHE.BoltBusters.Config;
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
        #region Fields

        // Level-related stuff.
        private PackedScene _backgroundLevelScene;
        private PackedScene _gameplayLevelScene;
        private LevelManager _backgroundLevel;
        private LevelManager _gameplayLevel;
        private Dictionary<LevelType, PackedScene> _levelScenes;

        // Camera-related stuff.
        private SubViewportContainer _levelViewportContainer;
        private SubViewport _levelViewport;
        private CameraRig _cameraRig;

        // Other.
        private SceneTree _sceneTree;

        #endregion Fields


        #region Properties

        public static GameManager Instance { get; private set; }

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

        /// <summary>
        /// Reference to the global camera.
        /// </summary>
        public Camera3D Camera => _cameraRig.GetChild<Camera3D>(0);

        #endregion Properties


        #region Overrides

        public override void _EnterTree()
        {
            // Load level scenes into memory.
            _backgroundLevelScene = GD.Load<PackedScene>(SceneFileConfig.BACKGROUND_LEVEL_PATH);
            _gameplayLevelScene = GD.Load<PackedScene>(SceneFileConfig.GAMEPLAY_LEVEL_PATH);
            _levelScenes = new Dictionary<LevelType, PackedScene>
            {
                { LevelType.Background, _backgroundLevelScene },
                { LevelType.Gameplay, _gameplayLevelScene },
            };

            // Set up state machine.
            StateMachine = new GameloopStateMachine(
                new GameStateMainMenu(),
                new GameStateSettingsMenu(),
                new GameStateRound(),
                new GameStatePaused()
            );

            // All done.
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;
        }

        public override void _Ready()
        {
            CreateCamera();
        }

        /// <summary>
        /// Handles non-movements inputs that happen during gameplay.
        /// For example, pausing the game.
        /// </summary>
        /// <param name="inputEvent">Input event that occurred.</param>
        public override void _Input(InputEvent inputEvent)
        {
            if (
                inputEvent.IsActionPressed(ControlConfig.PAUSE_GAME)
                && StateMachine.CurrentState != null
                && StateMachine.CurrentState.CanTransitionTo(StateType.Paused)
            )
            {
                StateMachine.TransitionTo(StateType.Paused);
            }
        }

        #endregion Overrides


        #region Public Methods

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void StartNewGame()
        {
            StateMachine.TransitionTo(StateType.Round);
        }

        /// <summary>
        ///  <para>
        ///   Switches the active level by unloading the current level and
        ///   loading a new one.
        ///  </para>
        ///  <para>
        ///    When called, this method will instantiate a new level of the
        ///    specified <paramref name="levelType"/>, remove the currently
        ///    active level from the scene tree, and add the new level.
        ///  </para>
        /// </summary>
        ///
        /// <param name="levelType">
        ///  The type of level to load and switch to.
        /// </param>
        ///
        /// <remarks>
        ///  The scene tree operation is deferred, ensuring it occurs at the
        ///  end of the current frame.
        /// </remarks>
        public void SwitchToLevelType(LevelType levelType)
        {
            if (!_levelScenes.TryGetValue(levelType, out PackedScene levelResource))
            {
                GD.PushError($"Cannot switch levels: no level of type '{levelType}' was found.");
                return;
            }

            var levelScene = levelResource.InstantiateOrNull<LevelManager>();

            if (levelScene == null)
            {
                GD.PushError($"Failed to instantiate level scene from '{levelResource.ResourcePath}'.");
                return;
            }

            LevelManager.Active?.QueueFree();
            SceneTree.Root.CallDeferred(Node.MethodName.AddChild, levelScene);
        }

        #region Pause Control

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

        #endregion Pause Control

        #endregion Public Methods


        #region Private Methods

        // TODO: Refactor this and make the parameters editable in the editor.
        /// <summary>
        /// Instantiates the <see cref="CameraRig"/> from a file and adds it to
        /// the <see cref="SceneTree"/>.
        /// </summary>
        private void CreateCamera()
        {
            // Create container and assign shader material.
            _levelViewportContainer = new SubViewportContainer();
            _levelViewportContainer.Material = GD.Load<Material>(MaterialConfig.CAMERA_SHADER_MATERIAL_FILE);

            // Create viewport, set its size, and add it to the container.
            _levelViewport = new SubViewport();
            _levelViewport.Size = (Vector2I)GetViewport().GetWindow().GetVisibleRect().Size;
            _levelViewportContainer.CallDeferred(Node.MethodName.AddChild, _levelViewport);

            // Create camera rig and add it to the viewport.
            _cameraRig = GD.Load<PackedScene>(SceneFileConfig.CAMERA_FILE).Instantiate<CameraRig>();
            _cameraRig.HeightAboveGround = 10f;
            _cameraRig.UseSmoothFollow = false;
            _levelViewport.CallDeferred(Node.MethodName.AddChild, _cameraRig);

            // Add the container to the scene tree.
            SceneTree.Root.CallDeferred(Node.MethodName.AddChild, _levelViewportContainer);
        }

        #endregion Private Methods
    }
}
