
struct VertexInput
{
    float3 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct HullInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
    float TessFactor : TESS;
};

struct DomainInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct GeometryInput
{
    float4 position : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 shadowCoord : TEXCOORD0;
    uint rtIndex : SV_RenderTargetArrayIndex;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};