namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a description for creating an unordered access view for a buffer resource.
    /// </summary>
    public struct BufferUnorderedAccessView : IEquatable<BufferUnorderedAccessView>
    {
        /// <summary>
        /// The index of the first element to be accessed.
        /// </summary>
        public int FirstElement;

        /// <summary>
        /// The number of elements to access.
        /// </summary>
        public int NumElements;

        /// <summary>
        /// Flags that describe the unordered access view.
        /// </summary>
        public BufferUnorderedAccessViewFlags Flags;

        public override readonly bool Equals(object? obj)
        {
            return obj is BufferUnorderedAccessView view && Equals(view);
        }

        public readonly bool Equals(BufferUnorderedAccessView other)
        {
            return FirstElement == other.FirstElement &&
                   NumElements == other.NumElements &&
                   Flags == other.Flags;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(FirstElement, NumElements, Flags);
        }

        public static bool operator ==(BufferUnorderedAccessView left, BufferUnorderedAccessView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BufferUnorderedAccessView left, BufferUnorderedAccessView right)
        {
            return !(left == right);
        }
    }
}