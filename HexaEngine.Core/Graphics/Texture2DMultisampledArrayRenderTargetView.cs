namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a render target view for a multisampled 2D texture array.
    /// </summary>
    public struct Texture2DMultisampledArrayRenderTargetView : IEquatable<Texture2DMultisampledArrayRenderTargetView>
    {
        /// <summary>
        /// The index of the first array slice.
        /// </summary>
        public int FirstArraySlice;

        /// <summary>
        /// The number of array slices.
        /// </summary>
        public int ArraySize;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DMultisampledArrayRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture2DMultisampledArrayRenderTargetView other)
        {
            return FirstArraySlice == other.FirstArraySlice &&
                   ArraySize == other.ArraySize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(FirstArraySlice, ArraySize);
        }

        public static bool operator ==(Texture2DMultisampledArrayRenderTargetView left, Texture2DMultisampledArrayRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DMultisampledArrayRenderTargetView left, Texture2DMultisampledArrayRenderTargetView right)
        {
            return !(left == right);
        }
    }
}