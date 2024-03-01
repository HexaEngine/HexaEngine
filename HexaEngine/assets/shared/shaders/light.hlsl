#ifndef LIGHT_H_INCLUDED
#define LIGHT_H_INCLUDED

#include "math.hlsl"

#define POINT_LIGHT 0
#define SPOT_LIGHT 1
#define DIRECTIONAL_LIGHT 2

#define SHADOW_ATLAS_SIZE 8192

struct GlobalProbe
{
    float exposure;
    float horizonCutOff;
    float3 orientation;
};

struct Light
{
    uint type;

    float4 color;
    float4 direction;
    float4 position;

    float range;
    float outerCosine;
    float innerCosine;

    int castsShadows;
    bool cascadedShadows;
    int shadowMapIndex;

    uint1 padd;
};

struct LightParams
{
    uint type;
    float4 color;
    float3 L;
    float attenuation;
    float3 position;
    float NdotL;
    float3 direction;
    float range;
    int castsShadows;
    int shadowMapIndex;
};

struct PixelParams
{
    float3 Pos;
    float3 N;
    float3 V;
    float NdotV;
    float3 F0;
    float3 DiffuseColor;
    float PerceptualRoughnessUnclamped;
    float PerceptualRoughness;
    float Roughness;
    float3 DFG;
    float EnergyCompensation;
};

struct ShadowData
{
    float4x4 views[8];
    float4 cascades[2];
    float size;
    float softness;
    uint cascadeCount;
    float4 regions[8];
    float bias;
    float slopeBias;
};

float Attenuation(float distance, float range)
{
    float att = saturate(1.0f - (distance * distance / (range * range)));
    return att * att;
}

float3 LambertDiffuse(float3 radiance, float3 L, float3 N)
{
    float NdotL = max(0, dot(N, L));
    return radiance * NdotL;
}

float3 BlinnPhong(float3 radiance, float3 L, float3 V, float3 N, float3 baseColor, float shininess)
{
    float NdotL = max(0, dot(N, L));
    float3 diffuse = radiance * NdotL;

    const float kEnergyConservation = (8.0 + shininess) / (8.0 * PI);
    float3 halfwayDir = normalize(L + V);
    float spec = kEnergyConservation * pow(max(dot(N, halfwayDir), 0.0), shininess);

    float3 specular = radiance * spec;

    return (diffuse + specular) * baseColor;
}

#endif