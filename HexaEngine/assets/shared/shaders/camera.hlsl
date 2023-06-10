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

float3 GetPositionVS(float2 uv, float depth)
{
    float4 ndc = float4(uv * 2.0f - 1.0f, depth, 1.0f);
    ndc.y *= -1;
    float4 wp = mul(ndc, viewInv);
    return wp.xyz / wp.w;
}

float3 GetPositionWS(float2 uv, float depth)
{
    float4 ndc = float4(uv * 2.0f - 1.0f, depth, 1.0f);
    ndc.y *= -1;
    float4 wp = mul(ndc, viewProjInv);
    return wp.xyz / wp.w;
}