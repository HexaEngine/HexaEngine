#include "../../camera.hlsl"
struct PixelInputType
{
	float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 normal : NORMAL;
};

float4 main(PixelInputType input) : SV_Target
{
    float3 dir = normalize(input.pos - GetCameraPos());
    float diff = max(dot(input.normal, dir), 0.0);
    return float4(1, 0, 0, 1) * diff + float4(0.2,0.2,0.2,0);
}