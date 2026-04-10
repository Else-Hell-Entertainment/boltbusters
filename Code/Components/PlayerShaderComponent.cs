// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Plays the player damage flash effect.
    /// When awaited, completes when the flash finishes or is overridden,
    /// depending on the selected await policy.
    /// </summary>
    [GlobalClass]
    public partial class PlayerShaderComponent : ShaderComponent, IFlashEffect
    {
        [Export]
        private EffectShaderPreset _playerEffectShaderPreset;

        #region IFlashEffect Implementation

        void IFlashEffect.Flash()
        {
            PlayPlayerDamageFlash();
        }

        Task IFlashEffect.FlashAsync(EffectAwaitPolicy policy)
        {
            return PlayPlayerDamageFlashAsync(policy);
        }

        #endregion IFlashEffect Implementation

        #region Flash

        public void PlayPlayerDamageFlash()
        {
            if (_playerEffectShaderPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayFlashAsync(_playerEffectShaderPreset, EffectAwaitPolicy.Interruptible);
        }

        public Task PlayPlayerDamageFlashAsync(EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible)
        {
            if (_playerEffectShaderPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayFlashAsync(_playerEffectShaderPreset, policy);
        }

        #endregion Flash
    }
}
