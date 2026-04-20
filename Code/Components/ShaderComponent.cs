// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for shader effect components.
    /// Handles:
    /// - Preparing unique ShaderMaterial instances for configured meshes.
    /// - Driving Flash, Pulse and Accelerating Pulse shader parameters.
    /// Derived classes provide the specific EffectShaderPreset to use.
    /// </summary>
    [GlobalClass]
    public abstract partial class ShaderComponent : Node
    {
        #region Fields

        private int _effectVersion = 0;

        // Pulse TIME replacement (script-driven), needed especially for infinite pulses.
        private bool _pulseTimeEnabled = false;
        private float _pulseTime = 0.0f;

        #endregion Fields

        #region Signals

        /// <summary>
        /// Emitted when the flash finishes normally (not overridden).
        /// </summary>
        [Signal]
        public delegate void FlashFinishedEventHandler();

        /// <summary>
        /// Emitted when a duration-based pulse finishes normally (not overridden).
        /// Not emitted for infinite pulses (PulseUseDuration == false).
        /// </summary>
        [Signal]
        public delegate void PulseFinishedEventHandler();

        /// <summary>
        /// Emitted when the accelerating pulse finishes normally (not overridden).
        /// </summary>
        [Signal]
        public delegate void AcceleratingPulseFinishedEventHandler();

        #endregion Signals

        #region Godot Callbacks

        public override void _Ready()
        {
            PrepareMaterials();
        }

        public override void _Process(double delta)
        {
            if (!_pulseTimeEnabled)
            {
                return;
            }

            _pulseTime += (float)delta;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("pulse_time", _pulseTime);
            }
        }

        /// <summary>
        /// Safety net: ensures locks do not remain held if the node leaves the tree mid-effect.
        /// </summary>
        public override void _ExitTree()
        {
            _effectsLocked = false;
            _lockOwnerVersion = 0;
            _pulseTimeEnabled = false;
        }

        #endregion Godot Callbacks


        #region Private Helpers

        /// <summary>
        /// Shared precondition checks for effect play calls.
        /// </summary>
        private bool ValidatePresetAndMaterials(EffectShaderPreset preset)
        {
            if (preset == null)
            {
                GD.PushWarning($"{Name}: ShaderComponent effect called, but EffectShaderPreset is null.");
                return false;
            }

            if (_materials.Count == 0)
            {
                GD.PushWarning(
                    $"{Name}: ShaderComponent has no prepared materials. Check mesh overlay materials and shader setup."
                );
                return false;
            }

            return true;
        }

        #endregion Private Helpers
    }
}
