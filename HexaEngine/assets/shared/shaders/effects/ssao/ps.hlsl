#include "../../camera.hlsl"

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer SSAOParams
{
    float TAP_SIZE = 0.0002;
    uint NUM_TAPS = 16;
    float THRESHOLD0 = 0.1;
    float SCALE = 1.0;
    float2 TARGET_SIZE;
};

Texture2D<float> depthTexture : register(t0);
Texture2D normalTexture : register(t1);

SamplerState samplerState;

static const float3 taps[16] =
{
	float3(-0.364452, -0.014985, -0.513535),
	float3(0.004669, -0.445692, -0.165899),
	float3(0.607166, -0.571184, 0.377880),
	float3(-0.607685, -0.352123, -0.663045),
	float3(-0.235328, -0.142338, 0.925718),
	float3(-0.023743, -0.297281, -0.392438),
	float3(0.918790, 0.056215, 0.092624),
	float3(0.608966, -0.385235, -0.108280),
	float3(-0.802881, 0.225105, 0.361339),
	float3(-0.070376, 0.303049, -0.905118),
	float3(-0.503922, -0.475265, 0.177892),
	float3(0.035096, -0.367809, -0.475295),
	float3(-0.316874, -0.374981, -0.345988),
	float3(-0.567278, -0.297800, -0.271889),
	float3(-0.123325, 0.197851, 0.626759),
	float3(0.852626, -0.061007, -0.144475)
};

#define TAP_SIZE 0.0002
#define NUM_TAPS 16
#define THRESHOLD 0.1
#define SCALE 1.0

float3 hash3(float2 p) {
	float3 q = float3(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)));
	return frac(sin(q) * 43758.5453);
}

float4 main(VSOut vs) : SV_Target
{
	float3 random = hash3(vs.Tex * float2(1920 / 4, 1080 / 4));
	// reconstruct the view space position from the depth map
    float start_Z = depthTexture.Sample(samplerState, vs.Tex);
	float start_Y = 1.0 - vs.Tex.y; // texture coordinates for D3D have origin in top left, but in camera space origin is in bottom left
	float2 start_Pos = float2(vs.Tex.x, start_Y);
	float2 ndc_Pos = (2.0 * start_Pos) - 1.0;
	float4 unproject = mul(float4(ndc_Pos.x, ndc_Pos.y, start_Z, 1.0), projInv);
	float3 viewPos = unproject.xyz / unproject.w;
	float3 viewNorm = normalTexture.Sample(samplerState, vs.Tex).xyz;

	// WORKNOTE: start_Z was a huge negative value at one point because we had a D16_UNORM depth target
	// but we set the shader resource view format to R16_FLOAT instead of R16_UNORM

	float total = 0.0;
	for (uint i = 0; i < NUM_TAPS; i++)
	{
		float3 offset = TAP_SIZE * taps[i] * random;
		float2 offTex = vs.Tex + float2(offset.x, -offset.y);

        float off_start_Z = depthTexture.Sample(samplerState, offTex);
		float2 off_start_Pos = float2(offTex.x, start_Y + offset.y);
		float2 off_ndc_Pos = (2.0 * off_start_Pos) - 1.0;
		float4 off_unproject = mul(float4(off_ndc_Pos.x, off_ndc_Pos.y, off_start_Z, 1.0), projInv);
		float3 off_viewPos = off_unproject.xyz / off_unproject.w;

		float3 diff = off_viewPos.xyz - viewPos.xyz;
		float distance = length(diff);
		float3 diffnorm = normalize(diff);
		float dotx = diffnorm.x * viewNorm.x;

		float occlusion = max(0.0, dot(viewNorm, diffnorm)); // * SCALE / (1.0 + distance);
		total += (1.0 - occlusion);
	}

	total /= NUM_TAPS;
	return float4(total, total, total, 1.0);
}