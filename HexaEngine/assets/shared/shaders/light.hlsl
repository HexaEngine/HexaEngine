struct DirectionalLightSD
{
	matrix views[16];
	float4 cascades[4];
	float4 color;
	float3 dir;
};

struct DirectionalLight
{
	float4 color;
	float3 dir;
};

struct PointLightSD
{
	float4 color;
	float3 position;
	float far;
};

struct PointLight
{
	float4 color;
	float3 position;
};

struct SpotlightSD
{
	matrix view;
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