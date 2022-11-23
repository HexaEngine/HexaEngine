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