namespace HexaEngine.Core.IO.Meshes
{
    using System;

    public struct Face : IEquatable<Face>
    {
        public uint Index1;
        public uint Index2;
        public uint Index3;

        public Face(uint index1, uint index2, uint index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }

        public override bool Equals(object? obj)
        {
            return obj is Face face && Equals(face);
        }

        public bool Equals(Face other)
        {
            return Index1 == other.Index1 &&
                   Index2 == other.Index2 &&
                   Index3 == other.Index3;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index1, Index2, Index3);
        }

        public static bool operator ==(Face left, Face right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Face left, Face right)
        {
            return !(left == right);
        }
    }
}