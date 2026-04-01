// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// ShaderComponent specialization for collectibles.
    /// </summary>
    [GlobalClass]
    public partial class CollectibleShaderComponent : ShaderComponent
    {
        [Export]
        private EffectShaderPreset _effectsPreset;

        private EffectShaderPreset EffectsPreset
        {
            get { return _effectsPreset; }
        }

        #region Flash

        /// <summary>
        /// Plays the collectible's flash effect. Returns a Task that completes
        /// when this flash finishes its timeline or is overridden by another effect.
        /// </summary>
        public Task PlayCollectibleFlashAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayFlashAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the collectible's flash effect without awaiting its completion.
        /// Use this when sequencing is not required.
        /// </summary>
        public void PlayCollectibleFlash()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayFlashAsync(_effectsPreset);
        }

        #endregion Flash

        #region Pulse

        /// <summary>
        /// Plays the collectible's pulse effect using the Pulse settings from the
        /// assigned EffectShaderPreset. Returns a Task that completes when the pulse
        /// finishes its timeline or is overridden.
        /// </summary>
        public Task PlayCollectiblePulseAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayPulseAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the collectible's pulse effect without awaiting its completion.
        /// Useful for simple visual triggers.
        /// </summary>
        public void PlayCollectiblePulse()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayPulseAsync(_effectsPreset);
        }

        #endregion Pulse

        #region Accelerating Pulse

        /// <summary>
        /// Plays the collectible's accelerating pulse effect. Returns a Task that completes
        /// when the accelerating pulse reaches its end or is overridden by another effect.
        /// </summary>
        public Task PlayCollectibleAcceleratingPulseAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayAcceleratingPulseAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the collectible's accelerating pulse effect without awaiting completion.
        /// Primarily used by the spawn sequence.
        /// </summary>
        public void PlayCollectibleAcceleratingPulse()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: CollectibleShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayAcceleratingPulseAsync(_effectsPreset);
        }

        #endregion Accelerating Pulse
    }
}
