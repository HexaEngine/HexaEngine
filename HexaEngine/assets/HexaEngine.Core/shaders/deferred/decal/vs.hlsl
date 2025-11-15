#include "../../camera.hlsl"

cbuffer WorldBuffer
{
    float4x4 model;
    float4x4 modelInv;
};

struct VS_INPUT
{
    float3 Pos : POSITION;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float4 ClipSpacePos : POSITION;
    float4x4 InverseModel : INVERSE_MODEL;
};

VS_OUTPUT main(VS_INPUT vin)
{
    VS_OUTPUT vout = (VS_OUTPUT) 0;
    float4 world_pos = mul(float4(vin.Pos, 1.0f), model);
    vout.Position = mul(world_pos, viewProj);
    vout.ClipSpacePos = vout.Position;
    vout.InverseModel = modelInv;
    return vout;
}