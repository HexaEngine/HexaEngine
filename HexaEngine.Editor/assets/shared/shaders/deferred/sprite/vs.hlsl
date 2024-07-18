#include "../../camera.hlsl"

cbuffer constants : register(b0)
{
    float2 ScreenSize;
    float2 AtlasSize;
    float4x4 world;
}

struct Sprite
{
    int2 screenpos;
    int2 size;
    int2 atlaspos;
    uint layer;
};

StructuredBuffer<Sprite> spritebuffer : register(t0);

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 TexCoord : TEXCOORD;
};

PixelInputType main(uint spriteId : SV_INSTANCEID, uint vertexId : SV_VERTEXID)
{
    Sprite spr = spritebuffer[spriteId];

    float4 pos = float4(spr.screenpos, spr.screenpos + spr.size);
    float4 tex = float4(spr.atlaspos, spr.atlaspos + spr.size);

    uint2 i = { vertexId & 2, (vertexId << 1 & 2) ^ 3 };

    PixelInputType output;

    output.Position = float4(float2(pos[i.x], pos[i.y]) / ScreenSize * float2(1, -1), 0, 1);
    output.Position = mul(output.Position, world);
    output.Position = mul(output.Position, viewProj);
    output.Normal = mul(float3(0, 0, -1), (float3x3) world);
    output.TexCoord = float3(float2(tex[i.x], tex[i.y]) / AtlasSize, spr.layer);

    return output;
}