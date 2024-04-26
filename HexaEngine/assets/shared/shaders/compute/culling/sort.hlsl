#include "../../camera.hlsl"

cbuffer Params
{
	uint NoofInstances;
	uint NoofPropTypes;
	bool ActivateCulling;
	float2 RTSize;
	uint MaxMipLevel;
	float2 Padding;
};

struct InstanceData
{
	float4x4 world;
	float3 bboxMin;
	float3 bboxMax;
	uint index;
	float padding;
};

StructuredBuffer<bool> instanceFlagsDataIn : register(t0);
StructuredBuffer<InstanceData> instanceDataIn : register(t1);
RWStructuredBuffer<float4x4> instanceDataOut : register(u0);
RWStructuredBuffer<uint> instanceCounts : register(u1);

groupshared uint temp[2048];

[numthreads(1024, 1, 1)]
void main(uint3 threadID : SV_DispatchThreadID)
{
	int tID = threadID.x;

	int offset = 1;
	temp[2 * tID] = instanceFlagsDataIn[2 * tID]; // load input into shared memory
	temp[2 * tID + 1] = instanceFlagsDataIn[2 * tID + 1];

	//perform reduction
	for (int d = NoofInstances >> 1; d > 0; d >>= 1)
	{
		GroupMemoryBarrierWithGroupSync();

		if (tID < d)
		{
			int ai = offset * (2 * tID + 1) - 1;
			int bi = offset * (2 * tID + 2) - 1;
			temp[bi] += temp[ai];
		}
		offset *= 2;
	}

	// clear the last element
	if (tID == 0)
		temp[NoofInstances - 1] = 0;

	//perform downsweep and build scan
	for (int d = 1; d < NoofInstances; d *= 2) {
		offset >>= 1;

		GroupMemoryBarrierWithGroupSync();

		if (tID < d)
		{
			int ai = offset * (2 * tID + 1) - 1;
			int bi = offset * (2 * tID + 2) - 1;
			int t = temp[ai];
			temp[ai] = temp[bi];
			temp[bi] += t;
		}
	}

	GroupMemoryBarrierWithGroupSync();

	//scatter results
	if (instanceFlagsDataIn[2 * tID] == true)
	{
		instanceDataOut[temp[2 * tID]] = instanceDataIn[2 * tID].world;
	}

	if (instanceFlagsDataIn[2 * tID + 1] == true)
	{
		instanceDataOut[temp[2 * tID + 1]] = instanceDataIn[2 * tID + 1].world;
	}

	if (tID == 0)
	{
		//patch up the visible instance counts per prop type, could possible be done in a different compute shader
		for (int k = 1; k < NoofPropTypes; k++)
		{
			instanceCounts[k * 5 + 4] = instanceCounts[(k - 1) * 5 + 4] +   //previous prop type offset
				instanceCounts[(k - 1) * 5 + 1];    //previous prop type number of instances
		}
	}
}