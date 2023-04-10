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