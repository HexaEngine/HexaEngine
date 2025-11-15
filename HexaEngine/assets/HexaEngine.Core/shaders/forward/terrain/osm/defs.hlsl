#ifndef Tessellation
#define Tessellation 0
#endif

#ifndef TessellationFactor
#define TessellationFactor 1
#endif

struct VertexInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float depth : DEPTH;
    float clip : TEXCOORD;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};