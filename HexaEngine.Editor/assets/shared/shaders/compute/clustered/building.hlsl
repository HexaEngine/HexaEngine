#include "../../camera.hlsl"
#include "../../cluster.hlsl"

RWStructuredBuffer<Cluster> clusters : register(u0);

float3 IntersectionZPlane(float3 B, float z_dist)
{
    //Because this is a Z based normal this is fixed
    float3 normal = float3(0.0, 0.0, -1.0);
    float3 d = B;
    //Computing the intersection length for the line and the plane
    float t = z_dist / d.z; //dot(normal, d);

    //Computing the actual xyz position of the point along the line
    float3 result = t * d;

    return result;
}

[numthreads(1, 1, 1)]
void main(uint3 groupId : SV_GroupID,
          uint3 dispatchThreadId : SV_DispatchThreadID,
          uint3 groupThreadId : SV_GroupThreadID,
          uint groupIndex : SV_GroupIndex)
{
    uint cluster_size, dummy;
    clusters.GetDimensions(cluster_size, dummy);

    uint tileSizePx = (uint) ceil(screenDim.x / (float) CLUSTERS_X);
    uint tile_index = groupId.x +
                      groupId.y * CLUSTERS_X +
                      groupId.z * (CLUSTERS_X * CLUSTERS_Y);

    float3 max_point_vs = GetPositionVS((groupId.xy + 1) * tileSizePx, 1.0f);
    float3 min_point_vs = GetPositionVS(groupId.xy * tileSizePx, 1.0f);

    float cluster_near = -camNear * pow(abs(camFar / camNear), groupId.z / float(CLUSTERS_Z));
    float cluster_far = -camNear * pow(abs(camFar / camNear), (groupId.z + 1) / float(CLUSTERS_Z));

    float3 minPointNear = IntersectionZPlane(min_point_vs, cluster_near);
    float3 minPointFar = IntersectionZPlane(min_point_vs, cluster_far);
    float3 maxPointNear = IntersectionZPlane(max_point_vs, cluster_near);
    float3 maxPointFar = IntersectionZPlane(max_point_vs, cluster_far);

    float3 minPointAABB = min(min(minPointNear, minPointFar), min(maxPointNear, maxPointFar));
    float3 maxPointAABB = max(max(minPointNear, minPointFar), max(maxPointNear, maxPointFar));

    clusters[tile_index].minPoint = float4(minPointAABB, 0.0);
    clusters[tile_index].maxPoint = float4(maxPointAABB, 0.0);
}