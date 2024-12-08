#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer SSAOParams
{
    float TAP_SIZE = 0.0002;
    uint NUM_TAPS = 16;
    float POWER;
    float2 NoiseScale;
};

#define BIAS 0.01f

Texture2D<float> depthTex : register(t0);
Texture2D normalTex : register(t1);
Texture2D metallicTex : register(t2);

SamplerState samplerState;

float4 main(VSOut vs) : SV_Target
{

}