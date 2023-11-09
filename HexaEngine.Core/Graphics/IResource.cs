namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a resource in graphics programming.
    /// </summary>
    public unsafe interface IResource : IDeviceChild
    {
        /// <summary>
        /// Gets the dimension of the resource.
        /// </summary>
        ResourceDimension Dimension { get; }

        /// <summary>
        /// Maximum number of mip levels for a resource.
        /// </summary>
        public const int MaximumMipLevels = unchecked(15);

        /// <summary>
        /// Maximum size in megabytes for a resource.
        /// </summary>
        public const int ResourceSizeInMegabytes = unchecked(128);

        /// <summary>
        /// Maximum size for a 1D texture array.
        /// </summary>
        public const int MaximumTexture1DArraySize = unchecked(2048);

        /// <summary>
        /// Maximum size for a 2D texture array.
        /// </summary>
        public const int MaximumTexture2DArraySize = unchecked(2048);

        /// <summary>
        /// Maximum size for a 1D texture.
        /// </summary>
        public const int MaximumTexture1DSize = unchecked(16384);

        /// <summary>
        /// Maximum size for a 2D texture.
        /// </summary>
        public const int MaximumTexture2DSize = unchecked(16384);

        /// <summary>
        /// Maximum size for a 3D texture.
        /// </summary>
        public const int MaximumTexture3DSize = unchecked(2048);

        /// <summary>
        /// Maximum size for a cube texture.
        /// </summary>
        public const int MaximumTextureCubeSize = unchecked(16384);

        /// <summary>
        /// Calculates the subresource index based on the mip slice, array slice, and mip levels.
        /// </summary>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="arraySlice">The array slice index.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <returns>The calculated subresource index.</returns>
        public static int CalculateSubresourceIndex(int mipSlice, int arraySlice, int mipLevels)
        {
            return mipSlice + (arraySlice * mipLevels);
        }
    }
}