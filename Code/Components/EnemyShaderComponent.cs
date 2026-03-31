// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// ShaderComponent specialization for enemies.
    /// </summary>
    [GlobalClass]
    public partial class EnemyShaderComponent : ShaderComponent
    {
        [Export]
        private EffectShaderPreset _effectsPreset;

        /// <summary>
        /// Effect preset used by this enemy instance.
        /// Configure colors, strengths and timings per enemy type.
        /// </summary>
        private EffectShaderPreset EffectsPreset
        {
            get { return _effectsPreset; }
        }

        #region Damage Flash

        /// <summary>
        /// Plays the enemy's damage flash effect. Returns a Task that completes
        /// when this flash finishes its timeline or is overridden by another effect.
        /// </summary>
        public Task PlayEnemyDamageFlashAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: EnemyShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayFlashAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the enemy's damage flash effect without awaiting its completion.
        /// Use this in gameplay code when sequencing is not required.
        /// </summary>
        public void PlayEnemyDamageFlash()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: EnemyShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayFlashAsync(_effectsPreset);
        }

        #endregion Damage Flash
    }
}
