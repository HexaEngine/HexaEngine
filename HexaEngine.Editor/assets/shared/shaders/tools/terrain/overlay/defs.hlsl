struct VertexInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    float2 ctex : TEXCOORD1;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};