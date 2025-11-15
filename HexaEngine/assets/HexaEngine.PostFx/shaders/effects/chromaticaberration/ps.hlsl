Texture2D hdrTexture : register(t0);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float ChromaticAberrationIntensity;
};

float4 main(VSOut vs) : SV_Target
{
	float2 redOffset = float2(0.005, 0.0) * ChromaticAberrationIntensity;
	float2 greenOffset = float2(0.0, 0.0) * ChromaticAberrationIntensity;
	float2 blueOffset = float2(0.0, -0.005) * ChromaticAberrationIntensity;

	float4 redSample = hdrTexture.Sample(linearClampSampler, vs.Tex + redOffset);
	float4 greenSample = hdrTexture.Sample(linearClampSampler, vs.Tex + greenOffset);
	float4 blueSample = hdrTexture.Sample(linearClampSampler, vs.Tex + blueOffset);

	return float4(redSample.r, greenSample.g, blueSample.b, 1.0);
}