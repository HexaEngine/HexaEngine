#include "HexaEngine.Core:shaders/camera.hlsl"
#include "HexaEngine.Core:shaders/gbuffer.hlsl"

Texture2D inputTex;
Texture2D<float> depthTex;
Texture2D normalTex;

SamplerState linearClampSampler;
SamplerState pointClampSampler;

cbuffer DenoiseParams
{
    float sigma_s = 2.0f;
    float sigma_r = 0.1f;
    float sigma_d = 0.5f;
    float sigma_n = 32.0f;
    float radius = 16.0f;
    float3 padding;
};

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEXCOORD;
};

float3 GetViewNormal(float2 uv)
{
    float3 normal = normalTex.Sample(linearClampSampler, uv).rgb;
    return normalize(mul(UnpackNormal(normal), (float3x3) view));
}

float3 bilateralFilter(float2 uv, float radius, float sigma_s, float sigma_r, float sigma_d, float sigma_n)
{
    float3 centerColor = inputTex.Sample(linearClampSampler, uv).rgb;
    float centerDepth = GetLinearDepth(depthTex.Sample(pointClampSampler, uv).r);
    float3 centerNormal = GetViewNormal(uv);
    
    float3 filteredColor = 0.0;
    float weightSum = 0.0;
    
    for (int y = -int(radius); y <= int(radius); ++y)
    {
        for (int x = -int(radius); x <= int(radius); ++x)
        {
            float2 offset = float2(x, y) * screenDimInv;
            float2 sampleUV = uv + offset;
            
            float3 neighborColor = inputTex.Sample(linearClampSampler, sampleUV).rgb;
            float depth = GetLinearDepth(depthTex.Sample(pointClampSampler, sampleUV).r);
            float3 normal = GetViewNormal(sampleUV);
            
            // Spatial weight (Gaussian based on distance)
            float spatialWeight = exp(-(x * x + y * y) / (2.0 * sigma_s * sigma_s));
            
            float spatial = exp(-(x * x + y * y) / (2.0 * sigma_s * sigma_s));
            float depthW = exp(-(depth - centerDepth) * (depth - centerDepth)
                                / (2.0 * sigma_d * sigma_d));
            float normalW = pow(saturate(dot(centerNormal, normal)), sigma_n);

            float w = spatial * depthW * normalW;
            
            filteredColor += neighborColor * w;
            weightSum += w;
        }
    }
    
    return filteredColor / max(weightSum, 1e-6);
}

float4 main(VertexOut input) : SV_TARGET
{
    float3 denoisedColor = bilateralFilter(input.Tex, radius, sigma_s, sigma_r, sigma_d, sigma_n);
    return float4(denoisedColor, 1.0);
}