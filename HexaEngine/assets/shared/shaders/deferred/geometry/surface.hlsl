#include "../../material.hlsl"
#include "defs.hlsl"

#ifndef DEPTH_TEST_ONLY

[earlydepthstencil]
GeometryData main(PixelInput input)
{
#if VtxNormals
    float3 normal = normalize(input.normal);
#else 
    float3 normal = 0;
#endif

#if VtxTangents
    float3 tangent = normalize(input.tangent);
#else
    float3 tangent = 0;
#endif
  	float3 bitangent = cross(normal, tangent);

  	Pixel geometry;
#if VtxColors
    geometry.color = input.color;
#else 
    geometry.color = 0;
#endif
  	geometry.pos = input.position;
#if VtxUVs
    geometry.uv = input.tex;
#else 
    geometry.uv = 0;
#endif 	
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