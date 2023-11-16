cbuffer ModelBuffer
{
    float4x4 model;
};

struct VS_INPUT
{
    float3 Position : POSITION;
    float3 Uvs : TEX;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Bitangent : BINORMAL;
};

struct VS_OUTPUT
{
    float4 PositionWS : POSITION;
    float2 Uvs : TEX;
    float3 NormalWS : NORMAL0;
};

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT Output;

    float4 pos = mul(float4(input.Position, 1.0), model);
    Output.PositionWS = pos / pos.w;
    Output.Uvs = input.Uvs.xy;
    float3 normal_ws = mul(input.Normal, (float3x3) model);
    Output.NormalWS = normalize(normal_ws);
    return Output;
}