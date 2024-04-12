namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a render target view for a 3D texture.
    /// </summary>
    public struct Texture3DRenderTargetView : IEquatable<Texture3DRenderTargetView>
    {
        /// <summary>
        /// The index of the mip level to use mip slice.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first depth slice to use in the render target view.
        /// </summary>
        public int FirstWSlice;

        /// <summary>
        /// The number of depth slices to use in the render target view.
        /// </summary>
        public int WSize;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture3DRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture3DRenderTargetView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstWSlice == other.FirstWSlice &&
                   WSize == other.WSize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstWSlice, WSize);
        }

        public static bool operator ==(Texture3DRenderTargetView left, Texture3DRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture3DRenderTargetView left, Texture3DRenderTargetView right)
        {
            return !(left == right);
        }
    }
}