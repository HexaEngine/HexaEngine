
#if (FLAT == 1)
struct VertexInput
{
	float3 position : POSITION;
	float2 tex : TEXCOORD0;
    nointerpolation float3 normal : NORMAL;
    nointerpolation float3 tangent : TANGENT;
};

struct HullInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    nointerpolation float3 normal : NORMAL;
    nointerpolation float3 tangent : TANGENT;
    float TessFactor : TESS;
};

struct DomainInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    nointerpolation float3 normal : NORMAL;
    nointerpolation float3 tangent : TANGENT;
};

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
    float2 tex : TEXCOORD0;
    nointerpolation float3 normal : NORMAL;
    nointerpolation float3 tangent : TANGENT;
    float depth : TEXCOORD1;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};
#elif (DEPTH == 1)
struct VertexInput
{
    float3 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct HullInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
    float TessFactor : TESS;
};

struct DomainInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelInput
{
	float4 position : SV_POSITION;
    float depth : TEXCOORD0;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};
#elif (INSTANCED == 1)
struct VertexInput
{
    float3 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float4x4 world : INSTANCED_MATS;
};

struct HullInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float TessFactor : TESS;
};

struct DomainInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float4 pos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float depth : TEXCOORD1;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};
#else
struct VertexInput
{
	float3 position : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct HullInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
    float TessFactor : TESS;
};

struct DomainInput
{
	float3 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
    float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
    float depth : TEXCOORD1;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};
#endif