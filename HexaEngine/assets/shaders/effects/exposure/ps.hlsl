Texture2D hdrTexture : register(t0);
Texture2D lumaTexture : register(t1);
SamplerState state;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params {
	bool enabled;
	float3 padd;
};

float3 convertRGB2XYZ(float3 _rgb)
{
	// Reference(s):
	// - RGB/XYZ Matrices
	//   https://web.archive.org/web/20191027010220/http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html
	float3 xyz;
	xyz.x = dot(float3(0.4124564, 0.3575761, 0.1804375), _rgb);
	xyz.y = dot(float3(0.2126729, 0.7151522, 0.0721750), _rgb);
	xyz.z = dot(float3(0.0193339, 0.1191920, 0.9503041), _rgb);
	return xyz;
}

float3 convertXYZ2RGB(float3 _xyz)
{
	float3 rgb;
	rgb.x = dot(float3(3.2404542, -1.5371385, -0.4985314), _xyz);
	rgb.y = dot(float3(-0.9692660, 1.8760108, 0.0415560), _xyz);
	rgb.z = dot(float3(0.0556434, -0.2040259, 1.0572252), _xyz);
	return rgb;
}

float3 convertXYZ2Yxy(float3 _xyz)
{
	// Reference(s):
	// - XYZ to xyY
	//   https://web.archive.org/web/20191027010144/http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_xyY.html
	float inv = 1.0 / dot(_xyz, float3(1.0, 1.0, 1.0));
	return float3(_xyz.y, _xyz.x * inv, _xyz.y * inv);
}

float3 convertYxy2XYZ(float3 _Yxy)
{
	// Reference(s):
	// - xyY to XYZ
	//   https://web.archive.org/web/20191027010036/http://www.brucelindbloom.com/index.html?Eqn_xyY_to_XYZ.html
	float3 xyz;
	xyz.x = _Yxy.x * _Yxy.y / _Yxy.z;
	xyz.y = _Yxy.x;
	xyz.z = _Yxy.x * (1.0 - _Yxy.y - _Yxy.z) / _Yxy.z;
	return xyz;
}

float3 convertRGB2Yxy(float3 _rgb)
{
	return convertXYZ2Yxy(convertRGB2XYZ(_rgb));
}

float3 convertYxy2RGB(float3 _Yxy)
{
	return convertXYZ2RGB(convertYxy2XYZ(_Yxy));
}

float4 main(VSOut vs) : SV_Target
{
	float4 color = hdrTexture.Sample(state, vs.Tex);

	if (!enabled)
	{
		return color;
	}

	float avgLum = lumaTexture.Sample(state, vs.Tex).r;
	float3 Yxy = convertRGB2Yxy(color.rgb);
	Yxy.x /= (9.6 * avgLum + 0.0001);
	color.rgb = convertYxy2RGB(Yxy);
	return color;
}