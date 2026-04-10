shader_type spatial;
render_mode blend_add;

// ------------------------------------------------------------
// EFFECT MODE
// 0 = Flash
// 1 = Pulse
// 2 = Accelerating Pulse
// ------------------------------------------------------------
uniform int effect_mode : hint_enum("None", "Flash", "Pulse", "AcceleratingPulse") = 0;

// ------------------------------------------------------------
// FLASH SETTINGS
// Driven by: FlashColor, FlashStrength, FlashDuration (C#)
// Shader needs current flash_value to know how "bright" the flash is.
// ------------------------------------------------------------
uniform vec3 flash_color : source_color = vec3(1.0, 1.0, 1.0);
uniform float flash_strength = 0.0;
// Typically driven from C# (0.0 → 1.0 → fade back to 0.0)
uniform float flash_value : hint_range(0.0, 1.0) = 0.0;

// ------------------------------------------------------------
// PULSE SETTINGS
// Driven by: PulseColor, PulseStrength, PulseSpeed, PulseDuration, PulseUseDuration (C#)
// Shader uses TIME and pulse_speed to create a sine wave.
// C# is responsible for any duration / fade-out behavior by adjusting strength.
// ------------------------------------------------------------
uniform vec3 pulse_color : source_color = vec3(1.0, 1.0, 1.0);
uniform float pulse_strength = 0.0;
uniform float pulse_speed = 0.0;
uniform float pulse_time = 0.0;

// ------------------------------------------------------------
// ACCELERATING PULSE SETTINGS
// Driven by:
//   AcceleratingPulseColor
//   AcceleratingPulseStartStrength
//   AcceleratingPulseFinishStrength
//   AcceleratingPulseSpeed
//   AccelerationSpeedFactor
//   AccelerationDelay
//   AcceleratingPulseDuration
//
// Shader expects:
//   accelerating_pulse_time      → local time for this effect (C# updates each frame).
//   accelerating_pulse_progress  → 0 → 1 during the accelerating phase only.
//      - During AccelerationDelay, progress should remain 0.
//      - During AcceleratingPulseDuration, progress should go 0 → 1.
// ------------------------------------------------------------
uniform vec3 accelerating_pulse_color : source_color = vec3(1.0, 1.0, 1.0);

uniform float accelerating_pulse_start_strength = 0.0;
uniform float accelerating_pulse_finish_strength = 0.0;

uniform float accelerating_pulse_speed = 0.0;
uniform float acceleration_speed_factor = 0.0;

uniform float acceleration_delay = 0.0;
uniform float accelerating_pulse_duration = 0.0;

// Controlled by script:
uniform float accelerating_pulse_time = 0.0;
uniform float accelerating_pulse_progress : hint_range(0.0, 1.0) = 0.0;

// ------------------------------------------------------------
// FRAGMENT LOGIC
// ------------------------------------------------------------
void fragment()
{
    // Final values for this pixel
    vec3 effect_color = vec3(0.0, 0.0, 0.0);
    float strength_final = 0.0;
    float v = 0.0; // waveform / mask in [0, 1]

	// --------------------------------------------------------
	// Default no effect
	// --------------------------------------------------------
	if (effect_mode == 0)
	{
		// None: no effect
	}
	
    // --------------------------------------------------------
    // FLASH
    // --------------------------------------------------------
    else if (effect_mode == 1)
    {
        effect_color = flash_color;
        strength_final = flash_strength;

        // flash_value is driven by script (0 → 1 → 0).
        v = flash_value;
    }

    // --------------------------------------------------------
    // CONSTANT / REGULAR PULSE
    // --------------------------------------------------------
    else if (effect_mode == 2)
    {
        effect_color = pulse_color;
        strength_final = pulse_strength;

        // Classic sine pulse using pulse_time and pulse_speed.
        float sin_wave = sin(pulse_time * pulse_speed) * 0.5 + 0.5;
		float wave = clamp(sin_wave, 0.0, 1.0);
        v = wave;
    }

    // --------------------------------------------------------
    // ACCELERATING PULSE
    // --------------------------------------------------------
    else if (effect_mode == 3)
    {
        effect_color = accelerating_pulse_color;

        // Two-phase behavior:
        // 1) AccelerationDelay: constant start strength & base speed.
        // 2) AcceleratingPulseDuration: strength lerps start → finish,
        //    speed increases based on acceleration_speed_factor and progress.
        if (accelerating_pulse_time < acceleration_delay)
        {
            // Delay phase: hold at start strength, base speed.
            float sin_wave = sin(accelerating_pulse_time * accelerating_pulse_speed) * 0.5 + 0.5;
			float wave = clamp(sin_wave, 0.0, 1.0);
            v = wave;
            strength_final = accelerating_pulse_start_strength;
        }
        else
        {
            // Accelerating phase:
            //   - accelerating_pulse_progress: 0 → 1 during this phase.
            //   - speed increases over time.
            float dynamic_speed =
                accelerating_pulse_speed +
                acceleration_speed_factor * accelerating_pulse_progress;

            float sin_wave = sin(accelerating_pulse_time * dynamic_speed) * 0.5 + 0.5;
			float wave = clamp(sin_wave, 0.0, 1.0);
            v = wave;

            // Strength interpolates from start → finish over the accelerating phase.
            strength_final = mix(
                accelerating_pulse_start_strength,
                accelerating_pulse_finish_strength,
                accelerating_pulse_progress
            );
        }
    }

    // --------------------------------------------------------
    // APPLY EMISSION
    // --------------------------------------------------------
    // v is in [0,1]; strength_final controls intensity.
    vec3 emission = effect_color * v * strength_final;

    EMISSION = emission;

    // Use strength as alpha for additive blending (clamped to [0,1]).
    ALPHA = clamp(strength_final, 0.0, 1.0);
}