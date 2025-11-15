#include "HexaEngine.Core:shaders/camera.hlsl"

Texture2D<float4> sceneTex : register(t0);
Texture2D<float> depthTex : register(t1);
Texture2D<float> cocTex : register(t2);

SamplerState linearWrapSampler : register(s0);

cbuffer BokehParams
{
	float bokehFallout;
	float bokehRadiusScale;
	float bokehColorScale;
	float bokehBlurThreshold;
	float bokehLumThreshold;
};

static const float4 LUM_FACTOR = float4(0.299, 0.587, 0.114, 0);

struct Bokeh
{
	float3 Position;
	float2 Size;
	float3 Color;
};

AppendStructuredBuffer<Bokeh> BokehStack : register(u0);

float GetCircleOfConfusion(float2 uv)
{
	float coc = cocTex.SampleLevel(linearWrapSampler, uv, 0) * 2 - 1;
	return abs(coc);
}

[numthreads(32, 32, 1)]
void main(uint3 dispatchThreadId : SV_DispatchThreadID)
{
	uint2 CurPixel = dispatchThreadId.xy;

	float2 uv = CurPixel / screenDim;

	float depth = depthTex.Load(int3(CurPixel, 0));
	float centerDepth = GetLinearDepth(depth);
	float coc = GetCircleOfConfusion(uv);

	if (depth >= 1.0f && coc < bokehLumThreshold)
	{
		return;
	}

	const uint NumSamples = 9;
	const uint2 SamplePoints[NumSamples] =
	{
		uint2(-1, -1), uint2(0, -1), uint2(1, -1),
		uint2(-1, 0), uint2(0, 0), uint2(1, 0),
		uint2(-1, 1), uint2(0, 1), uint2(1, 1)
	};

	float3 centerColor = sceneTex.Load(int3(CurPixel, 0)).rgb;

	float3 averageColor = 0.0f;
	for (uint i = 0; i < NumSamples; ++i)
	{
		float3 color = sceneTex.Load(int3(CurPixel + SamplePoints[i], 0)).rgb;
		averageColor += color;
	}
	averageColor /= NumSamples;

	// Calculate the difference between the current texel and the average
	float averageBrightness = dot(averageColor, 1.0f);
	float centerBrightness = dot(centerColor, 1.0f);
	float brightnessDiff = max(centerBrightness - averageBrightness, 0.0f);

	if (brightnessDiff < bokehLumThreshold)
	{
		return;
	}

	Bokeh bPoint;
	bPoint.Position = float3(uv, centerDepth);
	bPoint.Size = coc * bokehRadiusScale / screenDim;

	float cocRadius = coc * bokehRadiusScale * 0.45f;
	float cocArea = cocRadius * cocRadius * 3.14159f;
	float falloff = pow(saturate(1.0f / cocArea), bokehFallout);
	bPoint.Color = centerColor * falloff;

	BokehStack.Append(bPoint);
}