struct VertexInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct GeometryInput
{
    float3 pos : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    uint rtvIndex : SV_RenderTargetArrayIndex;
    float depth : DEPTH;
};