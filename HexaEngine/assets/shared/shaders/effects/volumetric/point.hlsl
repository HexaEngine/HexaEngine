#include "common.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"
#include "../../light.hlsl"

Texture2D<float> depthTx : register(t2);
Texture2D shadowAtlas : register(t5);

SamplerState linear_clamp_sampler : register(s3);
SamplerComparisonState shadow_sampler : register(s4);

cbuffer LightBuffer
{
    Light currentLight;
    ShadowData shadowData;
    float padd;
};

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEX;
};

int CubeFaceFromDirection(float3 direction)
{
    float3 absDirection = abs(direction);
    int faceIndex = 0;

    if (absDirection.x >= absDirection.y && absDirection.x >= absDirection.z)
    {
        faceIndex = (direction.x > 0.0) ? 0 : 1; // Positive X face (0), Negative X face (1)
    }
    else if (absDirection.y >= absDirection.x && absDirection.y >= absDirection.z)
    {
        faceIndex = (direction.y > 0.0) ? 2 : 3; // Positive Y face (2), Negative Y face (3)
    }
    else
    {
        faceIndex = (direction.z > 0.0) ? 4 : 5; // Positive Z face (4), Negative Z face (5)
    }

    return faceIndex;
}

#define HARD_SHADOWS_POINTLIGHTS 1
float ShadowFactorPointLight(ShadowData data, Light light, float3 position, SamplerComparisonState state)
{
    float3 light_to_pixelWS = position - light.position.xyz;
    float depthValue = length(light_to_pixelWS) / light.range;

    int face = CubeFaceFromDirection(normalize(light_to_pixelWS.xyz));
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.offsets[face], data.views[face]);
    uvd.z = depthValue;

#if HARD_SHADOWS_POINTLIGHTS
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, depthAtlas, uvd, data.size, data.softness);
#endif
}

float4 main(VertexOut input) : SV_TARGET
{
    float depth = max(input.PosH.z, depthTx.SampleLevel(linear_clamp_sampler, input.Tex, 2));
    float3 P = GetPositionVS(input.Tex, depth);
    float3 V = float3(0.0f, 0.0f, 0.0f) - P;
    float cameraDistance = length(V);
    V /= cameraDistance;

    float marchedDistance = 0;
    float accumulation = 0;

    float3 rayEnd = float3(0.0f, 0.0f, 0.0f);
    const uint sampleCount = 16;
    const float stepSize = length(P - rayEnd) / sampleCount;
    P = P + V * stepSize * dither(input.PosH.xy);
	[loop]
    for (uint i = 0; i < sampleCount; ++i)
    {
        float3 L = currentLight.position.xyz - P; //position in view space
        const float dist2 = dot(L, L);
        const float dist = sqrt(dist2);
        L /= dist;
        float SpotFactor = dot(L, normalize(-currentLight.direction.xyz));
        float spotCutOff = currentLight.outerCosine;
        float attenuation = Attenuation(dist, currentLight.range);
		[branch]
        if (currentLight.castsShadows)
        {
            float shadow_factor = ShadowFactorPointLight(shadowData, currentLight, P, shadow_sampler);
            attenuation *= shadow_factor;
        }
        //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
        accumulation += attenuation;
        marchedDistance += stepSize;
        P = P + V * stepSize;
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * currentLight.color.rgb * currentLight.volumetricStrength, 1));
}