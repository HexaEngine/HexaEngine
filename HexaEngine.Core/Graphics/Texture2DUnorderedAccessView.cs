namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents an unordered access view for a 2D texture.
    /// </summary>
    public struct Texture2DUnorderedAccessView : IEquatable<Texture2DUnorderedAccessView>
    {
        /// <summary>
        /// The mip slice of the texture.
        /// </summary>
        public int MipSlice;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(Texture2DUnorderedAccessView other)
        {
            return MipSlice == other.MipSlice;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(MipSlice);
        }

        public static bool operator ==(Texture2DUnorderedAccessView left, Texture2DUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DUnorderedAccessView left, Texture2DUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}