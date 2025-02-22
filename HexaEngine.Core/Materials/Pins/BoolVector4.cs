namespace HexaEngine.Materials.Pins
{
    public struct BoolVector4 : IEquatable<BoolVector4>
    {
        public bool X;
        public bool Y;
        public bool Z;
        public bool W;

        public override bool Equals(object? obj)
        {
            return obj is BoolVector4 vector && Equals(vector);
        }

        public bool Equals(BoolVector4 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        public static bool operator ==(BoolVector4 left, BoolVector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoolVector4 left, BoolVector4 right)
        {
            return !(left == right);
        }
    }
}