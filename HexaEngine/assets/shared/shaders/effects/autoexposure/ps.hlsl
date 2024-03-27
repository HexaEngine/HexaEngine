Texture2D hdrTexture : register(t0);
Texture2D<float> lumaTexture : register(t1);

SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
	const float sensitivity = 12.5;
	float avgLum = lumaTexture.Load(0);
	float ev100 = log2(avgLum * 100.0 / sensitivity);
	float exposure = 1.0 / (pow(2.0, ev100) * 1.2);

	float aperture = sqrt(100.0 * sensitivity / (avgLum * pow(2.0, ev100)));
	float shutterSpeed = 1.0 / (sensitivity * pow(2.0, ev100));

	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb * exposure;

	return float4(color, 1);
}