#include "../../gbuffer.hlsl"
#include "../../brdf2.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"
#include "../../common.hlsl"
#include "../../shadow.hlsl"

Texture2D GBufferA : register(t0);
Texture2D GBufferB : register(t1);
Texture2D GBufferC : register(t2);
Texture2D GBufferD : register(t3);
Texture2D<float> Depth : register(t4);
Texture2D Dither : register(t5);
Texture2D ssao : register(t8);

StructuredBuffer<DirectionalLightSD> directionalLights : register(t9);
StructuredBuffer<PointLightSD> pointLights : register(t10);
StructuredBuffer<SpotlightSD> spotlights : register(t11);

Texture2DArray depthCSM : register(t12);
TextureCube depthOSM[32] : register(t13);
Texture2D depthPSM[32] : register(t45);

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);
SamplerComparisonState shadowSampler : register(s3);

cbuffer constants : register(b0)
{
    uint directionalLightCount;
    uint pointLightCount;
    uint spotlightCount;
    uint PaddLight;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

/*
float InterleavedGradientNoise(float2 position_screen)
{
    position_screen += g_frame * any(g_taa_jitter_offset); // temporal factor
    float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
    return frac(magic.z * frac(dot(position_screen, magic.xy)));
}
*/

//https://panoskarabelas.com/posts/screen_space_shadows/
static const uint SSS_MAX_STEPS = 16; // Max ray steps, affects quality and performance.
static const float SSS_RAY_MAX_DISTANCE = 0.05f; // Max shadow length, longer shadows are less accurate.
static const float SSS_THICKNESS = 0.5f; // Depth testing thickness.
static const float SSS_STEP_LENGTH = SSS_RAY_MAX_DISTANCE / (float) SSS_MAX_STEPS;

float SSCS(float3 position, float2 uv, float3 direction)
{
    // Compute ray position and direction (in view-space)
    float3 ray_pos = mul(float4(position, 1.0f), view).xyz;
    float3 ray_dir = mul(float4(-direction, 0.0f), view).xyz;

    // Compute ray step
    float3 ray_step = ray_dir * SSS_STEP_LENGTH;
    
    //float offset = InterleavedGradientNoise(screen_dim * uv) * 2.0f - 1.0f;
    //ray_pos += ray_step * offset;
	
    // Ray march towards the light
    float occlusion = 0.0;
    float2 ray_uv = 0.0f;
    for (uint i = 0; i < SSS_MAX_STEPS; i++)
    {
        // Step the ray
        ray_pos += ray_step;
        ray_uv = ProjectUV(ray_pos, proj);

        // Ensure the UV coordinates are inside the screen
        if (IsSaturated(ray_uv))
        {
            // Compute the difference between the ray's and the camera's depth
            float depth_z = SampleLinearDepth(Depth, linearClampSampler, ray_uv);
            float depth_delta = ray_pos.z - depth_z;

            // Check if the camera can't "see" the ray (ray depth must be larger than the camera depth, so positive depth_delta)
            if ((depth_delta > 0.0f) && (depth_delta < SSS_THICKNESS))
            {
                // Mark as occluded
                occlusion = 1.0f;

                // Fade out as we approach the edges of the screen
                occlusion *= ScreenFade(ray_uv);

                break;
            }
        }
    }

    // Convert to visibility
    return 1.0f - occlusion;
}

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

float4 ComputeLightingPBR(VSOut input, float3 position, GeometryAttributes attrs)
{
    float3 baseColor = attrs.baseColor;
    float roughness = attrs.roughness;
    float metallic = attrs.metallic;

    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);

    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metallic);

    float3 Lo = float3(0, 0, 0);

	[unroll(1)]
    for (uint y = 0; y < directionalLightCount; y++)
    {
        DirectionalLightSD light = directionalLights[y];
        float shadow = ShadowCalculation(light, position, depthCSM, shadowSampler);
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        if (shadow == 0)
            continue;
        Lo += shadow * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
    }

	[unroll(32)]
    for (uint zd = 0; zd < pointLightCount; zd++)
    {
        PointLightSD light = pointLights[zd];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);

        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;
        float shadow = ShadowCalculation(light, position, V, depthOSM[zd], shadowSampler);
        if (shadow == 0)
            continue;
        Lo += shadow * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
    }

	[unroll(32)]
    for (uint wd = 0; wd < spotlightCount; wd++)
    {
        SpotlightSD light = spotlights[wd];
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
            Lo += shadow * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
        }
    }

    float ao = ssao.Sample(linearWrapSampler, input.Tex).r * attrs.ao;
    return float4(Lo * ao, 1);
}

float4 main(VSOut pixel) : SV_TARGET
{
    float depth = Depth.Sample(linearWrapSampler, pixel.Tex);
    float3 position = GetPositionWS(pixel.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.Tex, GBufferA, GBufferB, GBufferC, GBufferD, linearWrapSampler, attrs);

    return ComputeLightingPBR(pixel, position, attrs);
}