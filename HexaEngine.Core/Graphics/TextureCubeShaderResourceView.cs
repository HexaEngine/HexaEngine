namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a cube texture.
    /// </summary>
    public struct TextureCubeShaderResourceView
    {
        /// <summary>
        /// The most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;
    }
}