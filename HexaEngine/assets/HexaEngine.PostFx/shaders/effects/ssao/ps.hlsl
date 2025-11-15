#include "HexaEngine.Core:shaders/camera.hlsl"
#include "HexaEngine.Core:shaders/gbuffer.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer SSAOParams
{
    float TAP_SIZE = 0.0002;
    uint NUM_TAPS = 16;
    float POWER;
    float2 NoiseScale;
};

#define BIAS 0.01f

Texture2D<float> depthTex : register(t0);
Texture2D normalTex : register(t1);
Texture2D noiseTex : register(t2);

SamplerState linearClampSampler;

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

float4 main(VSOut vs) : SV_Target
{
    float3 random = noiseTex.Sample(linearClampSampler, vs.Tex * NoiseScale);

    float depth = depthTex.Sample(linearClampSampler, vs.Tex);

    float3 position = GetPositionVS(vs.Tex, depth);

    float3 normal = mul(UnpackNormal(normalTex.Sample(linearClampSampler, vs.Tex).xyz), (float3x3) view);

    float3 tangent = normalize(random - normal * dot(random, normal));
    float3 bitangent = cross(normal, tangent);
    float3x3 TBN = float3x3(tangent, bitangent, normal);

    float total = 0.0;
    for (uint i = 0; i < NUM_TAPS; i++)
    {
        float3 sampleDir = mul(taps[i].xyz, TBN);
        float3 samplePos = position + sampleDir * TAP_SIZE;

        float4 offset = float4(samplePos, 1.0);
        offset = mul(offset, proj);

        offset.xy = ((offset.xy / offset.w) * float2(1.0f, -1.0f)) * 0.5f + 0.5f;

        float sampleDepth = depthTex.Sample(linearClampSampler, offset.xy);

        sampleDepth = GetPositionVS(offset.xy, sampleDepth).z;

        float rangeCheck = smoothstep(0.0, 1.0, TAP_SIZE / abs(position.z - sampleDepth));
        total += (sampleDepth >= samplePos.z + BIAS ? 0.0 : 1.0) * rangeCheck;
    }

    total /= NUM_TAPS;

    return float4(pow(total, POWER).xxx, 1.0);
}