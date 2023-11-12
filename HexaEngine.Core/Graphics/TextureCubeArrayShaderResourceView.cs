namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for an array of cube textures.
    /// </summary>
    public struct TextureCubeArrayShaderResourceView
    {
        /// <summary>
        /// The most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// The index of the first 2D array face.
        /// </summary>
        public int First2DArrayFace;

        /// <summary>
        /// The number of cube textures in the array.
        /// </summary>
        public int NumCubes;
    }
}