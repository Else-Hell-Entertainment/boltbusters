shader_type spatial;
render_mode unshaded;

uniform sampler2D gradient_tex;
uniform sampler2D noise_tex;
uniform float speed = 3.0;
uniform float alpha_strength = 1.0;
uniform vec3 color : source_color = vec3(0.8, 0.8, 0.8);
uniform float brightness = 1.0;


void fragment() {
	vec2 pos = vec2(UV.y, UV.x);
	//vec2 noise_pos = UV + vec2(0.0, TIME * speed);
	
	float angle = TIME * speed;
	vec2 centered_uv = UV - 0.5;
	vec2 noise_pos = vec2(
    centered_uv.x * cos(angle) - centered_uv.y * sin(angle),
    centered_uv.x * sin(angle) + centered_uv.y * cos(angle)
) + 0.5;

	float gradient = texture(gradient_tex, pos).r;
	float noise = texture(noise_tex, noise_pos).r;
	ALBEDO = color * noise * gradient * brightness;
	ALPHA = gradient * alpha_strength;
	
}