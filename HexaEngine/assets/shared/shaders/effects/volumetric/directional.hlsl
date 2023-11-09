#include "common.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"
#include "../../light.hlsl"

Texture2D<float> depthTx : register(t2);
Texture2D shadowAtlas : register(t4);

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
    float2 Tex : TEXCOORD;
};

float ShadowFactorDirectionalLight(ShadowData data, float3 position, SamplerComparisonState state)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_DIRECTIONAL
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, shadowAtlas, uvd, data.size, 1);
#endif
}

float4 main(VertexOut input) : SV_TARGET
{
    if (currentLight.castsShadows == 0)
    {
        return 0;
    }
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
	[loop]
    for (uint i = 0; i < sampleCount; ++i)
    {
        float4 posShadowMap = mul(float4(P, 1.0), shadowData.views[0]);
        float3 UVD = posShadowMap.xyz / posShadowMap.w;

        UVD.xy = 0.5 * UVD.xy + 0.5;
        UVD.y = 1.0 - UVD.y;

        [branch]
        if (IsSaturated(UVD.xy))
        {
            float attenuation = ShadowFactorDirectionalLight(shadowData, P, shadow_sampler);
            //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
            accumulation += attenuation;
        }
        marchedDistance += stepSize;
        P = P + V * stepSize;
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * currentLight.color.rgb * currentLight.volumetricStrength, 1));
}