namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader resource view for a multisampled 2D texture array.
    /// </summary>
    public struct Texture2DMultisampledArrayShaderResourceView
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