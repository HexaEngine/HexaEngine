static const int SAMPLE_COUNT = 6;

static const float OFFSETS[6] =
{
    -4.378621204796657,
    -2.431625915613778,
    -0.4862426846689485,
    1.4588111840004858,
    3.4048471718931532,
    5
};

static const float WEIGHTS[6] =
{
    0.09461172151436463,
    0.20023097066826712,
    0.2760751120037518,
    0.24804559825032563,
    0.14521459357563646,
    0.035822003987654526
};

// blurDirection is:
//     vec2(1,0) for horizontal pass
//     vec2(0,1) for vertical pass
// The sourceTexture to be blurred MUST use linear filtering!
// pixelCoord is in [0..1]
float4 blur(Texture2D sourceTexture, SamplerState state, float2 blurDirection, float2 pixelCoord, float2 textureDimensions)
{
    float4 result = 0.0;
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float2 offset = blurDirection * OFFSETS[i] / textureDimensions;
        float weight = WEIGHTS[i];
        result += sourceTexture.Sample(state, pixelCoord + offset) * weight;
    }
    return result;
}