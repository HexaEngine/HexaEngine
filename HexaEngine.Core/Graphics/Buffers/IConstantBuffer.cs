namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a constant buffer in graphics programming.
    /// </summary>
    public interface IConstantBuffer : IBuffer
    {
        /// <summary>
        /// Gets the underlying buffer associated with the constant buffer.
        /// </summary>
        IBuffer Buffer { get; }

        unsafe void Resize(void* data, uint size);

        unsafe void Update(IGraphicsContext context, void* data, uint size);
    }

    /// <summary>
    /// Represents a generic constant buffer in graphics programming with unmanaged type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the constant buffer (must be unmanaged).</typeparam>
    public interface IConstantBuffer<T> : IConstantBuffer where T : unmanaged
    {
        /// <summary>
        /// Resizes the constant buffer to the specified length.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="length">The new length of the buffer.</param>
        unsafe void Resize(T* items, uint length);

        /// <summary>
        /// Updates the content of a specific element in the constant buffer using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <param name="value">The new value for the specified element.</param>
        void Update(IGraphicsContext context, in T value);

        unsafe void Update(IGraphicsContext context, T* values, uint count);
    }
}