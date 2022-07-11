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
};

struct DomainInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PatchTess
{
	float edges[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};