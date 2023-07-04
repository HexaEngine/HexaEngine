#include "defs.hlsl"
#include "../../light.hlsl"
#include "../../camera.hlsl"

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metalness
#define Metalness 0
#endif
#ifndef Ao
#define Ao 1
#endif
#ifndef Emissive
#define Emissive float3(0,0,0);
#endif

#ifndef HasBaseColorTex
#define HasBaseColorTex 0
#endif
#ifndef HasNormalTex
#define HasNormalTex 0
#endif
#ifndef HasDisplacementTex
#define HasDisplacementTex 0
#endif
#ifndef HasMetalnessTex
#define HasMetalnessTex 0
#endif
#ifndef HasRoughnessTex
#define HasRoughnessTex 0
#endif
#ifndef HasEmissiveTex
#define HasEmissiveTex 0
#endif
#ifndef HasAmbientOcclusionTex
#define HasAmbientOcclusionTex 0
#endif
#ifndef HasRoughnessMetalnessTex
#define HasRoughnessMetalnessTex 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex
#define HasAmbientOcclusionRoughnessMetalnessTex 0
#endif

#if HasBaseColorTex
Texture2D baseColorTexture : register(t0);
SamplerState baseColorTextureSampler : register(s0);
#endif
#if HasNormalTex
Texture2D normalTexture : register(t1);
SamplerState normalTextureSampler : register(s1);
#endif
#if HasRoughnessTex
Texture2D roughnessTexture : register(t2);
SamplerState roughnessTextureSampler : register(s2);
#endif
#if HasMetalnessTex
Texture2D metalnessTexture : register(t3);
SamplerState metalnessTextureSampler : register(s3);
#endif
#if HasEmissiveTex
Texture2D emissiveTexture : register(t4);
SamplerState emissiveTextureSampler : register(s4);
#endif
#if HasAmbientOcclusionTex
Texture2D ambientOcclusionTexture : register(t5);
SamplerState ambientOcclusionTextureSampler : register(s5);
#endif
#if HasRoughnessMetalnessTex
Texture2D roughnessMetalnessTexture : register(t6);
SamplerState roughnessMetalnessTextureSampler : register(s6);
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex
Texture2D ambientOcclusionRoughnessMetalnessTexture : register(t7);
SamplerState ambientOcclusionRoughnessMetalnessSampler : register(s7);
#endif

Texture2D brdfLUT : register(t8);
Texture2D ssao : register(t9);

StructuredBuffer<GlobalProbe> globalProbes : register(t10);

StructuredBuffer<DirectionalLight> directionalLights : register(t11);
StructuredBuffer<PointLight> pointLights : register(t12);
StructuredBuffer<Spotlight> spotlights : register(t13);

StructuredBuffer<DirectionalLightSD> directionalLightSDs : register(t14);
StructuredBuffer<PointLightSD> pointLightSDs : register(t15);
StructuredBuffer<SpotlightSD> spotlightSDs : register(t16);

TextureCube globalDiffuse[4] : register(t17);
TextureCube globalSpecular[4] : register(t21);

Texture2DArray depthCSM : register(t25);
TextureCube depthOSM[32] : register(t26);
Texture2D depthPSM[32] : register(t58);

SamplerState linearClampSampler : register(s8);
SamplerState linearWrapSampler : register(s9);
SamplerState pointClampSampler : register(s10);
SamplerState shadowSampler : register(s11);

cbuffer constants : register(b0)
{
    uint directionalLightCount;
    uint pointLightCount;
    uint spotlightCount;
    uint PaddLight0;
    uint directionalLightSDCount;
    uint pointLightSDCount;
    uint spotlightSDCount;
    uint PaddLight1;
    uint globalProbeCount;
    uint localProbeCount;
    uint PaddLight2;
    uint PaddLight3;
};

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW, float3 bitangent)
{
	// Uncompress each component from [0,1] to [-1,1].
    float3 normalT = 2.0f * normalMapSample - 1.0f;

	// Build orthonormal basis.
    float3 N = unitNormalW;
    float3 T = normalize(tangentW - dot(tangentW, N) * N);
    float3 B = cross(N, T);

    float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
    float3 bumpedNormalW = mul(normalT, TBN);

    return bumpedNormalW;
}

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW)
{
	// Uncompress each component from [0,1] to [-1,1].
    float3 normalT = 2.0f * normalMapSample - 1.0f;

	// Build orthonormal basis.
    float3 N = unitNormalW;
    float3 T = normalize(tangentW - dot(tangentW, N) * N);
    float3 B = cross(N, T);

    float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
    float3 bumpedNormalW = mul(normalT, TBN);

    return bumpedNormalW;
}

struct Pixel
{
    float4 Color : SV_Target0;
    float4 Position : SV_Target1;
    float4 Normal : SV_Target2;
};

