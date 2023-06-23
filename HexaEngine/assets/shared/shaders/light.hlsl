struct GlobalProbe
{
    float exposure;
    float horizonCutOff;
    float3 orientation;
};

struct DirectionalLightSD
{
    float4x4 views[16];
    float4 cascades[4];
    float4 color;
    float3 dir;
};

struct DirectionalLight
{
    float4 color;
    float3 dir;
};

struct PointLightSD
{
    float4 color;
    float3 position;
    float far;
};

struct PointLight
{
    float4 color;
    float3 position;
};

struct SpotlightSD
{
    float4x4 view;
    float4 color;
    float3 pos;
    float cutOff;
    float3 dir;
    float outerCutOff;
};

struct Spotlight
{
    float4 color;
    float3 pos;
    float cutOff;
    float3 dir;
    float outerCutOff;
};

float3 GetShadowUVD(float3 pos, float4x4 view)
{
    float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
    fragPosLightSpace.y = -fragPosLightSpace.y;
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords.xy = projCoords.xy * 0.5 + 0.5;
    return projCoords;
}