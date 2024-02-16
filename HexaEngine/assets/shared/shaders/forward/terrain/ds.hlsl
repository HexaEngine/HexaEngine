#include "defs.hlsl"
#include "../../world.hlsl"
#include "../../camera.hlsl"

Texture2D heightTexture : register(t0);
SamplerState heightTextureSampler : register(s0);

float3 ReconstructNormal(float2 tex)
{
    float2 size;
    heightTexture.GetDimensions(size.x, size.y);
    float2 texel = 1 / size;

    float4 h;
    h.x = heightTexture.SampleLevel(heightTextureSampler, tex + float2(1, 0) * texel, 0).r;
    h.y = heightTexture.SampleLevel(heightTextureSampler, tex + float2(-1, 0) * texel, 0).r;
    h.z = heightTexture.SampleLevel(heightTextureSampler, tex + float2(0, 1) * texel, 0).r;
    h.w = heightTexture.SampleLevel(heightTextureSampler, tex + float2(0, -1) * texel, 0).r;

    float3 n;
    n.z = h.w - h.x;
    n.x = h.z - h.y;
    n.y = 2;

    return normalize(n);
}

[domain("tri")]
PixelInput main(PatchTess patchTess, float3 bary : SV_DomainLocation, const OutputPatch<DomainInput, 3> tri)
{
    PixelInput output;

	// Interpolate patch attributes to generated vertices.
    output.position = float4(bary.x * tri[0].position + bary.y * tri[1].position + bary.z * tri[2].position, 1);
    output.tex = bary.x * tri[0].tex + bary.y * tri[1].tex + bary.z * tri[2].tex;

    output.normal = ReconstructNormal(output.tex.xy);

    const float3 rightVec = float3(1.0, 0.0, 0.0);
    float3 N = output.normal;
    float3 T = cross(rightVec, N);
    float3 B = cross(T, N);

    output.position.y += heightTexture.SampleLevel(heightTextureSampler, output.tex.xy, 0).r;

    output.position = mul(output.position, viewProj);

    output.normal = mul(output.normal, (float3x4) world);
    output.tangent = mul(T, (float3x4) world);

    return output;
}