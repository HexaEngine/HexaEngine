#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"

Texture2D txDiffuse : register(t0);
SamplerState linear_wrap_sampler : register(s0);

cbuffer TexParams
{
    float3 diffuse;
    float albedo_factor;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEX;
    float3 Normal : NORMAL;
};

GeometryData main(PS_INPUT IN)
{
    IN.TexCoord.y = 1 - IN.TexCoord.y;
    float4 texColor = txDiffuse.Sample(linear_wrap_sampler, IN.TexCoord) * float4(diffuse, 1.0) * albedo_factor;
    if (texColor.a < 0.5f)
        discard;
    
    return PackGeometryData(0, texColor.xyz, normalize(IN.Normal), 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f);
}