Texture2D sourceTex;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut input) : SV_TARGET
{
	float2 texSize = float2(0,0);
	sourceTex.GetDimensions(texSize.x, texSize.y);
	return sourceTex.Load(int3((int2)(input.Tex * texSize), 0));
}