#include "common.hlsl"

float3 DirectionalLightVolumetric(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
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
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float visibility = ShadowFactorDirectionalLight(shadow_sampler, shadowAtlas, shadowData, x);
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

float3 DirectionalLightVolumetric2(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
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
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float distanceAttenuation = exp(-density);
        float visibility = ShadowFactorDirectionalLight(shadow_sampler, shadowAtlas, shadowData, x);
        result += visibility * MieScattering(V, -light.direction.xyz) * distanceAttenuation;
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb * volumetricStrength;
}

float3 DirectionalLightVolumetric3(float4 screenCoords, float2 texCoords, float3 position, float3 V, Light light, ShadowData shadowData, float volumetricStrength)
{
    float3 camToFrag = position - GetCameraPos();
    if (length(camToFrag) > light.range)
    {
        camToFrag = normalize(camToFrag) * light.range;
    }
    float3 deltaStep = camToFrag / (SAMPLE_COUNT + 1);
    float3 fragToCamNorm = normalize(GetCameraPos() - position);
    float3 x = GetCameraPos();

    float3 L = normalize(-light.direction);

    float ditherValue = dither(screenCoords.xy);
    x += deltaStep * ditherValue;

    float result = 0.0;
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float distanceAttenuation = exp(-density);

        float rayleighScattering = RayleighScattering(V, -L);
        float mieScattering = MieScattering(V, -L);
        float scatteringContribution = rayleighScattering + mieScattering;

        float visibility = ShadowFactorDirectionalLight(shadow_sampler, shadowAtlas, shadowData, x);
        result += visibility * distanceAttenuation * scatteringContribution;
        x += deltaStep;
    }

    return result / SAMPLE_COUNT * light.color.rgb * volumetricStrength;
}