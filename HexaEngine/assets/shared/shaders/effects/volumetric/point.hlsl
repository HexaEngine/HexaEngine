#include "common.hlsl"

float3 PointLightVolumetric(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
{
    float3 camToFrag = position - GetCameraPos();
    if (length(camToFrag) > light.range)
    {
        camToFrag = normalize(camToFrag) * light.range;
    }
    float3 deltaStep = camToFrag / (SAMPLE_COUNT + 1);
    float3 fragToCamNorm = normalize(GetCameraPos() - position);
    float3 x = GetCameraPos();

    //Why this randomization of an initial step improves things? See
    //Michal Valient, GDC 2014 which explains it in one picture.
    float ditherValue = dither(screenCoords.xy);
    x += deltaStep * ditherValue;

    float result = 0.0;
    [unroll(SAMPLE_COUNT)]
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float visibility = ShadowFactorPointLight(shadow_sampler, shadowAtlas, shadowData, light, x);
        result += visibility;
        x += deltaStep;
    }

    // This is from Jake Ryan's code:
    float d = result * length(position - GetCameraPos()) / SAMPLE_COUNT;
    // float powder = 1.0 - exp(-d * 10.0);
    float powder = 1.0; // no need for that powder term really
    float beer = exp(-d * 0.01); // increasing exp const strengthens rays, but overexposes colors

    return (1.0 - beer) * powder * light.color.rgb * volumetricStrength;
}

float3 PointLightVolumetric2(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
{
    float3 camToFrag = position - GetCameraPos();
    if (length(camToFrag) > light.range)
    {
        camToFrag = normalize(camToFrag) * light.range;
    }
    float3 deltaStep = camToFrag / (SAMPLE_COUNT + 1);
    float3 fragToCamNorm = normalize(GetCameraPos() - position);
    float3 x = GetCameraPos();

    float ditherValue = dither(screenCoords.xy);
    x += deltaStep * ditherValue;

    float result = 0.0;
    [unroll(SAMPLE_COUNT)]
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float3 LN = light.position.xyz - x;
        float distance = length(LN);
        float3 L = normalize(LN);
        float visibility = ShadowFactorPointLight(shadow_sampler, shadowAtlas, shadowData, light, x);
        result += visibility * PhaseFunction(-L, fragToCamNorm);
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb * volumetricStrength;
}

float3 PointLightVolumetric3(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
{
    float3 camToFrag = position - GetCameraPos();
    if (length(camToFrag) > light.range)
    {
        camToFrag = normalize(camToFrag) * light.range;
    }
    float3 deltaStep = camToFrag / (SAMPLE_COUNT + 1);
    float3 fragToCamNorm = normalize(GetCameraPos() - position);
    float3 x = GetCameraPos();

    float ditherValue = dither(screenCoords.xy);
    x += deltaStep * ditherValue;

    float result = 0.0;
    [unroll(SAMPLE_COUNT)]
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float3 LN = light.position.xyz - x;
        float distance = length(LN);
        float3 L = normalize(LN);

        float distanceAttenuation = exp(-density * distance);

        float rayleighScattering = RayleighScattering(V, L);
        float mieScattering = MieScattering(V, L);
        float scatteringContribution = rayleighScattering + mieScattering;

        float visibility = ShadowFactorPointLight(shadow_sampler, shadowAtlas, shadowData, light, x);

        result += visibility * distanceAttenuation * scatteringContribution;
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb * volumetricStrength;
}