#define SAMPLE_COUNT 4

static const float OFFSETS[4] =
{
    -2.351564403533789,
    -0.46943377969837197,
    1.409199877085212,
    3
};

static const float WEIGHTS[4] =
{
    0.20281755282997538,
    0.4044856614512111,
    0.32139335373196054,
    0.07130343198685299
};

// blurDirection is:
//     float2(1,0) for horizontal pass
//     float2(0,1) for vertical pass
// The sourceTexture to be blurred MUST use linear filtering!
// pixelCoord is in [0..1]
float4 blur(Texture2D sourceTexture, SamplerState state, float2 blurDirection, float2 pixelCoord, float2 textureDimensions)
{
    float4 result = 0.0;
    [unroll(SAMPLE_COUNT)]
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float2 offset = blurDirection * OFFSETS[i] / textureDimensions;
        float weight = WEIGHTS[i];
        result += sourceTexture.Sample(state, pixelCoord + offset) * weight;
    }
    return result;
}