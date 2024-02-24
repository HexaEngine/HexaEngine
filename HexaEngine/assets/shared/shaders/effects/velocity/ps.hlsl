#include "../../camera.hlsl"

Texture2D<float> depth_texture : register(t0);

SamplerState linear_wrap_sampler : register(s0);

cbuffer VelocityBufferParam
{
#ifndef Scale
    float Scale;
#endif
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 GetClipSpacePosition(float2 texcoord, float depth)
{
    float4 clipSpaceLocation;
    clipSpaceLocation.xy = texcoord * 2.0f - 1.0f;
    clipSpaceLocation.y *= -1;
    clipSpaceLocation.z = depth;
    clipSpaceLocation.w = 1.0f;

    return clipSpaceLocation;
}

float2 main(VSOut pin) : SV_Target0
{
    float depth = depth_texture.Sample(linear_wrap_sampler, pin.Tex);
    float4 clip_space_position = GetClipSpacePosition(pin.Tex, depth);

    float4 world_pos = mul(clip_space_position, viewProjInv);

    world_pos /= world_pos.w;

    float4 prev_clip_space_position = mul(world_pos, prevViewProj);

    prev_clip_space_position /= prev_clip_space_position.w;

    float2 velocity = (clip_space_position - prev_clip_space_position).xy / Scale;

    return velocity;
}