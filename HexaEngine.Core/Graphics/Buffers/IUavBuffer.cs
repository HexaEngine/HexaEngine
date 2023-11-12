namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents an unordered access view buffer in graphics programming.
    /// </summary>
    public interface IUavBuffer : IBuffer
    {
        /// <summary>
        /// Gets the size, in bytes, of each element in the buffer.
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// Gets a value indicating whether the buffer can be read.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the buffer can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Gets the copy buffer associated with the unordered access view buffer.
        /// </summary>
        IBuffer? CopyBuffer { get; }

        /// <summary>
        /// Gets the shader resource view associated with the unordered access view buffer.
        /// </summary>
        IShaderResourceView SRV { get; }

        /// <summary>
        /// Gets the unordered access view associated with the unordered access view buffer.
        /// </summary>
        IUnorderedAccessView UAV { get; }

        /// <summary>
        /// Clears the unordered access view buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for clearing the buffer.</param>
        void Clear(IGraphicsContext context);

        /// <summary>
        /// Copies the contents of the unordered access view buffer to the specified buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for copying the data.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        /// Reads data from the unordered access view buffer to the specified destination pointer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for reading the data.</param>
        /// <param name="dst">A pointer to the destination memory where the data will be copied.</param>
        /// <param name="length">The length of the data to read, in bytes.</param>
        unsafe void Read(IGraphicsContext context, void* dst, int length);

        /// <summary>
        /// Writes data from the specified source pointer to the unordered access view buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for writing the data.</param>
        /// <param name="src">A pointer to the source memory containing the data to write.</param>
        /// <param name="length">The length of the data to write, in bytes.</param>
        unsafe void Write(IGraphicsContext context, void* src, int length);
    }
}