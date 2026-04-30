// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//            Pekka Heljakka <pekka.heljakka@tuni.fi>
//            Miko Reinholm <miko.reinholm@tuni.fi>

using System;
using EHE.BoltBusters.Config;
using EHE.BoltBusters.EnemyAI;
using EHE.BoltBusters.States;
using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  <para>Manages the overall state and lifecycle of a game level.</para>
    ///  <para>
    ///   This class is responsible for:
    ///   <list type="bullet">
    ///    <item>Initializing and managing the game level structure</item>
    ///    <item>Controlling round timing and progression</item>
    ///    <item>Keeping track of the player instance</item>
    ///    <item>
    ///     Spawning and managing enemies through the
    ///     <see cref="EnemySpawnManager"/>
    ///    </item>
    ///    <item>
    ///     Managing level objects (enemies, projectiles, collectibles) through
    ///     dedicated root nodes
    ///    </item>
    ///    <item>Handling end-of-round transitions and level cleanup</item>
    ///   </list>
    /// </para>
    ///
    /// <para>
    ///   The LevelManager maintains a static reference to the currently active
    ///   level instance via the <see cref="Active"/> property, allowing other
    ///   systems to access level data without maintaining their own references.
    ///  </para>
    /// </summary>
    ///
    /// <remarks>
    ///  <b>[WIP]</b> This class contains several unfinished features and
    ///  planned refactorings:
    ///  <list type="bullet">
    ///   <item>
    ///    Architecture: Planned to split into Background and Gameplay level
    ///    manager subclasses
    ///   </item>
    ///   <item>
    ///    ResetLevel: Method may become private; functionality is incomplete
    ///   </item>
    ///   <item>
    ///    AddLevelObject: Needs validation for null root nodes and type
    ///    checking
    ///   </item>
    ///  </list>
    /// </remarks>
    public partial class LevelManager : Node3D
    {
        [Signal]
        public delegate void InitializedEventHandler();

        #region Fields

        // Fields that are editable in the inspector.
        [Export]
        private LevelType _levelType = LevelType.None;

        // Nodes that are visible in the editor's node tree.
        private Node3D _arena;
        private EnemySpawnManager _enemySpawnManager;
        private Player _player;
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
        ///  Gets a reference to the Player instance in this level.
        /// </summary>
        public Player Player => _player;

        /// <summary>
        ///  Gets a value indicating whether a round is currently in progress.
        ///  <c>true</c> after <see cref="StartRound"/> is called,
        ///  <c>false</c> after the round timer expires.
        /// </summary>
        public bool RoundInProgress { get; private set; }

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
            // TODO: Replace getting by name with extension method.
            _arena = GetNodeOrNull<Node3D>("Arena");
            _enemySpawnManager = GetNodeOrNull<EnemySpawnManager>("EnemySpawnManager");
            _player = GetNodeOrNull<Player>("Player");
            _playerSpawnPosition = GetNodeOrNull<Node3D>("PlayerSpawnPosition");

            // TODO: Replace this with a proper differentiation between bg level and regular level.
            if (LevelType != LevelType.Background)
            {
                // TODO: Refactor validation code to a separate method.
                bool hasErrors = false;

                if (_arena == null)
                {
                    this.LogError("Arena node not found in level!");
                    hasErrors = true;
                }

                if (_enemySpawnManager == null)
                {
                    this.LogError("Enemy Spawner node not found in level!");
                    hasErrors = true;
                }

                if (_player == null)
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
        ///  Numerical index for the round data file to load.
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
        ///  initialization.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="FilePathConfig.ROUND_DATA_FILE_PATH_FORMAT"/>
        public void InitializeLevel(int roundIndex)
        {
            this.PrintDebug($"Initializing level '{roundIndex}'...");
            var roundDataPath = string.Format(DataConfig.ROUND_DATA_FILE_PATH_FORMAT, roundIndex);
            this.PrintDebug($"Loading round data from '{roundDataPath}'...");
            _roundData = GD.Load<RoundData>(roundDataPath);

            if (_roundData == null)
            {
                GD.PushError($"Failed to load round data from path '{roundDataPath}'");
                return;
            }

            GameManager.Instance.CurrentPlayerData.StartFromShop = false;
            DespawnLevelObjects();
            _roundTimer.WaitTime = _roundData.RoundLength;
            GameManager.Instance.SaveGame();
            this.PrintDebug("Initialized.");
            Player.PlayerDied += OnPlayerDeath;

            // Re-enable player input when a new round is loaded since it's
            // disabled when the round ends or when the player dies.
            Player.ToggleInputListening(true);

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
            _player.Initialize(playerData);
        }

        /// <summary>
        ///  Starts the round by activating the round timer and enemy spawn
        ///  manager.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///   This method must be called after <see cref="InitializeLevel(int)"/>
        ///   and <see cref="InitializePlayer(PlayerData)"/> to begin round
        ///   execution.
        ///  </para>
        ///  <para>
        ///   This method performs the following:
        ///    <list type="bullet">
        ///    <item>
        ///     Sets the <see cref="RoundInProgress"/> flag to <c>true</c>
        ///    </item>
        ///    <item>
        ///     Starts the round timer using the duration configured in
        ///     <see cref="InitializeLevel(int)"/>
        ///    </item>
        ///    <item>
        ///     Signals the <see cref="EnemySpawnManager"/> to begin spawning
        ///     enemies
        ///    </item>
        ///   </list>
        ///  </para>
        /// </remarks>
        public void StartRound()
        {
            this.PrintDebug("Starting round...");
            _roundTimer.Start();
            RoundInProgress = true;
            _enemySpawnManager.StartRound(_roundData);
            GameManager.Instance.EmitSignal(GameManager.SignalName.RoundStateChanged, true);

            UpdateMusicForRound(GameManager.Instance.RoundIndex);
            if (_currentMusicPlayer != null && !_isFirstRoundOfSong)
            {
                MusicManager.Instance.FadeInPlayer(_currentMusicPlayer);
            }
        }

        /// <summary>
        ///  <b>[WIP]</b>
        ///  Despawns enemies, projectiles, and collectibles. Resets the
        ///  player position.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///   <b>Status:</b> This method is a Work In Progress and not fully functional.
        ///   It is likely to become a private method in a future refactoring.
        ///  </para>
        ///  <para>
        ///   <b>Incomplete Features:</b>
        ///   <list type="bullet">
        ///    <item>Make player immobile during reset</item>
        ///    <item>Reset player health to full</item>
        ///   </list>
        ///  </para>
        /// </remarks>
        public void ResetLevel()
        {
            this.PrintDebug("Resetting level...");
            DespawnLevelObjects();
            GameManager.Instance.EmitSignal(GameManager.SignalName.RoundStateChanged, false);

            // TODO: Make player immobile.
            Player.GlobalPosition = _playerSpawnPosition.GlobalPosition; // TODO: Is this too hacky?
            // TODO: Reset player health.
        }

        /// <summary>
        ///  <b>[WIP]</b> Adds the given level object to the appropriate root
        ///  node in the level.
        /// </summary>
        ///
        /// <param name="levelObject">
        ///  The level object to add. Must be an <see cref="Enemy"/>,
        ///  <see cref="Projectile"/>, or <see cref="Collectible"/>.
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
            this.PrintDebug($"Adding level object '{levelObject.GetType()}'");
            if (levelObject is Enemy enemy)
            {
                _enemyRoot.AddChild(enemy);
                _enemyGroupManager.AddEnemy(enemy);
            }
            else if (levelObject is Projectile projectile)
            {
                _projectileRoot.AddChild(projectile);
            }
            else if (levelObject is Collectible collectible)
            {
                _collectibleRoot.AddChild(collectible);
            }
            // TODO: Check for null root nodes and incompatible levelObjects.
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

        // TODO: Add Load method that takes round index as param and loads the
        //       round data from a file.

        /// <summary>
        /// <b>[WIP]</b> Called when the round timer expires.
        /// Handles round completion and transitions to the next state.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///   <b>Status:</b>
        ///   This method contains incomplete features and planned refactorings.
        ///  </para>
        ///  <para>
        ///   <b>Current Functionality:</b>
        ///   <list type="bullet">
        ///    <item>Stops the round timer</item>
        ///    <item>Sets <see cref="RoundInProgress"/> to <c>false</c></item>
        ///    <item>Despawns all level objects via <see cref="ResetLevel"/></item>
        ///    <item>Increments the round index in GameManager</item>
        ///    <item>Saves the current game state</item>
        ///    <item>Transitions to the Shop state</item>
        ///   </list>
        ///  </para>
        ///  <para>
        ///   <b>Incomplete Features:</b>
        ///   <list type="bullet">
        ///    <item>Disable player movement when round ends</item>
        ///    <item>Disable enemy movement when round ends</item>
        ///    <item>Add delay before transitioning to shop state</item>
        ///   </list>
        ///  </para>
        /// </remarks>
        private void OnRoundEnded()
        {
            this.PrintDebug("Round ended.");
            _roundTimer.Stop();
            if (_currentMusicPlayer != null)
            {
                MusicManager.Instance.FadeToBackgroundLevel(_currentMusicPlayer);
            }
            RoundInProgress = false;
            ResetLevel();
            Player.ToggleInputListening(false);
            // TODO: Disable enemy movement.
            GameManager.Instance.CurrentPlayerData.StartFromShop = true;
            GameManager.Instance.RoundIndex++;

            if (GameManager.Instance.RoundIndex > GameManager.Instance.LastRoundIndex)
            {
                GameManager.Instance.StateMachine.TransitionTo(StateType.Victory);
            }
            else
            {
                GameManager.Instance.SaveGame();
                // TODO: Wait 5s before transitioning to shop state.
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
            this.PrintDebug("Player died.");
            Player.ToggleInputListening(false);
            _roundTimer.Stop();
            RoundInProgress = false;
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
