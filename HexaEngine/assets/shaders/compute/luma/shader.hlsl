#define NUM_HISTOGRAM_BINS 256
#define HISTOGRAM_THREADS_PER_DIMENSION 16
#define EPSILON 0.000001f

Texture2D HDRTexture : register(t0);
RWByteAddressBuffer LuminanceHistogram : register(u0);

cbuffer LuminanceHistogramBuffer : register(b0)
{
	uint inputWidth;
	uint inputHeight;
	float minLogLuminance;
	float oneOverLogLuminanceRange;
};

//Next we'll need some shared memory to store intermediate thread-group histogram bin counts. NUM_HISTOGRAM_BINS is, as you might've guessed, 256.

groupshared uint HistogramShared[NUM_HISTOGRAM_BINS];

//Finally, the compute shader itself :

float GetLuminance(float3 color)
{
	return dot(color, float3(0.2125f, 0.7154f, 0.0721f));
}

uint HDRToHistogramBin(float3 hdrColor)
{
	float luminance = GetLuminance(hdrColor);

	if (luminance < EPSILON)
	{
		return 0;
	}

	float logLuminance = clamp((log2(luminance) - minLogLuminance) * oneOverLogLuminanceRange, 0.0, 1.0);
	return (uint)(logLuminance * 254.0 + 1.0);
}

[numthreads(HISTOGRAM_THREADS_PER_DIMENSION, HISTOGRAM_THREADS_PER_DIMENSION, 1)]
void main(uint groupIndex : SV_GroupIndex, uint3 threadId : SV_DispatchThreadID)
{
	HistogramShared[groupIndex] = 0;

	GroupMemoryBarrierWithGroupSync();

	if (threadId.x < inputWidth && threadId.y < inputHeight)
	{
		float3 hdrColor = HDRTexture.Load(int3(threadId.xy, 0)).rgb;
		uint binIndex = HDRToHistogramBin(hdrColor);
		InterlockedAdd(HistogramShared[binIndex], 1);
	}

	GroupMemoryBarrierWithGroupSync();

	LuminanceHistogram.InterlockedAdd(groupIndex * 4, HistogramShared[groupIndex]);
}