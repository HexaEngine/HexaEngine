#include "defs.hlsl"
#include "../world.hlsl"
#include "../camera.hlsl"
#include "../material.hlsl"

Texture2D displacmentTexture : register(t0);
SamplerState displacmentSampler : register(s0);

cbuffer MaterialBuffer : register(b2)
{
    Material material;
}

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
    PixelInput output;

	// Interpolate patch attributes to generated vertices.
    output.position = float4(bary.x * tri[0].pos + bary.y * tri[1].pos + bary.z * tri[2].pos, 1);
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;
    output.normal = bary.x * tri[0].normal + bary.y * tri[1].normal + bary.z * tri[2].normal;
    output.tangent = bary.x * tri[0].tangent + bary.y * tri[1].tangent + bary.z * tri[2].tangent;

	// Calculate the position of the vertex against the world, view, and projection matrices.

    output.position = mul(output.position, world);

    if (material.HasDisplacementMap)
    {
        float h = displacmentTexture.SampleLevel(displacmentSampler, (float2) output.tex, 0).r;
        output.position += float4((h - 1.0) * output.normal, 0);
    }

    output.pos = output.position;
    output.position = mul(output.position, view);
    output.depth = output.position.z / 100;
    output.position = mul(output.position, proj);

	// Store the texture coordinates for the pixel shader.
    output.tex = output.tex;

    output.normal = mul(output.normal, (float3x3)world);
    output.normal = normalize(output.normal);

    output.tangent = mul(output.tangent, (float3x3)world);
    output.tangent = normalize(output.tangent);
    

    return output;
}