namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Describes an extended shader resource view for a buffer resource.
    /// </summary>
    public struct BufferExtendedShaderResourceView : IEquatable<BufferExtendedShaderResourceView>
    {
        /// <summary>
        /// Gets or sets the index of the first element to access in the buffer.
        /// </summary>
        public int FirstElement;

        /// <summary>
        /// Gets or sets the number of elements in the view.
        /// </summary>
        public int NumElements;

        /// <summary>
        /// Gets or sets flags that specify how the shader resource view is accessed.
        /// </summary>
        public BufferExtendedShaderResourceViewFlags Flags;

        public override readonly bool Equals(object? obj)
        {
            return obj is BufferExtendedShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(BufferExtendedShaderResourceView other)
        {
            return FirstElement == other.FirstElement &&
                   NumElements == other.NumElements &&
                   Flags == other.Flags;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(FirstElement, NumElements, Flags);
        }

        public static bool operator ==(BufferExtendedShaderResourceView left, BufferExtendedShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BufferExtendedShaderResourceView left, BufferExtendedShaderResourceView right)
        {
            return !(left == right);
        }
    }
}