struct VertexInput
{
	float3 pos : POSITION;
	float2 tex : TEXTURE0;
	float2 ctex : TEXTURE1;
};

struct HullInput
{
	float3 pos : POSITION;
	float2 tex : TEXTURE0;
	float2 ctex : TEXTURE1;
	float TessFactor : TESS;
};

struct DomainInput
{
	float3 pos : POSITION;
	float2 tex : TEXTURE0;
	float2 ctex : TEXTURE1;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PixelInput
{
	float4 spos : SV_POSITION;
	float4 pos : POSITION;
	float2 tex : TEXTURE0;
	float2 ctex : TEXTURE1;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};