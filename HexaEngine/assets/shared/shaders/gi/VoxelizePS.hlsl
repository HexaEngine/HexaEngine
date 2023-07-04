#include "common.hlsl"
#include "../light.hlsl"
#include "../camera.hlsl"
#include "../shadow.hlsl"

SamplerComparisonState shadow_sampler : register(s2);

cbuffer VoxelCbuf : register(b0)
{
    VoxelRadiance voxel_radiance;
}

#define global_ambient float4(0,0,0,0)
#define albedo_factor 1

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 PositionWS : POSITION;
    float2 Uvs : TEX;
    float3 NormalWS : NORMAL0;
};

RWStructuredBuffer<VoxelType> VoxelGrid : register(u0);

Texture2D albedoTx : register(t0);

Texture2D shadowMap : register(t4);

Texture2DArray cascadeShadowMaps : register(t6);

StructuredBuffer<Light> lights : register(t10);

cbuffer ShadowCBuf : register(b3)
{
    float4x4 lightviewprojection;
    float4x4 lightview;
    float4x4 shadow_matrices[4];
    float4 splits;
    float softness;
    int shadow_map_size;
    int visualize;
}

void main(PS_INPUT input)
{

    float3 diff = (input.PositionWS.xyz - voxel_radiance.GridCenter) * voxel_radiance.DataResRCP * voxel_radiance.DataSizeRCP;
    float3 uvw = diff * float3(0.5f, -0.5f, 0.5f) + 0.5f;

    [branch]
    if (any(0 > uvw || 1 <= uvw))
    {
        return;
    }

    float3 normal = normalize(input.NormalWS);

    input.Uvs.y = 1 - input.Uvs.y;
    float4 color = albedoTx.Sample(linear_wrap_sampler, input.Uvs) * albedo_factor;

    uint light_count, _dummy;
    lights.GetDimensions(light_count, _dummy);
    light_count = min(light_count, max_voxel_lights);

    float4 lighting = 0;
    for (int i = 0; i < light_count; ++i)
    {
        Light light = lights[i];

        switch (light.type)
        {
            case DIRECTIONAL_LIGHT:
                float4 diffuse = LambertDiffuse(light.color.xyz, normalize(-light.direction.xyz), normal);
                if (light.castsShadows)
                {
                    float4 PosVS = mul(input.PositionWS, view);
                    PosVS /= PosVS.w;

                    float viewDepth = PosVS.z;
                    for (uint i = 0; i < 4; ++i)
                    {
                        matrix light_space_matrix = shadow_matrices[i];
                        if (viewDepth < splits[i])
                        {
                            float4 pos_shadow_map = mul(float4(PosVS.xyz, 1.0), light_space_matrix);
                            float3 UVD = pos_shadow_map.xyz / pos_shadow_map.w;
                            UVD.xy = 0.5 * UVD.xy + 0.5;
                            UVD.y = 1.0 - UVD.y;
                            diffuse *= CSMCalcShadowFactor_PCF3x3(shadow_sampler, cascadeShadowMaps, i, UVD, shadow_map_size, softness);
                            break;
                        }
                    }
                }

                lighting += diffuse;
                break;

            case POINT_LIGHT:

                light.position.xyz /= light.position.w;
                float3 L_ = light.position.xyz - input.PositionWS.xyz;
                float distance_ = length(L_);
                L_ = L_ / distance_;
                float attenuation_ = Attenuation(distance_, light.range);
                if (attenuation_ < 0.0001f)
                    break;
                lighting += LambertDiffuse(light.color.xyz, L_, normal) * attenuation_;
                break;
            case SPOT_LIGHT:
                float3 L = light.position.xyz - input.PositionWS.xyz;
                float distance = length(L);
                L /= distance;
                float attenuation = Attenuation(distance, light.range);
                if (attenuation < 0.0001f)
                    break;
                float3 normalized_light_dir = normalize(light.direction.xyz);
                float cosAng = dot(-normalized_light_dir, L);
                float conAtt = saturate((cosAng - light.innerCosine) / (light.outerCosine - light.innerCosine));
                conAtt *= conAtt;
                lighting += LambertDiffuse(light.color.xyz, L, normal) * attenuation * conAtt;
                break;

            default:
                lighting += 0;
                break;
        }
    }

    color *= (lighting + global_ambient);

    uint colorEncoded = EncodeColor(color);
    uint normalEncoded = EncodeNormal(normal);

    uint3 writecoord = floor(uvw * voxel_radiance.DataRes);
    uint id = Flatten3D(writecoord, voxel_radiance.DataRes);

    InterlockedMax(VoxelGrid[id].ColorMask, colorEncoded);
    InterlockedMax(VoxelGrid[id].NormalMask, normalEncoded);
}