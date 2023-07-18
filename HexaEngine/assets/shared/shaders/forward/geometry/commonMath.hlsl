//------------------------------------------------------------------------------
// Common math
//------------------------------------------------------------------------------

#define PI                 3.14159265359
#define HALF_PI            1.570796327

#define MEDIUMP_FLT_MAX    65504.0
#define MEDIUMP_FLT_MIN    0.00006103515625

#ifdef TARGET_MOBILE
#define FLT_EPS            MEDIUMP_FLT_MIN
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)
#else
#define FLT_EPS            1e-5
#define saturateMediump(x) x
#endif

#define saturate(x)        clamp(x, 0.0, 1.0)
#define atan2(x, y)        atan(y, x)

//------------------------------------------------------------------------------
// Scalar operations
//------------------------------------------------------------------------------

float pow5(float x)
{
    float x2 = x * x;
    return x2 * x2 * x;
}

float sq(float x)
{
    return x * x;
}

//------------------------------------------------------------------------------
// Vector operations
//------------------------------------------------------------------------------

float max3(const float3 v)
{
    return max(v.x, max(v.y, v.z));
}

float vmax(const float2 v)
{
    return max(v.x, v.y);
}

float vmax(const float3 v)
{
    return max(v.x, max(v.y, v.z));
}

float vmax(const float4 v)
{
    return max(max(v.x, v.y), max(v.y, v.z));
}

float min3(const float3 v)
{
    return min(v.x, min(v.y, v.z));
}

float vmin(const float2 v)
{
    return min(v.x, v.y);
}

float vmin(const float3 v)
{
    return min(v.x, min(v.y, v.z));
}

float vmin(const float4 v)
{
    return min(min(v.x, v.y), min(v.y, v.z));
}

//------------------------------------------------------------------------------
// Trigonometry
//------------------------------------------------------------------------------

float acosFast(float x)
{
    // Lagarde 2014, "Inverse trigonometric functions GPU optimization for AMD GCN architecture"
    // This is the approximation of degree 1, with a max absolute error of 9.0x10^-3
    float y = abs(x);
    float p = -0.1565827 * y + 1.570796;
    p *= sqrt(1.0 - y);
    return x >= 0.0 ? p : PI - p;
}

float acosFastPositive(float x)
{
    float p = -0.1565827 * x + 1.570796;
    return p * sqrt(1.0 - x);
}

//------------------------------------------------------------------------------
// Matrix and quaternion operations
//------------------------------------------------------------------------------

float4 mulMat4x4Float3(
const float4x4 m,
const float3 v)
{
    return v.x * m[0] + (v.y * m[1] + (v.z * m[2] + m[3]));
}

float3 mulMat3x3Float3(
const float4x4 m,
const float3 v)
{
    return v.x * m[0].xyz + (v.y * m[1].xyz + (v.z * m[2].xyz));
}

void toTangentFrame(const float4 q, out float3 n)
{
    n = float3(0.0, 0.0, 1.0) +
        float3(2.0, -2.0, -2.0) * q.x * q.zwx +
        float3(2.0, 2.0, -2.0) * q.y * q.
wzy;
}

void toTangentFrame(const float4 q, out float3 n, out float3 t)
{
    toTangentFrame(q, n);
    t = float3(1.0, 0.0, 0.0) +
        float3(-2.0, 2.0, -2.0) * q.y * q.yxw +
        float3(-2.0, 2.0, 2.0) * q.z * q.
zwx;
}

float3x3 cofactor(const float3x3 m)
{

    float a = m[0][0];
    float b = m[1][0];
    float c = m[2][0];
    float d = m[0][1];
    float e = m[1][1];
    float f = m[2][1];
    float g = m[0][2];
    float h = m[1][2];
    float i = m[2][2];

    float3x3 cof;
    cof[0][0] = e * i - f * h;
    cof[0][1] = c * h - b * i;
    cof[0][2] = b * f - c * e;
    cof[1][0] = f * g - d * i;
    cof[1][1] = a * i - c * g;
    cof[1][2] = c * d - a * f;
    cof[2][0] = d * h - e * g;
    cof[2][1] = b * g - a * h;
    cof[2][2] = a * e - b * d;

    return cof;
}

//------------------------------------------------------------------------------
// Random
//------------------------------------------------------------------------------

float interleavedGradientNoise(float2 w)
{
    const float3 m = float3(0.06711056, 0.00583715, 52.9829189);
    return frac(m.z * frac(dot(w, m.xy)));
}