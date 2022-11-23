struct VertexInput
{
	float3 position : POSITION;
	float4 color : TEXCOORD0;
};

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 color : TEXCOORD0;
};