#ifndef RT_SIZE
#define RT_SIZE 64
#endif

#ifndef NUM_FACES
#define NUM_FACES 5
#endif

#ifndef NUM_BOUNCE_SUM_THREADS
#define NUM_BOUNCE_SUM_THREADS 512
#endif

//======================================================================
//
//	DX11 Radiosity Sample
//  by MJP
//  http://mynameismjp.wordpress.com/
//
//  All code and content licensed under Microsoft Public License (Ms-PL)
//
//======================================================================

cbuffer Constants : register(b0)
{
    float4x4 ToTangentSpace[5];
    float FinalWeight;
    uint VertexIndex;
    uint NumElements;
}

Buffer<float4> InputBuffer : register(t0);
RWBuffer<float4> OutputBuffer : register(u0);

// Shared memory for reducing H-Basis coefficients
groupshared float4 ColumnHBasis[RT_SIZE][3][NUM_FACES];

[numthreads(RT_SIZE, 1, NUM_FACES)]
void main(uint3 GroupID : SV_GroupID, uint3 DispatchThreadID : SV_DispatchThreadID,
					uint3 GroupThreadID : SV_GroupThreadID, uint GroupIndex : SV_GroupIndex)
{
    const int3 location = int3(GroupThreadID.x, GroupID.y, GroupThreadID.z);

	// Store in shared memory
    ColumnHBasis[location.x][location.y][location.z] = InputBuffer[location.x + RT_SIZE * location.y + RT_SIZE * 3 * location.z];
    GroupMemoryBarrierWithGroupSync();

	// Sum the coefficients for the column
	[unroll(RT_SIZE)]
    for (uint s = RT_SIZE / 2; s > 0; s >>= 1)
    {
        if (GroupThreadID.x < s)
            ColumnHBasis[location.x][location.y][location.z] += ColumnHBasis[location.x + s][location.y][location.z];

        GroupMemoryBarrierWithGroupSync();
    }

	// Have the first thread write out to the output buffer
    if (GroupThreadID.x == 0 && GroupThreadID.z == 0)
    {
        float4 output = 0.0f;
		[unroll(NUM_FACES)]
        for (uint i = 0; i < NUM_FACES; ++i)
            output += ColumnHBasis[location.x][location.y][i];
        output *= FinalWeight;
        OutputBuffer[VertexIndex * 3 + location.y] = output;
    }
}