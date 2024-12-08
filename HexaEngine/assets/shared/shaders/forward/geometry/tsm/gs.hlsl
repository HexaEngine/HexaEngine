#include "defs.hlsl"

static const float3 planeNormals[12] =
{
  float3(0.00000000, -0.03477280, 0.99939519),
  float3(-0.47510946, -0.70667917, 0.52428567),
  float3(0.47510946, -0.70667917, 0.52428567),
  float3(0.00000000, -0.03477280, -0.99939519),
  float3(0.47510946, -0.70667917, -0.52428567),
  float3(-0.47510946, -0.70667917, -0.52428567),
  float3(-0.52428567, 0.70667917, -0.47510946),
  float3(-0.52428567, 0.70667917, 0.47510946),
  float3(-0.99939519, 0.03477280, 0.00000000),
  float3(0.52428567, 0.70667917, -0.47510946),
  float3(0.99939519, 0.03477280, 0.00000000),
  float3(0.52428567, 0.70667917, 0.47510946)
};

float GetClipDistance(in float3 lightPosition, in float3 pos, in uint planeIndex)
{
	float3 normal = planeNormals[planeIndex];
	return (dot(pos.xyz, normal) + dot(-normal, lightPosition));
}

cbuffer lightBuffer : register(b1)
{
	float4x4 views[4];
	float lightNear;
	float lightFar;
	float2 padd;
	float3 lightPosition;
};

[maxvertexcount(12)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
	// back-face culling
	float3 normal = cross(input[2].pos.xyz - input[0].pos.xyz, input[1].pos.xyz - input[0].pos.xyz);
	float3 view = lightPosition - input[0].pos.xyz;
	//if (dot(normal, view) < 0.0f)
	//	return;

	PixelInput output;
	[unroll(4)]
		for (uint faceIndex = 0; faceIndex < 4; faceIndex++)
		{
			uint inside = 0;
			float clipDistances[9];
			[unroll(3)]
				for (uint sideIndex = 0; sideIndex < 3; sideIndex++)
				{
					const uint planeIndex = (faceIndex * 3) + sideIndex;
					const uint bit = 1 << sideIndex;
					[unroll(3)]
						for (uint vertexIndex = 0; vertexIndex < 3; vertexIndex++)
						{
							uint clipDistanceIndex = sideIndex * 3 + vertexIndex;
							clipDistances[clipDistanceIndex] = GetClipDistance(lightPosition, input[vertexIndex].pos, planeIndex);
							inside |= (clipDistances[clipDistanceIndex] > 0.00001) ? bit : 0;
						}
				}

			if (inside == 0x7)
			{
				[unroll(3)]
					for (uint vertexIndex = 0; vertexIndex < 3; vertexIndex++)
					{
						output.position = mul(float4(input[vertexIndex].pos, 1), views[faceIndex]);
						output.clipDistance[0] = clipDistances[vertexIndex];
						output.clipDistance[1] = clipDistances[3 + vertexIndex];
						output.clipDistance[2] = clipDistances[6 + vertexIndex];
						output.depth = length(input[vertexIndex].pos - lightPosition) / lightFar;
						triStream.Append(output);
					}
				triStream.RestartStrip();
			}
		}
}