#include "defs.hlsl"
#include "../../camera.hlsl"

#ifndef HasHeightTex
#define HasHeightTex 0
#endif

Texture2D heightTexture : register(t0);
SamplerState heightTextureSampler : register(s0);

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
	PixelInput output;

	// Interpolate patch attributes to generated vertices.
    output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);
	output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
	output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
	output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;
    output.bitangent = bary.x * tri[0].bitangent + bary.y * tri[1].bitangent + bary.z * tri[2].bitangent;
	

	output.position = mul(output.position, viewProj);
	output.tex = output.tex;
    output.normal = normalize(output.normal);
	output.tangent = normalize(output.tangent);
    output.bitangent = normalize(output.bitangent);


	return output;
}