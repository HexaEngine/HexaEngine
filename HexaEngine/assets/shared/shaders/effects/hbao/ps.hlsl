#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"
struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D<float> depthTex : register(t0);
Texture2D normalTex : register(t1);
Texture2D noiseTex : register(t2);

SamplerState samplerState;

cbuffer ConfigBuffer : register(b0)
{
    float SAMPLING_RADIUS;
    float SAMPLING_RADIUS_TO_SCREEN;
    uint NUM_SAMPLING_DIRECTIONS;
    float SAMPLING_STEP;
    uint NUM_SAMPLING_STEPS;
    float POWER;
    float2 NoiseScale;
};

#define NdotVBias 0.1f
#define PI 3.1415926535897932384626433832795

//----------------------------------------------------------------------------------
float Falloff(float DistanceSquare)
{
  // 1 scalar mad instruction
    return DistanceSquare * (-1.0f / SAMPLING_RADIUS * SAMPLING_RADIUS) + 1.0;
}

//----------------------------------------------------------------------------------
// P = view-space position at the kernel center
// N = view-space normal at the kernel center
// S = view-space position of the current sample
//----------------------------------------------------------------------------------
float ComputeAO(float3 P, float3 N, float3 S)
{
    float3 V = S - P;
    float VdotV = dot(V, V);
    float NdotV = dot(N, V) * 1.0 / sqrt(VdotV);

  // Use saturate(x) instead of max(x,0.f) because that is faster on Kepler
    return clamp(NdotV - NdotVBias, 0, 1) * clamp(Falloff(VdotV), 0, 1);
}

//----------------------------------------------------------------------------------
float2 RotateDirection(float2 Dir, float2 CosSin)
{
    return float2(Dir.x * CosSin.x - Dir.y * CosSin.y,
              Dir.x * CosSin.y + Dir.y * CosSin.x);
}

//----------------------------------------------------------------------------------
float ComputeCoarseAO(float2 UV, float radius_in_pixels, float3 rand, float3 pos_vs, float3 normal_vs)
{
  // Divide by NUM_STEPS+1 so that the farthest samples are not fully attenuated
    float step_size_in_pixels = radius_in_pixels / (NUM_SAMPLING_STEPS + 1);

    const float theta = 2.0 * PI / NUM_SAMPLING_DIRECTIONS;
    float AO = 0;

    for (float direction_index = 0; direction_index < NUM_SAMPLING_DIRECTIONS; ++direction_index)
    {
        float angle = theta * direction_index;

        // Compute normalized 2D direction
        float2 direction = RotateDirection(float2(cos(angle), sin(angle)), rand.xy);

        // Jitter starting sample within the first step
        float ray_t = (rand.z * step_size_in_pixels + 1.0);

        for (float step_index = 0; step_index < NUM_SAMPLING_STEPS; ++step_index)
        {
            float2 SnappedUV = round(ray_t * direction) / screenDim + UV;
           
            float depth = depthTex.Sample(samplerState, SnappedUV);
            float3 S = GetPositionVS(SnappedUV, depth);
            
            ray_t += step_size_in_pixels;

            AO += ComputeAO(pos_vs, normal_vs, S);
        }
    }

    AO *= (1.0f / (1.0f - NdotVBias)) / (NUM_SAMPLING_DIRECTIONS * NUM_SAMPLING_STEPS);
    return clamp(1.0 - AO * 2.0, 0, 1);
}

float4 main(VSOut input) : SV_Target
{
    float depth = depthTex.Sample(samplerState, input.Tex);
    
    float3 pos_vs = GetPositionVS(input.Tex, depth);

    float3 normal_vs = mul(UnpackNormal(normalTex.Sample(samplerState, input.Tex).rgb), (float3x3) view);
   
    // Compute projection of disk of radius control.R into screen space
    float radius_in_pixels = SAMPLING_RADIUS_TO_SCREEN / pos_vs.z;
    
    float3 rand = noiseTex.Sample(samplerState, input.Tex * NoiseScale).xyz; //use wrap sampler

    float AO = ComputeCoarseAO(input.Tex, radius_in_pixels, rand, pos_vs, normal_vs);
    
    return pow(AO, POWER);
}