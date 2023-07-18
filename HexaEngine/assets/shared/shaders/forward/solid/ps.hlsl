#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer overlay
{
    bool ShowWeights;
    int WeightMask;
    float2 pad;
};

float4 main(PixelInput input, uint primitiveId : SV_PrimitiveID) : SV_Target
{
#if VtxColor
    float3 baseColor = input.color.rgb;
#else
    float3 baseColor = float3(1, 1, 1);
#endif
    
    float amient = 0.2;

#if VtxNormal
    float3 L = normalize(GetCameraPos() - input.pos.xyz);
    float3 N = input.normal;
    float diffuse = dot(L, N);
    
    
    float3 shadeColor = baseColor * diffuse + amient;
#else
    float3 shadeColor = baseColor * 1 + amient;
#endif
	
    if (ShowWeights)
    {
#if VtxSkinned
        return float4(input.weightColor, 1);
#else
        return float4(shadeColor, 1);
#endif
    }
    else
    {
        return float4(shadeColor, 1);
    }
}