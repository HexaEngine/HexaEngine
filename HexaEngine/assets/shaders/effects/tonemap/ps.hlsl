Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);
Texture2D lumaTexture : register(t2);
SamplerState state;

#define FXAA

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float bloomStrength;
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

float3 ACESFilm(float3 x)
{
	return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
	float gamma = 2.2;
	return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float3 BloomMix(float2 texCoord, float3 hdr)
{
	float3 blm = bloomTexture.Sample(state, texCoord).rgb;
	float3 drt = float3(0, 0, 0);
	return lerp(hdr, blm + blm * drt, float3(bloomStrength, bloomStrength, bloomStrength));
}

float4 main(VSOut vs) : SV_Target
{
	float avgLum = lumaTexture.Sample(state, vs.Tex).r;

	if (vs.Tex.x < 0.1f && vs.Tex.y < 0.1f)
	{
		return float4(avgLum, avgLum, avgLum, 1);
	}

	float4 color = hdrTexture.Sample(state, vs.Tex);

	float3 Yxy = convertRGB2Yxy(color.rgb);

	Yxy.x /= (9.6 * avgLum + 0.0001);

	color.rgb = convertYxy2RGB(Yxy);

	color.rgb = BloomMix(vs.Tex, color.rgb);
	color.rgb = ACESFilm(color.rgb);
	color.rgb = OECF_sRGBFast(color.rgb);
#ifdef FXAA
	color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));
#endif

	return color;
}