#include "../../light.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"

Texture2D atlasTex : register(t0);
SamplerState samplerState : register(s0);

#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metallic
#define Metallic 0.0f
#endif
#ifndef Ao
#define Ao 1
#endif
#ifndef Emissive
#define Emissive float3(0,0,0);
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
SamplerComparisonState shadowSampler : register(s11);

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

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float3 TexCoord : TEXCOORD;
};

float ShadowCalculation(SpotlightSD light, float3 fragPos, Texture2D depthTex, SamplerComparisonState state)
{
    float3 uvd = GetShadowUVD(fragPos, light.view);
    
#if HARD_SHADOWS_SPOTLIGHTS
    
    return CalcShadowFactor_Basic(state, depthTex, uvd);
#else
    
    float w, h;
    depthTex.GetDimensions(w, h);
    
    return CalcShadowFactor_PCF3x3(state, depthTex, uvd, w, 1);
#endif
}

// array of offset direction for sampling
static const float3 gridSamplingDisk[20] =
{
    float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
	float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
	float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
	float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
	float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowCalculation(PointLightSD light, float3 fragPos, float3 V, TextureCube depthTex, SamplerComparisonState state)
{
    float3 light_to_pixelWS = fragPos - light.position.xyz;
    float depthValue = length(light_to_pixelWS) / light.far;
    
#if HARD_SHADOWS_POINTLIGHTS
    return depthTex.SampleCmpLevelZero(state, normalize(light_to_pixelWS.xyz), depthValue);
#else

    float percentLit = 0.0f;
    
    float viewDistance = length(V);
    float diskRadius = (1.0 + (viewDistance / light.far)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        percentLit += depthTex.SampleCmpLevelZero(state, normalize(light_to_pixelWS.xyz) + gridSamplingDisk[i] * diskRadius, depthValue);
    }

    return percentLit / 20;
#endif
}

float ShadowCalculation(DirectionalLightSD light, float3 fragPosWorldSpace, Texture2DArray depthTex, SamplerComparisonState state)
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
     
    float3 uvd = GetShadowUVD(fragPosWorldSpace, light.views[layer]);

#if HARD_SHADOWS_CASCADED
    return CSMCalcShadowFactor_Basic(state, depthTex, layer, uvd, w, 1);
#else
    return CSMCalcShadowFactor_PCF3x3(state, depthTex, layer, uvd, w, 1);
#endif
}

float4 main(PixelInputType input) : SV_TARGET
{
    float4 baseColor = atlasTex.Sample(samplerState, input.TexCoord.xy);
    
    if (baseColor.a == 0)
        discard;
    
    float3 position = input.Pos;

    float ao = Ao;
    float roughness = Roughness;
    float metallic = Metallic;
    float3 emissive = Emissive;

    float3 N = input.Normal;

    float3 V = normalize(GetCameraPos() - position);

    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor.xyz, metallic);

    float3 Lo;

    [loop]
    for (uint x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;

        Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
    }

    [unroll(32)]
    for (uint y = 0; y < directionalLightSDCount; y++)
    {
        DirectionalLightSD light = directionalLightSDs[y];
        float shadow = ShadowCalculation(light, position, depthCSM, shadowSampler);
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        if (shadow == 0)
            continue;
        Lo += shadow * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
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

        Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
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
        float shadow = ShadowCalculation(light, position, V, depthOSM[zd], shadowSampler);
        if (shadow == 0)
            continue;
        Lo += shadow * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
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
            Lo += BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
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
            float shadow = ShadowCalculation(light, position, depthPSM[wd], shadowSampler);
            if (shadow == 0)
                continue;
            Lo += shadow * BRDF(radiance, L, F0, V, N, baseColor.xyz, roughness, metallic);
        }
    }
    
    float3 ambient = emissive;
	
	[unroll(4)]
    for (uint i = 0; i < globalProbeCount; i++)
    {
        ambient += BRDF_IBL(linearWrapSampler, globalDiffuse[i], globalSpecular[i], brdfLUT, F0, N, V, baseColor.xyz, roughness, ao);
    }
    
    return float4(Lo, 1);
}
