#include "../../camera.hlsl"

struct Sphere
{
    float3 center;
    float radius;
};

struct InstanceData
{
    uint type;
    float4x4 world;
    Sphere boundingSphere;
};

struct DrawIndexedInstancedIndirectArgs
{
    uint IndexCountPerInstance;
    uint InstanceCount;
    uint StartIndexLocation;
    int BaseVertexLocation;
    uint StartInstanceLocation;
};

cbuffer CullingParams : register(b0)
{
    uint NoofInstances;
    uint NoofPropTypes;
    bool FrustumCulling;
    bool OcclusionCulling;
    uint MaxMipLevel;
    float P00;
    float P11;
    float depthBias;
    float4 frustum;
};