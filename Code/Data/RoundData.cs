using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines a single round in the game. Contains the total length and the
    /// wave definitions.
    /// </summary>
    /// <seealso cref="WaveData"/>
    [GlobalClass]
    [Tool]
    public partial class RoundData : Resource
    {
        // For resetting the exported array.
        private static readonly WaveData s_defaultWave = new WaveData();

        private double _roundLength = 10.0;
        private Array<WaveData> _waves = [s_defaultWave];

        /// <summary>
        /// The total length of the round in seconds.
        /// </summary>
        [Export(PropertyHint.Range, "0,120,1,or_greater,suffix:s")]
        public double RoundLength
        {
            get => _roundLength;
            private set => _roundLength = Mathf.Clamp(value, 0.0, double.MaxValue);
        }

        /// <summary>
        /// If enabled, indicates that the user of this resource should ignore
        /// the <see cref="WaveData.SpawnTimeAfterStart"/> set in each wave.
        /// This can be used, for example, to force the spawn manager to
        /// automatically distribute the waves based on the length of the round.
        /// </summary>
        [Export]
        public bool UseAutomaticTimings { get; private set; } = false;

        /// <summary>
        /// An array containing the definitions for each wave in the
        /// round.
        /// </summary>
        /// <seealso cref="WaveData"/>
        [Export]
        public Array<WaveData> Waves
        {
            get => _waves;
            // Prevent setting to an empty array.
            private set => _waves = value.Count == 0 ? [s_defaultWave] : value;
        }
    }
}
