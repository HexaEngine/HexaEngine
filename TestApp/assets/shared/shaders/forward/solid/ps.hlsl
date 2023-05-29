#include "defs.hlsl"
#include "../../camera.hlsl"

float4 ComputeLighting(float3 pos, float3 normal)
{
    float3 baseColor = float3(1, 1, 1);
    float3 L = normalize(GetCameraPos() - pos);
    float3 N = normal;
    float diffuse = dot(L, N);
    float amient = 0.2;
	
    float3 color = baseColor * diffuse + amient;
	
    return float4(color, 1);
}

float4 main(PixelInput input) : SV_Target
{
    return ComputeLighting(input.pos.xyz, normalize(input.normal));
}