namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for a 1D texture.
    /// </summary>
    public struct Texture1DShaderResourceView : IEquatable<Texture1DShaderResourceView>
    {
        /// <summary>
        /// The index of the most detailed mip level in the texture.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the texture.
        /// </summary>
        public int MipLevels;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture1DShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture1DShaderResourceView other)
        {
            return MostDetailedMip == other.MostDetailedMip &&
                   MipLevels == other.MipLevels;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MostDetailedMip, MipLevels);
        }

        public static bool operator ==(Texture1DShaderResourceView left, Texture1DShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DShaderResourceView left, Texture1DShaderResourceView right)
        {
            return !(left == right);
        }
    }
}