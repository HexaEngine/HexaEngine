#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer options: register(b0)
{
    bool Normal;
    bool Tangent;
    bool Bitangent;
    float size;
};

[maxvertexcount(2 * 3 * 3)]
void main(triangle GeometryInput input[3], inout LineStream<PixelInput> triStream)
{
    PixelInput output = (PixelInput) 0;
    
    for (uint i = 0; i < 3; i++)
    {
       
        float4 pos0 = float4(input[i].pos, 1);
        float3 n = input[i].normal;
        float4 pos1 = float4(pos0.xyz + n * size, 1);
        
        if (Normal)
        {
            output.color = float4(0, 0, 1, 1);
            output.position = mul(pos0, viewProj);
            triStream.Append(output);
        
            output.position = mul(pos1, viewProj);
            triStream.Append(output);
        
            triStream.RestartStrip();
        }
        
#if VtxTangent

        
        float3 t = input[i].tangent;
        float4 pos2 = float4(pos0.xyz + t * size, 1);
        
        if (Tangent)
        {
            output.color = float4(1, 0, 0, 1);
            output.position = mul(pos0, viewProj);
            triStream.Append(output);
        
            output.position = mul(pos2, viewProj);
            triStream.Append(output);
        
            triStream.RestartStrip();
        }
        
#if VtxBitangent
        
       
        float3 b = input[i].bitangent;
        float4 pos3 = float4(pos0.xyz + b * size, 1);
        
        if (Bitangent)
        {
            output.color = float4(0, 1, 0, 1);
            output.position = mul(pos0, viewProj);
            triStream.Append(output);
        
            output.position = mul(pos3, viewProj);
            triStream.Append(output);
        
            triStream.RestartStrip();
        }
        
#else
        
        output.color = float4(0, 1, 0, 1);
        float3 b = cross(n, t);
        float4 pos3 = float4(pos0.xyz + b * size, 1);
        if (Bitangent)
        {
            output.color = float4(0, 1, 0, 1);
            output.position = mul(pos0, viewProj);
            triStream.Append(output);
        
            output.position = mul(pos3, viewProj);
            triStream.Append(output);
        
            triStream.RestartStrip();
        }
        
#endif
#endif  
    }
}