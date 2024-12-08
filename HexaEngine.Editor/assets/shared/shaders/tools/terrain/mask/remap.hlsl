cbuffer ParamsBuffer
{
    uint2 textureSize;
    uint source;
    uint destination;
    float4 factor;
};

Texture2D<float4> maskTex;
RWTexture2D<float4> outputTex;

[numthreads(32, 32, 1)]
void main(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    if (dispatchThreadId.x >= textureSize.x || dispatchThreadId.y >= textureSize.y)
    {
        return;
    }

    int3 location = int3(dispatchThreadId.x, dispatchThreadId.y, 0);

    float4 pixel = maskTex.Load(location);

    float value = 0;

    switch (destination)
    {
        case 0:
            value = pixel.r;
            break;
        case 1:
            value = pixel.g;
            break;
        case 2:
            value = pixel.b;
            break;
        case 3:
            value = pixel.a;
            break;
    }

    switch (destination)
    {
        case 0:
            pixel.r = value;
            break;
        case 1:
            pixel.g = value;
            break;
        case 2:
            pixel.b = value;
            break;
        case 3:
            pixel.a = value;
            break;
    }

    outputTex[location.xy] = pixel * factor;
}