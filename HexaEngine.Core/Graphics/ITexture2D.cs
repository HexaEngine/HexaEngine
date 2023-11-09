namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a 2D texture in graphics programming.
    /// </summary>
    public interface ITexture2D : IResource
    {
        /// <summary>
        /// Gets the description of the 2D texture.
        /// </summary>
        Texture2DDescription Description { get; }
    }
}