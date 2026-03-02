// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano (tuominen.mika-95@hotmail.com)

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Handles spawning collectible items when enemies die.
    /// </summary>
    /// <remarks>
    /// This manager does not detect enemies itself. Instead,
    /// EnemySpawnManager calls <see cref="OnEnemyDiedSignal"/> whenever an enemy
    /// emits its death event.
    ///
    /// Internally the manager maintains:
    /// • A stable mapping: CollectibleType → PackedScene
    /// • A gameplay rule mapping: EnemyType → CollectibleType
    ///
    /// The latter ("drop rules") is intentionally flexible and can be modified
    /// later without touching the scene references.
    /// </remarks>
    public partial class CollectibleSpawnManager : Node3D
    {
        #region Exported Fields
        [Export]
        private PackedScene _boltScene = null;

        [Export]
        private PackedScene _nutScene = null;

        [Export]
        private PackedScene _wrenchScene = null;
        #endregion Exported Fields


        #region Runtime
        /// <summary>
        /// Stable mapping: each collectible type is associated with a specific scene.
        /// Scenes themselves are static representations and do not change at runtime.
        /// </summary>
        private Dictionary<CollectibleType, PackedScene> _collectibleTypeScenes =
            new Dictionary<CollectibleType, PackedScene>();

        /// <summary>
        /// Gameplay mapping: defines what collectible each enemy type drops.
        /// These rules are “for now” and can be changed later without touching the scene assets.
        /// </summary>
        private Dictionary<EnemyType, CollectibleType> _enemyToCollectibleType =
            new Dictionary<EnemyType, CollectibleType>();
        #endregion Runtime


        #region Godot Lifecycle
        public override void _Ready()
        {
            SetCollectibleSceneReferences();
        }
        #endregion Godot Lifecycle


        #region Public API
        /// <summary>
        /// Entry point called externally when an enemy dies.
        /// Spawns a collectible based on the enemy type and death position.
        /// </summary>
        /// <param name="enemyType">The enemy type as an integer.</param>
        /// <param name="deathPosition">World position where the enemy died.</param>
        public void OnEnemyDiedSignal(int enemyType, Vector3 deathPosition)
        {
            SpawnCollectible((EnemyType)enemyType, deathPosition);
        }
        #endregion Public API


        #region Private Helpers
        /// <summary>
        /// Initializes scene references and drop rules.
        /// </summary>
        private void SetCollectibleSceneReferences()
        {
            _collectibleTypeScenes = new Dictionary<CollectibleType, PackedScene>
            {
                { CollectibleType.Nut, _nutScene },
                { CollectibleType.Bolt, _boltScene },
                { CollectibleType.Wrench, _wrenchScene },
            };

            _enemyToCollectibleType = new Dictionary<EnemyType, CollectibleType>
            {
                { EnemyType.Melee, CollectibleType.Nut },
                { EnemyType.Ranged, CollectibleType.Bolt },
                { EnemyType.Shielded, CollectibleType.Wrench },
            };
        }

        /// <summary>
        /// Instantiates and places a collectible based on the defeated enemy's type.
        /// </summary>
        /// <param name="enemyType">Type of enemy that died.</param>
        /// <param name="position">World position of the death event.</param>
        private void SpawnCollectible(EnemyType enemyType, Vector3 position)
        {
            if (!_enemyToCollectibleType.TryGetValue(enemyType, out CollectibleType collectibleType))
            {
                GD.PushWarning($"No CollectibleType mapping for enemy type: {enemyType}");
                return;
            }

            if (!_collectibleTypeScenes.TryGetValue(collectibleType, out PackedScene scene) || scene == null)
            {
                GD.PushWarning($"No scene mapping for collectible type: {collectibleType}");
                return;
            }

            Collectible collectible = scene.Instantiate<Collectible>();

            // TODO:
            // Once LevelManager implements AddLevelObject(...) for collectibles,
            // replace AddChild(...) with:
            // LevelManager.active.AddLevelObject(collectible);

            AddChild(collectible);
            collectible.GlobalPosition = position;
        }
        #endregion Private Helpers
    }
}
