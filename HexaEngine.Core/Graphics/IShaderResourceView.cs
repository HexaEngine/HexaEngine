namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view (SRV) in graphics programming, which provides access to a resource from a shader.
    /// </summary>
    public interface IShaderResourceView : IDeviceChild
    {
        /// <summary>
        /// Gets the description of the shader resource view.
        /// </summary>
        ShaderResourceViewDescription Description { get; }

        /// <summary>
        /// Gets the native pointer to the underlying shader resource view object.
        /// </summary>
        new nint NativePointer { get; }

        /// <summary>
        /// Gets or sets the debug name for the shader resource view.
        /// </summary>
        new string? DebugName { get; set; }
    }
}