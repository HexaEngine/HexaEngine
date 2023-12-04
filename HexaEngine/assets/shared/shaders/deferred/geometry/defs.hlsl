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

#ifndef BATCH_DRAW
#define BATCH_DRAW 0
#endif

struct VertexInput
{
#if BATCH_DRAW
    uint batchIndex : BATCH_ID;
#endif
    float3 position : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;

#if VtxSkinned
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct HullInput
{
    float3 position : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
#if VtxSkinned
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
    float TessFactor : TESS;
};

struct DomainInput
{
    float3 position : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
#if VtxSkinned
    int4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct PixelInput
{
#if BATCH_DRAW
    uint batchIndex : BATCH_ID;
#endif
    float4 position : SV_POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};