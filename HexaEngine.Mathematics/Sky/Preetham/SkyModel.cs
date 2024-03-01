﻿namespace HexaEngine.Mathematics.Sky.Preetham
{
    using System.Numerics;

    /// <summary>
    /// Helper class for the Preetham sky model calculations.
    /// </summary>
    public static unsafe class SkyModel
    {
        /// <summary>
        /// Calculates the Perez distribution parameters.
        /// </summary>
        /// <param name="T">Turbidity value.</param>
        /// <param name="A">Resulting A vector.</param>
        /// <param name="B">Resulting B vector.</param>
        /// <param name="C">Resulting C vector.</param>
        /// <param name="D">Resulting D vector.</param>
        /// <param name="E">Resulting E vector.</param>
        public static void CalculatePerezDistribution(in float T, out Vector3 A, out Vector3 B, out Vector3 C, out Vector3 D, out Vector3 E)
        {
            A = new Vector3(0.17872f * T - 1.46303f, -0.01925f * T - 0.25922f, -0.01669f * T - 0.26078f);
            B = new Vector3(-0.35540f * T + 0.42749f, -0.06651f * T + 0.00081f, -0.09495f * T + 0.00921f);
            C = new Vector3(-0.02266f * T + 5.32505f, -0.00041f * T + 0.21247f, -0.00792f * T + 0.21023f);
            D = new Vector3(0.12064f * T - 2.57705f, -0.06409f * T - 0.89887f, -0.04405f * T - 1.65369f);
            E = new Vector3(-0.06696f * T + 0.37027f, -0.00325f * T + 0.04517f, -0.01092f * T + 0.05291f);
        }

        /// <summary>
        /// Calculates the zenith luminance in Yxy color space.
        /// </summary>
        /// <param name="t">Turbidity value.</param>
        /// <param name="thetaS">Solar zenith angle in radians.</param>
        /// <returns>A vector in Yxy color space representing the zenith luminance.</returns>
        public static Vector3 CalculateZenithLuminanceYxy(in float t, in float thetaS)
        {
            float chi = (4.0f / 9.0f - t / 120.0f) * (MathF.PI - 2.0f * thetaS);
            float Yz = (4.0453f * t - 4.9710f) * MathF.Tan(chi) - 0.2155f * t + 2.4192f;

            float theta2 = thetaS * thetaS;
            float theta3 = theta2 * thetaS;
            float T = t;
            float T2 = t * t;

            float xz =
                (0.00165f * theta3 - 0.00375f * theta2 + 0.00209f * thetaS + 0.0f) * T2 +
                (-0.02903f * theta3 + 0.06377f * theta2 - 0.03202f * thetaS + 0.00394f) * T +
                (0.11693f * theta3 - 0.21196f * theta2 + 0.06052f * thetaS + 0.25886f);

            float yz =
                (0.00275f * theta3 - 0.00610f * theta2 + 0.00317f * thetaS + 0.0f) * T2 +
                (-0.04214f * theta3 + 0.08970f * theta2 - 0.04153f * thetaS + 0.00516f) * T +
                (0.15346f * theta3 - 0.26756f * theta2 + 0.06670f * thetaS + 0.26688f);

            return new Vector3(Yz, xz, yz);
        }

        private static float PerezUpper(float* lambdas, float cosTheta, float gamma, float cosGamma)
        {
            return (1.0f + lambdas[0] * MathF.Exp(lambdas[1] / (cosTheta + 1e-6f)))
                  * (1.0f + lambdas[2] * MathF.Exp(lambdas[3] * gamma) + lambdas[4] * MathUtil.Sqr(cosGamma));
        }

        private static float PerezLower(float* lambdas, float cosThetaS, float thetaS)
        {
            return (1.0f + lambdas[0] * MathF.Exp(lambdas[1]))
                  * (1.0f + lambdas[2] * MathF.Exp(lambdas[3] * thetaS) + lambdas[4] * MathUtil.Sqr(cosThetaS));
        }

        private static Vector3 PerezLowerLuminanceYxy(in float cosThetaS, float thetaS, in Vector3 A, in Vector3 B, in Vector3 C, in Vector3 D, in Vector3 E)
        {
            return (Vector3.One + A * MathUtil.Exp(B)) * (Vector3.One + C * MathUtil.Exp(D * thetaS) + E * MathUtil.Sqr(cosThetaS));
        }

        /// <summary>
        /// Calculates sky parameters based on turbidity and sun direction.
        /// </summary>
        /// <param name="turbidity">Turbidity of the atmosphere.</param>
        /// <param name="sunDirection">Direction of the sun (assumes normalized).</param>
        /// <param name="overcast">The overcast term of the atmosphere.</param>
        /// <param name="horizCrush">The horizon crush term for "muddy" horizon.</param>
        /// <returns>Sky parameters for the given environmental conditions.</returns>
        public static SkyParameters CalculateSkyParameters(float turbidity, Vector3 sunDirection, float overcast, float horizCrush)
        {
            float theta = MathF.Acos(Math.Clamp(sunDirection.Y, 0.0f, 1.0f)); //assumes normalized sun direction

            SkyParameters parameters = default;

            CalculatePerezDistribution(turbidity, out parameters.A, out parameters.B, out parameters.C, out parameters.D, out parameters.E);

            parameters.F = CalculateZenithLuminanceYxy(turbidity, theta);

            if (sunDirection.Y < 0.0f)    // Handle sun going below the horizon
            {
                float s = Math.Clamp(1.0f + sunDirection.Y * 50.0f, 0, 1);   // goes from 1 to 0 as the sun sets

                // Take C/E which control sun term to zero
                parameters.C *= s;
                parameters.E *= s;
            }

            if (overcast != 0.0f)      // Handle overcast term
            {
                float invOvercast = 1.0f - overcast;

                // lerp back towards unity
                parameters.A.Y *= invOvercast;  // main sky chroma -> base
                parameters.A.Z *= invOvercast;

                // sun flare -> 0 strength/base chroma
                parameters.C *= invOvercast;
                parameters.E *= invOvercast;

                // lerp towards a fit of the CIE cloudy sky model: 4, -0.7
                parameters.A.X = MathUtil.Lerp(parameters.A.X, 4.0f, overcast);
                parameters.B.X = MathUtil.Lerp(parameters.B.X, -0.7f, overcast);

                // lerp base colour towards white point
                parameters.F.Y = parameters.F.Y * invOvercast + 0.333f * overcast;
                parameters.F.Z = parameters.F.Z * invOvercast + 0.333f * overcast;
            }

            if (horizCrush != 0.0f)
            {
                // The Preetham sky model has a "muddy" horizon, which can be objectionable in
                // typical game views. We allow artistic control over it.
                parameters.B *= horizCrush;
            }

            return parameters;
        }
    }
}