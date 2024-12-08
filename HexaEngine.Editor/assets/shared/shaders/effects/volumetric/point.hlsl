#include "common.hlsl"

float3 PointLightVolumetric(float ditherValue, float3 position, float3 VN, float3 V, Light light, ShadowData shadowData)
{
    float viewDistance = length(VN);
    if (viewDistance > light.range)
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
        float distance = length(LN);
        float3 L = normalize(LN);

        float distanceAttenuation = exp(-density) * Attenuation(distance, light.range);

        float visibility = ShadowFactorPointLight(linear_clamp_sampler, shadowAtlas, light, shadowData, x, 0);
        result += visibility * HenyeyGreenstein(V, L) * distanceAttenuation;
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb;
}