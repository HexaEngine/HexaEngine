static const int SAMPLE_COUNT = 6;

static const float OFFSETS[6] =
{
    -4.48876136715687,
    -2.493755926496706,
    -0.49874988611319804,
    1.4962530470023068,
    3.4912585165653676,
    5
};

static const float WEIGHTS[6] =
{
    0.17240384242538973,
    0.1848610194629113,
    0.190472258826138,
    0.1885828129221358,
    0.17941572863649413,
    0.08426433772693104
};

// blurDirection is:
//     float2(1,0) for horizontal pass
//     float2(0,1) for vertical pass
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