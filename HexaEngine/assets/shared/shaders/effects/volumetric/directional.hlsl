#include "common.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"


Texture2D<float> depthTx : register(t2);
Texture2D shadowDepthMap : register(t4);

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex  : TEX;
};


float4 main(VertexOut input) : SV_TARGET
{
    if (current_light.casts_shadows == 0)
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
    const float3 L = current_light.direction.xyz;
    float3 rayEnd = float3(0.0f, 0.0f, 0.0f);
    const uint sampleCount = 16;
    const float stepSize = length(P - rayEnd) / sampleCount;
    P = P + V * stepSize * dither(input.PosH.xy);
	[loop]
    for (uint i = 0; i < sampleCount; ++i)
    {
        float4 posShadowMap = mul(float4(P, 1.0), shadow_matrices[0]);
        float3 UVD = posShadowMap.xyz / posShadowMap.w;

        UVD.xy = 0.5 * UVD.xy + 0.5;
        UVD.y = 1.0 - UVD.y;
        
        [branch]
        if (IsSaturated(UVD.xy))
        {                          
            float attenuation = CalcShadowFactor_PCF3x3(shadow_sampler, shadowDepthMap, UVD, shadow_map_size, softness);
            //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
            accumulation += attenuation;
        }
        marchedDistance += stepSize;
		P = P + V * stepSize;
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * current_light.color.rgb * current_light.volumetric_strength, 1));
}
