namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a partial description of a render target view for a 1D texture array.
    /// </summary>
    public partial struct Texture1DArrayRenderTargetView : IEquatable<Texture1DArrayRenderTargetView>
    {
        /// <summary>
        /// The index of the mip level.
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
            return obj is Texture1DArrayRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture1DArrayRenderTargetView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture1DArrayRenderTargetView left, Texture1DArrayRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DArrayRenderTargetView left, Texture1DArrayRenderTargetView right)
        {
            return !(left == right);
        }
    }
}