#include "../../camera.hlsl"

float3 extract_scale(in float4x4 m)
{
    float sx = length(float3(m[0][0], m[0][1], m[0][2]));
    float sy = length(float3(m[1][0], m[1][1], m[1][2]));
    float sz = length(float3(m[2][0], m[2][1], m[2][2]));

    // if determine is negative, we need to invert one scale
    float det = determinant(m);
    if (det < 0)
    {
        sx = -sx;
    }

    float3 scale;
    scale.x = sx;
    scale.y = sy;
    scale.z = sz;
    return scale;
}

bool projectSphere(float3 c, float r, float znear, float P00, float P11, out float4 aabb)
{
    if (c.z < r + znear)
        return false;

    float3 cr = c * r;
    float czr2 = c.z * c.z - r * r;

    float vx = sqrt(c.x * c.x + czr2);
    float minx = (vx * c.x - cr.z) / (vx * c.z + cr.x);
    float maxx = (vx * c.x + cr.z) / (vx * c.z - cr.x);

    float vy = sqrt(c.y * c.y + czr2);
    float miny = (vy * c.y - cr.z) / (vy * c.z + cr.y);
    float maxy = (vy * c.y + cr.z) / (vy * c.z - cr.y);

    aabb = float4(minx * P00, miny * P11, maxx * P00, maxy * P11);
    aabb = aabb.xwzy * float4(0.5f, -0.5f, 0.5f, -0.5f) + float4(0.5f, 0.5f, 0.5f, 0.5f); // clip space -> uv space

    return true;
}

struct Sphere
{
    float3 center;
    float radius;
};

struct Box
{
    float3 min;
    float3 max;
};

struct InstanceData
{
    uint type;
    float4x4 world;
    Box boundingBox;
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
    float padding;
    float4 frustum;
};

Texture2D inputRT : register(t0);
StructuredBuffer<InstanceData> instanceDataIn : register(t1);
RWStructuredBuffer<float4x4> instanceDataOut : register(u0);
RWStructuredBuffer<uint> instanceOffsets : register(u1);
RWStructuredBuffer<DrawIndexedInstancedIndirectArgs> drawArgs : register(u2);
SamplerState samplerPoint : register(s0);

groupshared uint temp[1024];

[numthreads(1024, 1, 1)]
void main(uint3 threadID : SV_DispatchThreadID)
{
    uint di = threadID.x;

    if (di >= NoofInstances)
        return;

    uint visible = 1;

    InstanceData data = instanceDataIn[di];
    Sphere boundingSphere = data.boundingSphere;
    Box boundingBox = data.boundingBox;

    float3 center = mul(mul(float4(boundingSphere.center, 1), data.world), view);
    float radius = boundingSphere.radius * length(extract_scale(data.world));

    // the left/top/right/bottom plane culling utilizes frustum symmetry to cull against two planes at the same time
    visible = visible && center.z * frustum[1] - abs(center.x) * frustum[0] > -radius;
    visible = visible && center.z * frustum[3] - abs(center.y) * frustum[2] > -radius;
	// the near/far plane culling uses camera space Z directly
    visible = visible && center.z + radius > camNear && center.z - radius < camFar;

    visible = visible || FrustumCulling == 0;

    if (visible && OcclusionCulling)
    {
        float3 bboxMin = mul(float4(boundingBox.min, 1), data.world);
        float3 bboxMax = mul(float4(boundingBox.max, 1), data.world);
        float3 boxSize = bboxMax - bboxMin;

        float3 boxCorners[] =
        {
            bboxMin.xyz,
            bboxMin.xyz + float3(boxSize.x, 0, 0),
			bboxMin.xyz + float3(0, boxSize.y, 0),
			bboxMin.xyz + float3(0, 0, boxSize.z),
			bboxMin.xyz + float3(boxSize.xy, 0),
			bboxMin.xyz + float3(0, boxSize.yz),
			bboxMin.xyz + float3(boxSize.x, 0, boxSize.z),
			bboxMin.xyz + boxSize.xyz
        };

        float minZ = 1;
        float2 minXY = 1;
        float2 maxXY = 0;

	    [unroll]
        for (int i = 0; i < 8; i++)
        {
		    //transform world space aaBox to NDC
            float4 clipPos = mul(float4(boxCorners[i], 1), viewProj);

            clipPos.z = max(clipPos.z, 0);

            clipPos.xyz = clipPos.xyz / clipPos.w;

            clipPos.xy = clamp(clipPos.xy, -1, 1);
            clipPos.y = -clipPos.y;
            clipPos.xy = clipPos.xy * 0.5 + 0.5;

            minXY = min(clipPos.xy, minXY);
            maxXY = max(clipPos.xy, maxXY);

            minZ = saturate(min(minZ, clipPos.z));
        }

        float4 boxUVs = float4(minXY, maxXY);

	    // Calculate hi-Z buffer mip
        float2 size = (maxXY - minXY) * screenDim.xy;
        float mip = floor(log2(max(size.x, size.y)));

	    // Texel footprint for the lower (finer-grained) level
        float level_lower = max(mip - 1, 0);
        float2 scale = exp2(-level_lower) * screenDim.xy;
        float2 a = floor(boxUVs.xy * scale);
        float2 b = ceil(boxUVs.zw * scale);
        float2 dims = b - a;

	    // Use the lower level if we only touch <= 2 texels in both dimensions
        if (dims.x <= 2 && dims.y <= 2)
            mip = level_lower;

	    //load depths from high z buffer
        float4 depth = float4(
        inputRT.SampleLevel(samplerPoint, boxUVs.xy, mip).r,
		inputRT.SampleLevel(samplerPoint, boxUVs.zy, mip).r,
		inputRT.SampleLevel(samplerPoint, boxUVs.xw, mip).r,
		inputRT.SampleLevel(samplerPoint, boxUVs.zw, mip).r);

	    //find the max depth
        float maxDepth = max(max(max(depth.x, depth.y), depth.z), depth.w);
        visible = visible && minZ <= maxDepth;
    }

    if (visible)
    {
        instanceDataOut[di] = instanceDataIn[di].world;

		//increase instance count for this particular prop type
        InterlockedAdd(drawArgs[instanceDataIn[di].type].InstanceCount, 1);
    }

    temp[di] = visible;

    if (di == 0)
    {
        uint j = 0;
        for (uint i = 0; i < NoofInstances; i++)
        {
            if (temp[i] == 1)
            {
                instanceDataOut[j++] = instanceDataOut[i];
            }
        }

        uint baseOffset = 0;
        for (uint x = 0; x < NoofPropTypes; x++)
        {
            instanceOffsets[x] = baseOffset;
            baseOffset += drawArgs[x].InstanceCount;
        }
    }
}