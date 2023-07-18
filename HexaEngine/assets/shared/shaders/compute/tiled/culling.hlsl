#include "common.hlsl"
#include "../../light.hlsl"

#ifndef BLOCK_SIZE
#pragma message( "BLOCK_SIZE undefined. Default to 16.")
#define BLOCK_SIZE 16 // should be defined by the application.
#endif

struct ComputeShaderInput
{
    uint3 groupID : SV_GroupID; // 3D index of the thread group in the dispatch.
    uint3 groupThreadID : SV_GroupThreadID; // 3D index of local thread ID in a thread group.
    uint3 dispatchThreadID : SV_DispatchThreadID; // 3D index of global thread ID in the dispatch.
    uint groupIndex : SV_GroupIndex; // Flattened local index of the thread within a thread group.
};

// Global variables
cbuffer DispatchParams : register(b0)
{
    // Number of groups dispatched. (This parameter is not available as an HLSL system value!)
    uint3 numThreadGroups;
    // uint padding // implicit padding to 16 bytes.
    // Total number of threads dispatched. (Also not available as an HLSL system value!)
    // Note: This value may be less than the actual number of threads executed
    // if the screen size is not evenly divisible by the block size.
    uint3 numThreads;
    // uint padding // implicit padding to 16 bytes.
}

cbuffer LightParams : register(b2)
{
    uint LightCount;
}

StructuredBuffer<Light> Lights : register(t0);
SamplerState LinearClampSampler : register(s0);

// The depth from the screen space texture.
Texture2D DepthTextureVS : register(t1);
// Precomputed frustums for the grid.
StructuredBuffer<Frustum> in_Frustums : register(t2);

// Debug texture for debugging purposes.
Texture2D LightCountHeatMap : register(t3);
RWTexture2D<float4> DebugTexture : register(u0);

// Global counter for current index into the light index list.
// "o_" prefix indicates light lists for opaque geometry while
// "t_" prefix indicates light lists for transparent geometry.
globallycoherent RWStructuredBuffer<uint> o_LightIndexCounter : register(u1);
globallycoherent RWStructuredBuffer<uint> t_LightIndexCounter : register(u2);

// Light index lists and light grids.
RWStructuredBuffer<uint> o_LightIndexList : register(u3);
RWStructuredBuffer<uint> t_LightIndexList : register(u4);
RWTexture2D<uint2> o_LightGrid : register(u5);
RWTexture2D<uint2> t_LightGrid : register(u6);

// Group shared variables.
groupshared uint uMinDepth;
groupshared uint uMaxDepth;
groupshared Frustum GroupFrustum;

// Opaque geometry light lists.
groupshared uint o_LightCount;
groupshared uint o_LightIndexStartOffset;
groupshared uint o_LightList[1024];

// Transparent geometry light lists.
groupshared uint t_LightCount;
groupshared uint t_LightIndexStartOffset;
groupshared uint t_LightList[1024];

// Add the light to the visible light list for opaque geometry.
void o_AppendLight(uint lightIndex)
{
    uint index; // Index into the visible lights array.
    InterlockedAdd(o_LightCount, 1, index);
    if (index < 1024)
    {
        o_LightList[index] = lightIndex;
    }
}

// Add the light to the visible light list for transparent geometry.
void t_AppendLight(uint lightIndex)
{
    uint index; // Index into the visible lights array.
    InterlockedAdd(t_LightCount, 1, index);
    if (index < 1024)
    {
        t_LightList[index] = lightIndex;
    }
}

