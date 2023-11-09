namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a view that allows read and write access to a depth-stencil buffer.
    /// </summary>
    public interface IDepthStencilView : IDeviceChild
    {
        /// <summary>
        /// Gets the description of the depth-stencil view.
        /// </summary>
        DepthStencilViewDescription Description { get; }
    }
}