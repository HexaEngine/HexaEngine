namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Describes an extended shader resource view for a buffer resource.
    /// </summary>
    public struct BufferExtendedShaderResourceView
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
    }
}