#include "defs.hlsl"

#ifndef DisplacementStrength
#define DisplacementStrength 0.2
#endif

#ifndef HasDisplacementTex
#define HasDisplacementTex 0
#endif

#if HasDisplacementTex
Texture2D displacementTexture;
SamplerState displacementTextureSampler;
#endif

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
    PixelInput output;
#if VtxColors
    output.color = float4(bary.x * tri[0].color.rgb + bary.y * tri[1].color.rgb + bary.z * tri[2].color.rgb, 1);
#endif

    output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);

#if VtxUVs
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
#endif

#if VtxNormals
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
#endif

#if VtxTangents
    output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;
#endif

#if HasDisplacementTex
    output.position.xyz = ComputeDisplacement(displacementTexture, displacementTextureSampler, DisplacementStrength, output.position.xyz, output.tex.xy, output.normal);
#endif

    output.position = mul(output.position, viewProj);
    
#if VtxUVs
    output.tex = output.tex;
#endif

#if VtxNormals
    output.normal = normalize(output.normal);
#endif

#if VtxTangents
    output.tangent = normalize(output.tangent);
#endif

    return output;
}