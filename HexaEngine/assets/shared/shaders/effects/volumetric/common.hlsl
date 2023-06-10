bool IsSaturated(float value)
{
    return value == saturate(value);
}
bool IsSaturated(float2 value)
{
    return IsSaturated(value.x) && IsSaturated(value.y);
}
bool IsSaturated(float3 value)
{
    return IsSaturated(value.x) && IsSaturated(value.y) && IsSaturated(value.z);
}
bool IsSaturated(float4 value)
{
    return IsSaturated(value.x) && IsSaturated(value.y) && IsSaturated(value.z) && IsSaturated(value.w);
}

static const float2x2 BayerMatrix2 =
{
    1.0 / 5.0, 3.0 / 5.0,
	4.0 / 5.0, 2.0 / 5.0
};

static const float3x3 BayerMatrix3 =
{
    3.0 / 10.0, 7.0 / 10.0, 4.0 / 10.0,
	6.0 / 10.0, 1.0 / 10.0, 9.0 / 10.0,
	2.0 / 10.0, 8.0 / 10.0, 5.0 / 10.0
};

static const float4x4 BayerMatrix4 =
{
    1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
	13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
	4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
	16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
};

static const float BayerMatrix8[8][8] =
{
    { 1.0 / 65.0, 49.0 / 65.0, 13.0 / 65.0, 61.0 / 65.0, 4.0 / 65.0, 52.0 / 65.0, 16.0 / 65.0, 64.0 / 65.0 },
    { 33.0 / 65.0, 17.0 / 65.0, 45.0 / 65.0, 29.0 / 65.0, 36.0 / 65.0, 20.0 / 65.0, 48.0 / 65.0, 32.0 / 65.0 },
    { 9.0 / 65.0, 57.0 / 65.0, 5.0 / 65.0, 53.0 / 65.0, 12.0 / 65.0, 60.0 / 65.0, 8.0 / 65.0, 56.0 / 65.0 },
    { 41.0 / 65.0, 25.0 / 65.0, 37.0 / 65.0, 21.0 / 65.0, 44.0 / 65.0, 28.0 / 65.0, 40.0 / 65.0, 24.0 / 65.0 },
    { 3.0 / 65.0, 51.0 / 65.0, 15.0 / 65.0, 63.0 / 65.0, 2.0 / 65.0, 50.0 / 65.0, 14.0 / 65.0, 62.0 / 65.0 },
    { 35.0 / 65.0, 19.0 / 65.0, 47.0 / 65.0, 31.0 / 65.0, 34.0 / 65.0, 18.0 / 65.0, 46.0 / 65.0, 30.0 / 65.0 },
    { 11.0 / 65.0, 59.0 / 65.0, 7.0 / 65.0, 55.0 / 65.0, 10.0 / 65.0, 58.0 / 65.0, 6.0 / 65.0, 54.0 / 65.0 },
    { 43.0 / 65.0, 27.0 / 65.0, 39.0 / 65.0, 23.0 / 65.0, 42.0 / 65.0, 26.0 / 65.0, 38.0 / 65.0, 22.0 / 65.0 }
};

//bayer dithering

inline float ditherMask2(in float2 pixel)
{
    return BayerMatrix2[pixel.x % 2][pixel.y % 2];
}

inline float ditherMask3(in float2 pixel)
{
    return BayerMatrix3[pixel.x % 3][pixel.y % 3];
}

inline float ditherMask4(in float2 pixel)
{
    return BayerMatrix4[pixel.x % 4][pixel.y % 4];
}

inline float ditherMask8(in float2 pixel)
{
    return BayerMatrix8[pixel.x % 8][pixel.y % 8];
}

inline float dither(in float2 pixel)
{
    return ditherMask8(pixel);
}



//other

float2 dither(float2 coord, float seed, float2 size)
{
    float noiseX = ((frac(1.0 - (coord.x + seed * 1.0) * (size.x / 2.0)) * 0.25) + (frac((coord.y + seed * 2.0) * (size.y / 2.0)) * 0.75)) * 2.0 - 1.0;
    float noiseY = ((frac(1.0 - (coord.x + seed * 3.0) * (size.x / 2.0)) * 0.75) + (frac((coord.y + seed * 4.0) * (size.y / 2.0)) * 0.25)) * 2.0 - 1.0;
    return float2(noiseX, noiseY);
}

float2 mod_dither(float2 u)
{
    float noiseX = fmod(u.x + u.y + fmod(208. + u.x * 3.58, 13. + fmod(u.y * 22.9, 9.)), 7.) * .143;
    float noiseY = fmod(u.y + u.x + fmod(203. + u.y * 3.18, 12. + fmod(u.x * 27.4, 8.)), 6.) * .139;
    return float2(noiseX, noiseY) * 2.0 - 1.0;
}