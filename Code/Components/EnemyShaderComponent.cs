// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Plays the enemy damage flash effect.
    /// When awaited, completes when the flash finishes or is overridden,
    /// depending on the selected await policy.
    /// </summary>
    [GlobalClass]
    public partial class EnemyShaderComponent : ShaderComponent, IFlashEffect
    {
        [Export]
        private EffectShaderPreset _enemyEffectShaderPreset;

        #region IFlashEffect Implementation

        void IFlashEffect.Flash()
        {
            PlayEnemyDamageFlash();
        }

        Task IFlashEffect.FlashAsync(EffectAwaitPolicy policy)
        {
            return PlayEnemyDamageFlashAsync(policy);
        }

        #endregion IFlashEffect Implementation

        #region Flash

        public void PlayEnemyDamageFlash()
        {
            if (_enemyEffectShaderPreset == null)
            {
                GD.PushWarning($"{Name}: EnemyShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayFlashAsync(_enemyEffectShaderPreset, EffectAwaitPolicy.Interruptible);
        }

        public Task PlayEnemyDamageFlashAsync(EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible)
        {
            if (_enemyEffectShaderPreset == null)
            {
                GD.PushWarning($"{Name}: EnemyShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayFlashAsync(_enemyEffectShaderPreset, policy);
        }

        #endregion Flash
    }
}
