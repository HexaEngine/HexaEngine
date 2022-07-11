////////////////////////////////////////////////////////////////////////////////
// Filename: deferred.ps
////////////////////////////////////////////////////////////////////////////////

//////////////
// TEXTURES //
//////////////
TextureCube SkyMap : register(t0);

///////////////////
// SAMPLE STATES //
///////////////////
SamplerState SkyMapSampler : register(s0);

//////////////
// TYPEDEFS //
//////////////
struct PixelInputType
{
	float4 position : SV_POSITION;
	float3 tex : TEXCOORD0;
};

float3 Tonemap_ACES(const float3 x)
{
    // Narkowicz 2015, "ACES Filmic Tone Mapping Curve"
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return (x * (a * x + b)) / (x * (c * x + d) + e);
}

float3 OECF_sRGBFast(float3 color)
{
    float gamma = 2.2;
    return pow(abs(color.rgb), float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}


////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 main(PixelInputType input) : SV_Target
{
    float4 color = SkyMap.Sample(SkyMapSampler, input.tex);
    color.rgb = Tonemap_ACES(color.rgb);
    color.rgb = OECF_sRGBFast(color.rgb);
    return color;
}