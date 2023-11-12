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

        /// <summary>
        /// Resizes the constant buffer to the specified length.
        /// </summary>
        /// <param name="length">The new length of the buffer.</param>
        void Resize(uint length);

        /// <summary>
        /// Updates the content of the constant buffer using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        void Update(IGraphicsContext context);
    }

    /// <summary>
    /// Represents a generic constant buffer in graphics programming with unmanaged type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the constant buffer (must be unmanaged).</typeparam>
    public interface IConstantBuffer<T> : IConstantBuffer where T : unmanaged
    {
        /// <summary>
        /// Gets or sets the element at the specified index in the constant buffer.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        T this[int index] { get; set; }

        /// <summary>
        /// Gets a pointer to the local data of the constant buffer.
        /// </summary>
        unsafe T* Local { get; }

        /// <summary>
        /// Updates the content of a specific element in the constant buffer using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <param name="value">The new value for the specified element.</param>
        void Update(IGraphicsContext context, T value);
    }
}