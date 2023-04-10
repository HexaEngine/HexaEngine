
struct VertexInput
{
	float3 pos : POSITION;
    float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PixelInput
{
	float4 position : SV_POSITION;
    float4 pos : POSITION;
	float3 normal : NORMAL;
};

struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};
