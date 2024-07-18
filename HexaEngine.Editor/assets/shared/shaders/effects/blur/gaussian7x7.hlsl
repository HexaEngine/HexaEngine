#define SAMPLE_COUNT 8

static const float OFFSETS[8] =
{
	-6.328357272092126,
	-4.378621204796657,
	-2.431625915613778,
	-0.4862426846689484,
	1.4588111840004858,
	3.4048471718931532,
	5.353083811756559,
	7
};

static const float WEIGHTS[8] =
{
	0.027508406306604068,
	0.08940648616079577,
	0.18921490087565024,
	0.26088633929947086,
	0.2343989200518563,
	0.13722534949218246,
	0.052327012559001844,
	0.009032585254438357
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