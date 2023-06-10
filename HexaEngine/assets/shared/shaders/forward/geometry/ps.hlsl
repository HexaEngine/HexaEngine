#include "defs.hlsl"
#include "../../light.hlsl"
#include "../../camera.hlsl"
#include "../../brdf.hlsl"

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metalness
#define Metalness 0
#endif
#ifndef Specular
#define Specular 0.5
#endif
#ifndef SpecularTint
#define SpecularTint 0
#endif
#ifndef Sheen
#define Sheen 0
#endif
#ifndef SheenTint
#define SheenTint 1
#endif
#ifndef Clearcoat
#define Clearcoat 0
#endif
#ifndef ClearcoatGloss
#define ClearcoatGloss 1
#endif
#ifndef Anisotropic
#define Anisotropic 0
#endif
#ifndef Subsurface
#define Subsurface 0
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

SamplerState SampleTypePoint : register(s8);
SamplerState SampleTypeAnsio : register(s9);

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

float ShadowCalculation(SpotlightSD light, float3 fragPos, float bias, Texture2D depthTex, SamplerState state)
{
#if HARD_SHADOWS
	float4 fragPosLightSpace = mul(float4(fragPos, 1.0), light.view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;

	projCoords = projCoords * 0.5 + 0.5;

	float depth = depthTex.Sample(state, projCoords.xy).r;
	float shadow += (currentDepth - bias) > depth ? 1.0 : 0.0;

	if (currentDepth > 1.0)
		shadow = 0;

	return shadow;
#else
    float4 fragPosLightSpace = mul(float4(fragPos, 1.0), light.view);
    fragPosLightSpace.y = -fragPosLightSpace.y;
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    float currentDepth = projCoords.z;

    projCoords = projCoords * 0.5 + 0.5;

    float w;
    float h;
    depthTex.GetDimensions(w, h);

	// PCF
    float shadow = 0.0;
    float2 texelSize = 1.0 / float2(w, h);
	[unroll]
    for (int x = -1; x <= 1; ++x)
    {
		[unroll]
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = depthTex.Sample(state, projCoords.xy + float2(x, y) * texelSize).r;
            shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
        }
    }

    shadow /= 9;

    if (currentDepth > 1.0)
        shadow = 0;

    return shadow;
#endif
}

// array of offset direction for sampling
float3 gridSamplingDisk[20] =
{
    float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
   float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
   float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
   float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
   float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowCalculation(PointLightSD light, float3 fragPos, float3 V, TextureCube depthTex, SamplerState state)
{
#if (HARD_SHADOWS)
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	// use the light to fragment vector to sample from the depth map
	float closestDepth = depthTex.Sample(state, fragToLight).r; //texture(depthMap, fragToLight).r;
	// it is currently in linear range between [0,1]. Re-transform back to original value
	closestDepth *= light.far;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	// now test for shadows
	float bias = 0.05;
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	return shadow;
#else
	// get vector between fragment position and light position
    float3 fragToLight = fragPos - light.position;
	// now get current linear depth as the length between the fragment and light position
    float currentDepth = length(fragToLight);
    float shadow = 0.0;
    float bias = 0.15;
    float viewDistance = length(V);
    float diskRadius = (1.0 + (viewDistance / light.far)) / 25.0;

	[unroll]
    for (int i = 0; i < 20; ++i)
    {
        float closestDepth = depthTex.Sample(state, fragToLight + gridSamplingDisk[i] * diskRadius).r;
        closestDepth *= light.far; // undo mapping [0;1]
        shadow += (currentDepth - bias) > closestDepth ? 1.0 : 0.0;
    }
    shadow /= 20;
    return shadow;
#endif
}

float ShadowCalculation(DirectionalLightSD light, float3 fragPosWorldSpace, float3 normal, Texture2DArray depthTex, SamplerState state)
{
    float cascadePlaneDistances[16] = (float[16]) light.cascades;
    float farPlane = cam_far;

    float w;
    float h;
    uint cascadeCount;
    depthTex.GetDimensions(w, h, cascadeCount);

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(fragPosWorldSpace, 1.0), view);
    float depthValue = abs(fragPosViewSpace.z);
    float cascadePlaneDistance;
    uint layer = cascadeCount;
    for (uint i = 0; i < cascadeCount; ++i)
    {
        if (depthValue < cascadePlaneDistances[i])
        {
            cascadePlaneDistance = cascadePlaneDistances[i];
            layer = i;
            break;
        }
    }

    float4 fragPosLightSpace = mul(float4(fragPosWorldSpace, 1.0), light.views[layer]);
    fragPosLightSpace.y = -fragPosLightSpace.y;
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    float currentDepth = projCoords.z;
    projCoords = projCoords * 0.5 + 0.5;

	// calculate bias (based on depth map resolution and slope)
    normal = normalize(normal);
    float bias = max(0.05 * (1.0 - dot(normal, light.dir)), 0.005);
    const float biasModifier = 0.5f;
    if (layer == cascadeCount)
    {
        bias *= 1 / (farPlane * biasModifier);
    }
    else
    {
        bias *= 1 / (cascadePlaneDistance * biasModifier);
    }

#if (HARD_SHADOWS)
	float depth = depthTex.Sample(state, float3(projCoords.xy, layer)).r;
	float shadow = (currentDepth - bias) > depth ? 1.0 : 0.0;

	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
	if (currentDepth > 1.0)
	{
		shadow = 0.0;
	}

	return shadow;
#else
	// PCF
    float shadow = 0.0;
    float2 texelSize = 1.0 / float2(w, h);
	[unroll]
    for (int x = -1; x <= 1; ++x)
    {
		[unroll]
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = depthTex.Sample(state, float3(projCoords.xy + float2(x, y) * texelSize, layer)).r;
            shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
        }
    }

    shadow /= 9;

	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if (currentDepth > 1.0)
    {
        shadow = 0.0;
    }

    return shadow;
