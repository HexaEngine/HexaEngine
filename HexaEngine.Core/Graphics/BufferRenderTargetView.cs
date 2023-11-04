namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a description for creating a render target view for a buffer resource.
    /// </summary>
    public struct BufferRenderTargetView
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
    }
}