// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using System.Threading.Tasks;
using Godot;
using GenSysCollections = System.Collections.Generic;

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
        private const int EffectModeFlash = 0;
        private const int EffectModePulse = 1;
        private const int EffectModeAcceleratingPulse = 2;

        private int _effectVersion;

        [Export]
        private MeshInstance3D[] _meshes = new MeshInstance3D[0];

        private GenSysCollections.List<ShaderMaterial> _materials = new GenSysCollections.List<ShaderMaterial>();

        [Signal]
        public delegate void AcceleratingPulseFinishedEventHandler();

        public override void _Ready()
        {
            PrepareMaterials();
        }

        /// <summary>
        /// Duplicates overlay ShaderMaterials on configured meshes so that
        /// each instance has its own independent material for runtime changes.
        /// Logs warnings for any invalid or missing configuration.
        /// </summary>
        private void PrepareMaterials()
        {
            _materials.Clear();

            if (_meshes == null || _meshes.Length == 0)
            {
                GD.PushWarning($"{Name}: ShaderComponent has no meshes assigned. No effects will be visible.");
                return;
            }

            foreach (MeshInstance3D mesh in _meshes)
            {
                if (mesh == null)
                {
                    GD.PushWarning($"{Name}: A MeshInstance3D reference in _meshes is null.");
                    continue;
                }

                if (mesh.MaterialOverlay == null)
                {
                    GD.PushWarning(
                        $"{Name}: MaterialOverlay is NOT assigned on '{mesh.Name}'. Effects shader will not run."
                    );
                    continue;
                }

                if (mesh.MaterialOverlay is not ShaderMaterial shaderMaterial)
                {
                    GD.PushWarning(
                        $"{Name}: MaterialOverlay on '{mesh.Name}' is NOT a ShaderMaterial. "
                            + "Expected a ShaderMaterial using the effects shader."
                    );
                    continue;
                }

                ShaderMaterial uniqueMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
                uniqueMaterial.ResourceLocalToScene = true;

                mesh.MaterialOverlay = uniqueMaterial;
                _materials.Add(uniqueMaterial);
            }

            if (_materials.Count == 0)
            {
                GD.PushWarning(
                    $"{Name}: No valid ShaderMaterials prepared. "
                        + "Check that MaterialOverlay uses the correct effects shader."
                );
            }
        }

        // These are called by derived classes, which decide *which* preset to use.
        #region Protected Effect Play Methods

        /// <summary>
        /// Plays a one-shot flash effect based on the Flash settings in the given preset.
        /// </summary>
        protected async Task PlayFlashAsync(EffectShaderPreset preset)
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            if (preset.FlashStrength <= 0.0f || preset.FlashDuration <= 0.0f)
            {
                // No visible flash configured, nothing to do.
                GD.PushWarning(
                    $"{Name}: PlayFlashAsync called, but preset has non-positive FlashStrength or FlashDuration."
                        + " No flash will be played."
                );
                return;
            }

            int myVersion = ++_effectVersion;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EffectModeFlash);
                material.SetShaderParameter("flash_color", preset.FlashColor);
                material.SetShaderParameter("flash_strength", preset.FlashStrength);
                material.SetShaderParameter("flash_value", 1.0f);
            }

            // Hold full flash for the configured duration.
            await ToSignal(GetTree().CreateTimer(preset.FlashDuration), "timeout");

            // If a newer flash has started, stop this one.
            if (myVersion != _effectVersion)
            {
                return;
            }

            // Reset flash to "off".
            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("flash_value", 0.0f);
                material.SetShaderParameter("flash_strength", 0.0f);
            }
        }

        /// <summary>
        /// Starts a regular pulse effect. If PulseUseDuration is false, the pulse
        /// runs indefinitely. If true, it runs for PulseDuration and then fades out.
        /// </summary>
        protected async Task PlayPulseAsync(EffectShaderPreset preset)
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            if (preset.PulseStrength <= 0.0f)
            {
                // No visible pulse configured, nothing to do.
                GD.PushWarning(
                    $"{Name}: PlayPulseAsync called, but preset has non-positive PulseStrength. "
                        + "No pulse will be played."
                );
                return;
            }

            int myVersion = ++_effectVersion;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EffectModePulse);
                material.SetShaderParameter("pulse_color", preset.PulseColor);
                material.SetShaderParameter("pulse_strength", preset.PulseStrength);
                material.SetShaderParameter("pulse_speed", preset.PulseSpeed);
            }

            // Infinite pulse: shader handles it via TIME, no scripted fade-out.
            if (!preset.PulseUseDuration)
            {
                return;
            }

            // Simple fade-out model: over the configured PulseDuration,
            // reduce strength from the configured value down to 0.
            float duration = Mathf.Max(0.0f, preset.PulseDuration);
            if (duration <= 0.0f)
            {
                foreach (ShaderMaterial material in _materials)
                {
                    material.SetShaderParameter("pulse_strength", 0.0f);
                }
                return;
            }

            float time = 0.0f;
            while (time < duration)
            {
                // If a newer pulse started, stop this one.
                if (myVersion != _effectVersion)
                {
                    return;
                }

                float progress = Mathf.Clamp(time / duration, 0.0f, 1.0f);
                float currentStrength = Mathf.Lerp(preset.PulseStrength, 0.0f, progress);

                foreach (ShaderMaterial material in _materials)
                {
                    material.SetShaderParameter("pulse_strength", currentStrength);
                }

                if (!IsInsideTree())
                {
                    return;
                }
                await ToSignal(GetTree(), "process_frame");
                time += (float)GetProcessDeltaTime();
            }

            // Still the latest version? Then reset.
            if (myVersion != _effectVersion)
            {
                return;
            }

            // Ensure fully reset.
            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("pulse_strength", 0.0f);
            }
        }

        /// <summary>
        /// Plays an accelerating pulse based on the Accelerating Pulse settings in the preset.
        /// Emits AcceleratingPulseFinished when done.
        /// </summary>
        protected async Task PlayAcceleratingPulseAsync(EffectShaderPreset preset)
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            float totalTime = Mathf.Max(0.0f, preset.TotalAcceleratingPulseTime);
            if (totalTime <= 0.0f)
            {
                // No duration configured, nothing to play.
                GD.PushWarning(
                    $"{Name}: PlayAcceleratingPulseAsync called, but preset has non-positive "
                        + $"TotalAcceleratingPulseTime. No accelerating pulse will be played."
                );
                return;
            }

            float delay = Mathf.Max(0.0f, preset.AccelerationDelay);
            float accelDuration = Mathf.Max(0.0f, preset.AcceleratingPulseDuration);

            int myVersion = ++_effectVersion;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EffectModeAcceleratingPulse);
                material.SetShaderParameter("accelerating_pulse_color", preset.AcceleratingPulseColor);

                material.SetShaderParameter("accelerating_pulse_start_strength", preset.AcceleratingPulseStartStrength);
                material.SetShaderParameter(
                    "accelerating_pulse_finish_strength",
                    preset.AcceleratingPulseFinishStrength
                );

                material.SetShaderParameter("accelerating_pulse_speed", preset.AcceleratingPulseSpeed);
                material.SetShaderParameter("acceleration_speed_factor", preset.AccelerationSpeedFactor);

                material.SetShaderParameter("acceleration_delay", delay);
                material.SetShaderParameter("accelerating_pulse_duration", accelDuration);

                // Ensure initial state
                material.SetShaderParameter("accelerating_pulse_time", 0.0f);
                material.SetShaderParameter("accelerating_pulse_progress", 0.0f);
            }

            float t = 0.0f;
            while (t < totalTime)
            {
                // If a newer accelerating pulse started, stop this one.
                if (myVersion != _effectVersion)
                {
                    return;
                }

                float progress = 0.0f;

                if (t > delay && accelDuration > 0.0f)
                {
                    float accelTime = t - delay;
                    progress = Mathf.Clamp(accelTime / accelDuration, 0.0f, 1.0f);
                }

                foreach (ShaderMaterial material in _materials)
                {
                    material.SetShaderParameter("accelerating_pulse_time", t);
                    material.SetShaderParameter("accelerating_pulse_progress", progress);
                }

                if (!IsInsideTree())
                {
                    return;
                }
                await ToSignal(GetTree(), "process_frame");
                t += (float)GetProcessDeltaTime();
            }

            // Only the latest pulse should reset + emit signal.
            if (myVersion != _effectVersion)
            {
                return;
            }

            // Reset effect so nothing lingers.
            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("accelerating_pulse_time", 0.0f);
                material.SetShaderParameter("accelerating_pulse_progress", 0.0f);
                material.SetShaderParameter("accelerating_pulse_start_strength", 0.0f);
                material.SetShaderParameter("accelerating_pulse_finish_strength", 0.0f);
            }

            EmitSignal(SignalName.AcceleratingPulseFinished);
        }

        #endregion Protected Effect Play Methods

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

            if (_materials == null || _materials.Count == 0)
            {
                GD.PushWarning(
                    $"{Name}: ShaderComponent has no prepared materials. "
                        + "Check mesh overlay materials and shader setup."
                );
                return false;
            }

            return true;
        }
    }
}
