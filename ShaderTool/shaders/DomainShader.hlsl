//////////////////////
// CONSTANT BUFFERS //
//////////////////////

cbuffer MatrixBuffer : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
};

struct PatchTess
{
	float edges[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct HullOutputType
{
	float3 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct DomainOut
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

// The domain shader is called for every vertex created by the tessellator.
// It is like the vertex shader after tessellation.
[domain("tri")]
DomainOut main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<HullOutputType, 3> tri)
{
	DomainOut output;

	// Interpolate patch attributes to generated vertices.
	output.position = float4(bary.x * tri[0].pos + bary.y * tri[1].pos + bary.z * tri[2].pos, 1);
	output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
	output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;

	//if (material.HasDisplacementMap) {
		//const float MipInterval = 20.0f;
		//float mipLevel = clamp((distance(output.position, gEyePosW) - MipInterval) / MipInterval, 0.0f, 6.0f);

		//float h = displacmentTexture.SampleLevel(SampleTypeWrap, output.tex, mipLevel).a;
	//}

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(output.position, worldMatrix);
	output.pos = output.position;
	output.position = mul(output.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);

	// Store the texture coordinates for the pixel shader.
	output.tex = output.tex;

	// Calculate the normal vector against the world matrix only.
	output.normal = mul(output.normal, (float3x3)worldMatrix);
	output.normal = normalize(output.normal);

	return output;
}