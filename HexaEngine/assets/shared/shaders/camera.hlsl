cbuffer CameraBuffer : register(b1)
{
    float4x4 view;
    float4x4 proj;
    float4x4 viewInv;
    float4x4 projInv;
    float4x4 viewProj;
    float4x4 viewProjInv;
    float4x4 prevViewProj;
    float cam_far;
    float cam_near;
    float2 screen_dim;
};

float3 GetCameraPos()
{
    return float3(viewInv._41, viewInv._42, viewInv._43);
}

float GetLinearDepth(float depth)
{
    float z_b = depth;
    float z_n = 2.0 * z_b - 1.0;
    float z_e = 2.0 * cam_near * cam_far / (cam_far + cam_near - z_n * (cam_far - cam_near));
    return z_e;
}

float SampleLinearDepth(Texture2D tex, SamplerState smp, float2 texCoord)
{
    float depth = tex.Sample(smp, texCoord).r;
    return GetLinearDepth(depth);
}

float3 GetPositionVS(float2 texcoord, float depth)
{
    float4 clipSpaceLocation;
    clipSpaceLocation.xy = texcoord * 2.0f - 1.0f;
    clipSpaceLocation.y *= -1;
    clipSpaceLocation.z = depth;
    clipSpaceLocation.w = 1.0f;
    float4 homogenousLocation = mul(clipSpaceLocation, projInv);
    return homogenousLocation.xyz / homogenousLocation.w;
}