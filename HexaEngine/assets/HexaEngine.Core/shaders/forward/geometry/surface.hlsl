#include "defs.hlsl"
#include "../../commonShading.hlsl"

struct PixelOutput
{
    float4 Color : SV_Target0;
    float4 Normal : SV_Target1;
};

[earlydepthstencil]
PixelOutput main(PixelInput input)
{
    float3 position = input.pos;
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

    normal = material.normal;

    float opacity = material.baseColor.a;

    if (opacity < 0.1f)
        discard;

    float3 N = normalize(normal);
    float3 V = normalize(GetCameraPos() - position);

    PixelParams pixel = ComputeSurfaceProps(position, V, N, material.baseColor.rgb, material.roughness, material.metallic, material.reflectance);

    float2 screenUV = GetScreenUV(input.position);

    float3 direct = ComputeDirectLightning(input.position.z / input.position.w, pixel);
    float3 ambient = ComputeIndirectLightning(screenUV, pixel, material.ao, material.emissive.rgb * material.emissive.a);

    PixelOutput output;
    output.Color = float4(ambient + direct, opacity);
    output.Normal = float4(PackNormal(N), opacity);

    return output;
}