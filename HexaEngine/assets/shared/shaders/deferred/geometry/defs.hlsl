
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
#define VtxTangent 0
#endif
#ifndef VtxBitangent
#define VtxBitangent 0
#endif
#ifndef VtxSkinned
#define VtxSkinned 0
#endif

#ifndef Tessellation
#define Tessellation 0
#endif

#ifndef TessellationFactor
#define TessellationFactor 1
#endif

#ifndef MaxBones
#define MaxBones 100
#endif

#ifndef MaxBoneInfluence
#define MaxBoneInfluence 4
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
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct HullInput
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
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
    float TessFactor : TESS;
};

struct DomainInput
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
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct PixelInput
{
#if VtxColor
    float4 color : COLOR;
#endif
#if VtxPosition
    float4 position : SV_POSITION;
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

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};
