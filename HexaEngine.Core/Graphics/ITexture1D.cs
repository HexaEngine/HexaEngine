namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a 1D texture resource in a graphics device.
    /// </summary>
    public interface ITexture1D : IResource
    {
        /// <summary>
        /// Gets the description of the 1D texture.
        /// </summary>
        Texture1DDescription Description { get; }
    }
}