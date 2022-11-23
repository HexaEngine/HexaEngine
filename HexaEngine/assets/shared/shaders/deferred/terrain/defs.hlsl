struct VertexInput
{
	float3 pos : POSITION;
	float2 tex : TEXTURE0;
	float2 ctex : TEXTURE1;
#if (INSTANCED == 1)
	float4 instance : INSTANCED_MATS0;
	float4 instance1 : INSTANCED_MATS1;
	float4 instance2 : INSTANCED_MATS2;
	float4 instance3 : INSTANCED_MATS3;
#endif
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