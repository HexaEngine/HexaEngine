namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a partial description of an unordered access view for a 1D texture array.
    /// </summary>
    public partial struct Texture1DArrayUnorderedAccessView : IEquatable<Texture1DArrayUnorderedAccessView>
    {
        /// <summary>
        /// The mip slice.
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
            return obj is Texture1DArrayUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(Texture1DArrayUnorderedAccessView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture1DArrayUnorderedAccessView left, Texture1DArrayUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DArrayUnorderedAccessView left, Texture1DArrayUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}