#include "defs.hlsl"
#include "../../world.hlsl"
#include "../../camera.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

#define displacement_intensity 16

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
#endif
	output.ctex = bary.x * tri[0].ctex + bary.y * tri[1].ctex + bary.z * tri[2].ctex;

	float h = displacmentTexture.SampleLevel(displacmentSampler, output.ctex, 0).r;
	output.pos.y += h * displacement_intensity;
	output.spos = mul(output.pos, view);
	output.spos = mul(output.spos, proj);

#if (DEPTH != 1)
	CalculateNormalFromHeightmap(output.ctex, output.tangent, output.normal);
#if (INSTANCED != 1)
	output.tangent = mul(output.tangent, (float3x3)world);
#endif
	output.tangent = normalize(output.tangent);
#endif

	return output;
}