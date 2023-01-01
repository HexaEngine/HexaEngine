Texture2D<float4> input;
RWTexture2D<float4> output;

cbuffer params
{
	float2 texelSize;
	float2 padd;
};

[numthreads(1, 1, 1)]
void main(uint2 groupIndex : SV_GroupThreadID, uint3 threadId : SV_DispatchThreadID)
{
	float2 texelCoords = threadId.xy * texelSize;

	float4 vTexels;
	vTexels.x = input.Load(int3((int2)texelCoords, 0), int2(0, 0)).r;
	vTexels.y = input.Load(int3((int2)texelCoords, 0), int2(1, 0)).r;
	vTexels.z = input.Load(int3((int2)texelCoords, 0), int2(0, 1)).r;
	vTexels.w = input.Load(int3((int2)texelCoords, 0), int2(1, 1)).r;

	float gatheredTexelMins = max(max(vTexels.x, vTexels.y), max(vTexels.z, vTexels.w));

	output[threadId.xy] = float4(gatheredTexelMins, gatheredTexelMins, gatheredTexelMins, gatheredTexelMins);
}