namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a render target view (RTV) in graphics programming, which allows rendering to a resource as a target.
    /// </summary>
    public interface IRenderTargetView : IDeviceChild
    {
        /// <summary>
        /// Gets the description of the render target view.
        /// </summary>
        RenderTargetViewDescription Description { get; }

        /// <summary>
        /// Gets the native pointer to the underlying render target view object.
        /// </summary>
        new nint NativePointer { get; }

        /// <summary>
        /// Gets or sets the debug name for the render target view.
        /// </summary>
        new string? DebugName { get; set; }
    }
}