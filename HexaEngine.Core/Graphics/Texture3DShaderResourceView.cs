namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for a 3D texture.
    /// </summary>
    public struct Texture3DShaderResourceView : IEquatable<Texture3DShaderResourceView>
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the view.
        /// </summary>
        public int MipLevels;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture3DShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture3DShaderResourceView other)
        {
            return MostDetailedMip == other.MostDetailedMip &&
                   MipLevels == other.MipLevels;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MostDetailedMip, MipLevels);
        }

        public static bool operator ==(Texture3DShaderResourceView left, Texture3DShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture3DShaderResourceView left, Texture3DShaderResourceView right)
        {
            return !(left == right);
        }
    }
}