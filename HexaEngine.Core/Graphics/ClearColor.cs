using System.Numerics;

namespace HexaEngine.Core.Graphics
{
    public struct ClearColorValue : IEquatable<ClearColorValue>
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ClearColorValue(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ClearColorValue color && Equals(color);
        }

        public readonly bool Equals(ClearColorValue other)
        {
            return R == other.R &&
                   G == other.G &&
                   B == other.B &&
                   A == other.A;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public static bool operator ==(ClearColorValue left, ClearColorValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ClearColorValue left, ClearColorValue right)
        {
            return !(left == right);
        }

        public static implicit operator Vector4(ClearColorValue color) => new(color.R, color.G, color.B, color.A);

        public static implicit operator ClearColorValue(Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
    }
}