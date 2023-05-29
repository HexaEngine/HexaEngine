//--------------------------------------------------------------------------------------
// File: SimpleBezier11.hlsl
//
// This sample shows an simple implementation of the DirectX 11 Hardware Tessellator
// for rendering a Bezier Patch.
//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License (MIT).
//--------------------------------------------------------------------------------------

// This allows us to compile the shader with a #define to choose
// the different partition modes for the hull shader.
// See the hull shader: [partitioning(BEZIER_HS_PARTITION)]
// This sample demonstrates "integer", "fractional_even", and "fractional_odd"
#ifndef BEZIER_HS_PARTITION
#define BEZIER_HS_PARTITION "integer"
#endif // BEZIER_HS_PARTITION

// The input patch size.  In this sample, it is 16 control points.
// This value should match the call to IASetPrimitiveTopology()
#define INPUT_PATCH_SIZE 3

// The output patch size.  In this sample, it is also 16 control points.
#define OUTPUT_PATCH_SIZE 3

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
#define VtxNormal 0
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

//--------------------------------------------------------------------------------------
// Vertex shader section
//--------------------------------------------------------------------------------------
struct VS_CONTROL_POINT_INPUT
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

struct VS_CONTROL_POINT_OUTPUT
{
    float3 vPosition        : POSITION;
};

//--------------------------------------------------------------------------------------
// Constant data function for the BezierHS.  This is executed once per patch.
//--------------------------------------------------------------------------------------
struct HS_CONSTANT_DATA_OUTPUT
{
    float Edges[3] : SV_TessFactor;
    float Inside : SV_InsideTessFactor;
};

struct HS_OUTPUT
{
    float3 vPosition : BEZIERPOS;
};

//--------------------------------------------------------------------------------------
// Bezier evaluation domain shader section
//--------------------------------------------------------------------------------------
struct DS_OUTPUT
{
    float4 vPosition : SV_POSITION;
};