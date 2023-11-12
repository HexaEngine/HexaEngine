namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a render target view for a multisampled 2D texture array.
    /// </summary>
    public struct Texture2DMultisampledArrayRenderTargetView
    {
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