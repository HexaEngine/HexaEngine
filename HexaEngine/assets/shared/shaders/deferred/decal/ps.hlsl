#include "../../camera.hlsl"

Texture2D<float4> baseColorTex : register(t0);
Texture2D<float4> normalTex : register(t1);
Texture2D<float> depthTex : register(t2);

SamplerState linearWrapsampler : register(s0);
SamplerState pointClampsampler : register(s1);

struct PS_DECAL_OUT
{
    float4 GBufferA : SV_TARGET0;
#ifdef DECAL_MODIFY_NORMALS
    float4 GBufferB   : SV_TARGET1;
#endif
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 ClipSpacePos : POSITION;
    float4x4 InverseModel : INVERSE_MODEL;
};

#define DECAL_XY 0
#define DECAL_YZ 1
#define DECAL_XZ 2


cbuffer DecalBuffer : register(b0)
{
    int decalType;
}

PS_DECAL_OUT main(PS_INPUT input)
{
    PS_DECAL_OUT pout = (PS_DECAL_OUT) 0;

    float2 screen_pos = input.ClipSpacePos.xy / input.ClipSpacePos.w;
    float2 depth_coords = screen_pos * float2(0.5f, -0.5f) + 0.5f;
    float depth = depthTex.Sample(pointClampsampler, depth_coords).r;

    float4 posVS = float4(GetPositionVS(depth_coords, depth), 1.0f);
    float4 posWS = mul(posVS, viewInv);
    float4 posLS = mul(posWS, input.InverseModel);
    posLS.xyz /= posLS.w;

    clip(0.5f - abs(posLS.xyz));

    float2 tex_coords = 0.0f;
    switch (decalType)
    {
        case DECAL_XY:
            tex_coords = posLS.xy + 0.5f;
            break;
        case DECAL_YZ:
            tex_coords = posLS.yz + 0.5f;
            break;
        case DECAL_XZ:
            tex_coords = posLS.xz + 0.5f;
            break;
        default:
            pout.GBufferA.rgb = float3(1, 0, 0);
            return pout;
    }

    float4 albedo = baseColorTex.SampleLevel(linearWrapsampler, tex_coords, 0);
    if (albedo.a < 0.1)
        discard;
    pout.GBufferA.rgb = albedo.rgb;

#ifdef DECAL_MODIFY_NORMALS
    posWS /= posWS.w;
    float3 ddx_ws = ddx(posWS.xyz);
    float3 ddy_ws = ddy(posWS.xyz);

    float3 normal   = normalize(cross(ddx_ws, ddy_ws));
    float3 binormal = normalize(ddx_ws);
    float3 tangent  = normalize(ddy_ws);

    float3x3 TBN = float3x3(tangent, binormal, normal);

    float3 DecalNormal = normalTex.Sample(linearWrapsampler, tex_coords);
    DecalNormal = 2.0f * DecalNormal - 1.0f;
    DecalNormal = mul(DecalNormal, TBN);
    float3 DecalNormalVS = normalize(mul(DecalNormal, (float3x3)view));
    pout.GBufferB.rgb = 0.5 * DecalNormalVS + 0.5;
#endif

    return pout;
}