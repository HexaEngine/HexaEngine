#include "HexaEngine.Core:shaders/camera.hlsl"
#include "HexaEngine.Core:shaders/weather.hlsl"

#define SAMPLES 32

Texture2D hdrTexture : register(t0);
Texture2D<float> depthTex : register(t1);
Texture3D<float> volumeTex : register(t2);
SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float SampleDensityForFog(float3 pos)
{
	bool useHeightBased = (fog_mode & 0x04) != 0;
	float heightFactor = 1;
	if (useHeightBased)
	{
		heightFactor = GetFogHeightFactor(pos.y);
	}
	float density = volumeTex.SampleLevel(linearWrapSampler, pos * 0.003f, 0);
	return density * heightFactor;
}

float3 VolumetricFog(float3 hdr, float3 position, float3 V, float d)
{
	float3 x = GetCameraPos();

	float3 camToFrag = position - GetCameraPos();
	float3 deltaStep = camToFrag / (SAMPLES + 1);

	float accumulatedDensity = 0;

	[unroll(SAMPLES)]
		for (int i = 0; i < SAMPLES; ++i)
		{
			float currentDensity = SampleDensityForFog(x);
			accumulatedDensity += currentDensity;
			x += deltaStep;
		}

	accumulatedDensity /= SAMPLES;

	float factor = accumulatedDensity * fog_intensity;

	return max(lerp(hdr, fog_color.rgb, max(factor, 0)), 0);
}

float4 main(VSOut vs) : SV_Target
{
	float3 hdr = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;

	float depth = depthTex.Sample(linearClampSampler, vs.Tex);
	float3 position = GetPositionWS(vs.Tex, depth);
	float3 VN = GetCameraPos() - position;
	float3 V = normalize(VN);
	float d = length(VN);

	if (d < fog_start)
		return float4(hdr, 1);

	//float3 color = VolumetricFog(hdr, position, V, d);
	float3 color = lerp(hdr, fog_color, GetFogFactor(position, GetCameraPos()));

	return float4(color, 1);
}