using System.Numerics;

namespace HexaEngine.Materials.Pins
{
    public struct HalfVector4 : IEquatable<HalfVector4>
    {
        public Half X;
        public Half Y;
        public Half Z;
        public Half W;

        public HalfVector4(Half x, Half y, Half z, Half w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is HalfVector4 vector && Equals(vector);
        }

        public bool Equals(HalfVector4 other)
        {
            return X.Equals(other.X) &&
                   Y.Equals(other.Y) &&
                   Z.Equals(other.Z) &&
                   W.Equals(other.W);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        public static bool operator ==(HalfVector4 left, HalfVector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HalfVector4 left, HalfVector4 right)
        {
            return !(left == right);
        }

        public static implicit operator Vector4(HalfVector4 vector) => new((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);

        public static explicit operator HalfVector4(Vector4 vector) => new((Half)vector.X, (Half)vector.Y, (Half)vector.Z, (Half)vector.W);
    }
}