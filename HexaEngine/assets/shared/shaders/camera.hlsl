cbuffer CameraBuffer : register(b1)
{
    matrix view;
    matrix proj;
    matrix viewInv;
    matrix projInv;
    float cam_far;
    float cam_near;
    float2 cam_padd;
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