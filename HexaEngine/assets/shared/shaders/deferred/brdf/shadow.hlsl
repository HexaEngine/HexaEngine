#include "../../gbuffer.hlsl"
#include "../../brdf2.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"
#include "../../common.hlsl"

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

float ShadowCalculation(SpotlightSD light, float3 fragPos, float bias, Texture2D depthTex, SamplerState state)
{
#if HARD_SHADOWS_SPOTLIGHTS
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
static const float3 gridSamplingDisk[20] =
{
    float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
	float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
	float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
	float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
	float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowCalculation(PointLightSD light, float3 fragPos, float3 V, TextureCube depthTex, SamplerState state)
{
#if (HARD_SHADOWS_POINTLIGHTS)
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
    
    if (currentDepth > 1.0)
        shadow = 0;

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

#if (HARD_SHADOWS_CASCADED)
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
        float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
        float shadow = ShadowCalculation(light, position, N, depthCSM, linearClampSampler);
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        if (shadow == 1)
            continue;
        Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
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
        float shadow = ShadowCalculation(light, position, V, depthOSM[zd], linearClampSampler);
        if (shadow == 1)
            continue;
        Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
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
            float bias = max(0.00025f * (1.0f - dot(N, L)), 0.000005f);
            float shadow = ShadowCalculation(light, position, bias, depthPSM[wd], linearClampSampler);
            if (shadow == 1)
                continue;
            Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
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