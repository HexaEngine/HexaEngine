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
    return float4(0, 0, 0, 1);
}