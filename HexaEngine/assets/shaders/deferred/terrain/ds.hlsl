#include "defs.hlsl"
#include "../../world.hlsl"
#include "../../camera.hlsl"
#include "../../material.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

#define displacement_intensity 32

cbuffer MaterialBuffer : register(b2)
{
	Material material;
}

void CalculateNormalFromHeightmap(float2 uv, out float3 t, out float3 n)
{
	float wt;
	float ht;
	displacmentTexture.GetDimensions(wt, ht);
	float2 texelSize = 1.0 / float2(wt, ht);
	float4 h;
	h[0] = displacmentTexture.SampleLevel(displacmentSampler, uv + texelSize * float2(0, -1), 0).r * displacement_intensity;
	h[1] = displacmentTexture.SampleLevel(displacmentSampler, uv + texelSize * float2(-1, 0), 0).r * displacement_intensity;
	h[2] = displacmentTexture.SampleLevel(displacmentSampler, uv + texelSize * float2(1, 0), 0).r * displacement_intensity;
	h[3] = displacmentTexture.SampleLevel(displacmentSampler, uv + texelSize * float2(0, 1), 0).r * displacement_intensity;

	t = normalize(float3(2.0f, h[2] - h[1], 0.0f));
	float3 bitan = normalize(float3(0.0f, h[0] - h[3], -2.0f));
	n = cross(t, bitan);
}

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
	PixelInput output;

	// Interpolate patch attributes to generated vertices.
	output.pos = float4(bary.x * tri[0].pos + bary.y * tri[1].pos + bary.z * tri[2].pos, 1);
#if (DEPTH != 1)
	output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
	output.ctex = bary.x * tri[0].ctex + bary.y * tri[1].ctex + bary.z * tri[2].ctex;
#else
	float3 normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
	float2 tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
#endif

#if (INSTANCED != 1)
	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.pos = mul(output.pos, world);
#endif

#if (DEPTH != 1)
	float2 guv = float2(output.pos.x / 255, output.pos.z / 255);
	float h = displacmentTexture.SampleLevel(displacmentSampler, guv, 0).r;
	output.pos.y += h * displacement_intensity;
	CalculateNormalFromHeightmap(guv, output.tangent, output.normal);
#else
	float h = displacmentTexture.SampleLevel(displacmentSampler, float2(output.pos.x / 255, output.pos.z / 255), 0).r;
	output.pos += float4(h * normal, 0);
#endif

	output.spos = mul(output.pos, view);
	output.spos = mul(output.spos, proj);

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