#include "../../camera.hlsl"

struct InstanceData
{
    uint type;
    float4x4 world;
    float3 min;
    float3 max;
    float3 center;
    float radius;
};

struct DrawIndexedInstancedIndirectArgs
{
    uint IndexCountPerInstance;
    uint InstanceCount;
    uint StartIndexLocation;
    int BaseVertexLocation;
    uint StartInstanceLocation;
};

cbuffer Params
{
    uint NoofInstances;
    uint NoofPropTypes;
    bool ActivateCulling;
    uint MaxMipLevel;
    float2 RTSize;
    float2 Padding;
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

    uint predicate = 1;

    if (ActivateCulling)
    {
        float3 bboxMin = instanceDataIn[di].min.xyz;
        float3 bboxMax = instanceDataIn[di].max.xyz;
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
            float4 clipPos = mul(float4(boxCorners[i], 1), mul(view, proj));

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
        int2 size = (maxXY - minXY) * RTSize.xy;
        float mip = ceil(log2(max(size.x, size.y)));

        mip = clamp(mip, 0, MaxMipLevel);

	    // Texel footprint for the lower (finer-grained) level
        float level_lower = max(mip - 1, 0);
        float2 scale = exp2(-level_lower) * RTSize.xy;
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
        predicate = predicate && minZ <= maxDepth; 
    }
    
    if (predicate)
    {
        instanceDataOut[di] = instanceDataIn[di].world;

		//increase instance count for this particular prop type
        InterlockedAdd(drawArgs[instanceDataIn[di].type].InstanceCount, 1);
    }

    temp[di] = predicate;

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