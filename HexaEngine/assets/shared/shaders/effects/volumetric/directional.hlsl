#include "common.hlsl"

float3 DirectionalLightVolumetric(float ditherValue, float3 position, float3 VN, float3 V, Light light, ShadowData shadowData)
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
        float attenuation = exp(-density);
        float visibility = ShadowFactorDirectionalLight(shadow_sampler, shadowAtlas, shadowData, x);
        result += visibility * HenyeyGreenstein(V, light.direction.xyz) * attenuation;
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb;
}