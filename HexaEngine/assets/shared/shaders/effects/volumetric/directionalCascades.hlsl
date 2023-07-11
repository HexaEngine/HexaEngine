#include "common.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"
#include "../../light.hlsl"

Texture2D<float> depthTx : register(t2);
Texture2DArray cascadeShadowMaps : register(t6);

SamplerState linear_clamp_sampler;
SamplerComparisonState shadow_sampler;

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

float4 main(VertexOut input) : SV_TARGET
{
    float depth = max(input.PosH.z, depthTx.SampleLevel(linear_clamp_sampler, input.Tex, 2));
    float3 P = GetPositionVS(input.Tex, depth);
    float3 V = float3(0.0f, 0.0f, 0.0f) - P;
    float cameraDistance = length(V);
    V /= cameraDistance;

    float marchedDistance = 0;
    float3 accumulation = 0;

    const float3 L = currentLight.direction.xyz;
    float3 rayEnd = float3(0.0f, 0.0f, 0.0f);
    const uint sampleCount = 16;
    const float stepSize = length(P - rayEnd) / sampleCount;

    P = P + V * stepSize * dither(input.PosH.xy);
    float viewDepth = P.z;

    float cascadePlaneDistances[8] = (float[8]) shadowData.cascades;

	[loop]
    for (uint j = 0; j < sampleCount; ++j)
    {
        bool valid = false;

        for (uint cascade = 0; cascade < shadowData.cascadeCount; ++cascade)
        {
            matrix light_space_matrix = shadowData.views[cascade];
            float4 pos_shadow_map = mul(float4(P, 1.0), light_space_matrix);
            float3 UVD = pos_shadow_map.xyz / pos_shadow_map.w;

            UVD.xy = 0.5 * UVD.xy + 0.5;
            UVD.y = 1.0 - UVD.y;
            [branch]
            if (viewDepth < cascadePlaneDistances[cascade])
            {
                if (IsSaturated(UVD.xy))
                {
                    float attenuation = CSMCalcShadowFactor_PCF3x3(shadow_sampler, cascadeShadowMaps, cascade, UVD, shadowData.size, shadowData.softness);
                    //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
                    accumulation += attenuation;
                }
                marchedDistance += stepSize;
                P = P + V * stepSize;
                valid = true;
                break;
            }
        }
        if (!valid)
        {
            break;
        }
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * currentLight.color.rgb * currentLight.volumetricStrength, 1));
}