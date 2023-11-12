namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a depth-stencil view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayDepthStencilView
    {
        /// <summary>
        /// The index of the mip level to use for the view.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first array slice to use in the view.
        /// </summary>
        public int FirstArraySlice;

        /// <summary>
        /// The total number of slices in the view.
        /// </summary>
        public int ArraySize;
    }
}