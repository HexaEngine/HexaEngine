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
#if VtxPosition
    float3 pos : POSITION;
#endif
#if VtxUV
    float3 tex : TEXCOORD;
#endif
#if VtxNormal
    float3 normal : NORMAL;
#endif
#if VtxTangent
    float3 tangent : TANGENT;
#endif
#if VtxBitangent
    float3 bitangent : BINORMAL;
#endif
};

struct PixelInput
{
#if VtxPosition
    float4 position : SV_POSITION;
    float3 pos : POSITION;
#endif
#if VtxUV
    float3 tex : TEXCOORD0;
    float2 ctex : TEXCOORD1;
#endif
#if VtxNormal
    float3 normal : NORMAL;
#endif
#if VtxTangent
    float3 tangent : TANGENT;
#endif
#if VtxBitangent
    float3 bitangent : BINORMAL;
#endif
};