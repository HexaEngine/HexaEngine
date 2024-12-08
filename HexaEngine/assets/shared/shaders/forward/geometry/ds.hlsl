#include "defs.hlsl"
#include "../../camera.hlsl"

#ifndef HasHeightTex
#define HasHeightTex 0
#endif

Texture2D heightTexture : register(t0);
SamplerState heightTextureSampler : register(s0);

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
	PixelInput output;
#if VtxColors
    output.color = float4(bary.x * tri[0].color.rgb + bary.y * tri[1].color.rgb + bary.z * tri[2].color.rgb, 1);
#endif

	output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);

#if (DEPTH != 1)
#if VtxUVs
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
#endif
#if VtxNormals
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
#endif
#if VtxTangents
    output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;
#endif
#else
#if VtxUVs
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
#endif
#if VtxNormals
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
#endif
#endif

	// Calculate the normal vector against the world matrix only.
#if (DEPTH != 1)
#if VtxNormals
	output.normal = normalize(output.normal);
#endif
#endif

#if HasHeightTex
#if (DEPTH != 1)
    float h = heightTexture.SampleLevel(heightTextureSampler, (float2) output.tex, 0).r;
	output.position += float4((h - 1.0) * (output.normal * 0.05f), 0);
#else
	float h = heightTexture.SampleLevel(heightTextureSampler, (float2)tex, 0).r;
	output.position += float4((h - 1.0) * (normal * 0.05f), 0);
#endif
#endif
	
#if (DEPTH != 1)
	output.pos = output.position;
#endif
	output.position = mul(output.position, viewProj);

#if (DEPTH != 1)
#if VtxUVs
	output.tex = output.tex;
#endif
#endif

#if (DEPTH != 1)
#if VtxTangents
	output.tangent = normalize(output.tangent);
#endif
#endif

	return output;
}