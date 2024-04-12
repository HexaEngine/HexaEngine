namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a render target view for a 2D texture.
    /// </summary>
    public struct Texture2DRenderTargetView : IEquatable<Texture2DRenderTargetView>
    {
        /// <summary>
        /// The mip level to use for the render target view.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture2DRenderTargetView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture2DRenderTargetView left, Texture2DRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DRenderTargetView left, Texture2DRenderTargetView right)
        {
            return !(left == right);
        }
    }
}