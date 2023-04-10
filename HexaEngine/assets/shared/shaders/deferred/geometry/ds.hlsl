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

	// Interpolate patch attributes to generated vertices.
	output.position = float4(bary.x * tri[0].pos + bary.y * tri[1].pos + bary.z * tri[2].pos, 1);
#if (DEPTH != 1)
	output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
	output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
	output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;
#else
	float3 normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
	float2 tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
#endif

	// Calculate the normal vector against the world matrix only.
#if (DEPTH != 1)
	output.normal = normalize(output.normal);
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
	output.position = mul(output.position, view);
	output.depth = output.position.z / cam_far;
	output.position = mul(output.position, proj);

#if (DEPTH != 1)
	// Store the texture coordinates for the pixel shader.
	output.tex = output.tex;
#endif

#if (DEPTH != 1)
	output.tangent = normalize(output.tangent);
#endif

	return output;
}