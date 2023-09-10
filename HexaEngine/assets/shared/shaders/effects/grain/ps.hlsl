Texture2D hdrTexture : register(t0);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float3 FilmGrainColor;
	float Time;
	float FilmGrainIntensity;
	float3 padd0;
};

float rand2(float2 n)
{
	return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
}

float4 main(VSOut vs) : SV_Target
{
	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;
	float grain = rand2(vs.Tex + Time) * 2.0 - 1.0;
	color.rgb -= grain * FilmGrainIntensity * FilmGrainColor;
	return float4(color, 1);
}