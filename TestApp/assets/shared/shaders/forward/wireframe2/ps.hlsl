#include "defs.hlsl"

cbuffer selection
{           
    uint InstanceId;        
    uint TypeId;        
    uint PrimitiveId;
    uint VertexId;
};

float4 main(PixelInput input, uint primitiveId : SV_PrimitiveID) : SV_Target
{
    if (primitiveId == PrimitiveId - 1)
    {
        if (input.vertexId == VertexId - 1)
        {
            return float4(0, 0, 1, 1);
        }
        else
        {
            return float4(0, 1, 0, 1);
        }
    }
    return float4(1, 0, 0, 1);
}