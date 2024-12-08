#ifndef WORLD_H_INCLUDED
#define WORLD_H_INCLUDED

cbuffer WorldBuffer : register(b0)
{
    float4x4 world;
    float4x4 worldInv;
};

#endif