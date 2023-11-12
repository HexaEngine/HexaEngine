namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a 3D texture.
    /// </summary>
    public struct Texture3DShaderResourceView
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the view.
        /// </summary>
        public int MipLevels;
    }
}