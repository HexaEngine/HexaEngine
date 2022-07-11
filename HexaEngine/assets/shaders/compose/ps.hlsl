Texture2D lightTexture : register(t0);

SamplerState SampleTypePoint : register(s0);

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
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

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 main(PixelInputType pixel) : SV_TARGET
{
    float4 color = lightTexture.Sample(SampleTypePoint, pixel.tex);
    color = saturate(color);
    color.rgb = ACESFilm(color.rgb);
    color.rgb = OECF_sRGBFast(color.rgb);
	return color;

}