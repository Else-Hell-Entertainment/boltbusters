// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>

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
    public partial class DoorShaderComponent : ShaderComponent, IPulseEffect
    {
        [Export]
        private EffectShaderPreset _doorEffectShaderPreset;

        #region IAcceleratingPulseEffect Implementation

        void IPulseEffect.Pulse()
        {
            PlayDoorPulse();
        }

        Task IPulseEffect.PulseAsync(EffectAwaitPolicy policy)
        {
            return PlayDoorPulseAsync(policy);
        }

        #endregion IAcceleratingPulseEffect Implementation

        #region Accelerating Pulse

        public void PlayDoorPulse()
        {
            if (_doorEffectShaderPreset == null)
            {
                this.LogWarning($"DoorShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayPulseAsync(_doorEffectShaderPreset, EffectAwaitPolicy.Interruptible);
        }

        public Task PlayDoorPulseAsync(EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible)
        {
            if (_doorEffectShaderPreset == null)
            {
                this.LogWarning($"DoorShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayPulseAsync(_doorEffectShaderPreset, policy);
        }

        #endregion Accelerating Pulse
    }
}
