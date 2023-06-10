Texture2D cubemap;
SamplerState samplerState;

struct VSOut
{
	float4 Pos : SV_POSITION;
	float3 WorldPos : TEXCOORD0;
};

float2 SampleSphericalMap(float3 v)
{
	float2 uv = float2(atan2(v.z, v.x), asin(v.y));
	uv *= float2(0.1591, 0.3183);
	uv += 0.5;
	uv.y = abs(uv.y - 1);
	return uv;
}

float4 main(VSOut vs) : SV_Target
{
	float2 uv = SampleSphericalMap(normalize(vs.WorldPos));
	float3 color = cubemap.Sample(samplerState, uv).rgb;

	return float4(color, 1.0);
}