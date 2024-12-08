#ifndef WEATHER_H_INCLUDED
#define WEATHER_H_INCLUDED

cbuffer WeatherBuffer : register(b2)
{
	float4 light_dir;
	float4 light_color;
	float4 sky_color;
	float4 ambient_color;
	float4 wind_dir;

	float wind_speed;
	float time;
	float crispiness;
	float curliness;

	float coverage;
	float absorption;
	float clouds_bottom_height;
	float clouds_top_height;

	float density_factor;
	float cloud_type;
	float phaseFunctionG;
	float fog_intensity;

	float3 fog_color;
	float fog_height;

	float fog_start;
	float fog_end;
	int fog_mode;
	float fog_density;

	float3 A;
	float _paddA;
	float3 B;
	float _paddB;
	float3 C;
	float _paddC;
	float3 D;
	float _paddD;
	float3 E;
	float _paddE;
	float3 F;
	float _paddF;
	float3 G;
	float _paddG;
	float3 H;
	float _paddH;
	float3 I;
	float _paddI;
	float3 Z;
	float _paddZ;

	float3 sun_color;
	float sun_radius;

	float sun_intensity;
	float sun_falloff;
	float2 _padd0;
}

inline float3 ComputeSunContribution(float3 viewDir)
{
	float distanceToSun = distance(light_dir.xyz, viewDir);
	float brightness = saturate(1.0f - pow(distanceToSun / sun_radius, sun_falloff));
	float3 L0 = sun_color * brightness * sun_intensity;
	return L0;
}

// Linear Fog Factor
inline float GetLinearFogFactor(float distance)
{
	return saturate((distance - fog_start) / (fog_end - fog_start));
}

// Exponential Fog Factor
inline float GetExpFogFactor(float distance)
{
	return saturate(1.0 - exp(-distance * fog_density));
}

// Exponential Squared Fog Factor
inline float GetExp2FogFactor(float distance)
{
	return saturate(1.0 - exp(-distance * distance * fog_density));
}

// Height-based Fog Factor
inline float GetFogHeightFactor(float height)
{
	return saturate(exp(-height + fog_height));
}

float GetFogDistanceFactor(float distance, int mode)
{
	float fogFactor = 0.0;
	switch (mode)
	{
	case 0:
		fogFactor = GetLinearFogFactor(distance);
		break;

	case 1:
		fogFactor = GetExpFogFactor(distance);
		break;

	case 2:
		fogFactor = GetExp2FogFactor(distance);
		break;

	default:
		return 0;
	}

	return fogFactor;
}

float GetFogFactor(float3 pos, float3 cameraPos)
{
	float3 VN = cameraPos - pos;
	float d = length(VN);

	int mode = fog_mode & 0x03;
	bool useHeightBased = (fog_mode & 0x04) != 0;

	float fogFactor = GetFogDistanceFactor(d, mode);

	if (useHeightBased)
	{
		float heightFactor = GetFogHeightFactor(pos.y);
		fogFactor *= heightFactor;  // No need for saturate since both factors are between [0, 1]
	}

	return fogFactor * fog_intensity;  // Allow intensity to exceed 1 if necessary
}

#endif