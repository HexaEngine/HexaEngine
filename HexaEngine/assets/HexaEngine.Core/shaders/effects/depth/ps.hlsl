#include "HexaEngine.Core:shaders/camera.hlsl"

Texture2D<float> sourceTex;

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float main(VSOut input) : SV_TARGET
{
    float2 texSize = float2(0, 0);
    sourceTex.GetDimensions(texSize.x, texSize.y);
    float depth = sourceTex.Load(int3((int2) (input.Tex * texSize), 0));
    return GetLinearDepth(depth);
}
