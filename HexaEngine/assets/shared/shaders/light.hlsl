struct GlobalProbe
{
    float exposure;
    float horizonCutOff;
    float3 orientation;
};

#define POINT_LIGHT 0
#define SPOT_LIGHT 1
#define DIRECTIONAL_LIGHT 2

#define SHADOW_ATLAS_SIZE 8192

struct Light
{
    uint type;

    float4 color;
    float4 direction;
    float4 position;

    float range;
    float outerCosine;
    float innerCosine;

    bool castsShadows;
    bool cascadedShadows;
    float volumetricStrength;

    uint1 padd;
};

struct ShadowData
{
    float4x4 views[8];
    float4 cascades[2];
    float size;
    float softness;
    float cascadeCount;
    float2 offsets[8];
};

float3 GetShadowUVD(float3 pos, float4x4 view)
{
    float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
    fragPosLightSpace.y = -fragPosLightSpace.y;
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords.xy = projCoords.xy * 0.5 + 0.5;
    return projCoords;
}

float3 GetShadowAtlasUVD(float3 pos, float size, float2 offset, float4x4 view)
{
    float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
    fragPosLightSpace.y = -fragPosLightSpace.y;
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords.xy = projCoords.xy * 0.5 + 0.5;

    projCoords.xy = (projCoords.xy * size) / SHADOW_ATLAS_SIZE;
    projCoords.xy += (offset.xy) / SHADOW_ATLAS_SIZE;

    return projCoords;
}

#define PI 3.14159265358979323846

float Attenuation(float distance, float range)
{
    float att = saturate(1.0f - (distance * distance / (range * range)));
    return att * att;
}

float3 LambertDiffuse(float3 radiance, float3 L, float3 N)
{
    float NdotL = max(0, dot(N, L));
    return radiance * NdotL;
}

float3 BlinnPhong(float3 radiance, float3 L, float3 V, float3 N, float3 baseColor, float shininess)
{
    float NdotL = max(0, dot(N, L));
    float3 diffuse = radiance * NdotL;

    const float kEnergyConservation = (8.0 + shininess) / (8.0 * PI);
    float3 halfwayDir = normalize(L + V);
    float spec = kEnergyConservation * pow(max(dot(N, halfwayDir), 0.0), shininess);

    float3 specular = radiance * spec;

    return (diffuse + specular) * baseColor;
}

float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 BRDF(float3 radiance, float3 L, float3 F0, float3 V, float3 N, float3 albedo, float roughness, float metalness)
{
    float3 H = normalize(V + L);

    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

    float3 kD = float3(1.0f, 1.0f, 1.0f) - F;
    kD *= 1.0 - metalness;

    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    float3 specular = numerator / denominator;

    float NdotL = saturate(dot(N, L));
    return (kD * albedo / PI + specular) * radiance * NdotL;
}

float3 BRDF_IBL(
SamplerState samplerState,
TextureCube irradianceTex,
TextureCube prefilterMap,
Texture2D brdfLUT,
float3 F0, float3 N, float3 V, float3 albedo, float roughness)
{
    float3 irradiance = irradianceTex.Sample(samplerState, N).rgb;
    float3 kS = FresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);
    float3 kD = 1.0 - kS;
    float3 diffuse = irradiance * albedo;

    float3 R = reflect(-V, N);
    const float MAX_REFLECTION_LOD = 4.0;

    float3 prefilteredColor = prefilterMap.SampleLevel(samplerState, R, roughness * MAX_REFLECTION_LOD).rgb;
    float2 brdf = brdfLUT.Sample(samplerState, float2(max(dot(N, V), 0.0), roughness)).rg;
    float3 specular = prefilteredColor * (kS * brdf.x + brdf.y);

    return kD * diffuse + specular;
}

float3 DirectionalLightBRDF(Light light, float3 F0, float3 V, float3 N, float3 baseColor, float roughness, float metallic)
{
    float3 L = normalize(-light.direction);
    float3 radiance = light.color.rgb;

    return BRDF(radiance, L, F0, V, N, baseColor, roughness, metallic);
}

float3 PointLightBRDF(Light light, float3 position, float3 F0, float3 V, float3 N, float3 baseColor, float roughness, float metallic)
{
    float3 LN = light.position.xyz - position;
    float distance = length(LN);
    float3 L = normalize(LN);

    float attenuation = Attenuation(distance, light.range);
    float3 radiance = light.color.rgb * attenuation;

    return BRDF(radiance, L, F0, V, N, baseColor, roughness, metallic);
}

float3 SpotlightBRDF(Light light, float3 position, float3 F0, float3 V, float3 N, float3 baseColor, float roughness, float metallic)
{
    float3 LN = light.position.xyz - position;
    float3 L = normalize(LN);

    float theta = dot(L, normalize(-light.direction.xyz));
    if (theta > light.outerCosine)
    {
        float distance = length(LN);
        float epsilon = light.innerCosine - light.outerCosine;
        float falloff = 1;
        if (epsilon != 0)
            falloff = 1 - smoothstep(0.0, 1.0, (theta - light.innerCosine) / epsilon);
        float attenuation = Attenuation(distance, light.range);
        float3 radiance = light.color.rgb * attenuation * falloff;
        return BRDF(radiance, L, F0, V, N, baseColor, roughness, metallic);
    }

    return 0;
}