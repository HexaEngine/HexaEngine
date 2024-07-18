#include "../../commonShadows.hlsl"

cbuffer FilterParams
{
    float4 region;
    float2 texel;
    float2 size;
};

RWTexture2D shadowMap;

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
float4 blur(RWTexture2D sourceTexture, float2 blurDirection, float2 pixelCoord)
{
    float4 result = 0.0;
    for (int i = 0; i < SAMPLE_COUNT; ++i)
    {
        float2 offset = blurDirection * OFFSETS[i];
        float weight = WEIGHTS[i];
        result += sourceTexture[(pixelCoord + offset)] * weight;
    }
    return result;
}

groupshared float4 samples[];

[numthreads(1, 1, SAMPLE_COUNT)]
void main(uint3 threadId : SV_DispatchThreadID)
{
    float2 coords = float2(threadId.x, threadId.y) * texel;

    coords = NormalizeShadowAtlasUV(coords, region) * size;

    uint z = threadId.z;

    {
        float2 offset = float2(1, 0) * OFFSETS[z];
        float weight = WEIGHTS[z];
        samples[z] = shadowMap[(coords + offset)] * weight;

        GroupMemoryBarrierWithGroupSync();

        if (z == 0)
        {
            float4 result = 0.0;
            for (int i = 0; i < SAMPLE_COUNT; ++i)
            {
                result += samples[i];
            }
            shadowMap[int2(coords.x, coords.y)] = result;
        }
    }

    AllMemoryBarrierWithGroupSync();

    {
        float2 offset = float2(0, 1) * OFFSETS[z];
        float weight = WEIGHTS[z];
        samples[z] = shadowMap[(coords + offset)] * weight;

        GroupMemoryBarrierWithGroupSync();

        if (z == 0)
        {
            float4 result = 0.0;
            for (int i = 0; i < SAMPLE_COUNT; ++i)
            {
                result += samples[i];
            }
            shadowMap[int2(coords.x, coords.y)] = result;
        }
    }

    AllMemoryBarrierWithGroupSync();
}