#endif
}

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
#if VtxColor
    float4 baseColor = input.color;
#else
    float4 baseColor = BaseColor;
#endif
#if VtxNormal
    float3 normal = normalize(input.normal);
#endif
#if VtxTangent
    float3 tangent = normalize(input.tangent);
#endif
#if VtxBitangent
    float3 bitangent = normalize(input.bitangent);
#endif
    float3 emissive = Emissive;
    float opacity = 1;

    float ao = Ao;
    float specular = Specular;
    float specularTint = SpecularTint;
    float sheen = Sheen;
    float sheenTint = SheenTint;
    float clearcoat = Clearcoat;
    float clearcoatGloss = ClearcoatGloss;
    float anisotropic = Anisotropic;
    float subsurface = Subsurface;
    float roughness = Roughness;
    float metalness = Metalness;
	
#if VtxUV
#if HasBaseColorTex
	float4 color = baseColorTexture.Sample(baseColorTextureSampler, (float2) input.tex);
    baseColor = float4(color.rgb * color.a, color.a);
#endif

#if VtxTangent
#if HasNormalTex
#if VtxBitangent
    normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2) input.tex).rgb, normal, tangent, bitangent);
#else
	normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2) input.tex).rgb, normal, tangent);
#endif
#endif
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
#endif

    if (baseColor.a < 0.1f)
        discard;

    float3 N = normal;
    float3 X = tangent;
#if VtxBitangent
	float3 Y = bitangent;
#else
    float3 Y = normalize(cross(N, X));
#endif
    float3 V = normalize(GetCameraPos() - position);

    float3 Lo = 0;

    [loop]
    for (uint x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        Lo += BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
    }

    [unroll(32)]
    for (uint y = 0; y < directionalLightSDCount; y++)
    {
        DirectionalLightSD light = directionalLightSDs[y];
        float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
        float shadow = ShadowCalculation(light, position, N, depthCSM, SampleTypeAnsio);
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
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

        Lo += BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
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
        float shadow = ShadowCalculation(light, position, V, depthOSM[zd], SampleTypeAnsio);
        Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
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
            Lo += BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
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
            float shadow = ShadowCalculation(light, position, bias, depthPSM[wd], SampleTypeAnsio);

            Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor.xyz, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
        }
    }

	
    float3 ambient = emissive;
	
	[unroll(4)]
    for (uint i = 0; i < globalProbeCount; i++)
    {
        ambient += BRDFIndirect(SampleTypeAnsio, globalDiffuse[i], globalSpecular[i], brdfLUT, N, V, baseColor.xyz, metalness, roughness, clearcoat, clearcoatGloss, sheen, sheenTint, ao, anisotropic);
    }
	
    Pixel output;
    output.Color = float4(ambient + Lo * ao, baseColor.a);
    output.Position = float4(position, 1);
    output.Normal = float4(N, baseColor.a);
    return output;
}