Pixel main(PixelInput input)
{
    float3 position = input.pos;
    float4 baseColor = BaseColor;
    float3 normal = normalize(input.normal);
    float3 tangent = normalize(input.tangent);
    float3 bitangent = normalize(input.bitangent);

    
    float opacity = 1;

    float ao = Ao;
    float roughness = Roughness;
    float metalness = Metalness;
    float3 emissive = Emissive;

#if HasBaseColorTex
	float4 color = baseColorTexture.Sample(baseColorTextureSampler, (float2) input.tex);
    baseColor = float4(color.rgb * color.a, color.a);
#endif

#if HasNormalTex
    normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2) input.tex).rgb, normal, tangent, bitangent);
#endif
	
#if HasRoughnessTex
    roughness = roughnessTexture.Sample(roughnessTextureSampler, (float2) input.tex).r;
#endif
	
#if HasMetalnessTex
    metalness = metalnessTexture.Sample(metalnessTextureSampler, (float2) input.tex).r;
#endif
	
#if HasEmissiveTex
    emissive = emissiveTexture.Sample(emissiveTextureSampler, (float2) input.tex).rgb;
#endif
	
#if HasAmbientOcclusionTex
    ao = ambientOcclusionTexture.Sample(ambientOcclusionTextureSampler, (float2) input.tex).r;
#endif
	
#if HasRoughnessMetalnessTex
    float2 rm = roughnessMetalnessTexture.Sample(roughnessMetalnessTextureSampler, (float2) input.tex).gb;
    roughness = rm.x;
    metalness = rm.y;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex
    float3 orm = ambientOcclusionRoughnessMetalnessTexture.Sample(ambientOcclusionRoughnessMetalnessSampler, (float2) input.tex).rgb;
    ao = orm.r;
    roughness = orm.g;
    metalness = orm.b;
#endif

    if (baseColor.a < 0.1f)
        discard;

    float3 N = normal;
    float3 X = tangent;

    float3 V = normalize(GetCameraPos() - position);

    float3 Lo = 0;
    
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor.xyz, metalness);

    [loop]
    for (uint x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;

        Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
    }

    [unroll(32)]
    for (uint y = 0; y < directionalLightSDCount; y++)
    {
        DirectionalLightSD light = directionalLightSDs[y];
        float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
        float shadow = ShadowCalculation(light, position, N, depthCSM, linearClampSampler);
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        Lo += (1.0f - shadow) * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
    }

    [loop]
    for (uint z = 0; z < pointLightCount; z++)
    {
        PointLight light = pointLights[z];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);

        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;

        Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
    }

    [unroll(32)]
    for (uint zd = 0; zd < pointLightSDCount; zd++)
    {
        PointLightSD light = pointLightSDs[zd];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);

        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;
        float shadow = ShadowCalculation(light, position, V, depthOSM[zd], linearClampSampler);
        Lo += (1.0f - shadow) * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
    }

    [loop]
    for (uint w = 0; w < spotlightCount; w++)
    {
        Spotlight light = spotlights[w];
        float3 LN = light.pos - position;
        float3 L = normalize(LN);

        float theta = dot(L, normalize(-light.dir));
        if (theta > light.cutOff)
        {
            float distance = length(LN);
            float attenuation = 1.0 / (distance * distance);
            float epsilon = light.cutOff - light.outerCutOff;
            float falloff = 1;
            if (epsilon != 0)
                falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
            float3 radiance = light.color.rgb * attenuation * falloff;
            Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
        }
    }

    [unroll(32)]
    for (uint wd = 0; wd < spotlightSDCount; wd++)
    {
        SpotlightSD light = spotlightSDs[wd];
        float3 LN = light.pos - position;
        float3 L = normalize(LN);

        float theta = dot(L, normalize(-light.dir));
        if (theta > light.cutOff)
        {
            float distance = length(LN);
            float attenuation = 1.0 / (distance * distance);
            float epsilon = light.cutOff - light.outerCutOff;
            float falloff = 1;
            if (epsilon != 0)
                falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
            float3 radiance = light.color.rgb * attenuation * falloff;
            float cosTheta = dot(N, -L);
            float bias = clamp(0.005 * tan(acos(cosTheta)), 0, 0.01);
            float shadow = ShadowCalculation(light, position, bias, depthPSM[wd], linearClampSampler);

            Lo += (1.0f - shadow) * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metalness);
        }
    }

	
    float3 ambient = emissive;
	
	[unroll(4)]
    for (uint i = 0; i < globalProbeCount; i++)
    { 
        ambient += BRDF_IBL(linearWrapSampler, globalDiffuse[i], globalSpecular[i], brdfLUT, F0, N, V, baseColor.xyz, roughness, ao);
    }
	
    Pixel output;
    output.Color = float4(ambient + Lo * ao, baseColor.a);
    output.Position = float4(position, 1);
    output.Normal = float4(N, baseColor.a);
    return output;
}
