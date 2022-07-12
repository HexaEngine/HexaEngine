Texture2D lightTexture : register(t0);

SamplerState SampleTypePoint : register(s0);

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
};



////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 main(PixelInputType pixel) : SV_TARGET
{
    float4 color = lightTexture.Sample(SampleTypePoint, pixel.tex);
	return color;

}