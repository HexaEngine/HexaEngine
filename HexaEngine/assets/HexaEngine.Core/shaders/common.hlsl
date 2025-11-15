#ifndef COMMON_H_INCLUDED
#define COMMON_H_INCLUDED

#include "gbuffer.hlsl"
#include "camera.hlsl"
#include "math.hlsl"
inline bool GetBit(int value, int bit)
{
	int mask = 1 << bit;
	value &= mask;
	return value != 0;
}

bool IsSaturated(float value)
{
	return value == saturate(value);
}
bool IsSaturated(float2 value)
{
	return IsSaturated(value.x) && IsSaturated(value.y);
}
bool IsSaturated(float3 value)
{
	return IsSaturated(value.x) && IsSaturated(value.y) && IsSaturated(value.z);
}
bool IsSaturated(float4 value)
{
	return IsSaturated(value.x) && IsSaturated(value.y) && IsSaturated(value.z) && IsSaturated(value.w);
}

float2 ProjectUV(float3 uv, float4x4 proj)
{
	float4 uv_projected = mul(float4(uv, 1.0), proj);
	uv_projected.xy /= uv_projected.w;
	return uv_projected.xy * float2(0.5f, -0.5f) + 0.5f;
}

float ScreenFade(float2 uv)
{
	float2 fade = max(12 * abs(uv - 0.5) - 5, 0);
	return saturate(1 - dot(fade, fade));
}

#endif