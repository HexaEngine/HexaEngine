////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../../gbuffer.hlsl" 
#include "../../brdf.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"
#define Shininess 20.0;
#define SHADOW_SAMPLE_COUNT 17;
#define SHADOW_SAMPLE_COUNT_HALF 8;
#define SHADOW_SAMPLE_COUNT_HALF_NEG -8;
#define SHADOW_SAMPLE_COUNT_SQUARE 289;
#define PI 3.14159265359;

Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D cleancoatNormalTexture : register(t3);
Texture2D emissionTexture : register(t4);
Texture2D misc0Texture : register(t5);
Texture2D misc1Texture : register(t6);
Texture2D misc2Texture : register(t7);
TextureCube irradianceTexture : register(t8);
TextureCube prefilterTexture : register(t9);
Texture2D brdfLUT : register(t10);
Texture2D ssao : register(t11);

Texture2DArray depthMapTexture : register(t12);
TextureCubeArray depthOSM : register(t13);

SamplerState SampleTypePoint : register(s0);

//////////////
// TYPEDEFS //
//////////////
struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float ShadowCalculation(PointLightSD light, float3 fragPos, uint index, TextureCubeArray depthTex, SamplerState state)
{
    // get vector between fragment position and light position
    float3 fragToLight = fragPos - light.position;
    float3 texCoord = float3(fragToLight.x, fragToLight.y, fragToLight.z);
    // use the light to fragment vector to sample from the depth map    
    float closestDepth = depthTex.Sample(state, float4(texCoord, index)).r; //texture(depthMap, fragToLight).r;
    // it is currently in linear range between [0,1]. Re-transform back to original value
    closestDepth *= 25;
    // now get current linear depth as the length between the fragment and light position
    float currentDepth = length(fragToLight);
    // now test for shadows
    float bias = 0.05;
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

    return shadow;
}

float ShadowCalculation(DirectionalLightSD light, float3 fragPosWorldSpace, float3 normal)
{
    float cascadePlaneDistances[16] = (float[16]) light.cascades;
    float farPlane = 100;
    
    float w;
    float h;
    uint cascadeCount;
    depthMapTexture.GetDimensions(w, h, cascadeCount);

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

    // PCF
    float shadow = 0.0;
    float2 texelSize = 1.0 / float2(w, h);
    [unroll]
    for (int x = -1; x <= 1; ++x)
    {
        [unroll]
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = depthMapTexture.Sample(SampleTypePoint, float3(projCoords.xy + float2(x, y) * texelSize, layer)).r;
            shadow += (currentDepth - 0.005) > pcfDepth ? 1.0 : 0.0;
        }
    }

    shadow /= 9;

        // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if (currentDepth > 1.0)
    {
        shadow = 0.0;
    }
        
    return shadow;
}

float4 ComputeLightingPBR(VSOut input, GeometryAttributes attrs)
{
    float3 position = attrs.pos;
    float3 baseColor = attrs.albedo;
    
    float roughness = attrs.roughness;
    float metalness = attrs.metalness;
	
    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metalness);

    float3 Lo = float3(0.0f, 0.0f, 0.0f);
    
    for (uint x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        Lo += BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, baseColor, roughness, metalness);
    }
    
    for (uint y = 0; y < directionalLightSDCount; y++)
    {
        DirectionalLightSD light = directionalLightSDs[y];
        float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
        float shadow = ShadowCalculation(light, attrs.pos, N);
        Lo += (1.0f - shadow) * BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, baseColor, roughness, metalness);
    }
    
    for (uint z = 0; z < pointLightCount; z++)
    {
        PointLight light = pointLights[z];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);
        
        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;
        
        Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
    }

    for (uint zd = 0; zd < pointLightSDCount; zd++)
    {
        PointLightSD light = pointLightSDs[zd];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);
        
        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;
        float shadow = ShadowCalculation(light, attrs.pos, zd, depthOSM, SampleTypePoint);
        //Lo = float3(shadow,0,0);
        Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
    }
    
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
            Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
        }
    }
		
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = BRDFIndirect2(SampleTypePoint, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, baseColor, roughness, ao);
	
    float3 color = ambient + Lo;
    
    return float4(color, attrs.opacity);
}

float3 ACESFilm(float3 x)
{
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
    float gamma = 2.0;
    return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float4 main(VSOut pixel) : SV_TARGET
{
    GeometryAttributes attrs;
    ExtractGeometryData(
	pixel.Tex,
	colorTexture,
	positionTexture,
	normalTexture,
	cleancoatNormalTexture,
	emissionTexture,
	misc0Texture,
	misc1Texture,
	misc2Texture,
	SampleTypePoint,
	attrs);
    float4 color = ComputeLightingPBR(pixel, attrs);
    color.rgb = OECF_sRGBFast(color.rgb);
    color.rgb = ACESFilm(color.rgb);
    return color;
}

