#include "../../camera.hlsl"
struct PixelInputType
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 normal : NORMAL;
};

float4 main(PixelInputType input) : SV_Target
{
    float3 lightPos = float3(0, 0, 0);
    float3 ambient = float3(0.2, 0.2, 0.2);
    
    float3 norm = normalize(input.normal);
    float3 lightDir = normalize(lightPos - input.pos);
    float diff = max(dot(norm, lightDir), 0.0);
    float3 diffuse = diff * float3(1, 1, 1);
    
    float3 viewDir = normalize(GetCameraPos() - input.pos);
    float3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 256);
    float specularStrength = 0.5;
    float3 specular = spec * specularStrength * float3(1, 1, 1);
    
    float3 color = (ambient + diffuse + specular) * float3(0.8, 0.8, 0.8);
    return float4(color, 1);
}