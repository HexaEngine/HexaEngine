struct DirectionalLightSD
{
    matrix views[16];
    float4 cascades[4];
    float4 color;
    float3 dir;
    int padd;
};

struct DirectionalLight
{
    float4 color;
    float3 dir;
    int padd;
};

struct PointLightSD
{
    float4 color;
    float3 position;
    int padd;
};

struct PointLight
{
    float4 color;
    float3 position;    
    int padd;
};

struct SpotlightSD
{
    matrix view;
    matrix proj;
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


cbuffer LightBuffer : register(b0)
{
    DirectionalLightSD directionalLightSDs[1];
    uint directionalLightSDCount;
    float3 padd1;
    
    PointLightSD pointLightSDs[8];
    uint pointLightSDCount;
    float3 padd2;
    
    SpotlightSD spotlightSDs[8];
    uint spotlightSDCount;
    float3 padd5;
    
    DirectionalLight directionalLights[4];
    uint directionalLightCount;
    float3 padd3;
    
    PointLight pointLights[32];
    uint pointLightCount;
    float3 padd4;
    
    Spotlight spotlights[32];
    uint spotlightCount;
    float3 padd6;
};


