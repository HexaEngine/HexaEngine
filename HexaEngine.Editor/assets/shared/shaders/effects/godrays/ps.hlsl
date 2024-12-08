#ifndef VOLUMETRIC_SCATTERING_QUALITY
#define VOLUMETRIC_SCATTERING_QUALITY     1   //0 - low, 1 - medium, 2 - high, 3 - extreme
#endif

#if (VOLUMETRIC_SCATTERING_QUALITY == 3)
#define SAMPLES    128
#elif (VOLUMETRIC_SCATTERING_QUALITY == 2)
#define SAMPLES    64
#elif (VOLUMETRIC_SCATTERING_QUALITY == 1)
#define SAMPLES    32
#elif (VOLUMETRIC_SCATTERING_QUALITY == 0)
#define SAMPLES    16
#endif

cbuffer GodrayParams
{
    float4 sunPos;
    float4 rayColor;
    float density;
    float weight;
    float decay;
    float exposure;
};

SamplerState linearClampSampler : register(s0);

Texture2D<float> sunTex : register(t0);
Texture2D<float> noiseTex : register(t1);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

static const float gr = (1.0 + sqrt(5.0)) * 0.5;

float4 main(VSOut pin) : SV_TARGET
{
    float2 uv = pin.Tex;
    float jitter = noiseTex.SampleLevel(linearClampSampler, uv.xy, 0);

    float2 lightPos = sunPos.xy;

    float2 deltaUV = (uv - lightPos);
    deltaUV *= density / SAMPLES;

    float illuminationDecay = 1.0f;
    float3 accumulated = 0.0f;

    for (int i = 0; i < SAMPLES; i++)
    {
        jitter = frac(jitter + gr * i);
        uv.xy -= deltaUV;
        float sam = sunTex.SampleLevel(linearClampSampler, uv.xy + deltaUV * jitter, 0);
        sam *= illuminationDecay * weight;
        accumulated += sam;
        illuminationDecay *= decay;
    }

    float3 color = rayColor.rgb * accumulated * exposure;

    return float4(color, 1.0f);
}