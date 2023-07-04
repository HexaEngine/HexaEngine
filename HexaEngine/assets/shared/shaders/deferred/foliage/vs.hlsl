#include "../../camera.hlsl"
#include "../../weather.hlsl"

cbuffer WorldBuffer
{
    float4x4 model;
};

struct VS_INPUT
{
    float3 Pos : POSITION;
    float2 Uvs : TEX;
    float3 Normal : NORMAL;
    float3 Offset : INSTANCE_OFFSET;
    float RotationY : INSTANCE_ROTATION;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEX;
    float3 Normal : NORMAL;
};

float4x4 RotationAroundYAxis(float angle)
{
    return
    float4x4(cos(angle), 0.0f, -sin(angle), 0.0f,
             0.0f, 1.0f, 0.0f, 0.0f,
             sin(angle), 0.0f, cos(angle), 0.0f,
             0.0f, 0.0f, 0.0f, 1.0f);
}

VS_OUTPUT main(VS_INPUT vin)
{
    VS_OUTPUT vout;

    matrix model_matrix = mul(RotationAroundYAxis(vin.RotationY), model);
    float3 position = vin.Pos;
    position += vin.Offset;
    model_matrix[3].xyz += vin.Offset;
    
    if (vin.Pos.y > 0.75f)
    {
        float xWindOffset = (2 * sin(time + vin.Pos.x + vin.Pos.y + vin.Pos.z) + 1) * wind_speed * wind_dir.x * 0.005f;
        float zWindOffset = (1 * sin(2 * (time + vin.Pos.x + vin.Pos.y + vin.Pos.z)) + 0.5) * wind_speed * wind_dir.z * 0.005f;
        model_matrix[3].xyz += float3(xWindOffset, 0, zWindOffset);
    }
    
    matrix model_view = mul(model_matrix, view);
    float4 ViewPosition = mul(float4(vin.Pos, 1.0), model_view);

    vout.Position = mul(ViewPosition, proj);
    vout.TexCoord = vin.Uvs;
    
    // Transform the normal to world space
    float3 normal_ws = float3(0.0, 1, 0.0); //mul(vin.Normal, (float3x3) transposed_inverse_model);
    vout.Normal = mul(normal_ws, (float3x3)viewInv);
    return vout;
}