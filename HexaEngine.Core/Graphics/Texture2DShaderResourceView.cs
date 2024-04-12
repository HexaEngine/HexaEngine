namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for a 2D texture.
    /// </summary>
    public struct Texture2DShaderResourceView : IEquatable<Texture2DShaderResourceView>
    {
        /// <summary>
        /// The most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture2DShaderResourceView other)
        {
            return MostDetailedMip == other.MostDetailedMip &&
                   MipLevels == other.MipLevels;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MostDetailedMip, MipLevels);
        }

        public static bool operator ==(Texture2DShaderResourceView left, Texture2DShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DShaderResourceView left, Texture2DShaderResourceView right)
        {
            return !(left == right);
        }
    }
}