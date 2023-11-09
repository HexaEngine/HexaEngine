namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a buffer resource used in graphics rendering.
    /// </summary>
    public unsafe interface IBuffer : IResource
    {
        /// <summary>
        /// Gets the description of the buffer, which includes details such as size, usage, and flags.
        /// </summary>
        BufferDescription Description { get; }

        /// <summary>
        /// Gets the length or the number of elements in the buffer.
        /// </summary>
        int Length { get; }
    }
}