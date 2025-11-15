#include "../../weather.hlsl"

struct VertexOut
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

float3 HosekWilkie(float cosTheta, float gamma, float cosGamma)
{
    float3 expM = exp(E * gamma);
    float rayM = cosGamma * cosGamma;
    float3 mieM = (1.0f + rayM) / pow(1.0f + H * H - 2.0f * H * cosGamma, 1.5f);
    float zenith = sqrt(cosTheta); // vertical zenith gradient

    float3 temp1 = A * exp(B * (1.0f / (cosTheta + 0.01f)));
    float3 temp2 = C + D * expM + F * rayM + mieM * G + I * zenith;
    float3 temp = temp1 * temp2;

    return temp;
    float3 chi = (1 + cosGamma * cosGamma) / pow(1 + H * H - 2 * cosGamma * H, float3(1.5f, 1.5f, 1.5f));
    return (1 + A * exp(B / (cosTheta + 0.01))) * (C + D * exp(E * gamma) + F * (cosGamma * cosGamma) + G * chi + I * sqrt(cosTheta));
}

float3 HosekWilkieSky(float3 v, float3 sun_dir)
{
    float cosTheta = saturate(v.y);
    float cosGamma = dot(sun_dir, v);
    float gamma = acos(cosGamma);

    if (cosTheta < 0.0f)
        cosTheta = 0.0f;

    float3 R = Z * HosekWilkie(cosTheta, gamma, cosGamma);
    return R;
}

float4 main(VertexOut pin) : SV_TARGET
{
    float3 dir = normalize(pin.pos);

    float3 col = HosekWilkieSky(dir, light_dir.xyz);

    return float4(col, 1.0);

}