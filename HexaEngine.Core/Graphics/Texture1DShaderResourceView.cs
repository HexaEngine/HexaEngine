namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a 1D texture.
    /// </summary>
    public struct Texture1DShaderResourceView
    {
        /// <summary>
        /// The index of the most detailed mip level in the texture.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the texture.
        /// </summary>
        public int MipLevels;
    }
}