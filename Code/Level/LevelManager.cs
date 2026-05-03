// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//            Pekka Heljakka <pekka.heljakka@tuni.fi>
//            Miko Reinholm <miko.reinholm@tuni.fi>

using System.Threading.Tasks;
using EHE.BoltBusters.Config;
using EHE.BoltBusters.EnemyAI;
using EHE.BoltBusters.States;
using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  <para>
    ///   The LevelManager is a singleton-like class that orchestrates the
    ///   events happening during a single round from the initialization of the
    ///   level to round completion and level cleanup. It serves as a
    ///   coordinator for different subsystems such as enemy spawning and game
    ///   state transitions.
    ///  </para>
    ///  <para>
    ///   The LevelManager maintains a static reference to the currently active
    ///   instance via the <see cref="Active"/> property, allowing other systems
    ///   to access level data without maintaining their own references.
    ///  </para>
    /// </summary>
    public partial class LevelManager : Node3D
    {
        [Signal]
        public delegate void InitializedEventHandler();

        [Signal]
        public delegate void RoundStartingEventHandler();

        [Signal]
        public delegate void RoundEndedEventHandler();

        /// <summary>
        ///  Emitted when the round has started.
        /// </summary>
        [Signal]
        public delegate void RoundStartedEventHandler();

        #region Fields

        // Fields that are editable in the inspector.
        [Export]
        private LevelType _levelType = LevelType.None;

        // Nodes that are visible in the editor's node tree.
        private EnemySpawnManager _enemySpawnManager;
        private Node3D _playerSpawnPosition;
        private Node3D _enemyRoot;
        private Node3D _projectileRoot;
        private Node3D _collectibleRoot;

        // Nodes that are created from the code.
        private Timer _roundTimer;
        private RoundData _roundData;
        private EnemyGroupManager _enemyGroupManager;

        // Audio stuff
        private MusicManager.Song _currentSong = MusicManager.Song.MainTheme;
        private AudioStreamPlayer _currentMusicPlayer;
        private bool _isFirstRoundOfSong;

        #endregion Fields


        #region Properties

        /// <summary>
        ///  Reference to the currently active <see cref="LevelManager"/>
        ///  instance. Set automatically when <see cref="_Ready"/> is called.
        /// </summary>
        public static LevelManager Active { get; private set; }

        /// <summary>
        ///  Gets the type of the level (e.g., Arena, Training, etc.).
        /// </summary>
        public LevelType LevelType => _levelType;

        /// <summary>
        ///  Gets a reference to the <see cref="Player"/> instance in this
        ///  level.
        /// </summary>
        public Player Player { get; private set; }

        /// <summary>
        ///  Indicates whether the round is in progress or not.
        /// </summary>
        ///
        /// <remarks>
        ///  This property returns the status of the internal round timer.
        ///  If the timer is currently running, returns <c>true</c>. If the
        ///  timer is stopped or if the timer has not been set up, returns
        ///  <c>false</c>.
        /// </remarks>
        public bool RoundInProgress
        {
            get
            {
                if (_roundTimer == null)
                {
                    return false;
                }

                return !_roundTimer.IsStopped();
            }
        }

        #endregion Properties


        #region Overrides

        /// <summary>
        ///  Unsubscribes from the <see cref="Player.PlayerDied"/> event if
        ///  applicable.
        /// </summary>
        public override void _ExitTree()
        {
            if (Player != null)
            {
                // This ensures that the signal is disconnected when the level
                // manager exits the scene tree. The signal is also
                // disconnected in the OnPlayerDeath method but this is only
                // done if the player dies during the round.
                Player.PlayerDied -= OnPlayerDeath;
            }
        }

        /// <inheritdoc/>
        public override void _Ready()
        {
            Active = this;

            // Get references to nodes defined in the editor.
            _enemySpawnManager = this.GetFirstChildOfType<EnemySpawnManager>(recurse: true);
            _playerSpawnPosition = this.GetFirstChildOfType<Marker3D>(recurse: false);
            Player = this.GetFirstChildOfType<Player>(recurse: true);

            // TODO: Replace this with a proper differentiation between bg level and regular level.
            if (LevelType != LevelType.Background)
            {
                // TODO: Refactor validation code to a separate method.
                bool hasErrors = false;

                if (_enemySpawnManager == null)
                {
                    this.LogError("Enemy Spawner node not found in level!");
                    hasErrors = true;
                }

                if (Player == null)
                {
                    this.LogError("Player node not found in level!");
                    hasErrors = true;
                }

                if (_playerSpawnPosition == null)
                {
                    this.LogError("Player Spawn Position node not found in level!");
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    this.LogError($"Encountered problems when creating {Name} ({typeof(LevelManager)}).");
                    return;
                }
            }

            // Create object root nodes.
            _enemyRoot = new Node3D();
            _projectileRoot = new Node3D();
            _collectibleRoot = new Node3D();

            _enemyRoot.SetName("EnemyRoot");
            _projectileRoot.SetName("ProjectileRoot");
            _collectibleRoot.SetName("CollectibleRoot");

            AddChild(_enemyRoot);
            AddChild(_projectileRoot);
            AddChild(_collectibleRoot);

            // Create enemy group AI manager
            _enemyGroupManager = new EnemyGroupManager();
            _enemyGroupManager.SetName("EnemyGroupManager");
            AddChild(_enemyGroupManager);

            CreateRoundTimer();

            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
            this.LogDebug($"{LevelType} level scene is ready.");
        }

        #endregion Overrides


        #region Public Methods

        /// <summary>
        ///  Loads the round data from a resource file using the given
        ///  <paramref name="roundIndex"/>, caches it, and configures the
        ///  round accordingly to the loaded data.
        /// </summary>
        ///
        /// <param name="roundIndex">
        ///  Numerical index for the round data file to load. This corresponds
        ///  to the number in the file name of the round data resource.
        /// </param>
        ///
        /// <remarks>
        ///  <para>
        ///   This method must be called after <see cref="_Ready"/> and before
        ///   <see cref="StartRound"/> to properly initialize the level for a
        ///   specific round.
        ///  </para>
        ///  <para>
        ///   If the round data file cannot be found at the computed path, an
        ///   error is logged and the method returns without completing
        ///   initialization.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="FilePathConfig.ROUND_DATA_FILE_PATH_FORMAT"/>
        public void InitializeLevel(int roundIndex)
        {
            this.LogInfo($"Initializing level {roundIndex}.");

            // Load new round data.
            if (!LoadRoundData(roundIndex))
            {
                return;
            }

            DespawnLevelObjects();
            ResetRoundTimer();

            // Reset player.
            Player.ResetAll();
            Player.GlobalPosition = _playerSpawnPosition.GlobalPosition;
            // Re-enable player input when a new round is loaded since it's
            // disabled when the round ends or when the player dies.
            Player.ToggleInputListening(true);
            Player.PlayerDied += OnPlayerDeath;

            // Perform autosave.
            // TODO: Move these to GameManager.
            GameManager.Instance.CurrentPlayerData.StartFromShop = false;
            GameManager.Instance.SaveGame();

            this.LogInfo($"Level {roundIndex} initialized.");
            EmitSignal(SignalName.Initialized);
        }

        /// <summary>
        ///  Initializes the player instance with the provided
        ///  <see cref="PlayerData"/>.
        /// </summary>
        ///
        /// <param name="playerData">
        ///  The object containing player data.
        /// </param>
        ///
        /// <remarks>
        ///  This method must be called after <see cref="_Ready"/> and
        ///  typically before <see cref="StartRound"/>. It delegates the actual
        ///  initialization logic to the <see cref="Player"/> instance's
        ///  <see cref="Player.Initialize"/> method.
        /// </remarks>
        public void InitializePlayer(PlayerData playerData)
        {
            Player.Initialize(playerData);
        }

        /// <summary>
        ///  Starts the round.
        /// </summary>
        ///
        /// <remarks>
        ///  This method starts the round timer, instructs the enemy spawner
        ///  to start its logic, and instructs the music player to play the
        ///  appropriate song. At the end, the
        ///  <seealso cref="GameManager.RoundStateChanged"/> signal is emitted
        ///  telling other systems that the round has started.
        /// </remarks>
        public async void StartRound()
        {
            this.LogInfo("Round starting in 6.5 s.");
            EmitSignal(SignalName.RoundStarting);
            await Task.Delay(6500);
            _roundTimer.Start();
            _enemySpawnManager.StartRound(_roundData);
            GameManager.Instance.EmitSignal(GameManager.SignalName.RoundStateChanged, RoundInProgress);

            UpdateMusicForRound(GameManager.Instance.RoundIndex);
            if (_currentMusicPlayer != null && !_isFirstRoundOfSong)
            {
                MusicManager.Instance.FadeInPlayer(_currentMusicPlayer);
            }

            EmitSignal(SignalName.RoundStarted);
            this.LogInfo("Round started.");
        }

        /// <summary>
        ///  Adds the given level objects under their appropriate root nodes.
        ///  Unidentified level objects are added to the level root and a
        ///  warning is logged.
        /// </summary>
        ///
        /// <param name="levelObject">
        ///  The level object to add. This should be one of the following types:
        ///  <see cref="Enemy"/>, <see cref="Projectile"/>, or
        ///  <see cref="Collectible"/>.
        /// </param>
        ///
        /// <remarks>
        ///  <b>Incomplete Features:</b>
        ///  <list type="bullet">
        ///   <item>
        ///    Validate that root nodes are not null before adding children
        ///   </item>
        ///   <item>
        ///    Add type checking and error handling for incompatible object
        ///    types
        ///   </item>
        ///  </list>
        /// </remarks>
        public void AddLevelObject(Node3D levelObject)
        {
            this.LogDebug($"Adding level object of type '{levelObject.GetType()}'.");

            switch (levelObject)
            {
                case Enemy enemy:
                    _enemyRoot.AddChild(enemy);
                    _enemyGroupManager.AddEnemy(enemy);
                    break;
                case Projectile projectile:
                    _projectileRoot.AddChild(projectile);
                    break;
                case Collectible collectible:
                    _collectibleRoot.AddChild(collectible);
                    break;
                default:
                    AddChild(levelObject);
                    this.LogWarning("Unidentified level object added to level root.");
                    break;
            }
        }

        /// <summary>
        ///  Gets the remaining time in seconds for the current round.
        /// </summary>
        ///
        /// <returns>
        ///  The number of seconds remaining until the round timer expires.
        ///  Returns 0 if the round is not in progress.
        /// </returns>
        public double GetRemainingRoundTime()
        {
            return _roundTimer.GetTimeLeft();
        }

        #endregion Public Methods


        #region Private Methods

        /// <summary>
        ///  Creates a new timer for tracking the round timer if one doesn't
        ///  exist already and connects its <see cref="Timer.Timeout"/> signal
        ///  to the <see cref="OnRoundEnded"/> method.
        /// </summary>
        private void CreateRoundTimer()
        {
            if (_roundTimer == null)
            {
                _roundTimer = new Timer();
                _roundTimer.Timeout += OnRoundEnded;
                AddChild(_roundTimer);
            }
        }

        /// <summary>
        ///  Loads the round data from a resource file defined by the round
        ///  index. Saved to the <seealso cref="_roundData"/> variable.
        /// </summary>
        ///
        /// <param name="roundIndex">
        ///  Index number of the round. This corresponds to the number in the
        ///  file name of the resource file.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if round data was loaded successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool LoadRoundData(int roundIndex)
        {
            var roundDataPath = string.Format(FilePathConfig.ROUND_DATA_FILE_PATH_FORMAT, roundIndex);
            this.LogInfo($"Loading data from '{roundDataPath}'");
            _roundData = GD.Load<RoundData>(roundDataPath);

            if (_roundData != null)
            {
                return true;
            }

            this.LogError($"Failed to load round data from path '{roundDataPath}'!");
            return false;
        }

        /// <summary>
        ///  Stops <seealso cref="_roundTimer"/> and sets its wait time to
        ///  what's defined in <seealso cref="_roundData"/>.
        /// </summary>
        private void ResetRoundTimer()
        {
            _roundTimer.Stop();
            _roundTimer.SetWaitTime(_roundData.RoundLength);
        }

        /// <summary>
        ///  Handles round completion and transitions to the next state.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///   This method is called automatically when the
        ///   <see cref="_roundTimer"/> invokes its <see cref="Timer.Timeout"/>
        ///   event.
        ///  </para>
        ///  <para>
        ///   This method handles stopping the <see cref="_roundTimer"/>,
        ///   disabling player input, requesting level objects to be despawned,
        ///   autosaving the game, and handling transition to the next game
        ///   state (game over or victory).
        ///  </para>
        /// </remarks>
        private async void OnRoundEnded()
        {
            this.LogInfo("Round ended.");

            _roundTimer.Stop();
            EmitSignal(SignalName.RoundEnded);
            GameManager.Instance.EmitSignal(GameManager.SignalName.RoundStateChanged, RoundInProgress);

            DespawnLevelObjects();
            Player.ToggleInputListening(false);

            await Task.Delay(2500); // TODO: Remove hardcoding. This is the length of the round ended label animation.

            if (_currentMusicPlayer != null)
            {
                MusicManager.Instance.FadeToBackgroundLevel(_currentMusicPlayer);
            }

            GameManager.Instance.CurrentPlayerData.StartFromShop = true;
            GameManager.Instance.RoundIndex++;

            if (GameManager.Instance.RoundIndex > GameManager.Instance.LastRoundIndex)
            {
                GameManager.Instance.StateMachine.TransitionTo(StateType.Victory);
            }
            else
            {
                GameManager.Instance.SaveGame();
                GameManager.Instance.StateMachine.TransitionTo(StateType.Shop);
            }
        }

        /// <summary>
        ///  Despawns all objects from the level that implement the
        ///  <see cref="ISpawnable"/> interface, excluding the
        ///  <see cref="Player"/> instance.
        /// </summary>
        ///
        /// <remarks>
        ///  This method recursively searches through all children in the
        ///  level and calls <see cref="ISpawnable.OnDespawn"/>
        ///  on objects that implement the <see cref="ISpawnable"/> interface.
        /// </remarks>
        private void DespawnLevelObjects()
        {
            var children = this.GetChildrenOfType<Node>(recurse: true, recurseMatching: true);

            if (children.Count == 0)
            {
                return;
            }

            foreach (var child in children)
            {
                if (child is ISpawnable spawnable && child is not BoltBusters.Player)
                {
                    spawnable.OnDespawn();
                }
            }
        }

        /// <summary>
        ///  Disables the player input, stops the round, and transitions to the
        ///  game over state.
        /// </summary>
        ///
        /// <param name="player">Reference to the player that died.</param>
        ///
        /// <remarks>
        ///  When the player dies, the <see cref="LevelManager"/> unsibscribes
        ///  from its <see cref="Player.PlayerDied"/> event to prevent this
        ///  method from being triggered multiple times.
        /// </remarks>
        ///
        /// <seealso cref="StateType"/>
        /// <seealso cref="GameOverState"/>
        private void OnPlayerDeath(Player player)
        {
            this.LogDebug("Player died.");
            Player.ToggleInputListening(false);
            _roundTimer.Stop();
            GameManager.Instance.StateMachine.TransitionTo(StateType.GameOver);
            Player.PlayerDied -= OnPlayerDeath;
        }

        private void UpdateMusicForRound(int roundIndex)
        {
            var song = PickSong(roundIndex);

            if (song == _currentSong)
            {
                _isFirstRoundOfSong = false; // Same song, so it's not the first round
                return;
            }

            _currentSong = song;
            _isFirstRoundOfSong = true; // New song, so this is the first round

            AudioStreamPlayer previousPlayer = _currentMusicPlayer;

            switch (song)
            {
                case MusicManager.Song.StageTheme1:
                    _currentMusicPlayer = MusicManager.Instance.StageThemePlayer1;
                    if (previousPlayer != null && previousPlayer != _currentMusicPlayer)
                    {
                        MusicManager.Instance.FadeOutPlayer(previousPlayer);
                    }
                    _currentMusicPlayer.VolumeDb = 0f; // Start at normal volume
                    MusicManager.Instance.PlayMusic(_currentMusicPlayer, song);
                    break;
                case MusicManager.Song.StageTheme2:
                    _currentMusicPlayer = MusicManager.Instance.StageThemePlayer2;
                    if (previousPlayer != null && previousPlayer != _currentMusicPlayer)
                    {
                        MusicManager.Instance.FadeOutPlayer(previousPlayer);
                    }
                    _currentMusicPlayer.VolumeDb = 0f; // Start at normal volume
                    MusicManager.Instance.PlayMusic(_currentMusicPlayer, song);
                    break;
                case MusicManager.Song.StageTheme3:
                    _currentMusicPlayer = MusicManager.Instance.StageThemePlayer3;
                    if (previousPlayer != null && previousPlayer != _currentMusicPlayer)
                    {
                        MusicManager.Instance.FadeOutPlayer(previousPlayer);
                    }
                    _currentMusicPlayer.VolumeDb = 0f; // Start at normal volume
                    MusicManager.Instance.PlayMusic(_currentMusicPlayer, song);
                    break;
                case MusicManager.Song.StageTheme4:
                    _currentMusicPlayer = MusicManager.Instance.StageThemePlayer4;
                    if (previousPlayer != null && previousPlayer != _currentMusicPlayer)
                    {
                        MusicManager.Instance.FadeOutPlayer(previousPlayer);
                    }
                    _currentMusicPlayer.VolumeDb = 0f; // Start at normal volume
                    MusicManager.Instance.PlayMusic(_currentMusicPlayer, song);
                    break;
            }
        }

        private MusicManager.Song PickSong(int roundIndex)
        {
            var song = MusicManager.Song.MainTheme;

            switch (roundIndex)
            {
                case >= 1 and <= 5:
                    song = MusicManager.Song.StageTheme1;
                    break;
                case >= 6 and <= 10:
                    song = MusicManager.Song.StageTheme2;
                    break;
                case >= 11 and <= 15:
                    song = MusicManager.Song.StageTheme3;
                    break;
                case >= 16 and <= 20:
                    song = MusicManager.Song.StageTheme4;
                    break;
            }

            return song;
        }

        #endregion Private Methods
    }
}
