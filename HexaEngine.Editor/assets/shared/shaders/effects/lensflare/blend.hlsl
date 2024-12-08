cbuffer DownsampleParams : register(b0)
{
    float lensDirtIntensity;
    float starburstIntensity;
    float3x3 lensStarburstMatrix;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D inputTexture : register(t0);
Texture2D lensFlareTex : register(t1);
#ifdef LENS_DIRT_TEXTURE
Texture2D lensDirtTex : register(t2);
#endif
#ifdef LENS_STARBURST_TEXTURE
Texture2D lensStarburstTex : register(t2);
#endif

SamplerState linearClampSampler : register(s0);

float4 main(VSOut pin) : SV_Target
{

    float4 lensFlare = lensFlareTex.SampleLevel(linearClampSampler, pin.Tex, 0);

#ifdef LENS_DIRT_TEXTURE
    float4 lensDirt = lensDirtTex.SampleLevel(linearClampSampler, pin.Tex, 0) * lensDirtIntensity;

#ifdef LENS_STARBURST_TEXTURE
    float2 lensStarTexcoord = mul(lensStarburstMatrix, float3(pin.Tex, 1.0)).xy;
    lensDirt += lensStarburstTex.SampleLevel(linearClampSampler, pin.Tex, 0) * starburstIntensity;
#endif

    lensFlare *= lensDirt;
#endif

    float4 color = inputTexture.SampleLevel(linearClampSampler, pin.Tex, 0);

    return color + lensFlare;
}