#include "common.hlsl"

float3 SpotlightVolumetric(float ditherValue, float3 position, float3 VN, float3 V, Light light, ShadowData shadowData)
{
	if (length(VN) > light.range)
	{
		VN = normalize(VN) * light.range;
	}

	float3 deltaStep = -VN / (SAMPLE_COUNT + 1);
	float3 x = GetCameraPos();

	x += deltaStep * ditherValue;

	float result = 0.0;
	for (int i = 0; i < SAMPLE_COUNT; ++i)
	{
		float3 LN = light.position.xyz - x;
		float3 L = normalize(LN);

		float theta = dot(L, normalize(-light.direction.xyz));

		[branch]
			if (theta > light.outerCosine)
			{
				float distance = length(LN);
				float epsilon = light.innerCosine - light.outerCosine;
				float falloff = (epsilon != 0) ? smoothstep(0.0, 1.0, (theta - light.innerCosine) / epsilon) : 1.0;

				float distanceAttenuation = exp(-density) * Attenuation(distance, light.range);

				float visibility = ShadowFactorSpotlight(linear_clamp_sampler, shadowAtlas, light, shadowData, x, 0);
				result += visibility * HenyeyGreenstein(V, L) * falloff * distanceAttenuation;
			}
		x += deltaStep;
	}

	return result / SAMPLE_COUNT * light.color.rgb;
}