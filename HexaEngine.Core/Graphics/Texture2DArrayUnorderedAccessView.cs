namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents an unordered access view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayUnorderedAccessView : IEquatable<Texture2DArrayUnorderedAccessView>
    {
        /// <summary>
        /// The mip slice index.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first array slice.
        /// </summary>
        public int FirstArraySlice;

        /// <summary>
        /// The number of array slices.
        /// </summary>
        public int ArraySize;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DArrayUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(Texture2DArrayUnorderedAccessView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture2DArrayUnorderedAccessView left, Texture2DArrayUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DArrayUnorderedAccessView left, Texture2DArrayUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}