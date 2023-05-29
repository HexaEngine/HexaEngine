#include "defs.hlsl"
#include "../../camera.hlsl"

[maxvertexcount(2 * 3 * 3)]
void main(triangle GeometryInput input[3], inout LineStream<PixelInput> triStream)
{
    PixelInput output = (PixelInput) 0;

    for (uint i = 0; i < 3; i++)
    {
        output.color = float4(0, 0, 1, 1);
        float4 pos0 = float4(input[i].pos, 1);
        float3 n = input[i].normal;
        float4 pos1 = float4(pos0.xyz + n * 0.1f, 1);
        
        output.position = mul(mul(pos0, view), proj);
        triStream.Append(output);
        
        output.position = mul(mul(pos1, view), proj);
        triStream.Append(output);
        
        triStream.RestartStrip();
        
#if VtxTangent

        output.color = float4(1, 0, 0, 1);
        float3 t = input[i].tangent;
        float4 pos2 = float4(pos0.xyz + t * 0.1f, 1);
        
        output.position = mul(mul(pos0, view), proj);
        triStream.Append(output);
        
        output.position = mul(mul(pos2, view), proj);
        triStream.Append(output);
        
        triStream.RestartStrip();
        
#if VtxBitangent
        
        output.color = float4(0, 1, 0, 1);
        float3 b = input[i].bitangent;
        float4 pos3 = float4(pos0.xyz + b * 0.1f, 1);
        
        output.position = mul(mul(pos0, view), proj);
        triStream.Append(output);
        
        output.position = mul(mul(pos3, view), proj);
        triStream.Append(output);
        
        triStream.RestartStrip();
        
#else
        
        output.color = float4(0, 1, 0, 1);
        float3 b = cross(n, t);
        float4 pos3 = float4(pos0.xyz + b * 0.1f, 1);
        
        output.position = mul(mul(pos0, view), proj);
        triStream.Append(output);
        
        output.position = mul(mul(pos3, view), proj);
        triStream.Append(output);
        
        triStream.RestartStrip();
        
#endif
#endif  
    }
}