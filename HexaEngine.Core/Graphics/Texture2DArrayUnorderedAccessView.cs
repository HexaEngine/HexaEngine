namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents an unordered access view for a 2D texture array.
    /// </summary>
    public struct Texture2DArrayUnorderedAccessView
    {
        /// <summary>
        /// The mip slice index.
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