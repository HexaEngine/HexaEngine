#include "defs.hlsl"
#include "../../world.hlsl"
#include "../../camera.hlsl"
#include "../../material.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

cbuffer MaterialBuffer : register(b2)
{
	Material material;
}

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
#if (INSTANCED != 1)
	output.normal = mul(output.normal, (float3x3) world);
#endif
	output.normal = normalize(output.normal);
#endif

#if (INSTANCED != 1)
	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(output.position, world);
#endif

	if (material.DANR.r)
	{
#if (DEPTH != 1)
		float h = displacmentTexture.SampleLevel(displacmentSampler, (float2) output.tex, 0).r;
		output.position += float4((h - 1.0) * (output.normal * 0.05f), 0);
#else
		float h = displacmentTexture.SampleLevel(displacmentSampler, (float2)tex, 0).r;
		output.position += float4((h - 1.0) * (normal * 0.05f), 0);
#endif
	}

#if (DEPTH != 1)
	output.pos = output.position;
#endif
	output.position = mul(output.position, view);
	output.depth = output.position.z / 100;
	output.position = mul(output.position, proj);

#if (DEPTH != 1)
	// Store the texture coordinates for the pixel shader.
	output.tex = output.tex;
#endif

#if (DEPTH != 1)
#if (INSTANCED != 1)
	output.tangent = mul(output.tangent, (float3x3)world);
#endif
	output.tangent = normalize(output.tangent);
#endif

	return output;
}