namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a depth-stencil view for a 2D texture.
    /// </summary>
    public struct Texture2DDepthStencilView : IEquatable<Texture2DDepthStencilView>
    {
        /// <summary>
        /// The mip slice index.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DDepthStencilView view && Equals(view);
        }

        public readonly bool Equals(Texture2DDepthStencilView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture2DDepthStencilView left, Texture2DDepthStencilView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DDepthStencilView left, Texture2DDepthStencilView right)
        {
            return !(left == right);
        }
    }
}