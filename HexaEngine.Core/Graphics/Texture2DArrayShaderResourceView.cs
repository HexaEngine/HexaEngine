namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayShaderResourceView : IEquatable<Texture2DArrayShaderResourceView>
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the view.
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
            return obj is Texture2DArrayShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture2DArrayShaderResourceView other)
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

        public static bool operator ==(Texture2DArrayShaderResourceView left, Texture2DArrayShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DArrayShaderResourceView left, Texture2DArrayShaderResourceView right)
        {
            return !(left == right);
        }
    }
}