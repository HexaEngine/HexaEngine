#ifndef VtxColor
#define VtxColor 0
#endif
#ifndef VtxPosition
#define VtxPosition 1
#endif
#ifndef VtxUV
#define VtxUV 0
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
#ifndef VtxSkinned
#define VtxSkinned 0
#endif

struct VertexInput
{
#if VtxColor
    float4 color : COLOR;
#endif
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
    
#if VtxSkinned
    uint4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct GeometryInput
{
#if VtxPosition
    float3 pos : POSITION;
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
    float4 position : SV_POSITION;
    float4 color : COLOR;
};