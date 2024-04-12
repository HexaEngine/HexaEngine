namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a partial description of a depth-stencil view for a 1D texture.
    /// </summary>
    public partial struct Texture1DDepthStencilView : IEquatable<Texture1DDepthStencilView>
    {
        /// <summary>
        /// The mip slice.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture1DDepthStencilView view && Equals(view);
        }

        public readonly bool Equals(Texture1DDepthStencilView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture1DDepthStencilView left, Texture1DDepthStencilView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DDepthStencilView left, Texture1DDepthStencilView right)
        {
            return !(left == right);
        }
    }
}