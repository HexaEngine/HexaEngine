#include "../../camera.hlsl"

#define SAMPLES 32

Texture2D hdrTexture : register(t0);
Texture2D<float> depthTexture : register(t1);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer FogParams
{
    float FogIntensity;
    float FogStart;
    float FogEnd;
    float FogDensity;

    float3 FogColor;
    float padd1;
};

float3 LinearFog(float3 color, float d)
{
    float factor = clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogIntensity;
    return lerp(color, FogColor, factor);

}

float3 VolumetricFog(float3 hdr, float3 V, float d)
{
    float factor = clamp((d - FogStart) / (FogEnd - FogStart), 0, 1);

    float3 rayStart = GetCameraPos();

    float3 rayPosition = rayStart;
    float accumulatedDensity = 0;
    float stepSize = 0.1;

    [unroll(SAMPLES)]
    for (int i = 0; i < SAMPLES; ++i)
    {
        float currentDistance = distance(rayPosition, rayStart);
        if (currentDistance > d)
            break;

        float currentDensity = FogDensity * currentDistance;

        accumulatedDensity += currentDensity;

        rayPosition = rayStart + V * stepSize * i;
    }

    float3 fogColor = FogColor * FogIntensity * factor;
    float3 color = lerp(fogColor, hdr.rgb, exp(-accumulatedDensity));

    return color;
}

float4 main(VSOut vs) : SV_Target
{
    float3 hdr = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;

    float depth = depthTexture.Sample(linearClampSampler, vs.Tex);
    float3 position = GetPositionWS(vs.Tex, depth);
    float3 VN = GetCameraPos() - position;
    float3 V = normalize(VN);
    float d = length(VN);

    if (d < FogStart)
        discard;

    float3 color = VolumetricFog(hdr, V, d);

    return float4(color, 1);
}