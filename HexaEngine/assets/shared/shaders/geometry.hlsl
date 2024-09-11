#ifndef GEOMETRY_H_INCLUDED
#define GEOMETRY_H_INCLUDED

#include "camera.hlsl"
#include "tessellation.hlsl"
#include "gbuffer.hlsl"

struct Pixel
{
	float4 color;
	float3 pos;
	VtxUV0Type uv;
	float3 normal;
	float3 tangent;
	float3 binormal;
};

#ifndef VtxColors
#define VtxColors 0
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
	VtxUV0Type tex : TEXCOORD;
#endif
#if VtxNormals
	float3 normal : NORMAL;
#endif
#if VtxTangents
	float3 tangent : TANGENT;
#endif

#if VtxSkinned
	int4 boneIds : BLENDINDICES;
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
	VtxUV0Type tex : TEXCOORD;
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
	VtxUV0Type tex : TEXCOORD;
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
#endif
#if VtxUVs
	VtxUV0Type tex : TEXCOORD;
#endif
#if VtxNormals
	float3 normal : NORMAL;
#endif
#if VtxTangents
	float3 tangent : TANGENT;
#endif
};

struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};

#endif