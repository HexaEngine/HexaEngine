namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents an unordered access view for a 3D texture.
    /// </summary>
    public struct Texture3DUnorderedAccessView
    {
        /// <summary>
        /// The index of the mip level to use mip slice.
        /// </summary>
        public int MipSlice;

        /// <summary>
        /// The index of the first depth level to use in the view.
        /// </summary>
        public int FirstWSlice;

        /// <summary>
        /// The number of depth levels in the view.
        /// </summary>
        public int WSize;
    }
}