// Implementation of light culling compute shader is based on the presentation
// "DirectX 11 Rendering in Battlefield 3" (2011) by Johan Andersson, DICE.
// Retrieved from: http://www.slideshare.net/DICEStudio/directx-11-rendering-in-battlefield-3
// Retrieved: July 13, 2015
// And "Forward+: A Step Toward Film-Style Shading in Real Time", Takahiro Harada (2012)
// published in "GPU Pro 4", Chapter 5 (2013) Taylor & Francis Group, LLC.
[numthreads(BLOCK_SIZE, BLOCK_SIZE, 1)]
void main(ComputeShaderInput IN)
{
    // Calculate min & max depth in threadgroup / tile.
    int2 texCoord = IN.dispatchThreadID.xy;
    float fDepth = DepthTextureVS.Load(int3(texCoord, 0)).r;

    uint uDepth = asuint(fDepth);

    if (IN.groupIndex == 0) // Avoid contention by other threads in the group.
    {
        uMinDepth = 0xffffffff;
        uMaxDepth = 0;
        o_LightCount = 0;
        t_LightCount = 0;
        GroupFrustum = in_Frustums[IN.groupID.x + (IN.groupID.y * numThreadGroups.x)];
    }

    GroupMemoryBarrierWithGroupSync();

    InterlockedMin(uMinDepth, uDepth);
    InterlockedMax(uMaxDepth, uDepth);

    GroupMemoryBarrierWithGroupSync();

    float fMinDepth = asfloat(uMinDepth);
    float fMaxDepth = asfloat(uMaxDepth);

    // Convert depth values to view space.
    float minDepthVS = ScreenToView(float4(0, 0, fMinDepth, 1)).z;
    float maxDepthVS = ScreenToView(float4(0, 0, fMaxDepth, 1)).z;
    float nearClipVS = ScreenToView(float4(0, 0, 0, 1)).z;

    // Clipping plane for minimum depth value
    // (used for testing lights within the bounds of opaque geometry).
    Plane minPlane = { float3(0, 0, -1), -minDepthVS };

    // Cull lights
    // Each thread in a group will cull 1 light until all lights have been culled.
    for (uint i = IN.groupIndex; i < LightCount; i += BLOCK_SIZE * BLOCK_SIZE)
    {

        Light light = Lights[i];

        switch (light.type)
        {
            case POINT_LIGHT:
                {
                    Sphere sphere = { light.position.xyz, light.range };
                    if (SphereInsideFrustum(sphere, GroupFrustum, nearClipVS, maxDepthVS))
                    {
                        // Add light to light list for transparent geometry.
                        t_AppendLight(i);

                        if (!SphereInsidePlane(sphere, minPlane))
                        {
                            // Add light to light list for opaque geometry.
                            o_AppendLight(i);
                        }
                    }
                }
                break;
            case SPOT_LIGHT:
                {
                    float coneRadius = tan(light.cutOff) * light.range;
                    Cone cone = { light.position.xyz, light.range, light.dir.xyz, coneRadius };
                    if (ConeInsideFrustum(cone, GroupFrustum, nearClipVS, maxDepthVS))
                    {
                        // Add light to light list for transparent geometry.
                        t_AppendLight(i);

                        if (!ConeInsidePlane(cone, minPlane))
                        {
                            // Add light to light list for opaque geometry.
                            o_AppendLight(i);
                        }
                    }
                }
                break;
            case DIRECTIONAL_LIGHT:
                {
                    // Directional lights always get added to our light list.
                    // (Hopefully there are not too many directional lights!)
                    t_AppendLight(i);
                    o_AppendLight(i);
                }
                break;
        }

    }

    // Wait till all threads in group have caught up.
    GroupMemoryBarrierWithGroupSync();

    // Update global memory with visible light buffer.
    // First update the light grid (only thread 0 in group needs to do this)
    if (IN.groupIndex == 0)
    {
        // Update light grid for opaque geometry.
        InterlockedAdd(o_LightIndexCounter[0], o_LightCount, o_LightIndexStartOffset);
        o_LightGrid[IN.groupID.xy] = uint2(o_LightIndexStartOffset, o_LightCount);

        // Update light grid for transparent geometry.
        InterlockedAdd(t_LightIndexCounter[0], t_LightCount, t_LightIndexStartOffset);
        t_LightGrid[IN.groupID.xy] = uint2(t_LightIndexStartOffset, t_LightCount);
    }

    GroupMemoryBarrierWithGroupSync();

    // Now update the light index list (all threads).
    // For opaque goemetry.
    for (i = IN.groupIndex; i < o_LightCount; i += BLOCK_SIZE * BLOCK_SIZE)
    {
        o_LightIndexList[o_LightIndexStartOffset + i] = o_LightList[i];
    }
    // For transparent geometry.
    for (i = IN.groupIndex; i < t_LightCount; i += BLOCK_SIZE * BLOCK_SIZE)
    {
        t_LightIndexList[t_LightIndexStartOffset + i] = t_LightList[i];
    }

    // Update the debug texture output.
    if (o_LightCount > 0)
    {
        float normalizedLightCount = o_LightCount + t_LightCount / LightCount;
        float4 lightCountHeatMapColor = LightCountHeatMap.SampleLevel(LinearClampSampler, float2(normalizedLightCount, 0), 0);
        DebugTexture[texCoord] = lightCountHeatMapColor;
    }
    else
    {
        DebugTexture[texCoord] = float4(0, 0, 0, 1);
    }
}