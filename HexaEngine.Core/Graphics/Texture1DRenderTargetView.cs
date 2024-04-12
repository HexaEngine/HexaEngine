namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a render target view for a 1D texture.
    /// </summary>
    public struct Texture1DRenderTargetView : IEquatable<Texture1DRenderTargetView>
    {
        /// <summary>
        /// The index of the mip level to use mip slice.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture1DRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture1DRenderTargetView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture1DRenderTargetView left, Texture1DRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DRenderTargetView left, Texture1DRenderTargetView right)
        {
            return !(left == right);
        }
    }
}