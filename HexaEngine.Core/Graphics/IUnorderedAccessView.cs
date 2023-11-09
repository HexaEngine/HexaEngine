namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents an unordered access view (UAV) in graphics programming, which provides read and write access to a resource from a shader.
    /// </summary>
    public interface IUnorderedAccessView : IDeviceChild
    {
        /// <summary>
        /// Gets the description of the unordered access view.
        /// </summary>
        UnorderedAccessViewDescription Description { get; }

        /// <summary>
        /// Gets the native pointer to the underlying unordered access view object.
        /// </summary>
        new nint NativePointer { get; }

        /// <summary>
        /// Gets or sets the debug name for the unordered access view.
        /// </summary>
        new string? DebugName { get; set; }
    }
}