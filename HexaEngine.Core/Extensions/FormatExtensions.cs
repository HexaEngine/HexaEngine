namespace HexaEngine.Core.Extensions
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Globalization;
    using System.Numerics;

    /// <summary>
    /// Provides extension methods for formatting various data types into HLSL shader language representations.
    /// </summary>
    public static class FormatExtensions
    {
        /// <summary>
        /// Converts a boolean value to its HLSL representation (1 for true, 0 for false).
        /// </summary>
        /// <param name="scalar">The boolean value to convert.</param>
        /// <returns>The HLSL representation of the boolean value.</returns>
        public static string ToHLSL(this bool scalar)
        {
            return scalar ? "1" : "0";
        }

        /// <summary>
        /// Converts a float value to its HLSL representation.
        /// </summary>
        /// <param name="scalar">The float value to convert.</param>
        /// <returns>The HLSL representation of the float value.</returns>
        public static string ToHLSL(this float scalar)
        {
            return scalar.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a Vector2 value to its HLSL representation.
        /// </summary>
        /// <param name="vector">The Vector2 value to convert.</param>
        /// <returns>The HLSL representation of the Vector2 value.</returns>
        public static string ToHLSL(this Vector2 vector)
        {
            return $"float2({vector.X.ToString(CultureInfo.InvariantCulture)}, {vector.Y.ToString(CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        /// Converts a Vector3 value to its HLSL representation.
        /// </summary>
        /// <param name="vector">The Vector3 value to convert.</param>
        /// <returns>The HLSL representation of the Vector3 value.</returns>
        public static string ToHLSL(this Vector3 vector)
        {
            return $"float3({vector.X.ToString(CultureInfo.InvariantCulture)}, {vector.Y.ToString(CultureInfo.InvariantCulture)}, {vector.Z.ToString(CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        /// Converts a Vector4 value to its HLSL representation.
        /// </summary>
        /// <param name="vector">The Vector4 value to convert.</param>
        /// <returns>The HLSL representation of the Vector4 value.</returns>
        public static string ToHLSL(this Vector4 vector)
        {
            return $"float4({vector.X.ToString(CultureInfo.InvariantCulture)}, {vector.Y.ToString(CultureInfo.InvariantCulture)}, {vector.Z.ToString(CultureInfo.InvariantCulture)}, {vector.W.ToString(CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        /// Converts a UVType to its HLSL type.
        /// </summary>
        /// <param name="uvType"></param>
        /// <returns></returns>
        public static string ToHLSL(this UVType uvType)
        {
            switch (uvType)
            {
                case UVType.UV2D:
                    return "float2";

                case UVType.UV3D:
                    return "float3";

                case UVType.UV4D:
                    return "float4";
            }
            return string.Empty;
        }
    }
}