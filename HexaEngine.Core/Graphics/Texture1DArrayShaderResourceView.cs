namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a partial description of a shader resource view for a 1D texture array.
    /// </summary>
    public partial struct Texture1DArrayShaderResourceView
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// The index of the first array slice.
        /// </summary>
        public int FirstArraySlice;

        /// <summary>
        /// The number of array slices.
        /// </summary>
        public int ArraySize;
    }
}