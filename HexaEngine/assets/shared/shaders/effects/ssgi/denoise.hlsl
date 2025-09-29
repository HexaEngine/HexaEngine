#include "../../camera.hlsl"

Texture2D inputTex;

SamplerState linearClampSampler;

cbuffer DenoiseParams
{
	float sigma_s = 2.0f;
	float sigma_r = 0.1f;
	float radius = 16.0f;
};

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float3 bilateralFilter(float2 uv, float radius, float sigma_s, float sigma_r)
{
	float3 color = inputTex.Sample(linearClampSampler, uv).rgb;
	float3 filteredColor = float3(0.0, 0.0, 0.0);
	float weightSum = 0.0;

	// Loop through a neighborhood of pixels (a square around the current UV)
	for (int y = -int(radius); y <= int(radius); ++y)
	{
		for (int x = -int(radius); x <= int(radius); ++x)
		{
			float2 offset = float2(x, y) * screenDimInv; // Normalize the offset
			float3 neighborColor = inputTex.Sample(linearClampSampler, uv + offset).rgb;

			// Spatial weight: Gaussian based on distance from center
			float spatialWeight = exp(-(x * x + y * y) / (2.0f * sigma_s * sigma_s));

			// Range weight: Gaussian based on color difference
			float rangeWeight = exp(-dot(color - neighborColor, color - neighborColor) / (2.0f * sigma_r * sigma_r));

			// Combined weight
			float weight = spatialWeight * rangeWeight;

			filteredColor += neighborColor * weight;
			weightSum += weight;
		}
	}

	// Normalize the result by the sum of weights
	return filteredColor / weightSum;
}

float4 main(VertexOut input) : SV_TARGET
{
	float3 nlmDenoisedColor = bilateralFilter(input.Tex, radius, sigma_s, sigma_r);

	return float4(nlmDenoisedColor, 1.0);
}