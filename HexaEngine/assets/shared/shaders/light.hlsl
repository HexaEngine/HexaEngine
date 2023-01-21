struct DirectionalLightSD
{
	matrix views[16];
	float4 cascades[4];
	float4 color;
	float3 dir;
	int padd;
};

struct DirectionalLight
{
	float4 color;
	float3 dir;
	int padd;
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
	int padd;
};

struct SpotlightSD
{
	matrix view;
	matrix proj;
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