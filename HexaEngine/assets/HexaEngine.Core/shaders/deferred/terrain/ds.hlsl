#include "defs.hlsl"

SamplerState linearClampSampler : register(s0);
Texture2D maskTex : register(t0);

#ifndef DisplacementStrength0
#define DisplacementStrength0 0.2
#endif

#ifndef DisplacementStrength1
#define DisplacementStrength1 0.2
#endif

#ifndef DisplacementStrength2
#define DisplacementStrength2 0.2
#endif

#ifndef DisplacementStrength3
#define DisplacementStrength3 0.2
#endif

#ifndef HasDisplacementTex0
#define HasDisplacementTex0 0
#endif
#ifndef HasDisplacementTex1
#define HasDisplacementTex1 0
#endif
#ifndef HasDisplacementTex2
#define HasDisplacementTex2 0
#endif
#ifndef HasDisplacementTex3
#define HasDisplacementTex3 0
#endif

#if HasDisplacementTex0
Texture2D displacementTexture0;
SamplerState displacementTextureSampler0;
#endif

#if HasDisplacementTex1
Texture2D displacementTexture1;
SamplerState displacementTextureSampler1;
#endif

#if HasDisplacementTex2
Texture2D displacementTexture2;
SamplerState displacementTextureSampler2;
#endif

#if HasDisplacementTex3
Texture2D displacementTexture3;
SamplerState displacementTextureSampler3;
#endif

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
    PixelInput output;

    output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);
    output.ctex = output.position.xz / TILESIZE;
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
    output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;

#if HasDisplacementTex0 | HasDisplacementTex1 | HasDisplacementTex2 | HasDisplacementTex3
    float4 mask = maskTex.SampleLevel(linearClampSampler, output.ctex, 0).xyzw;
#if HasDisplacementTex0
    float3 position0;
#else
    float3 position0 = output.position.xyz;
#endif

#if HasDisplacementTex1
    float3 position1;
#else
    float3 position1 = output.position.xyz;
#endif

#if HasDisplacementTex2
    float3 position2;
#else
    float3 position2 = output.position.xyz;
#endif

#if HasDisplacementTex3
    float3 position3;
#else
    float3 position3 = output.position.xyz;
#endif

#if HasDisplacementTex0
    position0 = ComputeDisplacement(displacementTexture0, displacementTextureSampler0, DisplacementStrength0, output.position.xyz, output.tex, output.normal);
#endif

#if HasDisplacementTex1
    position1 = ComputeDisplacement(displacementTexture1, displacementTextureSampler1, DisplacementStrength1, output.position.xyz, output.tex, output.normal);
#endif

#if HasDisplacementTex2
    position2 = ComputeDisplacement(displacementTexture2, displacementTextureSampler2, DisplacementStrength2, output.position.xyz, output.tex, output.normal);
#endif

#if HasDisplacementTex3
    position3 = ComputeDisplacement(displacementTexture3, displacementTextureSampler3, DisplacementStrength3, output.position.xyz, output.tex, output.normal);
#endif

    float3 position = 0;
    position = Mask(position, position0, mask.x);
    position = Mask(position, position1, mask.y);
    position = Mask(position, position2, mask.z);
    position = Mask(position, position3, mask.w);

    output.position.xyz = position;

#endif

    output.position = mul(output.position, viewProj);

    return output;
}