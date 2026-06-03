struct Uniforms {
    projection_view: mat4x4<f32>,
};

@group(0) @binding(0) var<uniform> uniforms: Uniforms;
@group(1) @binding(0) var t_diffuse: texture_2d<f32>;
@group(1) @binding(1) var s_diffuse: sampler;

struct VertexInput {
    // Per-vertex
    @location(0) position:   vec2<f32>,
    @location(1) uv: vec2<f32>,

    // Per-instance
    @location(2) inst_position: vec2<f32>,
    @location(3) inst_scale:    vec2<f32>,
    @location(4) inst_rotation: f32,
    @location(5) inst_zindex:   f32,
    @location(6) inst_color:    vec4<f32>,
};

struct VertexOutput {
    @builtin(position) clip_position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) uv: vec2<f32>,
};

fn rotate2d(v: vec2<f32>, angle: f32) -> vec2<f32> {
    let s = sin(angle);
    let c = cos(angle);
    return vec2<f32>(
        v.x * c - v.y * s,
        v.x * s + v.y * c,
    );
}

@vertex
fn vs_main(input: VertexInput) -> VertexOutput {
    let scaled   = input.position * input.inst_scale;
    let rotated  = rotate2d(scaled, input.inst_rotation);
    let world_xy = rotated + input.inst_position;

    let world_pos = vec4<f32>(world_xy, input.inst_zindex, 1.0);

    var out: VertexOutput;
    out.clip_position = uniforms.projection_view * world_pos;
    out.color = input.inst_color;
    out.uv = input.uv;
    return out;
}

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4<f32> {
    return textureSample(t_diffuse, s_diffuse, input.uv) * input.color;
}
