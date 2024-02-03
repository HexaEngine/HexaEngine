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

Buffer<float4> InputBuffer0 : register(t0);
Buffer<float4> InputBuffer1 : register(t1);
RWBuffer<float4> OutputBuffer : register(u0);

[numthreads(NUM_BOUNCE_SUM_THREADS, 1, 1)]
void main(uint3 GroupID : SV_GroupID, uint3 DispatchThreadID : SV_DispatchThreadID,
					uint3 GroupThreadID : SV_GroupThreadID, uint GroupIndex : SV_GroupIndex)
{
    const uint index = GroupThreadID.x + GroupID.x * NUM_BOUNCE_SUM_THREADS;
    if (index < NumElements)
        OutputBuffer[index] = InputBuffer0[index] + InputBuffer1[index];
}