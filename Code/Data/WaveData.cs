using System;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines a single wave in a round including its start time and the types
    /// and numbers of enemies to spawn.
    /// </summary>
    /// <seealso cref="RoundData"/>
    [GlobalClass]
    public partial class WaveData : Resource
    {
        [Export(PropertyHint.None, "suffix:seconds")]
        private double _startTime = -1;

        [Export]
        private Dictionary<EnemyType, int> _enemies = null;

        /// <summary>
        /// The start time of the wave in seconds since the beginning of a
        /// round. Leave this to <c>-1</c> for automatically deciding when the
        /// round should start.
        /// </summary>
        [Obsolete("This is not implemented yet.")]
        public double StartTimeSinceRoundStart => _startTime;

        /// <summary>
        /// A <see cref="Dictionary"/> that contains enemies and their max
        /// spawn number in the wave. For example, <c>frog: 10</c> means 10
        /// frogs will spawn during the wave.
        /// </summary>
        public Dictionary<EnemyType, int> Enemies => _enemies;
    }
}
