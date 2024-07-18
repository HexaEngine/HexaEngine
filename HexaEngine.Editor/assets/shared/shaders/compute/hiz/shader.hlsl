Texture2D<float> input;
RWTexture2D<float> output;
SamplerState samplerPoint;

cbuffer params
{
	float2 texelSize;
	float2 padd;
};

[numthreads(32, 32, 1)]
void main(uint3 threadId : SV_DispatchThreadID)
{
	float4 depths = input.Gather(samplerPoint, (threadId.xy + 0.5) * texelSize);
	output[threadId.xy] = max(max(depths.x, depths.y), max(depths.z, depths.w));
}