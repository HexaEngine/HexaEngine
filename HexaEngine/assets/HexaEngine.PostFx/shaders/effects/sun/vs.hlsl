#include "HexaEngine.Core:shaders/camera.hlsl"

cbuffer WorldBuffer
{
    float4x4 model;
};

struct VertexInputType
{
    float3 Pos : POSITION;
    float2 Uvs : TEX;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEX;
};


PixelInputType main(VertexInputType vin)
{
    PixelInputType vout;

    float4x4 model_matrix = model;

    model_matrix[3][0] += viewInv[3][0];
    model_matrix[3][1] += viewInv[3][1];
    model_matrix[3][2] += viewInv[3][2];

    float4x4 model_view = mul(model_matrix, view);

    model_view[0][0] = 1;
    model_view[0][1] = 0;
    model_view[0][2] = 0;
    
    model_view[1][0] = 0;
    model_view[1][1] = 1;
    model_view[1][2] = 0;
    
    model_view[2][0] = 0;
    model_view[2][1] = 0;
    model_view[2][2] = 1;
    
    
    float4 ViewPosition = mul(float4(vin.Pos, 1.0), model_view);

    float4 PosH = mul(ViewPosition, proj); // + float4(vin.Pos.xy * dist, 0, 0)  float dist = ViewPosition.z;     

    vout.Position = float4(PosH.xy, PosH.w - 0.001f, PosH.w); //depth leq state
    vout.TexCoord = vin.Uvs;
    return vout;
}