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
	float _padd;

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
}

#endif