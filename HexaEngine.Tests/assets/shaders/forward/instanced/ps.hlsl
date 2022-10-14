#include "../../camera.hlsl"
struct PixelInput
{
	float4 position : SV_POSITION;
	float3 pos : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float depth : TEXCOORD1;
};

float4 main(PixelInput input) : SV_Target
{
	float3 lightPos = float3(0, 0, 0);
	float3 lightColor = float3(10, 10, 10);

	float3 LN = lightPos - input.pos;
	float3 L = normalize(LN);
	float distance = length(LN);
	float attenuation = 1.0 / (distance * distance);
	float3 radiance = lightColor * attenuation;

	float3 ambient = float3(0.2, 0.2, 0.2);

	float3 norm = normalize(input.normal);
	float diff = max(dot(norm, L), 0.0);
	float3 diffuse = diff * radiance;

	float3 viewDir = normalize(GetCameraPos() - input.pos);
	float3 reflectDir = reflect(-L, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 256);
	float specularStrength = 0.5;
	float3 specular = spec * specularStrength * radiance;

	float3 color = (ambient + diffuse + specular) * float3(0.8, 0.8, 0.8);
	return float4(color, 1);
}