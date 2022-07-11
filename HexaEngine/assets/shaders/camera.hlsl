cbuffer CameraBuffer : register(b1)
{
    matrix view;
    matrix proj;
    matrix viewInv;
    matrix projInv;
};


float3 GetCameraPos()
{
    return float3(viewInv._41, viewInv._42, viewInv._43);
}