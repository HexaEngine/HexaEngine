struct VertexInput
{
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};

struct GeometryInput
{
    float3 pos : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    uint rtvIndex : SV_RenderTargetArrayIndex;
};