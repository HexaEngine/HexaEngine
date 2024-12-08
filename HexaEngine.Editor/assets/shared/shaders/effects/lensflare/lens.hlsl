cbuffer DownsampleParams : register(b0)
{
    uint uGhosts;
    float uGhostDispersal;
    float2 textureSize;
    float uHaloWidth;
    float uDistortion;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D inputTexture : register(t0);
#ifdef LENS_COLOR_TEX
Texture1D lensColorTexture : register(t1);
#endif
SamplerState linearClampSampler : register(s0);

#define LENS_CHROMATIC_DISTORTION

float4 textureDistorted(
      in Texture2D tex,
      in SamplerState samplerState,
      in float2 texcoord,
      in float2 direction, // direction of distortion
      in float3 distortion // per-channel distortion factor
   )
{
    return float4(
         tex.SampleLevel(samplerState, texcoord + direction * distortion.r, 0).r,
         tex.SampleLevel(samplerState, texcoord + direction * distortion.g, 0).g,
         tex.SampleLevel(samplerState, texcoord + direction * distortion.b, 0).b,
         1
      );
}

float4 main(VSOut pin) : SV_Target
{

    float2 texcoord = -pin.Tex + 1.0;
    float2 texelSize = 1.0 / textureSize;

    // ghost vector to image centre:
    float2 ghostVec = (0.5 - texcoord) * uGhostDispersal;

    float3 distortion = float3(-texelSize.x * uDistortion, 0.0, texelSize.x * uDistortion);

    float2 direction = normalize(ghostVec);

    // sample ghosts:
    float4 result = 0.0;
    for (uint i = 0; i < uGhosts; ++i)
    {
        float2 offset = frac(texcoord + ghostVec * float(i));

        float weight = length(0.5.xx - offset) / length(0.5.xx);
        weight = pow(1.0 - weight, 10.0);
#ifdef LENS_CHROMATIC_DISTORTION
        result += textureDistorted(inputTexture, linearClampSampler, offset, direction, distortion) * weight;
#else
        result += inputTexture.SampleLevel(linearClampSampler, offset, 0) * weight;
#endif
    }

#ifdef LENS_COLOR_TEX
    result *= lensColorTexture.SampleLevel(linearClampSampler, length(0.5.xx - texcoord) / length((0.5.xx)), 0);
#endif

    {
        // sample halo:
        float2 haloVec = normalize(ghostVec) * uHaloWidth;
        float weight = length(float2(0.5, 0.5) - frac(texcoord + haloVec)) / length(float2(0.5, 0.5));
        weight = pow(1.0 - weight, 5.0);
#ifdef LENS_CHROMATIC_DISTORTION
        result += textureDistorted(inputTexture, linearClampSampler, texcoord + haloVec, direction, distortion) * weight;
#else
        result += inputTexture.SampleLevel(linearClampSampler, texcoord + haloVec, 0) * weight;
#endif
    }

    return result;
}