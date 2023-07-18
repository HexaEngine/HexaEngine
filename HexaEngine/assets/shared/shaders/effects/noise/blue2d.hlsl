float white(float2 p)
{
    return frac(sin(fmod(dot(p, float2(12.9898, 78.233)), 6.283)) * 43758.5453);
}

float blue(float2 p)
{
    return ((white(p + float2(-1, -1)) + white(p + float2(0, -1)) + white(p + float2(1, -1)) + white(p + float2(-1, 0)) - 8. * white(p) + white(p + float2(1, 0)) + white(p + float2(-1, 1)) + white(p + float2(0, 1)) + white(p + float2(1, 1))) * .5 / 9. * 2.1 + .5);
}