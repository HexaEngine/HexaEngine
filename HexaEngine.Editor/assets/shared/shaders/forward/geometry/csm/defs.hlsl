#ifndef VtxSkinned
#define VtxSkinned 0
#endif

#ifndef MaxBones
#define MaxBones 100
#endif

#ifndef MaxBoneInfluence
#define MaxBoneInfluence 4
#endif

struct VertexInput
{
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;

#if VtxSkinned
    uint4 boneIds : BLENDINDICES;
    float4 weights : BLENDWEIGHT;
#endif
};

struct GeometryInput
{
    float3 pos : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    uint rtvIndex : SV_RenderTargetArrayIndex;
    float depth : DEPTH;
};