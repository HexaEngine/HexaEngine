#include "../../camera.hlsl"

Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);
Texture2D positionTexture : register(t2);
SamplerState state;

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer Params
{
    float BloomStrength;
    float FogEnabled;
    float FogStart;
    float FogEnd;
    float3 FogColor;
    float Padding;
};

float3 ACESFilm(float3 x)
{
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 Uncharted2Tonemap(float3 x)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;
    float W = 11.2;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 OECF_sRGBFast(float3 color)
{
    float gamma = 2.2;
    return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float3 BloomMix(float2 texCoord, float3 hdr)
{
    float3 blm = bloomTexture.Sample(state, texCoord).rgb;
    float3 drt = float3(0, 0, 0);
    return lerp(hdr, blm + blm * drt, float3(BloomStrength, BloomStrength, BloomStrength));
}

float ComputeFogFactor(float d)
{
    //d is the distance to the geometry sampling from the camera
    //this simply returns a value that interpolates from 0 to 1 
    //with 0 starting at FogStart and 1 at FogEnd 
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}

float3 FogMix(float2 texCoord, float3 color)
{
    float3 position = positionTexture.Sample(state, texCoord).xyz;
    float d = distance(position, GetCameraPos());
    float factor = ComputeFogFactor(d);
    return lerp(color, FogColor, factor);
}

float4 main(VSOut vs) : SV_Target
{
    float4 color = hdrTexture.Sample(state, vs.Tex);

    color.rgb = BloomMix(vs.Tex, color.rgb);
    color.rgb = FogMix(vs.Tex, color.rgb);
    color.rgb = ACESFilm(color.rgb);
    color.rgb = OECF_sRGBFast(color.rgb);
#if FXAA == 1
    color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));
#endif

    return color;
}