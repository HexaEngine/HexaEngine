#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"

Texture2D atlasTex : register(t0);
SamplerState samplerState : register(s0);

#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metallic
#define Metallic 0.0f
#endif
#ifndef Ao
#define Ao 1
#endif
#ifndef Emissive
#define Emissive float3(0,0,0);
#endif

SamplerState linearClampSampler : register(s8);
SamplerState linearWrapSampler : register(s9);
SamplerState pointClampSampler : register(s10);
SamplerComparisonState shadowSampler : register(s11);

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 TexCoord : TEXCOORD;
};

GeometryData main(PixelInputType input) : SV_TARGET
{
    float4 baseColor = atlasTex.Sample(samplerState, input.TexCoord.xy);
    
    if (baseColor.a == 0)
        discard;

    float ao = Ao;
    float roughness = Roughness;
    float metallic = Metallic;
    float3 emissive = Emissive;

    float3 N = input.Normal;

    return PackGeometryData(0, baseColor.xyz, normalize(N), roughness, metallic, 0.0f, ao, 0.0f, emissive, 1.0f);
}
