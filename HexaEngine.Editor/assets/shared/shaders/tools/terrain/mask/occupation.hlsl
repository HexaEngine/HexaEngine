cbuffer ParamsBuffer
{
    uint2 textureSize;
};

Texture2D<float4> maskTex;
RWStructuredBuffer<uint4> outputBuffer;

[numthreads(32, 32, 1)]
void main(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    if (dispatchThreadId.x >= textureSize.x || dispatchThreadId.y >= textureSize.y)
    {
        return;
    }

    float4 pixel = maskTex.Load(int3(dispatchThreadId.x, dispatchThreadId.y, 0));

    if (pixel.r > 0)
    {
        InterlockedAdd(outputBuffer[0].r, 1);
    }
    if (pixel.g > 0)
    {
        InterlockedAdd(outputBuffer[0].g, 1);
    }
    if (pixel.b > 0)
    {
        InterlockedAdd(outputBuffer[0].b, 1);
    }
    if (pixel.a > 0)
    {
        InterlockedAdd(outputBuffer[0].a, 1);
    }
}