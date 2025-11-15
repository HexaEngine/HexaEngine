#ifndef VtxColors
#define VtxColors 0
#endif

#ifndef VtxPos
#define VtxPos 0
#endif

#ifndef VtxUVs
#define VtxUVs 0
#endif

#ifndef VtxNormals
#define VtxNormals 0
#endif

#ifndef VtxTangents
#define VtxTangents 0
#endif

#ifndef VtxSkinned
#define VtxSkinned 0
#endif

// Ensure that VtxPos is defined, if not, throw a compilation error
#ifndef VtxPos
    #error "Vertex position (VtxPos) is required but not defined!"
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

#ifndef HasBakedLightMap
#define HasBakedLightMap 0
#endif

#ifndef BAKE_PASS
#define BAKE_PASS 0
#endif

struct VertexInput
{
#if VtxColors
    float4 color : COLOR;
#endif
#if VtxPos
    float3 position : POSITION;
#endif
#if VtxUVs
    float3 tex : TEXCOORD;
#endif
#if VtxNormals
    float3 normal : NORMAL;
#endif
#if VtxTangents
    float3 tangent : TANGENT;
#endif

#if VtxSkinned
    uint4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct HullInput
{
#if VtxColors
    float4 color : COLOR;
#endif
#if VtxPos
    float3 position : POSITION;
#endif
#if VtxUVs
    float3 tex : TEXCOORD;
#endif
#if VtxNormals
    float3 normal : NORMAL;
#endif
#if VtxTangents
    float3 tangent : TANGENT;
#endif
    float TessFactor : TESS;
};

struct DomainInput
{
#if VtxColors
    float4 color : COLOR;
#endif
#if VtxPos
    float3 position : POSITION;
#endif
#if VtxUVs
    float3 tex : TEXCOORD;
#endif
#if VtxNormals
    float3 normal : NORMAL;
#endif
#if VtxTangents
    float3 tangent : TANGENT;
#endif
};

struct PixelInput
{
#if VtxColors
    float4 color : COLOR;
#endif
#if VtxPos
    float4 position : SV_POSITION;
    float3 pos : POSITION;
#endif
#if VtxUVs
    float3 tex : TEXCOORD;
#endif
#if VtxNormals
    float3 normal : NORMAL;
#endif
#if VtxTangents
    float3 tangent : TANGENT;
#endif

#if HasBakedLightMap || BAKE_PASS
    float3 H0 : H0;
    float3 H1 : H1;
    float3 H2 : H2;
    float3 H3 : H3;
#endif
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};