// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Plays the collectible accelerating pulse.
    /// When awaited, completes when the pulse finishes or is overridden,
    /// depending on the selected await policy.
    /// </summary>
    [GlobalClass]
    public partial class CollectibleShaderComponent : ShaderComponent, IAcceleratingPulseEffect
    {
        [Export]
        private EffectShaderPreset _collectibleEffectShaderPreset;

        #region IAcceleratingPulseEffect Implementation

        void IAcceleratingPulseEffect.AcceleratingPulse()
        {
            PlayCollectibleAcceleratingPulse();
        }

        Task IAcceleratingPulseEffect.AcceleratingPulseAsync(EffectAwaitPolicy policy)
        {
            return PlayCollectibleAcceleratingPulseAsync(policy);
        }

        #endregion IAcceleratingPulseEffect Implementation

        #region Accelerating Pulse

        public void PlayCollectibleAcceleratingPulse()
        {
            if (_collectibleEffectShaderPreset == null)
            {
                this.LogWarning($"EffectShaderPreset not assigned.");
                return;
            }

            _ = PlayAcceleratingPulseAsync(_collectibleEffectShaderPreset, EffectAwaitPolicy.Interruptible);
        }

        public Task PlayCollectibleAcceleratingPulseAsync(EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible)
        {
            if (_collectibleEffectShaderPreset == null)
            {
                this.LogWarning($"EffectShaderPreset not assigned.");
                return Task.CompletedTask;
            }

            return PlayAcceleratingPulseAsync(_collectibleEffectShaderPreset, policy);
        }

        #endregion Accelerating Pulse
    }
}
