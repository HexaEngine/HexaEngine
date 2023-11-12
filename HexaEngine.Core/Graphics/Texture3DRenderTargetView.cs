namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a render target view for a 3D texture.
    /// </summary>
    public struct Texture3DRenderTargetView
    {
        /// <summary>
        /// The index of the mip level to use mip slice.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first depth slice to use in the render target view.
        /// </summary>
        public int FirstWSlice;

        /// <summary>
        /// The number of depth slices to use in the render target view.
        /// </summary>
        public int WSize;
    }
}