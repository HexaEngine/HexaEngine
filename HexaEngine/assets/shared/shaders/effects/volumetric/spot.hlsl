#include "common.hlsl"


Texture2D<float> depthTx : register(t2);
Texture2D shadowDepthMap : register(t4);


struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEX;
};

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
        float3 L = current_light.position.xyz - P; //position in view space
        const float dist2 = dot(L, L);
        const float dist = sqrt(dist2);
        L /= dist;

        float SpotFactor = dot(L, normalize(-current_light.direction.xyz));
        float spotCutOff = current_light.outer_cosine;

		[branch]
        if (SpotFactor > spotCutOff)
        {
            float attenuation = DoAttenuation(dist, current_light.range);
            float conAtt = saturate((SpotFactor - current_light.outer_cosine) / (current_light.inner_cosine - current_light.outer_cosine));
            conAtt *= conAtt;
            attenuation *= conAtt;
			[branch]
            if (current_light.casts_shadows)
            {
                float4 posShadowMap = mul(float4(P, 1.0), shadow_matrices[0]);
                float3 UVD = posShadowMap.xyz / posShadowMap.w;

                UVD.xy = 0.5 * UVD.xy + 0.5;
                UVD.y = 1.0 - UVD.y;
                [branch]
                if (IsSaturated(UVD.xy))
                {             
                    float shadow_factor = CalcShadowFactor_PCF3x3(shadow_sampler, shadowDepthMap, UVD, shadow_map_size, softness);
                    attenuation *= shadow_factor;
                }
            }
            //attenuation *= ExponentialFog(cameraDistance - marchedDistance);
            accumulation += attenuation;
        }
        marchedDistance += stepSize;
        P = P + V * stepSize;
    }
    accumulation /= sampleCount;
    return max(0, float4(accumulation * current_light.color.rgb * current_light.volumetric_strength, 1));
}