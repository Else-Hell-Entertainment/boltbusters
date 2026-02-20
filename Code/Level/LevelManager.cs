// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
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

        public override void _Ready()
        {
            _arena = GetNodeOrNull<Node3D>("Arena");
            _enemySpawnManager = GetNodeOrNull<EnemySpawnManager>("EnemySpawnManager");
            _player = GetNodeOrNull<Player>("Player");
            _playerSpawnPosition = GetNodeOrNull<Node3D>("PlayerSpawnPosition");

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

            Active = this;

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
            _roundTimer = new Timer();
            _roundTimer.Timeout += OnRoundEnded;
            AddChild(_roundTimer);
        }

        /// <summary>
        /// Handles non-movements inputs that happen during gameplay.
        /// For example, pausing the game.
        /// </summary>
        /// <param name="inputEvent">Input event that occurred.</param>
        public override void _Input(InputEvent inputEvent)
        {
            // TODO: Move the key name to a config file.
            if (inputEvent.IsActionPressed("Pause"))
            {
                GameManager.Instance.StateMachine.TransitionTo(StateType.Paused);
            }
        }

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Initializes the round from provided <see cref="RoundData"/>.
        /// </summary>
        /// <param name="roundData">Data describing the round.</param>
        public void InitializeLevel(RoundData roundData)
        {
            _roundTimer.WaitTime = roundData.RoundLength;
        }

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Starts the round timer and the enemy spawn manager.
        /// </summary>
        public void StartRound()
        {
            _roundTimer.Start();
            _enemySpawnManager.StartRound(_roundData);
        }

        /// <summary>
        /// WIP! NOT FUNCTIONAL YET!
        /// Despawns enemies, projectiles and collectible, and resets the
        /// player.
        /// </summary>
        public void ResetLevel()
        {
            // TODO: Enable this code once the enemy spawner is integrated!
            // foreach (var enemy in _enemyRoot.GetChildren())
            // {
            //     enemy.OnDespawn();
            // }

            // TODO: Reset player health.
            // TODO: Reset player position.
        }

        /// <summary>
        /// WIP! NOT FULLY FUNCTIONAL YET!
        /// Adds the given level object to the level.
        /// </summary>
        /// <param name="levelObject"></param>
        public void AddLevelObject(Node3D levelObject)
        {
            if (levelObject is Enemy enemy)
            {
                _enemyRoot.AddChild(enemy);
            }
            // else if (levelObject is Projectile projectile)
            // {
            //     _projectileRoot.AddChild(projectile);
            // }
            else if (levelObject is Collectible collectible)
            {
                _collectibleRoot.AddChild(collectible);
            }
        }

        /// <summary>
        /// WIP!
        /// Called when the round timer runs out. Stops the round timer.
        /// </summary>
        private void OnRoundEnded()
        {
            _roundTimer.Stop();
            // TODO: Wait 5s and transition to shop state.
        }
    }
}
