// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Stores all shader effect settings for a given object type (e.g. Player, Enemy, Collectible).
    /// Contains independent configuration blocks for:
    /// - Flash effect
    /// - Pulse effect
    /// - Accelerating Pulse effect
    /// Future effects can be added as new export groups.
    /// </summary>
    [GlobalClass]
    public partial class EffectShaderPreset : Resource
    {
        #region Flash Settings

        [ExportGroup("Flash Settings")]
        [Export]
        private Color _flashColor = Colors.White;

        [Export]
        private float _flashStrength = 0.0f;

        [Export]
        private float _flashDuration = 0.0f;

        /// <summary>
        /// Color used for the flash effect.
        /// </summary>
        public Color FlashColor
        {
            get { return _flashColor; }
        }

        /// <summary>
        /// Emission intensity during the flash.
        /// Set to 0 to effectively disable the flash.
        /// </summary>
        public float FlashStrength
        {
            get { return _flashStrength; }
        }

        /// <summary>
        /// Duration (in seconds) of the flash effect before it fades to zero.
        /// </summary>
        public float FlashDuration
        {
            get { return _flashDuration; }
        }

        #endregion Flash Settings

        #region Pulse Settings

        [ExportGroup("Pulse Settings")]
        [Export]
        private Color _pulseColor = Colors.White;

        [Export]
        private float _pulseStrength = 0.0f;

        [Export]
        private float _pulseSpeed = 0.0f;

        /// <summary>
        /// If true, the pulse runs for PulseDuration and then fades out.
        /// If false, the pulse runs indefinitely until manually stopped/replaced.
        /// </summary>
        [Export]
        private bool _pulseUseDuration = false;

        [Export]
        private float _pulseDuration = 0.0f;

        /// <summary>
        /// Color used for the regular pulse effect.
        /// </summary>
        public Color PulseColor
        {
            get { return _pulseColor; }
        }

        /// <summary>
        /// Emission intensity for the regular pulse.
        /// Set to 0 to effectively disable the pulse.
        /// </summary>
        public float PulseStrength
        {
            get { return _pulseStrength; }
        }

        /// <summary>
        /// Oscillation speed of the pulse.
        /// </summary>
        public float PulseSpeed
        {
            get { return _pulseSpeed; }
        }

        /// <summary>
        /// If true, the pulse has a limited duration and should fade out
        /// and reset when PulseDuration has elapsed.
        /// </summary>
        public bool PulseUseDuration
        {
            get { return _pulseUseDuration; }
        }

        /// <summary>
        /// Duration (in seconds) of the pulse when PulseUseDuration is true.
        /// </summary>
        public float PulseDuration
        {
            get { return _pulseDuration; }
        }

        #endregion Pulse Settings

        #region Accelerating Pulse Settings

        [ExportGroup("Accelerating Pulse Settings")]
        [Export]
        private Color _acceleratingPulseColor = Colors.White;

        [Export]
        private float _acceleratingPulseStartStrength = 0.0f;

        [Export]
        private float _acceleratingPulseFinishStrength = 0.0f;

        /// <summary>
        /// Base oscillation speed at the start of the accelerating pulse.
        /// </summary>
        [Export]
        private float _acceleratingPulseSpeed = 0.0f;

        /// <summary>
        /// Controls how quickly the pulse speed increases over the accelerating phase.
        /// </summary>
        [Export]
        private float _accelerationSpeedFactor = 0.0f;

        /// <summary>
        /// Time (in seconds) the effect stays at the start speed and strength
        /// before the accelerating phase begins.
        /// </summary>
        [Export]
        private float _accelerationDelay = 0.0f;

        /// <summary>
        /// Duration (in seconds) of the accelerating phase, during which
        /// both strength and speed interpolate from their start values
        /// to their final values.
        /// </summary>
        [Export]
        private float _acceleratingPulseDuration = 0.0f;

        /// <summary>
        /// Color used for the accelerating pulse.
        /// </summary>
        public Color AcceleratingPulseColor
        {
            get { return _acceleratingPulseColor; }
        }

        /// <summary>
        /// Emission intensity at the beginning of the accelerating pulse.
        /// </summary>
        public float AcceleratingPulseStartStrength
        {
            get { return _acceleratingPulseStartStrength; }
        }

        /// <summary>
        /// Emission intensity at the end of the accelerating pulse.
        /// </summary>
        public float AcceleratingPulseFinishStrength
        {
            get { return _acceleratingPulseFinishStrength; }
        }

        /// <summary>
        /// Base oscillation speed at the start of the pulse.
        /// </summary>
        public float AcceleratingPulseSpeed
        {
            get { return _acceleratingPulseSpeed; }
        }

        /// <summary>
        /// Amount by which the pulse speed increases over the accelerating phase.
        /// </summary>
        public float AccelerationSpeedFactor
        {
            get { return _accelerationSpeedFactor; }
        }

        /// <summary>
        /// Delay before the accelerating phase begins.
        /// During this time the pulse uses the start strength and base speed.
        /// </summary>
        public float AccelerationDelay
        {
            get { return _accelerationDelay; }
        }

        /// <summary>
        /// Duration of the accelerating phase (0 → 1 progress).
        /// </summary>
        public float AcceleratingPulseDuration
        {
            get { return _acceleratingPulseDuration; }
        }

        /// <summary>
        /// Convenience property: total time from start until the end of the
        /// accelerating pulse (delay + accelerating phase).
        /// </summary>
        public float TotalAcceleratingPulseTime
        {
            get { return _accelerationDelay + _acceleratingPulseDuration; }
        }

        #endregion Accelerating Pulse Settings
    }
}
