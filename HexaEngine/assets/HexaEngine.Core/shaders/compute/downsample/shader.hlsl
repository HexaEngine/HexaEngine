Texture2D<float4> input;
RWTexture2D<float4> output;
SamplerState samplerState;

cbuffer params
{
    float2 texelSize;
    float2 padd;
};

[numthreads(32, 32, 1)]
void main(uint3 threadId : SV_DispatchThreadID)
{
    float4 sample1 = input.SampleLevel(samplerState, (threadId.xy + uint2(0, 0)) * texelSize, 0);
    float4 sample2 = input.SampleLevel(samplerState, (threadId.xy + uint2(1, 0)) * texelSize, 0);
    float4 sample3 = input.SampleLevel(samplerState, (threadId.xy + uint2(0, 1)) * texelSize, 0);
    float4 sample4 = input.SampleLevel(samplerState, (threadId.xy + uint2(1, 1)) * texelSize, 0);

    float averaged = (sample1 + sample2 + sample3 + sample4) / 4.0f;

    output[threadId.xy] = averaged;
}