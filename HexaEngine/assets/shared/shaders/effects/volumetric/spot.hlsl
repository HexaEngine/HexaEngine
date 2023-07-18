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
    float2 Tex : TEX;
};

float ShadowFactorSpotlight(ShadowData data, float3 position, SamplerComparisonState state)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.offsets[0], data.views[0]);

#if HARD_SHADOWS_SPOTLIGHTS
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, shadowAtlas, uvd, data.size, data.softness);
#endif
}

float4 main(VertexOut input) : SV_TARGET
{
     //float2 ScreenCoord = input.pos2D.xy / input.pos2D.w * float2(0.5f, -0.5f) + 0.5f;
    float depth = max(input.PosH.z, depthTx.SampleLevel(linear_clamp_sampler, input.Tex, 2));
    float3 P = GetPositionVS(input.Tex, depth);
    float3 V = float3(0.0f, 0.0f, 0.0f) - P;
    float cameraDistance = length(V);
    V /= cameraDistance;

    float marchedDistance = 0;
    float accumulation = 0;

    float3 rayEnd = float3(0.0f, 0.0f, 0.0f);
	// todo: rayEnd should be clamped to the closest cone intersection point when camera is outside volume

    const uint sampleCount = 16;
    const float stepSize = length(P - rayEnd) / sampleCount;

	// dither ray start to help with undersampling:
    P = P + V * stepSize * dither(input.PosH.xy);

	// Perform ray marching to integrate light volume along view ray:
	[loop]
    for (uint i = 0; i < sampleCount; ++i)
    {
        float3 L = currentLight.position.xyz - P; //position in view space
        const float dist2 = dot(L, L);
        const float dist = sqrt(dist2);
        L /= dist;

        float SpotFactor = dot(L, normalize(-currentLight.direction.xyz));
        float spotCutOff = currentLight.outerCosine;

		[branch]
        if (SpotFactor > spotCutOff)
        {
            float attenuation = Attenuation(dist, currentLight.range);
            float conAtt = saturate((SpotFactor - currentLight.outerCosine) / (currentLight.innerCosine - currentLight.outerCosine));
            conAtt *= conAtt;
            attenuation *= conAtt;
			[branch]
            if (currentLight.castsShadows)
            {
                float shadow_factor = ShadowFactorSpotlight(shadowData, currentLight.position.xyz, shadow_sampler);
                attenuation *= shadow_factor;
            }
            //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
            accumulation += attenuation;
        }
        marchedDistance += stepSize;
        P = P + V * stepSize;
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * currentLight.color.rgb * currentLight.volumetricStrength, 1));
}