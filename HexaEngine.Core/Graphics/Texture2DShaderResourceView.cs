namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a 2D texture.
    /// </summary>
    public struct Texture2DShaderResourceView
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