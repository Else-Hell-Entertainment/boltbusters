using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines a single round in the game including the total length of the
    /// round and the waves in it.
    /// </summary>
    /// <seealso cref="WaveData"/>
    [GlobalClass]
    public partial class RoundData : Resource
    {
        [Export(PropertyHint.Range, "0,120,1,or_grater,suffix:seconds")]
        private int _roundLength = 10;

        [Export]
        private WaveData[] _waves = null;

        /// <summary>
        /// The total length of the round in seconds.
        /// </summary>
        public int RoundLength => _roundLength;

        /// <summary>
        /// Array of waves in the round.
        /// </summary>
        /// <seealso cref="WaveData"/>
        public WaveData[] Waves => _waves;
    }
}
