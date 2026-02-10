using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines a single wave in a round. Contains the start time, enemy types,
    /// and the number of each enemy type.
    /// </summary>
    /// <seealso cref="RoundData"/>
    [GlobalClass]
    [Tool]
    public partial class WaveData : Resource
    {
        // For resetting the exported dictionary.
        private static readonly Dictionary<EnemyType, int> s_defaultEnemies = new()
        {
            { EnemyType.Melee, 0 },
            { EnemyType.Ranged, 0 },
            { EnemyType.Shielded, 0 },
        };

        private double _spawnTimeAfterStart = 0;
        private Dictionary<EnemyType, int> _enemies = s_defaultEnemies;

        /// <summary>
        /// Tells how many seconds should be passed since the beginning of the
        /// round before this wave should be spawned.
        /// </summary>
        [Export(PropertyHint.Range, "0,60,1,or_greater,suffix:s")]
        public double SpawnTimeAfterStart
        {
            get => _spawnTimeAfterStart;
            private set => _spawnTimeAfterStart = Mathf.Clamp(value, 0, double.MaxValue);
        }

        /// <summary>
        /// Tells what enemy types to spawn and how many.
        /// </summary>
        [Export]
        public Dictionary<EnemyType, int> Enemies
        {
            get => _enemies;
            // Prevent setting to empty dictionary.
            private set => _enemies = value.Count == 0 ? s_defaultEnemies : value;
        }
    }
}
