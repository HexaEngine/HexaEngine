namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a partial description of a shader resource view for a 1D texture array.
    /// </summary>
    public partial struct Texture1DArrayShaderResourceView : IEquatable<Texture1DArrayShaderResourceView>
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

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
            return obj is Texture1DArrayShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture1DArrayShaderResourceView other)
        {
            return MostDetailedMip == other.MostDetailedMip &&
                   MipLevels == other.MipLevels &&
                   FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MostDetailedMip, MipLevels, FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture1DArrayShaderResourceView left, Texture1DArrayShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DArrayShaderResourceView left, Texture1DArrayShaderResourceView right)
        {
            return !(left == right);
        }
    }
}