// Based on: http://wdobbie.com/post/gpu-text-rendering-with-vector-textures/

cbuffer CBSolidColorBrush
{
	float4 color;
}

struct PSIn
{
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
	uint bufferIndex : COLOR;
};

struct Glyph
{
	int start, count;
};

struct Curve
{
	float2 p0, p1, p2;
};

StructuredBuffer<Glyph> glyphs;
StructuredBuffer<Curve> curves;

cbuffer BrushParams
{
	//float4 color = 1;
}

// Size of the window (in pixels) used for 1-dimensional anti-aliasing along each rays.
//   0 - no anti-aliasing
//   1 - normal anti-aliasing
// >=2 - exaggerated effect
//float antiAliasingWindowSize = 1.0;

// Enable a second ray along the y-axis to achieve 2-dimensional anti-aliasing.
//bool enableSuperSamplingAntiAliasing = true;

Glyph loadGlyph(uint index)
{
	return glyphs[index];
}

Curve loadCurve(uint index)
{
	return curves[index];
}

float computeCoverage(float inverseDiameter, float2 p0, float2 p1, float2 p2) {
	if (p0.y > 0 && p1.y > 0 && p2.y > 0) return 0.0;
	if (p0.y < 0 && p1.y < 0 && p2.y < 0) return 0.0;

	// Note: Simplified from abc formula by extracting a factor of (-2) from b.
	float2 a = p0 - 2 * p1 + p2;
	float2 b = p0 - p1;
	float2 c = p0;

	float t0, t1;
	if (abs(a.y) >= 1e-5)
	{
		// Quadratic segment, solve abc formula to find roots.
		float radicand = b.y * b.y - a.y * c.y;
		if (radicand <= 0) return 0.0;

		float s = sqrt(radicand);
		t0 = (b.y - s) / a.y;
		t1 = (b.y + s) / a.y;
	}
	else {
		// Linear segment, avoid division by a.y, which is near zero.
		// There is only one root, so we have to decide which variable to
		// assign it to based on the direction of the segment, to ensure that
		// the ray always exits the shape at t0 and enters at t1. For a
		// quadratic segment this works 'automatically', see readme.
		float t = p0.y / (p0.y - p2.y);
		if (p0.y < p2.y)
		{
			t0 = -1.0;
			t1 = t;
		}
		else {
			t0 = t;
			t1 = -1.0;
		}
	}

	float alpha = 0;

	if (t0 >= 0 && t0 < 1)
	{
		float x = (a.x * t0 - 2.0 * b.x) * t0 + c.x;
		alpha += clamp(x * inverseDiameter + 0.5, 0, 1);
	}

	if (t1 >= 0 && t1 < 1)
	{
		float x = (a.x * t1 - 2.0 * b.x) * t1 + c.x;
		alpha -= clamp(x * inverseDiameter + 0.5, 0, 1);
	}

	return alpha;
}

float2 rotate(float2 v) {
	return float2(v.y, -v.x);
}

float4 main(PSIn input) : SV_TARGET
{
	float2 uv = input.uv;
uint bufferIndex = input.bufferIndex;

	float alpha = 0;

	// Size of the window (in pixels) used for 1-dimensional anti-aliasing along each rays.
//   0 - no anti-aliasing
//   1 - normal anti-aliasing
// >=2 - exaggerated effect
	float antiAliasingWindowSize = 1.0;

	// Enable a second ray along the y-axis to achieve 2-dimensional anti-aliasing.
	bool enableSuperSamplingAntiAliasing = true;

	// Inverse of the diameter of a pixel in uv units for anti-aliasing.
	float2 inverseDiameter = 1.0 / (antiAliasingWindowSize * fwidth(uv));

	Glyph glyph = loadGlyph(bufferIndex);
	for (int i = 0; i < glyph.count; i++) {
		Curve curve = loadCurve(glyph.start + i);

		float2 p0 = curve.p0 - uv;
		float2 p1 = curve.p1 - uv;
		float2 p2 = curve.p2 - uv;

		alpha += computeCoverage(inverseDiameter.x , p0, p1, p2);
		if (enableSuperSamplingAntiAliasing) {
			alpha += computeCoverage(inverseDiameter.y, rotate(p0), rotate(p1), rotate(p2));
		}
	}

	if (enableSuperSamplingAntiAliasing) {
		alpha *= 0.5;
	}

	alpha = clamp(alpha, 0.0, 1.0);

	return float4(color.rgb, color.a * alpha);
}