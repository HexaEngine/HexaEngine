#include "defs.hlsl"

cbuffer MatrixBuffer
{
	matrix model;
	matrix view;
	matrix proj;
};

PixelInput main(VertexInput input)
{
	PixelInput output;

	// Calculate the position of the vertex against the world, view, and projection matrices.
    output.position = mul(float4(input.position, 1), model);
	output.position = mul(output.position, view);
	output.position = mul(output.position, proj);

	// Store the texture coordinates for the pixel shader.
	output.color = input.color;

	return output;
}