#include "../../camera.hlsl"
#include "../../light.hlsl"
#include "../../cluster.hlsl"

#define GROUP_SIZE (CLUSTERS_X_THREADS * CLUSTERS_Y_THREADS * CLUSTERS_Z_THREADS)

StructuredBuffer<Cluster> clusters : register(t0);
StructuredBuffer<Light> lights : register(t1);

RWStructuredBuffer<uint> lightIndexCounter : register(u0); //1
RWStructuredBuffer<uint> lightIndexList : register(u1); //MAX_CLUSTER_LIGHTS * 16^3
RWStructuredBuffer<LightGrid> lightGrid : register(u2); //16^3

struct SharedLight
{
    uint type;
    float3 pos;
    float range;
};

groupshared SharedLight sharedLights[MAX_LIGHTS_PER_CLUSTER];

bool LightIntersectsCluster(SharedLight light, Cluster cluster)
{
    if (light.type == DIRECTIONAL_LIGHT)
        return true;

    float3 closest = max(cluster.minPoint.xyz, min(light.pos, cluster.maxPoint.xyz));

    float3 dist = closest - light.pos;
    return dot(dist, dist) <= (light.range * light.range);
}

[numthreads(CLUSTERS_X_THREADS, CLUSTERS_Y_THREADS, CLUSTERS_Z_THREADS)]
void main(uint3 groupId : SV_GroupID, uint3 dispatchThreadId : SV_DispatchThreadID, uint3 groupThreadId : SV_GroupThreadID, uint groupIndex : SV_GroupIndex)
{
    if (all(dispatchThreadId == 0))
    {
        lightIndexCounter[0] = 0;
    }

    uint visibleLightCount = 0;
    uint visibleLightIndices[MAX_LIGHTS_PER_CLUSTER];

    uint clusterIndex = groupIndex + GROUP_SIZE * groupId.z;

    Cluster cluster = clusters[clusterIndex];

    uint lightOffset = 0;
    uint lightCount, dummy;
    lights.GetDimensions(lightCount, dummy);

    while (lightOffset < lightCount)
    {
        uint batchSize = min(GROUP_SIZE, lightCount - lightOffset);

        if (groupIndex < batchSize)
        {
            uint lightIndex = lightOffset + groupIndex;

            Light light = lights[lightIndex];
            SharedLight sharedLight;
            sharedLight.pos = mul(light.position, view).xyz;
            sharedLight.type = light.type;
            sharedLight.range = light.range;
            sharedLights[groupIndex] = sharedLight;
        }

        GroupMemoryBarrierWithGroupSync();

        for (uint i = 0; i < batchSize; i++)
        {
            if (visibleLightCount < MAX_LIGHTS_PER_CLUSTER && LightIntersectsCluster(sharedLights[i], cluster))
            {
                visibleLightIndices[visibleLightCount] = lightOffset + i;
                visibleLightCount++;
            }
        }

        lightOffset += batchSize;
    }

    GroupMemoryBarrierWithGroupSync();

    uint offset = 0;
    InterlockedAdd(lightIndexCounter[0], visibleLightCount, offset);

    for (uint i = 0; i < visibleLightCount; i++)
    {
        lightIndexList[offset + i] = visibleLightIndices[i];
    }

    lightGrid[clusterIndex].lightOffset = offset;
    lightGrid[clusterIndex].lightCount = visibleLightCount;
}