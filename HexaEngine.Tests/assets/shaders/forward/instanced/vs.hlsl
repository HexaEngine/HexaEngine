#include "../../world.hlsl"
#include "../../camera.hlsl"
struct VertexInput
{
	float3 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float4 instance : INSTANCED_MATS0;
	float4 instance1 : INSTANCED_MATS1;
	float4 instance2 : INSTANCED_MATS2;
	float4 instance3 : INSTANCED_MATS3;
};

struct HullInput
{
	float3 pos : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float TessFactor : TESS;
};

HullInput main(VertexInput input)
{
	HullInput output;
	float4x4 mat = float4x4(input.instance, input.instance1, input.instance2, input.instance3);
	output.pos = mul(float4(input.pos, 1), mat).xyz;
	output.tex = input.tex.xy;
	output.normal = mul(input.normal, (float3x3)mat);
	output.tangent = input.tangent;
	output.TessFactor = 1;

	return output;
}