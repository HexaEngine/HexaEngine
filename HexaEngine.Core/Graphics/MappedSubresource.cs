namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a mapped subresource with information about the data pointer, row pitch, and depth pitch.
    /// </summary>
    public struct MappedSubresource
    {
        /// <summary>
        /// Gets or sets a pointer to the mapped data.
        /// </summary>
        public unsafe void* PData;

        /// <summary>
        /// Gets or sets the row pitch.
        /// </summary>
        public uint RowPitch;

        /// <summary>
        /// Gets or sets the depth pitch.
        /// </summary>
        public uint DepthPitch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedSubresource"/> struct.
        /// </summary>
        /// <param name="pData">The pointer to the mapped data.</param>
        /// <param name="rowPitch">The row pitch.</param>
        /// <param name="depthPitch">The depth pitch.</param>
        public unsafe MappedSubresource(void* pData = null, uint? rowPitch = null, uint? depthPitch = null)
        {
            this = default;
            if (pData != null)
            {
                PData = pData;
            }

            if (rowPitch.HasValue)
            {
                RowPitch = rowPitch.Value;
            }

            if (depthPitch.HasValue)
            {
                DepthPitch = depthPitch.Value;
            }
        }

        /// <summary>
        /// Creates a span of type <typeparamref name="T"/> from the mapped data pointer.
        /// </summary>
        /// <typeparam name="T">The type of data in the span.</typeparam>
        /// <param name="length">The length of the span.</param>
        /// <returns>A span representing the mapped data.</returns>
        public readonly unsafe Span<T> AsSpan<T>(int length) where T : unmanaged
        {
            return new Span<T>(PData, length);
        }
    }
}