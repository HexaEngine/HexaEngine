#include "../../camera.hlsl"
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

	Pixel geometry;

#if VtxTangents
    float3 tangent = normalize(input.tangent);
	float3 binormal = normalize(input.binormal);

	float3 VN = input.tangentViewPos - input.tangentPos;
	float3 V = normalize(VN);
	geometry.viewDir = V;

#else
    float3 tangent = 0;
	float3 binormal = 0;

	geometry.viewDir = 0;
#endif

  	
#if VtxColors
    geometry.color = input.color;
#else 
    geometry.color = 0;
#endif
  	geometry.pos = input.pos; // input.pos is in world space.

#if VtxUVs
    geometry.uv = input.tex;
#else 
    geometry.uv = 0;
#endif 	
  	geometry.normal = normal;
  	geometry.tangent = tangent;
  	geometry.binormal = binormal;

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