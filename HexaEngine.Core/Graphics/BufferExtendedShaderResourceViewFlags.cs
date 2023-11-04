namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags that specify how a shader resource view for a buffer resource is accessed.
    /// </summary>
    [Flags]
    public enum BufferExtendedShaderResourceViewFlags : int
    {
        /// <summary>
        /// Specifies raw access to the buffer data.
        /// </summary>
        Raw = unchecked(1),

        /// <summary>
        /// No special flags for buffer access.
        /// </summary>
        None = unchecked(0)
    }
}