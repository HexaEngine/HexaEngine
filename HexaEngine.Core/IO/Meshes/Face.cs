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

        public unsafe uint this[int index]
        {
            get
            {
                fixed (Face* p = &this)
                {
                    return ((uint*)p)[index];
                }
            }
            set
            {
                fixed (Face* p = &this)
                {
                    ((uint*)p)[index] = value;
                }
            }
        }

        public unsafe uint this[uint index]
        {
            get
            {
                fixed (Face* p = &this)
                {
                    return ((uint*)p)[index];
                }
            }
            set
            {
                fixed (Face* p = &this)
                {
                    ((uint*)p)[index] = value;
                }
            }
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

        public readonly bool Shares(Face other)
        {
            return Index1 == other.Index1 || Index2 == other.Index2 || Index3 == other.Index3;
        }

        public readonly bool Shares(uint index)
        {
            return Index1 == index || Index2 == index || Index3 == index;
        }
    }
}