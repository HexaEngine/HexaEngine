struct VertexInputType
{
	float3 position : POSITION;
	float4 color : COLOR;
};
struct GeometryInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};
cbuffer MVPBuffer
{
	matrix view;
	matrix proj;
	matrix world;
};
GeometryInput main(VertexInputType input)
{
	GeometryInput output;
	output.position = mul(float4(input.position, 1), world);
	output.position = mul(output.position, view);
	output.position = mul(output.position, proj);
	output.color = input.color;
	return output;
}