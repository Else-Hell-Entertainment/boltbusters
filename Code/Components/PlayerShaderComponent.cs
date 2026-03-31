// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// ShaderComponent specialization for the player.
    /// </summary>
    [GlobalClass]
    public partial class PlayerShaderComponent : ShaderComponent
    {
        [Export]
        private EffectShaderPreset _effectsPreset;

        /// <summary>
        /// Effect preset used by this player instance.
        /// Configure Flash (damage) and Pulse (healing) values here.
        /// </summary>
        private EffectShaderPreset EffectsPreset
        {
            get { return _effectsPreset; }
        }

        #region Damage Flash

        /// <summary>
        /// Plays the player's damage flash effect. Returns a Task that completes
        /// when this flash finishes its timeline or is overridden by another effect.
        /// </summary>
        public Task PlayPlayerDamageFlashAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayFlashAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the player's damage flash effect without awaiting its completion.
        /// Use this in gameplay code when sequencing is not required.
        /// </summary>
        public void PlayPlayerDamageFlash()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayFlashAsync(_effectsPreset);
        }

        #endregion Damage Flash

        #region Healing Pulse

        /// <summary>
        /// Plays the player's healing pulse effect using the Pulse settings in the
        /// assigned EffectShaderPreset. Returns a Task that completes when the pulse
        /// finishes its timeline or is overridden by another effect.
        /// </summary>
        public Task PlayPlayerHealingPulseAsync()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return Task.CompletedTask;
            }

            return PlayPulseAsync(_effectsPreset);
        }

        /// <summary>
        /// Starts the player's healing pulse effect without awaiting its completion.
        /// Use this when a simple visual trigger is sufficient.
        /// </summary>
        public void PlayPlayerHealingPulse()
        {
            if (_effectsPreset == null)
            {
                GD.PushWarning($"{Name}: PlayerShaderComponent has no EffectShaderPreset assigned.");
                return;
            }

            _ = PlayPulseAsync(_effectsPreset);
        }

        #endregion Healing Pulse
    }
}
