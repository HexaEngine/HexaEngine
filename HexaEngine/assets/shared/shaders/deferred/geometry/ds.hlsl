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
    output.pos = output.position.xyz;

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
    output.pos = output.position.xyz;
#endif
    
#if VtxUVs
    output.tex = output.tex;
#endif

#if VtxNormals
    output.normal = normalize(output.normal);
#endif

#if VtxTangents
    output.tangent = normalize(output.tangent);

    float3 N = output.normal;
    float3 T = normalize(output.tangent - dot(output.tangent, N) * N);
    float3 B = cross(N, T);
	output.binormal = B;
	float3x3 TBN = float3x3(T, B, N);
	output.tangentViewPos = mul(camPos, TBN);
	output.tangentPos = mul(output.pos, TBN);

#endif

    output.position = mul(output.position, viewProj);

    return output;
}