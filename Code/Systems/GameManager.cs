// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//            Pekka Heljakka <pekka.heljakka@tuni.fi>

using System;
using System.Collections.Generic;
using EHE.BoltBusters.Config;
using EHE.BoltBusters.States;
using EHE.BoltBusters.Systems;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Manages the overall game state, level transitions, camera setup, and
    ///  game loop control.
    /// </summary>
    ///
    /// <remarks>
    ///  <para>
    ///   GameManager is a singleton that serves as the central hub for game
    ///   management. It handles:
    ///  </para>
    ///  <list type="bullet">
    ///   <item>Level loading and switching between different level types</item>
    ///   <item>Camera rig initialization and viewport management</item>
    ///   <item>Game state transitions via a finite state machine</item>
    ///   <item>Game pause/resume functionality</item>
    ///   <item>Input handling for pause actions</item>
    ///   <item>Game save and load operations</item>
    ///  </list>
    ///  <para>
    ///   The GameManager is a singleton accessible via the
    ///   <see cref="Instance"/> property, ensuring only one instance exists
    ///   throughout the game's lifetime. It is automatically initialized
    ///   during the tree entry phase and remains persistent across scene
    ///   changes.
    ///  </para>
    ///  <para>
    ///   <b>NOTE!</b> The GameManager is always processed regardless the
    ///   paused state of the scene tree.
    ///  </para>
    /// </remarks>
    ///
    /// <seealso cref="GameloopStateMachine"/>
    /// <seealso cref="LevelManager"/>
    /// <seealso cref="PlayerData"/>
    public partial class GameManager : Node
    {
        #region Signals

        /// <summary>
        ///  Emitted when the player chooses to upgrade a weapon in the shop.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Numerical representation of the weapon type to upgrade.
        /// </param>
        [Signal]
        public delegate bool RequestWeaponUpgradeEventHandler(int weaponType);

        /// <summary>
        ///  Emitted when the player chooses to downgrade a weapon in the shop.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Numerical representation of the weapon type to downgrade.
        /// </param>
        [Signal]
        public delegate bool RequestWeaponDowngradeEventHandler(int weaponType);

        /// <summary>
        ///  Emitted when the HUD should refresh its display.
        /// </summary>
        [Signal]
        public delegate void RequestHudRefreshEventHandler();

        #endregion Signals


        #region Fields

        // Level-related stuff.
        private PackedScene _backgroundLevelScene;
        private PackedScene _gameplayLevelScene;
        private LevelManager _backgroundLevel;
        private LevelManager _gameplayLevel;
        private SaveManager _saveManager;
        private Dictionary<LevelType, PackedScene> _levelScenes;

        // Camera-related stuff.
        private SubViewportContainer _levelViewportContainer;
        private SubViewport _levelViewport;
        private CameraRig _cameraRig;

        // Other.
        private SceneTree _sceneTree;

        #endregion Fields


        #region Properties

        /// <summary>
        ///  Reference to the GameManager singleton.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        ///  Reference to the SceneTree of the game.
        /// </summary>
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

        /// <summary>
        ///  A finite state machine responsible for controlling the transitions
        ///  between different <see cref="GameState"/>s.
        /// </summary>
        public GameloopStateMachine StateMachine { get; private set; }

        /// <summary>
        ///  Reference to the global camera.
        /// </summary>
        public Camera3D Camera => _cameraRig.GetChild<Camera3D>(0);

        /// <summary>
        ///  The index number for the current round.
        /// </summary>
        public int RoundIndex
        {
            get => CurrentPlayerData.LevelIndex;
            set => CurrentPlayerData.LevelIndex = value;
        }

        /// <summary>
        ///  Default player data values. Defined in the editor.
        /// </summary>
        public PlayerData DefaultPlayerData { get; private set; }

        /// <summary>
        ///  Player data for the current session.
        /// </summary>
        public PlayerData CurrentPlayerData { get; private set; }

        #endregion Properties


        #region Overrides

        /// <summary>
        ///  <inheritdoc/>
        /// </summary>
        ///
        /// <remarks>
        ///  This method loads level managers, sets up the state machine, initializes
        ///  the save manager, and loads default player data. The GameManager instance
        ///  is registered as a singleton at the end of this method.
        /// </remarks>
        public override void _EnterTree()
        {
            LoadLevelManagersIntoMemory();
            SetUpStateMachine();

            _saveManager = new SaveManager();
            DefaultPlayerData = GD.Load<PlayerData>(FilePathConfig.DEFAULT_PLAYER_DATA_RESOURCE_PATH);
            PlayerData.UpdateDefaultValues(DefaultPlayerData);

            // All done.
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;
        }

        /// <summary>
        ///  <inheritdoc/>
        /// </summary>
        ///
        /// <remarks>
        ///  Initializes the camera system when the node is ready.
        /// </remarks>
        public override void _Ready()
        {
            CreateCamera();
        }

        /// <summary>
        ///  Handles non-movements inputs that happen during gameplay.
        ///  For example, pausing the game.
        /// </summary>
        ///
        /// <param name="inputEvent">
        ///  Input event that occurred.
        /// </param>
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
#if DEBUG
            if (inputEvent.IsActionPressed("DebugSaveGame"))
            {
                SaveGame();
            }
#endif
        }

        #endregion Overrides


        #region Public Methods

        /// <summary>
        ///  Saves the current game state to a save file.
        /// </summary>
        ///
        /// <remarks>
        ///  WIP! Support for selecting the save slot is not implemented.
        ///  Currently, all saves are written to slot 0. If a save file exists,
        ///  it is overwritten.
        /// </remarks>
        public void SaveGame()
        {
            this.PrintDebug("Saving game...");

            var saveSlot = 0;
            var saveFile = string.Format(SaveConfig.SAVE_FILE_PATH_FORMAT, saveSlot);

            var saveData = new Godot.Collections.Dictionary();
            var playerData = CurrentPlayerData.Save();
            //var levelData = LevelManager.Active.Save();

            saveData.Add(SaveConfig.KEY_PLAYER_DATA, playerData);
            //saveData.Add(SaveConfig.KEY_LEVEL_DATA, levelData);

            if (!_saveManager.WriteToFile(saveFile, saveData))
            {
                GD.PushError("Failed to save the game.");
                return;
            }

            this.PrintDebug($"Game saved successfully to '{saveFile}'");
        }

        /// <summary>
        ///  Loads a previously saved game state from a save file and resumes
        ///  gameplay.
        /// </summary>
        ///
        /// <remarks>
        ///  WIP! Support for selecting the save slot is not implemented.
        ///  Currently, all saves are read from slot 0.
        /// </remarks>
        public void LoadGame()
        {
            this.PrintDebug("Loading game...");

            var saveSlot = 0;
            var saveFile = string.Format(SaveConfig.SAVE_FILE_PATH_FORMAT, saveSlot);
            var saveData = _saveManager.ReadFromFile(saveFile);

            if (saveData == null)
            {
                GD.PushError("Failed to load the game.");
                return;
            }

            if (!saveData.TryGetValue(SaveConfig.KEY_PLAYER_DATA, out var playerData))
            {
                GD.PushError("Failed to load player data.");
                return;
            }

            CurrentPlayerData = (PlayerData)DefaultPlayerData.Duplicate(deep: true);
            CurrentPlayerData.Load((Godot.Collections.Dictionary)playerData);

            this.PrintDebug("Game loaded successfully.");

            StartGame();
        }

        /// <summary>
        ///  Starts a new game.
        /// </summary>
        ///
        /// <seealso cref="StartFromRound"/>
        /// <seealso cref="StartFromShop"/>
        /// <seealso cref="OnLevelStartDelayTimeout"/>
        public void StartNewGame()
        {
            // IMPORTANT!
            // Creating a timer and linking it directly to a method call that
            // starts the round cannot be done here because connecting the
            // signal seems to pass the references that are valid during this
            // frame. E.g., the linked method would call the StartRound method
            // on the background level that is no longer present. The timer is
            // therefore created in the StartFromRound method and the timeout
            // is connected to the OnLevelStartDelayTimeout method.

            CurrentPlayerData = (PlayerData)DefaultPlayerData.Duplicate(deep: true);
            this.PrintDebug("Starting new game...");
            StartGame();
        }

        /// <summary>
        ///  Quits the game.
        /// </summary>
        public void Quit()
        {
            this.PrintDebug("Quitting game.");
            GetTree().Quit();
        }

        /// <summary>
        ///  Saves and quits the game.
        /// </summary>
        public void SaveAndQuit()
        {
            SaveGame();
            Quit();
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
        ///  Pauses the game.
        /// </summary>
        public void Pause()
        {
            SceneTree.Paused = true;
        }

        /// <summary>
        ///  Unpauses the game.
        /// </summary>
        public void Resume()
        {
            SceneTree.Paused = false;
        }

        /// <summary>
        ///  Toggles the pause state of the game.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if the game is currently paused after toggling,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool TogglePaused()
        {
            SceneTree.Paused = !SceneTree.Paused;
            return SceneTree.Paused;
        }

        #endregion Pause Control

        #endregion Public Methods


        #region Private Methods

        /// <summary>
        ///  Loads level scene resources from disk into memory and creates a
        ///  mapping of level types to their corresponding
        ///  <see cref="PackedScene"/> resources for fast lookup during level
        ///  transitions.
        /// </summary>
        private void LoadLevelManagersIntoMemory()
        {
            _backgroundLevelScene = GD.Load<PackedScene>(SceneFileConfig.BACKGROUND_LEVEL_PATH);
            _gameplayLevelScene = GD.Load<PackedScene>(SceneFileConfig.GAMEPLAY_LEVEL_PATH);
            _levelScenes = new Dictionary<LevelType, PackedScene>
            {
                { LevelType.Background, _backgroundLevelScene },
                { LevelType.Gameplay, _gameplayLevelScene },
            };
        }

        /// <summary>
        ///  Initializes the game state machine with all available game states.
        /// </summary>
        private void SetUpStateMachine()
        {
            StateMachine = new GameloopStateMachine(
                new GameStateMainMenu(),
                new GameStateSettingsMenu(),
                new GameStateRound(),
                new GameStatePaused(),
                new ShopState()
            );
        }

        // TODO: Refactor this and make the parameters editable in the editor.
        /// <summary>
        ///  Instantiates the <see cref="CameraRig"/> from a file and adds it
        ///  to the <see cref="SceneTree"/>.
        /// </summary>
        private void CreateCamera()
        {
            // Create container and assign shader material.
            _levelViewportContainer = new SubViewportContainer();
            _levelViewportContainer.Material = GD.Load<Material>(MaterialConfig.CAMERA_SHADER_MATERIAL_FILE);

            // Create viewport, set its size, and add it to the container.
            _levelViewport = new SubViewport();
            _levelViewport.Size = (Vector2I)GetViewport().GetWindow().GetVisibleRect().Size;
            _levelViewport.AudioListenerEnable3D = true;
            _levelViewportContainer.CallDeferred(Node.MethodName.AddChild, _levelViewport);

            // Create camera rig and add it to the viewport.
            _cameraRig = GD.Load<PackedScene>(SceneFileConfig.CAMERA_FILE).Instantiate<CameraRig>();
            _cameraRig.HeightAboveGround = 10f;
            _cameraRig.UseSmoothFollow = false;
            _levelViewport.CallDeferred(Node.MethodName.AddChild, _cameraRig);

            // Add the container to the scene tree.
            SceneTree.Root.CallDeferred(Node.MethodName.AddChild, _levelViewportContainer);
        }

        /// <summary>
        ///  Initiates game startup by transitioning to the
        ///  <see cref="GameStateRound"/> state and determining whether to
        ///  start from the shop or directly from a round.
        /// </summary>
        ///
        /// <remarks>
        ///  This method transitions the state machine to the Round state,
        ///  then defers a call to either <see cref="StartFromShop"/> or
        ///  <see cref="StartFromRound"/> based on the
        ///  <see cref="PlayerData.StartFromShop"/> flag in
        ///  <see cref="CurrentPlayerData"/>.
        /// </remarks>
        ///
        /// <seealso cref="StartNewGame"/>
        /// <seealso cref="LoadGame"/>
        /// <seealso cref="StartFromRound"/>
        /// <seealso cref="StartFromShop"/>
        private void StartGame()
        {
            this.PrintDebug("Starting game...");
            StateMachine.TransitionTo(StateType.Round);

            this.PrintDebug($"Start from shop: {CurrentPlayerData.StartFromShop}");
            if (CurrentPlayerData.StartFromShop)
            {
                CallDeferred(nameof(StartFromShop));
            }
            else
            {
                CallDeferred(nameof(StartFromRound));
            }
        }

        /// <summary>
        ///  Called when the game should start from the round state.
        /// </summary>
        ///
        /// <remarks>
        ///  Instructs the currently active <see cref="LevelManager"/> to
        ///  initialize the round data and the player. After this, delays the
        ///  starting of the round for 5 seconds.
        /// </remarks>
        ///
        /// <seealso cref="StartNewGame"/>
        /// <seealso cref="StartGame"/>
        /// <seealso cref="StartFromShop"/>
        /// <seealso cref="OnLevelStartDelayTimeout"/>
        private void StartFromRound()
        {
            // Used specifically to add delay between starting entering the
            // level and starting the round. To know why this separate method
            // is necessary, see the comments in StartNewGame.
            LevelManager.Active.InitializeLevel(RoundIndex);
            LevelManager.Active.InitializePlayer(CurrentPlayerData);
            SceneTree.CreateTimer(5f).Timeout += OnLevelStartDelayTimeout;
        }

        /// <summary>
        ///  Called when the game should start from the shop state.
        /// </summary>
        ///
        /// <remarks>
        ///  Transitions the state machine to the Shop state and initializes the
        ///  player with the current player data.
        /// </remarks>
        ///
        /// <seealso cref="StartNewGame"/>
        /// <seealso cref="StartGame"/>
        /// <seealso cref="StartFromRound"/>
        private void StartFromShop()
        {
            StateMachine.TransitionTo(StateType.Shop);
            LevelManager.Active.InitializePlayer(CurrentPlayerData);
        }

        /// <summary>
        ///  Called when the level start delay timer expires.
        /// </summary>
        ///
        /// <remarks>
        ///  This method is invoked after the delay timer created in
        ///  <see cref="StartFromRound"/> times out. It instructs the currently
        ///  active <see cref="LevelManager"/> to begin the round.
        /// </remarks>
        ///
        /// <seealso cref="StartFromRound"/>
        /// <seealso cref="StartGame"/>
        private void OnLevelStartDelayTimeout()
        {
            LevelManager.Active.StartRound();
        }

        #endregion Private Methods
    }
}
