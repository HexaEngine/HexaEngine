#include "../../world.hlsl"
#include "../../camera.hlsl"

#define UpVector float3(0, 0.91739964, 0.39796725)

struct VertexInputType
{
    float3 pos : POSITION;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    output.position = mul(float4(input.pos, 1), world);
    output.position = mul(output.position, viewProj);

    output.tex = normalize(input.pos.xyz);

        // Rotate the texture coordinates around the custom up vector
    float cosRot = cos(0.1f * cumulativeTime); // Assuming _Time.y is the time value passed from your application
    float sinRot = sin(0.1f * cumulativeTime);

    float3 rotatedTexCoord = float3(
        output.tex.x * (cosRot + (1 - cosRot) * UpVector.x * UpVector.x) +
        output.tex.y * ((1 - cosRot) * UpVector.x * UpVector.y - sinRot * UpVector.z) +
        output.tex.z * ((1 - cosRot) * UpVector.x * UpVector.z + sinRot * UpVector.y),

        output.tex.x * ((1 - cosRot) * UpVector.y * UpVector.x + sinRot * UpVector.z) +
        output.tex.y * (cosRot + (1 - cosRot) * UpVector.y * UpVector.y) +
        output.tex.z * ((1 - cosRot) * UpVector.y * UpVector.z - sinRot * UpVector.x),

        output.tex.x * ((1 - cosRot) * UpVector.z * UpVector.x - sinRot * UpVector.y) +
        output.tex.y * ((1 - cosRot) * UpVector.z * UpVector.y + sinRot * UpVector.x) +
        output.tex.z * (cosRot + (1 - cosRot) * UpVector.z * UpVector.z)
    );

    // Output rotated texture coordinates
    output.tex = rotatedTexCoord;

    output.pos = input.pos.xyz;
    return output;
}