namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents an unordered access view for a 1D texture.
    /// </summary>
    public struct Texture1DUnorderedAccessView : IEquatable<Texture1DUnorderedAccessView>
    {
        /// <summary>
        /// The index of the mip level to use for the view.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture1DUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(Texture1DUnorderedAccessView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture1DUnorderedAccessView left, Texture1DUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DUnorderedAccessView left, Texture1DUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}