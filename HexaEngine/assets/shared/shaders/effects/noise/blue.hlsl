struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float WhiteNose(float2 p)
{
    return frac(sin(fmod(dot(p, float2(12.9898, 78.233)), 6.283)) * 43758.5453);
}

float BlueNoise(float2 p)
{
    return ((WhiteNose(p + float2(-1, -1)) + WhiteNose(p + float2(0, -1)) + WhiteNose(p + float2(1, -1)) + WhiteNose(p + float2(-1, 0)) - 8. * WhiteNose(p) + WhiteNose(p + float2(1, 0)) + WhiteNose(p + float2(-1, 1)) + WhiteNose(p + float2(0, 1)) + WhiteNose(p + float2(1, 1))) * .5 / 9. * 2.1 + .5);
}

float4 main(VSOut vs) : SV_Target
{
    return BlueNoise(vs.Tex);
}