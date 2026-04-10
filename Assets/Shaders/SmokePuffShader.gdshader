shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;


uniform sampler2D noise_tex;
uniform float displacement = 0.5;
uniform float noise_scale : hint_range(0.1, 5.0) = 1.0;
uniform float time_scale = 1.0;
uniform float fade_progress : hint_range(0.0, 1.0) = 0.0;
uniform float fade_sharpness : hint_range(0.1, 2.0) = 0.5;

// Color controls
uniform vec3 base_color : source_color = vec3(0.8, 0.8, 0.8);
uniform float color_contrast : hint_range(0.0, 1.0) = 0.3;
uniform float noise_color_scale : hint_range(0.1, 5.0) = 2.0;

varying float vertex_noise;
varying float particle_life;

void vertex() {
	vec3 pos = VERTEX * noise_scale + vec3(TIME * time_scale);
	float noise_value = texture(noise_tex, pos.xy).r;
	noise_value += texture(noise_tex, pos.yz).r;
	noise_value += texture(noise_tex, pos.zx).r;
	noise_value /= 3.0;
	VERTEX += NORMAL * noise_value * displacement;
	
	vertex_noise = noise_value;
	particle_life = INSTANCE_CUSTOM.y;
}

void fragment() {
	float progress = 0.0;
	// Check if using particle system or code
	
	progress = particle_life > 0.001 || fade_progress > 0.001
		? max(particle_life, fade_progress) : 0.0;
	
	
	float alpha = smoothstep(progress - fade_sharpness, progress + fade_sharpness, vertex_noise);
	vec3 pos = VERTEX * noise_color_scale + vec3(TIME * time_scale * 0.5);
	float color_noise = texture(noise_tex, pos.xy).r;
	color_noise = texture(noise_tex, pos.yz).r;
	color_noise = texture(noise_tex, pos.zx).r;
	color_noise /= 3.0;
	
	float brightness = mix(vertex_noise, color_noise, 0.3);
	brightness = mix(1.0 - color_contrast, 1.0 + color_contrast, brightness);
	
	ALPHA = alpha;
	ALBEDO = base_color * brightness;
	
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
