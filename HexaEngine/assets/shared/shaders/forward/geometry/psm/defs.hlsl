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

#ifndef MaxBones
#define MaxBones 100
#endif

#ifndef MaxBoneInfluence
#define MaxBoneInfluence 4
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

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
};

struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};