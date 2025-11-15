cbuffer ParamBuffer
{
    float4 scale;
    float4 offset;
    float4 repeat;
    float2 period;
    float2 rotation;
};

float lt(float a, float b)
{
    return a < b ? 1.0 : 0.0;
}

float lessThan(float a, float b)
{
    return lt(a, b);
}
float2 lessThan(float2 a, float2 b)
{
    return float2(lt(a.x, b.x), lt(a.y, b.y));
}
float3 lessThan(float3 a, float3 b)
{
    return float3(lt(a.x, b.x), lt(a.y, b.y), lt(a.z, b.z));
}
float4 lessThan(float4 a, float4 b)
{
    return float4(lt(a.x, b.x), lt(a.y, b.y), lt(a.z, b.z), lt(a.w, b.w));
}