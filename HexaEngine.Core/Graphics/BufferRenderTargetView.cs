namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a description for creating a render target view for a buffer resource.
    /// </summary>
    public struct BufferRenderTargetView : IEquatable<BufferRenderTargetView>
    {
        /// <summary>
        /// The index of the first element to be accessed.
        /// </summary>
        public int FirstElement;

        /// <summary>
        /// The offset of the first element.
        /// </summary>
        public int ElementOffset;

        /// <summary>
        /// The number of elements to access.
        /// </summary>
        public int NumElements;

        /// <summary>
        /// The width of each element in bytes.
        /// </summary>
        public int ElementWidth;

        public override readonly bool Equals(object? obj)
        {
            return obj is BufferRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(BufferRenderTargetView other)
        {
            return FirstElement == other.FirstElement &&
                   ElementOffset == other.ElementOffset &&
                   NumElements == other.NumElements &&
                   ElementWidth == other.ElementWidth;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(FirstElement, ElementOffset, NumElements, ElementWidth);
        }

        public static bool operator ==(BufferRenderTargetView left, BufferRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BufferRenderTargetView left, BufferRenderTargetView right)
        {
            return !(left == right);
        }
    }
}