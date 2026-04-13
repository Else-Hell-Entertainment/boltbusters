// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class ShaderComponent
    {
        #region Constants

        private const int EFFECT_MODE_NONE = 0;
        private const int EFFECT_MODE_FLASH = 1;
        private const int EFFECT_MODE_PULSE = 2;
        private const int EFFECT_MODE_ACCELERATING_PULSE = 3;

        #endregion Constants

        #region Protected Effect Play Methods

        /// <summary>
        /// Plays a one-shot flash effect based on the Flash settings in the given preset.
        /// If policy is Locked, other effects started while it runs will be ignored.
        /// </summary>
        protected async Task PlayFlashAsync(
            EffectShaderPreset preset,
            EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible
        )
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            if (preset.FlashStrength <= 0.0f || preset.FlashDuration <= 0.0f)
            {
                GD.PushWarning(
                    $"{Name}: PlayFlashAsync called, but preset has non-positive FlashStrength or FlashDuration. No flash will be played."
                );
                return;
            }

            if (_effectsLocked && policy == EffectAwaitPolicy.Interruptible)
            {
                // A locked effect is running; ignore interruptible requests.
                return;
            }

            int myVersion = ++_effectVersion;

            if (policy == EffectAwaitPolicy.Locked)
            {
                if (!TryAcquireLock(myVersion))
                {
                    return;
                }
            }

            // Flash overrides pulse visuals; stop driving pulse time while flash is active.
            _pulseTimeEnabled = false;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EFFECT_MODE_FLASH);
                material.SetShaderParameter("flash_color", preset.FlashColor);
                material.SetShaderParameter("flash_strength", preset.FlashStrength);
                material.SetShaderParameter("flash_value", 1.0f);
            }

            SceneTree tree = GetTree();
            await ToSignal(tree.CreateTimer(preset.FlashDuration, false), "timeout");

            if (!IsInsideTree())
            {
                ReleaseLockIfOwner(myVersion);
                return;
            }

            // Interruptible behaviour: if a newer effect started, stop.
            if (policy == EffectAwaitPolicy.Interruptible && myVersion != _effectVersion)
            {
                ReleaseLockIfOwner(myVersion);
                return;
            }

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("flash_value", 0.0f);
                material.SetShaderParameter("flash_strength", 0.0f);
                material.SetShaderParameter("effect_mode", EFFECT_MODE_NONE);
            }

            EmitSignal(SignalName.FlashFinished);

            ReleaseLockIfOwner(myVersion);
        }

        /// <summary>
        /// Starts a regular pulse effect.
        /// - If PulseUseDuration is false, the pulse runs indefinitely until overridden.
        /// - If true, it runs for PulseDuration and then fades out to 0.
        /// Pulse waveform uses shader uniform pulse_time, which is driven by _Process.
        /// </summary>
        protected async Task PlayPulseAsync(
            EffectShaderPreset preset,
            EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible
        )
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            if (preset.PulseStrength <= 0.0f)
            {
                GD.PushWarning(
                    $"{Name}: PlayPulseAsync called, but preset has non-positive PulseStrength. No pulse will be played."
                );
                return;
            }

            if (_effectsLocked && policy == EffectAwaitPolicy.Interruptible)
            {
                return;
            }

            int myVersion = ++_effectVersion;

            // Infinite pulse cannot be "locked await" because it never completes.
            if (!preset.PulseUseDuration && policy == EffectAwaitPolicy.Locked)
            {
                GD.PushWarning(
                    $"{Name}: PlayPulseAsync called with Locked policy, but PulseUseDuration is false. "
                        + "Infinite pulse cannot be awaited to completion; ignoring lock."
                );
                policy = EffectAwaitPolicy.Interruptible;
            }

            if (policy == EffectAwaitPolicy.Locked)
            {
                if (!TryAcquireLock(myVersion))
                {
                    return;
                }
            }

            // Reset and enable script-driven time for the pulse.
            _pulseTime = 0.0f;
            _pulseTimeEnabled = true;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EFFECT_MODE_PULSE);
                material.SetShaderParameter("pulse_color", preset.PulseColor);
                material.SetShaderParameter("pulse_strength", preset.PulseStrength);
                material.SetShaderParameter("pulse_speed", preset.PulseSpeed);
                material.SetShaderParameter("pulse_time", 0.0f);
            }

            // Infinite pulse: no completion. _Process will keep driving pulse_time.
            if (!preset.PulseUseDuration)
            {
                ReleaseLockIfOwner(myVersion);
                return;
            }

            float duration = Mathf.Max(0.0f, preset.PulseDuration);
            if (duration <= 0.0f)
            {
                foreach (ShaderMaterial material in _materials)
                {
                    material.SetShaderParameter("pulse_strength", 0.0f);
                    material.SetShaderParameter("effect_mode", EFFECT_MODE_NONE);
                }

                _pulseTimeEnabled = false;

                ReleaseLockIfOwner(myVersion);
                return;
            }

            SceneTree tree = GetTree();
            float time = 0.0f;

            while (time < duration)
            {
                if (!IsInsideTree())
                {
                    _pulseTimeEnabled = false;
                    ReleaseLockIfOwner(myVersion);
                    return;
                }

                // Interruptible: stop if overridden.
                if (policy == EffectAwaitPolicy.Interruptible && myVersion != _effectVersion)
                {
                    _pulseTimeEnabled = false;
                    ReleaseLockIfOwner(myVersion);
                    return;
                }

                await ToSignal(tree, "process_frame");

                if (tree.Paused)
                {
                    continue;
                }

                float dt = (float)GetProcessDeltaTime();
                time += dt;

                float progress = Mathf.Clamp(time / duration, 0.0f, 1.0f);
                float currentStrength = Mathf.Lerp(preset.PulseStrength, 0.0f, progress);

                foreach (ShaderMaterial material in _materials)
                {
                    material.SetShaderParameter("pulse_strength", currentStrength);
                }
            }

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("pulse_strength", 0.0f);
                material.SetShaderParameter("effect_mode", EFFECT_MODE_NONE);
            }

            _pulseTimeEnabled = false;

            EmitSignal(SignalName.PulseFinished);

            ReleaseLockIfOwner(myVersion);
        }

        /// <summary>
        /// Plays an accelerating pulse based on the Accelerating Pulse settings in the preset.
        /// Emits AcceleratingPulseFinished when done.
        /// </summary>
        protected async Task PlayAcceleratingPulseAsync(
            EffectShaderPreset preset,
            EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible
        )
        {
            if (!ValidatePresetAndMaterials(preset))
            {
                return;
            }

            float totalTime = Mathf.Max(0.0f, preset.TotalAcceleratingPulseTime);
            if (totalTime <= 0.0f)
            {
                GD.PushWarning(
                    $"{Name}: PlayAcceleratingPulseAsync called, but preset has non-positive TotalAcceleratingPulseTime. No accelerating pulse will be played."
                );
                return;
            }

            if (_effectsLocked && policy == EffectAwaitPolicy.Interruptible)
            {
                return;
            }

            float delay = Mathf.Max(0.0f, preset.AccelerationDelay);
            float accelDuration = Mathf.Max(0.0f, preset.AcceleratingPulseDuration);

            int myVersion = ++_effectVersion;

            if (policy == EffectAwaitPolicy.Locked)
            {
                if (!TryAcquireLock(myVersion))
                {
                    return;
                }
            }

            // Accelerating pulse overrides pulse visuals; stop driving pulse time while it is active.
            _pulseTimeEnabled = false;

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("effect_mode", EFFECT_MODE_ACCELERATING_PULSE);
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

                material.SetShaderParameter("accelerating_pulse_time", 0.0f);
                material.SetShaderParameter("accelerating_pulse_progress", 0.0f);
            }

            SceneTree tree = GetTree();
            float t = 0.0f;

            while (t < totalTime)
            {
                if (!IsInsideTree())
                {
                    ReleaseLockIfOwner(myVersion);
                    return;
                }

                if (policy == EffectAwaitPolicy.Interruptible && myVersion != _effectVersion)
                {
                    ReleaseLockIfOwner(myVersion);
                    return;
                }

                await ToSignal(tree, "process_frame");

                if (tree.Paused)
                {
                    continue;
                }

                t += (float)GetProcessDeltaTime();

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
            }

            foreach (ShaderMaterial material in _materials)
            {
                material.SetShaderParameter("accelerating_pulse_time", 0.0f);
                material.SetShaderParameter("accelerating_pulse_progress", 0.0f);
                material.SetShaderParameter("accelerating_pulse_start_strength", 0.0f);
                material.SetShaderParameter("accelerating_pulse_finish_strength", 0.0f);
                material.SetShaderParameter("effect_mode", EFFECT_MODE_NONE);
            }

            EmitSignal(SignalName.AcceleratingPulseFinished);

            ReleaseLockIfOwner(myVersion);
        }

        #endregion Protected Effect Play Methods
    }
}
