Texture2D sourceTex;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut input) : SV_TARGET
{
	return sourceTex.Load((int3)input.Tex);
}