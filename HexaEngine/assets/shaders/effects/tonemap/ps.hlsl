Texture2D input;
SamplerState state;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float3 ACESFilm(float3 x)
{
	return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
	float gamma = 2.2;
	return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float4 main(VSOut vs) : SV_Target
{
	float4 color = input.Sample(state, vs.Tex);
	color.rgb = ACESFilm(color.rgb);
	color.rgb = OECF_sRGBFast(color.rgb);
	color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));
	return color;
}