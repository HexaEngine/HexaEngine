namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents data for a subresource.
    /// </summary>
    public struct SubresourceData
    {
        /// <summary>
        /// The pointer to the data.
        /// </summary>
        public IntPtr DataPointer;

        /// <summary>
        /// The row pitch, which is the width of the data.
        /// </summary>
        public int RowPitch;

        /// <summary>
        /// The slice pitch, which is the depth of the data.
        /// </summary>
        public int SlicePitch;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubresourceData"/> struct.
        /// </summary>
        /// <param name="dataPointer">The pointer to the data.</param>
        /// <param name="rowPitch">The row pitch.</param>
        /// <param name="slicePitch">The slice pitch.</param>
        public SubresourceData(IntPtr dataPointer, int rowPitch = 0, int slicePitch = 0)
        {
            DataPointer = dataPointer;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubresourceData"/> struct.
        /// </summary>
        /// <param name="dataPointer">The pointer to the data.</param>
        /// <param name="rowPitch">The row pitch.</param>
        /// <param name="slicePitch">The slice pitch.</param>
        public unsafe SubresourceData(void* dataPointer, int rowPitch = 0, int slicePitch = 0)
        {
            DataPointer = new IntPtr(dataPointer);
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }

        /// <summary>
        /// Converts the data to a span of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="length">The length of the span.</param>
        /// <returns>A span of the specified type.</returns>
        public unsafe Span<T> AsSpan<T>(int length) where T : unmanaged
        {
            return new Span<T>(DataPointer.ToPointer(), length);
        }
    }
}