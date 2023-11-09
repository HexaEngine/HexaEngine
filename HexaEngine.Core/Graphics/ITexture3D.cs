namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a 3D texture resource in a graphics device.
    /// </summary>
    public interface ITexture3D : IResource
    {
        /// <summary>
        /// Gets the description of the 3D texture.
        /// </summary>
        Texture3DDescription Description { get; }
    }
}