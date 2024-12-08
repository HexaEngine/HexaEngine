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

    output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
    output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;

#if HasDisplacementTex
    output.position.xyz = ComputeDisplacement(displacementTexture, displacementTextureSampler, DisplacementStrength, output.position.xyz, output.tex.xy, output.normal);
#endif

    output.position = mul(output.position, viewProj);
    output.tex = output.tex;
    output.normal = normalize(output.normal);
    output.tangent = normalize(output.tangent);

    return output;
}