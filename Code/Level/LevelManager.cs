// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using EHE.BoltBusters.Config;
using EHE.BoltBusters.States;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// <para>Manages the overall state and lifecycle of a game level.</para>
    ///
    /// <para>
    /// This class is responsible for:
    /// <list type="bullet">
    /// <item>Initializing and managing the game level structure</item>
    /// <item>Controlling round timing and progression</item>
    /// <item>Spawning and managing enemies through the <see cref="EnemySpawner"/></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// The LevelManager maintains a static reference to the currently active
    /// level instance, allowing other systems to access level data without
    /// maintaining their own references.
    /// </para>
    /// </summary>
    ///
    /// <remarks>
    /// Note: This class is marked as WIP (Work In Progress) with several
    /// features not yet fully functional.
    /// </remarks>
    public partial class LevelManager : Node3D
    {
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

        #endregion Fields


        #region Properties

        /// <summary>
        /// Reference to the currently active LevelManager.
        /// </summary>
        public static LevelManager Active { get; private set; }

        /// <summary>
        /// The type of the level.
        /// </summary>
        public LevelType LevelType => _levelType;

        /// <summary>
        /// Reference to the player.
        /// </summary>
        public Player Player => _player;

        public bool RoundInProgress { get; private set; }

        #endregion Properties


        #region Overrides

        // TODO: Create a base class for LevelManager.
        // TODO: Create separate classes for Background and Gameplay level managers.
        public override void _Ready()
        {
            Active = this;

            // Get references to nodes defined in the editor.
            // TODO: Replace getting by name with extension method.
            _arena = GetNodeOrNull<Node3D>("Arena");
            _enemySpawnManager = GetNodeOrNull<EnemySpawnManager>("EnemySpawnManager");
            _player = GetNodeOrNull<Player>("Player");
            _playerSpawnPosition = GetNodeOrNull<Node3D>("PlayerSpawnPosition");

            // TODO: Refactor validation code to a separate method.
            bool hasErrors = false;

            if (_arena == null)
            {
                GD.PushError("Arena node not found in level!");
                hasErrors = true;
            }

            if (_enemySpawnManager == null)
            {
                GD.PushError("Enemy Spawner node not found in level!");
                hasErrors = true;
            }

            if (_player == null)
            {
                GD.PushError("Player node not found in level!");
                hasErrors = true;
            }

            if (_playerSpawnPosition == null)
            {
                GD.PushError("Player Spawn Position node not found in level!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                GD.PushError($"Encountered problems when creating {Name} ({typeof(LevelManager)}).");
                return;
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

            // Create round timer.
            // TODO: Create timer in separate method when round starts.
            _roundTimer = new Timer();
            _roundTimer.Timeout += OnRoundEnded;
            AddChild(_roundTimer);
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
            this.PrintDebug("Ready.");
        }

        #endregion Overrides


        #region Public Methods

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Initializes the round from provided <see cref="RoundData"/>.
        /// </summary>
        /// <param name="roundData">Data describing the round.</param>
        [Obsolete]
        public void InitializeLevel(RoundData roundData)
        {
            this.PrintDebug("Initializing level...");
            _roundData = roundData;
            _roundTimer.WaitTime = _roundData.RoundLength;
        }

        /// <summary>
        ///  Fetches the round data from a resource file using the given
        ///  <paramref name="roundIndex"/>, caches it, and sets up the round
        ///  timer.
        /// </summary>
        ///
        /// <param name="roundIndex">
        ///  Numerical index for the round data.
        /// </param>
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

            GameManager.Instance.CurrentPlayerData.IsLevelCleared = false;
            DespawnLevelObjects();
            _roundTimer.WaitTime = _roundData.RoundLength;
            GameManager.Instance.RoundIndex++;
            GameManager.Instance.SaveGame();
        }

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Starts the round timer and the enemy spawn manager.
        /// </summary>
        public void StartRound()
        {
            this.PrintDebug("Starting round...");
            _roundTimer.Start();
            RoundInProgress = true;
            _enemySpawnManager.StartRound(_roundData);
        }

        /// <summary>
        /// THIS WILL LIKELY BECOME PRIVATE!
        /// WIP! NOT FUNCTIONAL YET!
        /// Despawns enemies, projectiles and collectible, and resets the
        /// player.
        /// </summary>
        public void ResetLevel()
        {
            this.PrintDebug("Resetting level...");
            DespawnLevelObjects();

            // TODO: Make player immobile.
            Player.GlobalPosition = _playerSpawnPosition.GlobalPosition; // TODO: Is this too hacky?
            // TODO: Reset player health.
        }

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Adds the given level object to the level.
        /// </summary>
        /// <param name="levelObject"></param>
        public void AddLevelObject(Node3D levelObject)
        {
            this.PrintDebug($"Adding level object '{levelObject.GetType()}'");
            if (levelObject is Enemy enemy)
            {
                _enemyRoot.AddChild(enemy);
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

        public double GetRemainingRoundTime()
        {
            return _roundTimer.GetTimeLeft();
        }

        #endregion Public Methods


        #region Private Methods

        // TODO: Add Load method that takes round index as param and loads the
        //       round data from a file.

        /// <summary>
        /// WIP!
        /// Called when the round timer runs out. Stops the round timer.
        /// </summary>
        private void OnRoundEnded()
        {
            this.PrintDebug("Round ended.");
            _roundTimer.Stop();
            RoundInProgress = false;
            ResetLevel();
            // TODO: Disable player movement.
            // TODO: Disable enemy movement.
            GameManager.Instance.CurrentPlayerData.IsLevelCleared = true;
            GameManager.Instance.SaveGame();
            // TODO: Wait 5s before transitioning to shop state.
            GameManager.Instance.StateMachine.TransitionTo(StateType.Shop);
        }

        /// <summary>
        ///  Despawns all objects from the level that implement the
        ///  <see cref="ISpawnable"/> interface.
        /// </summary>
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

        #endregion Private Methods
    }
}
