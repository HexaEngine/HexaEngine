namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents an unordered access view for a 3D texture.
    /// </summary>
    public struct Texture3DUnorderedAccessView : IEquatable<Texture3DUnorderedAccessView>
    {
        /// <summary>
        /// The index of the mip level to use mip slice.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first depth level to use in the view.
        /// </summary>
        public int FirstWSlice;

        /// <summary>
        /// The number of depth levels in the view.
        /// </summary>
        public int WSize;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture3DUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(Texture3DUnorderedAccessView other)
        {
            return MipSlice == other.MipSlice &&
                   FirstWSlice == other.FirstWSlice &&
                   WSize == other.WSize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice, FirstWSlice, WSize);
        }

        public static bool operator ==(Texture3DUnorderedAccessView left, Texture3DUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture3DUnorderedAccessView left, Texture3DUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}