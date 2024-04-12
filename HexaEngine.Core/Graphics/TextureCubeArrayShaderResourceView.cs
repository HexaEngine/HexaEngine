namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for an array of cube textures.
    /// </summary>
    public struct TextureCubeArrayShaderResourceView : IEquatable<TextureCubeArrayShaderResourceView>
    {
        /// <summary>
        /// The most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// The index of the first 2D array face.
        /// </summary>
        public int First2DArrayFace;

        /// <summary>
        /// The number of cube textures in the array.
        /// </summary>
        public int NumCubes;

        public override readonly bool Equals(object? obj)
        {
            return obj is TextureCubeArrayShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(TextureCubeArrayShaderResourceView other)
        {
            return MostDetailedMip == other.MostDetailedMip &&
                   MipLevels == other.MipLevels &&
                   First2DArrayFace == other.First2DArrayFace &&
                   NumCubes == other.NumCubes;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MostDetailedMip, MipLevels, First2DArrayFace, NumCubes);
        }

        public static bool operator ==(TextureCubeArrayShaderResourceView left, TextureCubeArrayShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureCubeArrayShaderResourceView left, TextureCubeArrayShaderResourceView right)
        {
            return !(left == right);
        }
    }
}