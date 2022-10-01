struct DirectionalLightSD
{
    matrix view;
    matrix proj;
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
    matrix y;
    matrix yneg;
    matrix x;
    matrix xneg;
    matrix z;
    matrix zneg;
    matrix proj;
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
    int directionalLightSDCount;
    float3 padd1;
    
    PointLightSD pointLightSDs[8];
    int pointLightSDCount;
    float3 padd2;
    
    DirectionalLight directionalLights[4];
    int directionalLightCount;
    float3 padd3;
    
    PointLight pointLights[32];
    int pointLightCount;
    float3 padd4;
    
    SpotlightSD spotlightSDs[8];
    int spotlightSDCount;
    float3 padd5;
    
    Spotlight spotlights[32];
    int spotlightCount;
    float3 padd6;
};


