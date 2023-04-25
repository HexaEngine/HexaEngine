#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer selection
{
    uint InstanceId;
    uint TypeId;
    uint PrimitiveId;
    uint VertexId;
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
    
    
    float3 color = baseColor * diffuse + amient;
#else
    float3 color = baseColor * 1 + amient;
#endif

    if (primitiveId == PrimitiveId - 1)
    {
        float cmp = abs(input.vertexId - (VertexId - 1));
        if (cmp < 1)
        {
            return lerp(float4(0, 0, 1, 1), float4(1, 1, 1, 1), cmp);
        }
    }
	
    return float4(color, 1);
}