#ifndef VtxPosition
#define VtxPosition 1
#endif
#ifndef VtxUV
#define VtxUV 1
#endif
#ifndef VtxNormal
#define VtxNormal 1
#endif
#ifndef VtxTangent
#define VtxTangent 1
#endif
#ifndef VtxBitangent
#define VtxBitangent 1
#endif

struct VertexInput
{
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};

struct PixelInput
{
    float4 pos : SV_POSITION;
    float3 tex : TEXCOORD0;
    float2 ctex : TEXCOORD1;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};