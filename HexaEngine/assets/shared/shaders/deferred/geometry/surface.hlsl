#include "../../material.hlsl"
#include "defs.hlsl"

#ifndef DEPTH_TEST_ONLY

[earlydepthstencil]
GeometryData main(PixelInput input)
{
	float3 normal = normalize(input.normal);
  	float3 tangent = normalize(input.tangent);
  	float3 bitangent = cross(normal, tangent);

  	Pixel geometry;
  	geometry.pos = input.position;
  	geometry.uv = input.tex;
  	geometry.normal = normal;
  	geometry.tangent = tangent;
  	geometry.binormal = bitangent;

	Material material = setupMaterial(geometry);

	if (material.baseColor.a < 0.5f)
		discard;

	int matID = 1;
	return PackGeometryData(matID, 
		material.baseColor.rgb, 
		material.normal, 
		material.roughness, 
		material.metallic, 
		material.reflectance, 
		material.ao, 
		1, 
		material.emissive.rgb, 
		material.emissive.a);
}

#else

void main(PixelInput input)
{
	float4 baseColor = BaseColor;

#if HasBaseColorTex
	baseColor = baseColorTexture.Sample(baseColorTextureSampler, (float2)input.tex);
#endif

	if (baseColor.a < 0.5f)
		discard;
}

#endif