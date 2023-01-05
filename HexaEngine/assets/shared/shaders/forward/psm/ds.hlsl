#include "defs.hlsl"
#include "../../material.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

cbuffer LightView : register(b1)
{
	matrix view;
};

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
	float3 normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
	float2 tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;

	normal = normalize(normal);

	if (material.DANR.r)
	{
		float h = displacmentTexture.SampleLevel(displacmentSampler, (float2)tex, 0).r;
		output.position += float4((h - 1.0) * (normal * 0.05f), 0);
	}

	output.position = mul(output.position, view);
	output.shadowCoord = output.position;

	return output;
}