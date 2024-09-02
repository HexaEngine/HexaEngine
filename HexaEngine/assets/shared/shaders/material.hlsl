#ifndef MATERIAL_H_INCLUDED
#define MATERIAL_H_INCLUDED

struct Pixel
{
    float3 pos;    
    float3 uv;
    float3 normal;
    float3 tangent;
    float3 binormal;
};

#if defined(TARGET_MOBILE)
    // min roughness such that (MIN_PERCEPTUAL_ROUGHNESS^4) > 0 in fp16 (i.e. 2^(-14/4), rounded up)
#define MIN_PERCEPTUAL_ROUGHNESS 0.089
#define MIN_ROUGHNESS            0.007921
#else
#define MIN_PERCEPTUAL_ROUGHNESS 0.045
#define MIN_ROUGHNESS            0.002025
#endif

struct Material
{
    float3 baseColor;
    float3 normal;
    float roughness;
    float metallic;
    float reflectance;
    float ao;
    float3 emissive;
};

float ComputeDielectricF0(float reflectance)
{
    return 0.16 * reflectance * reflectance;
}

float3 ComputeF0(const float3 baseColor, float metallic, float reflectance)
{
    return baseColor.rgb * metallic + (reflectance * (1.0 - metallic));
}

float3 ComputeDiffuseColor(const float3 baseColor, float metallic)
{
    return baseColor.rgb * (1.0 - metallic);
}

float PerceptualRoughnessToRoughness(float perceptualRoughness)
{
    return perceptualRoughness * perceptualRoughness;
}

float RoughnessToPerceptualRoughness(float roughness)
{
    return sqrt(roughness);
}

#endif