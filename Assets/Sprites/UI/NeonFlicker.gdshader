shader_type canvas_item;
render_mode blend_add;

uniform vec4 glow_color : source_color = vec4(0.0, 1.0, 1.0, 1.0);
uniform float glow_strength = 1.5;
uniform float glow_radius = 3.0;

uniform float flicker_speed = 8.0;
uniform float flicker_amount = 0.2;

float rand(float n) {
	return fract(sin(n) * 43758.5453);
}

void fragment() {
	vec4 base = texture(TEXTURE, UV);

	// Discard transparent pixels
	if (base.a < 0.01) {
		COLOR = vec4(0.0);
	}
	else
	{
		// --- Flicker ---
		float time_noise = rand(floor(TIME * flicker_speed));
		float flicker = 1.0 + (time_noise - 0.5) * flicker_amount;

		// --- Glow sampling ---
		vec4 glow = vec4(0.0);
		float radius = glow_radius / TEXTURE_PIXEL_SIZE.x;

		for (int x = -1; x <= 1; x++) {
	        for (int y = -1; y <= 1; y++) {
				vec2 offset = vec2(float(x), float(y)) * radius * TEXTURE_PIXEL_SIZE;
				glow += texture(TEXTURE, UV + offset);
			}
		}

		glow /= 25.0;
		glow *= glow_color * glow_strength * flicker + 0.3 * UV.x - 0.01 * UV.y;

		// Final color
		vec3 final_rgb = 0.5 * COLOR.rgb + glow.rgb;
		float final_alpha = base.a;// + glow.a * flicker;
		COLOR = vec4(final_rgb, final_alpha);
	}

	//COLOR = shine(TEXTURE, UV, TEXTURE_PIXEL_SIZE, base);
}