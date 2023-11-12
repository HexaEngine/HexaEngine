namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayShaderResourceView
    {
        /// <summary>
        /// The index of the most detailed mip level.
        /// </summary>
        public int MostDetailedMip;

        /// <summary>
        /// The number of mip levels in the view.
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