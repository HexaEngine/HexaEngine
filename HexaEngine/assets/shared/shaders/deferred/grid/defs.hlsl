struct VertexInput
{
	float3 pos : POSITION;
	float4 color : COLOR;
#if (INSTANCED == 1)
	float4 instance : INSTANCED_MATS0;
	float4 instance1 : INSTANCED_MATS1;
	float4 instance2 : INSTANCED_MATS2;
	float4 instance3 : INSTANCED_MATS3;
#endif
};

struct PixelInput
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
};