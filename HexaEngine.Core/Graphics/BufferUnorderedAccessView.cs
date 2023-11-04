namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a description for creating an unordered access view for a buffer resource.
    /// </summary>
    public struct BufferUnorderedAccessView
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
    }
}