#include "defs.hlsl"
#include "../../world.hlsl"
#include "../../material.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

cbuffer MaterialBuffer : register(b2)
{
    Material material;
}

[domain("tri")]
GeometryInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
    GeometryInput output;

	// Interpolate patch attributes to generated vertices.
    output.position = float4(bary.x * tri[0].pos + bary.y * tri[1].pos + bary.z * tri[2].pos, 1);
    float3 normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
    float2 tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;

    
    // Calculate the normal vector against the world matrix only.
    normal = mul(normal, (float3x3) world);
    normal = normalize(normal);


	// Calculate the position of the vertex against the world, view, and projection matrices.
    output.position = mul(output.position, world);

    if (material.DANR.r)
    {
        float h = displacmentTexture.SampleLevel(displacmentSampler, (float2)tex, 0).r;
        output.position += float4((h - 1.0) * (normal * 0.05f), 0); 
    }

    return output;
}