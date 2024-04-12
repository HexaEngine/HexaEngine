namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a depth-stencil view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayDepthStencilView : IEquatable<Texture2DArrayDepthStencilView>
    {
        /// <summary>
        /// The index of the mip level to use for the view.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first array slice to use in the view.
        /// </summary>
        public int FirstArraySlice;

        /// <summary>
        /// The total number of slices in the view.
        /// </summary>
        public int ArraySize;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DArrayDepthStencilView view && Equals(view);
        }

        public readonly bool Equals(Texture2DArrayDepthStencilView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture2DArrayDepthStencilView left, Texture2DArrayDepthStencilView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DArrayDepthStencilView left, Texture2DArrayDepthStencilView right)
        {
            return !(left == right);
        }
    }
}