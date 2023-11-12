namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a partial description of a depth-stencil view for a 1D texture array.
    /// </summary>
    public partial struct Texture1DArrayDepthStencilView
    {
        /// <summary>
        /// The index of the mip level.
        /// </summary>
        public int MipSlice;

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